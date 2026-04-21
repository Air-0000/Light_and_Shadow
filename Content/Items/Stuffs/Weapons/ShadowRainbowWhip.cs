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

        private int summonTimer;
        public override void HoldItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            summonTimer++;
            if (summonTimer >= 30)
            {
                summonTimer = 0;
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
                    int SummonDamage = WhipCalculator.WhipDamage(WhipCalculator.SummonDamageTypeId);
                    int newProj = Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item),
                        spawnPos, 
                        Vector2.Zero,
                        ModContent.ProjectileType<RainbowSummon>(),
                        SummonDamage, 
                        Item.knockBack, 
                        player.whoAmI
                        );
                    Main.projectile[newProj].minionSlots = neededSlots;
                }
            }

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
                .DisableDecraft()
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