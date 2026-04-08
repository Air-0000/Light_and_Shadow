using Microsoft.Xna.Framework;
using Terraria;
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


            Projectile.timeLeft = 2; // 保持不死

            GameStage stage = GetCurrentGameStage();

            float detectRange = 0;      // 索敌范围
            int attackCooldown = 0;      // 攻击间隔
            bool canPassThroughWalls ; // 是否穿墙

            if ( (int)stage < 6  )
            {
                detectRange = 350f;
                attackCooldown = 45;
                canPassThroughWalls = false; // 肉前 ❌ 不可穿墙
            }
            else if (stage == GameStage.HardModePrePlantera)
            {
                detectRange = 550f;
                attackCooldown = 32;
                canPassThroughWalls = true; // 肉后 ✅ 穿墙
            }
            else if (stage == GameStage.PostPlantera)
            {
                detectRange = 750f;
                attackCooldown = 22;
                canPassThroughWalls = true;
            }
            else // PostMoonLord
            {
                detectRange = 950f;
                attackCooldown = 16;
                canPassThroughWalls = true;
            }

            // 应用穿墙设置
            Projectile.tileCollide = !canPassThroughWalls;

            ImpAI(player, detectRange, attackCooldown);

            Projectile.timeLeft = 2;
        }

        private void ImpAI(Player player, float detectRange, int attackCooldown)
        {
            NPC target = null;
            float maxDist = detectRange;

            // 索敌
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(this) && npc.active && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < maxDist)
                    {
                        maxDist = dist;
                        target = npc;
                    }
                }
            }

            // 攻击逻辑
            if (target != null)
            {
                Projectile.ai[0]++;
                if (Projectile.ai[0] >= attackCooldown)
                {
                    Projectile.ai[0] = 0;

                    Vector2 vel = Vector2.Normalize(target.Center - Projectile.Center) * 10f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center,
                        vel,
                        207,         // 火焰小鬼火球
                        Projectile.damage,
                        Projectile.knockBack,
                        player.whoAmI
                    );
                }

                // 靠近目标
                Vector2 dir = target.Center - Projectile.Center;
                if (dir.Length() > 150f)
                {
                    dir.Normalize();
                    Projectile.velocity = dir * 8f;
                }
            }
            else
            {
                // 没有目标就回到玩家身边
                Vector2 toPlayer = player.Center - Projectile.Center;
                if (toPlayer.Length() > 150f)
                {
                    toPlayer.Normalize();
                    Projectile.velocity = toPlayer * 7f;
                }
                else
                {
                    // 悬浮
                    Projectile.velocity *= 0.92f;
                }
            }

            // 朝向
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

        

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4; // 火焰小鬼是4帧动画
        }



    }
}