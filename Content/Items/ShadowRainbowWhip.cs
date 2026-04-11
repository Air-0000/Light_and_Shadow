using System;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

using static Light_and_Shadow.Content.Items.GameStageHelper;
using Light_and_Shadow.Content.Projectiles.Minions;
using UtfUnknown.Core.Models.MultiByte.Korean;

namespace Light_and_Shadow.Content.Items
{
    
    public class ShadowRainbowWhip : ModItem
    {   
        
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.RainbowWhip);
            Item.shoot = ModContent.ProjectileType<Content.Projectiles.Whip.ShadowRainbowWhip>();  // ✅ 应该发射你自己的
            Item.damage = 10;
            Item.useStyle = ItemUseStyleID.Swing;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            GameStage stage = GetCurrentGameStage();
            damage.Flat += WhipDamageCalculator.WhipDamage(
                stage: (int)stage, 
                DamageType: WhipDamageCalculator.BasicDamageTypeId,
                basicDamage: Item.damage
            );
        }

        // public override void UpdateInventory(Player player)
        // {
            
            
        // }

        // public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        // {
            
        // }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(ModContent.TileType<Tiles.ShadowAnvil>())
                .AddIngredient(ModContent.ItemType<RainbowCrystal>(), 1)
                .AddIngredient(ModContent.ItemType<ShadowCrystal>(), 6)
                .AddIngredient(ModContent.ItemType<LightCrystal>(), 6)
                .Register();
        }
        /// <summary>
        /// 当玩家手持这个物品时，持续调用
        /// </summary>
        // public override void HoldItem(Player player)
        // {
        //     // 1. 多人游戏核心：只让“自己”运行，服务器/别人不运行
        //     if (player.whoAmI != Main.myPlayer)
        //         return;

        //     // 2. 计时器：每帧 +1，用来控制召唤频率
        //     player.taxTimer++;

        //     // 3. 每 30 帧（0.5 秒）执行一次
        //     if (player.taxTimer >= 30)
        //     {
        //         player.taxTimer = 0; // 重置计时器

        //         // 4. 检查：仆从位没满，才允许召唤
        //         if (player.numMinions < player.maxMinions)
        //         {
        //             int realDamage = WhipDamageCalculator.WhipDamage(
        //                 stage: (int)GetCurrentGameStage(),
        //                 DamageType: WhipDamageCalculator.SummonDamageTypeId,
        //                 basicDamage: Item.damage);
        //             // 5. 创建召唤物（安全、标准、多人兼容）
        //             int proj = Projectile.NewProjectile(
        //                 player.GetSource_ItemUse(Item),  // 来源：物品使用（正确不报错）
        //                 player.Center,                   // 生成位置：玩家中心
        //                 Vector2.Zero,                    // 移动速度：静止生成
        //                 ModContent.ProjectileType<RainbowSummon>(), // 召唤物实体
        //                 realDamage,                  // 伤害
        //                 Item.knockBack,                 // 击退
        //                 player.whoAmI                   // 归属玩家
        //             );

        //             // 6. 必须设置：标记为仆从（吃鞭子、正确继承属性）
        //             Main.projectile[proj].minion = true;
        //             Main.projectile[proj].originalDamage = realDamage;
        //         }
        //     }
        // }



    }

    public  class WhipDamage : DamageClass  
    {

    }

    public class WhipDamageCalculator // 伤害计算类
    {
        public const int BasicDamageTypeId = 0;
        public const int AdvancedDamageTypeId = 1;
        public const int SummonDamageTypeId = 2;

        public static int WhipDamage(int stage,int DamageType,int basicDamage = 10)
        {       
            
            if (DamageType == BasicDamageTypeId)
            {
                if (stage < (int)GameStage.HardModePrePlantera) 
                {
                    return (int)(basicDamage * (1f + Math.Log(stage + 1)));
                    
                }
                if (stage < (int)GameStage.PostPlantera) 
                {
                    return (int)(basicDamage * ( stage * 0.8f * (stage-6) ));
                    
                }
                return (int)((basicDamage+1) * ( stage * 0.8f * (stage-6) ));
            }
            else if(DamageType == AdvancedDamageTypeId)
            {
                if (stage >= (int)GameStage.PostMoonLord)
                {
                    return basicDamage + (stage - 10)*5;
                }
            }
            else if (DamageType == SummonDamageTypeId)
            {
                if (stage < (int)GameStage.HardModePrePlantera) 
                {
                    return (int)(basicDamage * (1f + Math.Log(stage + 1)) * 0.5f);
                    
                }
                if (stage < (int)GameStage.PostPlantera) 
                {
                    return (int)(basicDamage * ( stage * 0.8f * (stage-6) ));
                    
                }
                return (int)((basicDamage+1) * ( stage * 0.8f * (stage-6) ));
            }
            return 0;
            
        }
    }
    //11111
    public class WhipAdvancedDamageHandler : GlobalNPC // 额外伤害buff处理 
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 1. 检查击中者是不是召唤物 (Minion)
            if (projectile.minion && npc.HasBuff(BuffID.RainbowWhipNPCDebuff))
            {
                modifiers.FlatBonusDamage += 
                WhipDamageCalculator.WhipDamage(
                    stage: (int)GetCurrentGameStage(), 
                    DamageType: WhipDamageCalculator.AdvancedDamageTypeId); 
            }
        }
    }

}