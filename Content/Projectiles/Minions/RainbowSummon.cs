using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Calamity_OverHaul_Patch.Content.Items.GameStageHelper;

namespace Calamity_Overhaul_Patch.Content.Projectiles.Minions
{
    public class Summonofrainbow : ModProjectile
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


        // 召唤物AI（最简单的悬浮跟随）
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead )
            {
                Projectile.Kill();
                return;
            }

            bool holdingShadowRainbowWhip = player.HeldItem.ModItem is Calamity_OverHaul_Patch.Content.Items.ShadowRainbowWhip;

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

            if ( (int)stage < 6  )
            {
                detectRange = 350f;
                attackCooldown = 45;
                canWallDetect = false;    // 肉前 ❌ 不能穿墙索敌
                canPenetrateWall = false; // 肉前 ❌ 射弹不能穿墙
            }
            else if (stage == GameStage.HardModePrePlantera)
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
            ImpAI(player, detectRange, attackCooldown, canWallDetect, canPenetrateWall);

            Projectile.timeLeft = 2;
        }

        private void ImpAI(Player player, float detectRange, int attackCooldown, bool canWallDetect, bool canPenetrateWall)
        {
            NPC target = null;
            float maxDist = detectRange;

            // 索敌逻辑不变
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

            // 共用平滑参数（越大越丝滑，越不顿挫）
            float inertia = 25f;
            float smoothSpeed = 6.5f;

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
                        207, 
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
                // 【无目标时平滑跟随玩家】
                Vector2 toPlayer = player.Center - Projectile.Center;
                float playerDist = toPlayer.Length();

                // 离玩家超过 100 才缓慢回归，不会猛冲
                if (playerDist > 100f)
                {
                    toPlayer.Normalize();

                    // 速度 = 基础速度 + 距离比例加成（正比）
                    float speedMulti = MathHelper.Clamp(playerDist / 150f, 1f, 2.8f);
                    float finalSpeed = smoothSpeed * 0.9f * speedMulti;

                    Vector2 wishVel = toPlayer * finalSpeed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1.5f) + wishVel) / inertia;
                }
                else
                {
                    // 近距离轻柔悬浮，几乎不顿
                    Projectile.velocity *= 0.94f;
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