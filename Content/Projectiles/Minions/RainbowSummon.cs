using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Light_and_Shadow.Content.Items.GameStageHelper;

namespace Light_and_Shadow.Content.Projectiles.Minions
{
    public class RainbowSummon : ModProjectile
    {
        // 召唤物基础设置
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;

            Projectile.friendly = true;
            Projectile.minion = true;          // 标记为仆从
            Projectile.minionSlots = 1;         // 占用1个仆从位
            Projectile.penetrate = -1;          // 无限穿透
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.netImportant = true;     // 多人同步
            Projectile.usesLocalNPCImmunity = true;  // 关闭碰撞攻击
            Projectile.localNPCHitCooldown = -1;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4; // 火焰小鬼是4帧动画
        }
        public override void PostDraw(Color lightColor)
        {
            // 发光颜色：R, G, B, 透明度
            Lighting.AddLight(Projectile.Center, 0.9f, 0.6f, 1.0f);
        }


        // 召唤物AI（最简单的悬浮跟随）
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead )
            {
                Projectile.Kill();
                return;
            }

            bool holdingShadowRainbowWhip = player.HeldItem.ModItem is Items.Stuffs.Weapons.ShadowRainbowWhip;

            if (!holdingShadowRainbowWhip)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2; // 保持不死

            GameStage stage = GetCurrentGameStage();

            float detectRange = 0;      // 索敌范围
            int attackCooldown = 0;      // 攻击间隔
            bool canWallDetect = false;    // 索敌是否穿墙
            bool canPenetrateWall; // 射弹是否穿墙

            if (  stage < GameStage.HardModePrePlantera )
            {
                detectRange = 350f;
                attackCooldown = 45;
                canWallDetect = false;    // 肉前 ❌ 不能穿墙索敌
                canPenetrateWall = false; // 肉前 ❌ 射弹不能穿墙
            }
            else if (stage < GameStage.PostPlantera )
            {
                detectRange = 550f;
                attackCooldown = 32;
                canWallDetect = true;     // 肉后 ✅ 穿墙索敌
                canPenetrateWall = true;  // 肉后 ✅ 射弹穿墙
            }
            else if (stage == GameStage.PostPlantera)
            {
                detectRange = 750f;
                attackCooldown = 22;
                canWallDetect = true;     // 肉后 ✅ 穿墙索敌
                canPenetrateWall = true;  // 肉后 ✅ 射弹穿墙
            }
            else // PostMoonLord
            {
                detectRange = 950f;
                attackCooldown = 16;
                canWallDetect = true;     // 肉后 ✅ 穿墙索敌
                canPenetrateWall = true;  // 肉后 ✅ 射弹穿墙
            }

            
            Projectile.tileCollide = false;  //召唤物始终穿墙
            // 应用穿墙设置
            CrystalAI(player, detectRange, attackCooldown, canWallDetect, canPenetrateWall);

            Projectile.timeLeft = 2;
        }

        private void CrystalAI(Player player, float detectRange, int attackCooldown, bool canWallDetect, bool canPenetrateWall)
        {
            // 给每只召唤物分配一个固定偏移角度
            float rotateOffset = Projectile.identity % 2 == 0 ? 45f : -45f;
            float radius = 60f;

            // 计算偏移位置
            Vector2 idleOffset = new Vector2(1, 0).RotatedBy(MathHelper.ToRadians(rotateOffset)) * radius;
            Vector2 targetPos = player.Center + idleOffset;

            NPC target = null;
            float maxDist = detectRange;
            float ownerMoveRange = 10 * 16f;  // 召唤物移动范围限制：主人为圆心，10格（1格=16像素）
            float pullBackSpeed = 3f;  // 拉回力度（越大拉回越快，推荐2~5）

            int[] whipDebuffIDs = new int[]
            {
                BuffID.BlandWhipEnemyDebuff,    // 307
                BuffID.SwordWhipNPCDebuff,      // 309
                BuffID.ScytheWhipEnemyDebuff,   // 310
                BuffID.FlameWhipEnemyDebuff,    // 313
                BuffID.ThornWhipNPCDebuff,      // 315
                BuffID.RainbowWhipNPCDebuff,    // 316
                BuffID.MaceWhipNPCDebuff,       // 319      
                BuffID.BoneWhipNPCDebuff,       // 326
                BuffID.CoolWhipNPCDebuff        // 340
            };

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy(this))
                {
                    // 多人兼容关键：仅匹配当前召唤物主人的鞭子标记（player.whoAmI是当前主人ID）
                    bool hasWhipDebuff = false;

                    for (int i = 0; i < npc.buffType.Length; i++)
                    {
                        int buffType = npc.buffType[i];
                        // 判断是否是鞭子标记Buff（BuffID.WhipTag）
                        if (Array.IndexOf(whipDebuffIDs, npc.buffType[i]) != -1 && npc.buffTime[i] > 0)
                        {
                            hasWhipDebuff = true;
                            break;
                        }
                    }

                    bool canSee = canWallDetect || Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1);  // 视野判定

                    if (hasWhipDebuff && canSee)
                    {
                        target = npc;
                        maxDist = Vector2.Distance(Projectile.Center, npc.Center);
                        break; // 找到主人标记的敌人，直接设为优先目标
                    }
                }
            }

            if (target == null)
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy(this) && npc.active && !npc.friendly)
                    {
                        bool canSee = canWallDetect || Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1);
                        if (canSee)
                        {
                            float dist = Vector2.Distance(Projectile.Center, npc.Center);
                            if (dist < maxDist)
                            {
                                maxDist = dist;
                                target = npc;
                            }
                        }
                    }
                }
            }

            float distToOwner = Vector2.Distance(Projectile.Center, player.Center);
            // 如果超出10格范围，执行拉回逻辑
            if (distToOwner > ownerMoveRange)
            {
                // 计算从召唤物指向主人的归一化方向（仅保留方向，去掉距离）
                Vector2 pullBackDir = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                // 给召唤物施加拉回速度（叠加到原有速度上，保证优先拉回）
                Projectile.velocity = pullBackDir * pullBackSpeed;
            }

            float smoothSpeed = 6.5f;
            // 共用平滑参数（越大越丝滑，越不顿挫）
            float inertia = 25f;
            if (target != null)
            {
                // 攻击冷却
                Projectile.ai[0]++;
                if (Projectile.ai[0] >= attackCooldown)
                {
                    Projectile.ai[0] = 0;
                    Vector2 vel = Vector2.Normalize(target.Center - Projectile.Center) * 10f;

                    int fireball = Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center,
                        vel,
                        ProjectileID.CrystalPulse,
                        Projectile.damage,
                        Projectile.knockBack,
                        player.whoAmI
                    );

                    if (canPenetrateWall)
                        Main.projectile[fireball].tileCollide = false;
                }


                // 【平滑远程站位】保持 180-320 距离，不贴脸、不顿挫
                Vector2 toTarget = target.Center - Projectile.Center;
                float dist = toTarget.Length();

                if (dist > 320f) // 太远 → 缓慢靠近
                {
                    toTarget.Normalize();
                    Vector2 wishVel = toTarget * smoothSpeed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + wishVel) / inertia;
                }
                else if (dist < 180f) // 太近 → 轻柔后退
                {
                    toTarget.Normalize();
                    Vector2 wishVel = -toTarget * (smoothSpeed * 1.1f);
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + wishVel) / inertia;
                }
                else // 合适距离 → 轻柔悬浮
                {
                    Projectile.velocity *= 0.96f;
                }
            }

            else
            {
                // 无目标时：均匀环绕玩家，彻底杜绝重合
                int ownIndex = 0;
                int totalMinions = 0;

                // 先统计总数，并确定自己是第几个
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == Projectile.type)
                    {
                        // 找到自己之前的仆从，计数+1
                        if (proj.whoAmI < Projectile.whoAmI)
                            ownIndex++;

                        totalMinions++;
                    }
                }

                // 确保不会除0
                totalMinions = Math.Max(1, totalMinions);

                // 按数量均分圆周
                float anglePer = MathHelper.TwoPi / totalMinions;
                float baseAngle = ownIndex * anglePer;

                // 缓慢旋转
                float angle = baseAngle + (float)Main.time * 0.015f;

                // 环绕半径
                float circleRadius = 120f;
                Vector2 desiredPos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * circleRadius;

                // 丝滑移动
                Vector2 diff = desiredPos - Projectile.Center;
                float dist = diff.Length();

                if (dist > 8f)
                {
                    float speed = MathHelper.Lerp(0f, 10f, dist / 80f);
                    diff.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, diff * speed, 0.15f);
                }
                else
                {
                    Projectile.velocity *= 0.92f;
                }
            }

            Projectile.spriteDirection = System.Math.Sign(Projectile.velocity.X);

            // 动画
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 4)
                    Projectile.frame = 0;
            }
        }
    }
}