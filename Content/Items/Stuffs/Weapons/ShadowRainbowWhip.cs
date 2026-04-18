using JetBrains.Annotations;
using Light_and_Shadow.Content.Items.Placeable;
using Light_and_Shadow.Content.Projectiles.Minions;
using Light_and_Shadow.Content.Tiles.Placeable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Light_and_Shadow.Content.Items.GameStageHelper;

namespace Light_and_Shadow.Content.Items.Stuffs.Weapons
{
    public class ShadowRainbowWhip : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.knockBack = 2f;
            Item.useTime = 40;
            Item.useAnimation = 30;
            Item.shootSpeed = 1f;

            Item.width = 30;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item152;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ProjectileID.RainbowWhip;
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.useTurn = true;
        }

        // 镜像技能核心数据
        private Vector2[] positionHistory = new Vector2[120];
        private int historyIndex = 0;
        private bool skillActive = false;
        private int skillTimer = 0;
        private int cooldown = 0;
        private float mirrorX;
        private Vector2 oldPos;
        private bool lastKeyDown = false;

        public override void HoldItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            // 原有自动召唤仆从
            player.taxTimer++;
            if (player.taxTimer >= 30)
            {
                player.taxTimer = 0;
                int ownCount = 0;
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<RainbowSummon>())
                        ownCount++;
                }
                float neededSlots = ownCount + 1;
                float freeSlots = player.maxMinions - player.numMinions;
                if (freeSlots >= neededSlots)
                {
                    int offset = ownCount * 24;
                    Vector2 spawnPos = player.Center + new Vector2(-24 + offset, -8);
                    int realDamage = WhipCalculator.WhipDamage(WhipCalculator.SummonDamageTypeId);
                    int newProj = Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item),
                        spawnPos, Vector2.Zero,
                        ModContent.ProjectileType<RainbowSummon>(),
                        realDamage, Item.knockBack, player.whoAmI);
                    Main.projectile[newProj].minionSlots = neededSlots;
                }
            }

            // 记录2秒位置
            positionHistory[historyIndex] = player.Center;
            historyIndex = (historyIndex + 1) % 120;

            // CD
            if (cooldown > 0) cooldown--;

            // 按键 Y 触发
            if (!skillActive && cooldown <= 0)
            {
                if (Main.keyState.IsKeyDown(Keys.Y) && !lastKeyDown)
                {
                    lastKeyDown = true;

                    // 2秒前位置
                    oldPos = positionHistory[(historyIndex + 1) % 120];
                    Vector2 currentPos = player.Center;

                    // 瞬移
                    player.Center = oldPos;
                    player.velocity = Vector2.Zero;

                    // 镜面（贯穿世界）
                    mirrorX = (oldPos.X + currentPos.X) / 2f;
                    skillActive = true;
                    skillTimer = 0;

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item112, player.Center);
                }
                else if (!Main.keyState.IsKeyDown(Keys.Y))
                {
                    lastKeyDown = false;
                }
            }

            // 技能持续
            if (skillActive)
            {
                skillTimer++;

                // ======================
                // 镜子：贯穿世界上下
                // ======================
                Vector2 top = new Vector2(mirrorX, -20000);
                Vector2 bot = new Vector2(mirrorX, 20000);
                for (float k = 0; k < 1; k += 0.01f)
                {
                    Vector2 pos = Vector2.Lerp(top, bot, k);
                    Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, Vector2.Zero, 0, Color.Magenta, 1.5f);
                    d.noGravity = true;
                }

                // ======================
                // 过去身虚影（2秒前位置）
                // ======================
                for (int i = 0; i < 4; i++)
                {
                    Dust ghost = Dust.NewDustPerfect(oldPos, DustID.RainbowTorch, Vector2.Zero, 0, Color.Cyan, 1.8f);
                    ghost.noGravity = true;
                }

                // ======================
                // 镜中像（对称）
                // ======================
                Vector2 mirrorMe = Mirror(player.Center);
                for (int i = 0; i < 3; i++)
                {
                    Dust shadow = Dust.NewDustPerfect(mirrorMe, DustID.Shadowflame, Vector2.Zero, 0, Color.Purple, 1.5f);
                    shadow.noGravity = true;
                }

                // 仆从镜像
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.active && p.owner == player.whoAmI && p.minion)
                    {
                        Vector2 mPos = Mirror(p.Center);
                        Dust mp = Dust.NewDustPerfect(mPos, DustID.TintableDust, Vector2.Zero, 0, Color.Magenta, 1f);
                        mp.noGravity = true;
                    }
                }

                // 穿过镜子 / 15秒 → 结束
                bool cross = (player.oldPosition.X < mirrorX) != (player.position.X < mirrorX);
                if (cross || skillTimer >= 60 * 15)
                {
                    EndSkill(player);
                }
            }
        }

        private Vector2 Mirror(Vector2 pos)
        {
            return new Vector2(2 * mirrorX - pos.X, pos.Y);
        }

        private void EndSkill(Player player)
        {
            skillActive = false;
            skillTimer = 0;
            cooldown = 60 * 25;
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item113, player.Center);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
            => velocity *= WhipCalculator.WhipVelocity();

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
            => damage.Flat += WhipCalculator.WhipDamage(WhipCalculator.BasicDamageTypeId);

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(ModContent.TileType<ShadowAnvilTile>())
                .AddIngredient<RainbowCrystal>(1)
                .AddIngredient<ShadowCrystal>(6)
                .AddIngredient<LightCrystal>(6)
                .Register();
        }
    }

    public class WhipDamage : DamageClass { }

    public class WhipCalculator
    {
        public const int BasicDamageTypeId = 0;
        public const int AdvancedDamageTypeId = 1;
        public const int SummonDamageTypeId = 2;

        public static int WhipDamage(int DamageType)
        {
            int basicDamage = 10;
            GameStage stage = GetCurrentGameStage();
            int intStage = (int)stage;
            if (DamageType == BasicDamageTypeId)
            {
                if (stage < GameStage.HardModePrePlantera)
                    return (int)(basicDamage * (1f + Math.Log(intStage + 1)));
                if (stage < GameStage.PostPlantera)
                    return (int)(basicDamage * (intStage * 0.8f * (intStage - 6)));
                return (int)((basicDamage + 1) * (intStage * 0.8f * (intStage - 6)));
            }
            else if (DamageType == AdvancedDamageTypeId)
            {
                if (stage >= GameStage.PostMoonLord)
                    return basicDamage + (intStage - 10) * 5;
            }
            else if (DamageType == SummonDamageTypeId)
            {
                if (stage < GameStage.HardModePrePlantera)
                    return (int)(basicDamage * (1f + Math.Log(intStage + 1)) * 0.5f);
                if (stage < GameStage.PostPlantera)
                    return (int)(basicDamage * (intStage * 0.8f * (intStage - 6)));
                return (int)((basicDamage + 1) * (intStage * 0.8f * (intStage - 6)));
            }
            return 0;
        }

        public static float WhipVelocity()
        {
            GameStage stage = GetCurrentGameStage();
            float multiplier = 1;
            if (stage >= GameStage.PostEyeOfCthulhu) multiplier = 2;
            if (stage >= GameStage.HardModePrePlantera) multiplier = 3;
            if (stage >= GameStage.PostMechanicalBoss) multiplier = 3.5f;
            if (stage >= GameStage.PostPlantera) multiplier = 4;
            if (stage >= GameStage.PostGolem) multiplier = 5;
            if (stage >= GameStage.PostFishron) multiplier = 6;
            if (stage >= GameStage.PostMoonLord) multiplier = 7;
            return multiplier;
        }
    }

    public class WhipAdvancedDamageHandler : GlobalNPC
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.minion && npc.HasBuff(BuffID.RainbowWhipNPCDebuff))
            {
                modifiers.FlatBonusDamage += WhipCalculator.WhipDamage(WhipCalculator.AdvancedDamageTypeId);
            }
        }
    }
}