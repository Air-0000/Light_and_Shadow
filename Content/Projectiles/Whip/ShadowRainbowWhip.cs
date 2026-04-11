using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Projectiles.Whip
{
    public class ShadowRainbowWhip : ModProjectile
    {
        // 你要的射程（彻底生效）
        public const float MAX_RANGE = 110f;

        private Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.RainbowWhip);
            AIType = 0; // 禁用原版AI
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
        }

        public override void AI()
        {
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemAnimation = 2;
            Owner.itemTime = 2;

            Vector2 dir = (Main.MouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            Projectile.velocity = dir;

            // 伸长/收回（完全自己控制）
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[1] += 25f;
                if (Projectile.ai[1] >= MAX_RANGE)
                    Projectile.ai[0] = 1;
            }
            else
            {
                Projectile.ai[1] -= 30f;
                if (Projectile.ai[1] <= 0)
                    Projectile.Kill();
            }

            // 同步位置
            Projectile.Center = Owner.MountedCenter + dir * Projectile.ai[1];
        }

        // 碰撞（射程生效）
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                Owner.MountedCenter,
                Owner.MountedCenter + Projectile.velocity * Projectile.ai[1],
                22f, ref _);
        }

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowWhip;
    }
}