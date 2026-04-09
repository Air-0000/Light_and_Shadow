using System;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

using static Calamity_OverHaul_Patch.Content.Items.GameStageHelper;
using Calamity_Overhaul_Patch.Content.Projectiles.Minions;

namespace Calamity_OverHaul_Patch.Content.Items
{
    public class ShadowRainbowWhip : ModItem
    {   
        
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.RainbowWhip);

            Item.damage = 10;
            Item.useStyle = ItemUseStyleID.Swing;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            Item.damage = ModContent.GetInstance<WhipDamage>().GetDamage(DamageTypeId: WhipAdvancedDamageHandler.BasicDamageTypeId);
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
                .AddIngredient(ItemID.Amber, 5)
                .AddIngredient(ItemID.Ruby, 5)
                .AddIngredient(ItemID.Topaz, 5)
                .AddIngredient(ItemID.Amethyst, 5)
                .AddIngredient(ItemID.Sapphire, 5)
                .AddIngredient(ItemID.Emerald, 5)
                .Register();
        }
        /// <summary>
        /// 当玩家手持这个物品时，持续调用
        /// </summary>
        public override void HoldItem(Player player)
        {
            // 1. 多人游戏核心：只让“自己”运行，服务器/别人不运行
            if (player.whoAmI != Main.myPlayer)
                return;

            // 2. 计时器：每帧 +1，用来控制召唤频率
            player.taxTimer++;

            // 3. 每 30 帧（0.5 秒）执行一次
            if (player.taxTimer >= 30)
            {
                player.taxTimer = 0; // 重置计时器

                // 4. 检查：仆从位没满，才允许召唤
                if (player.numMinions < player.maxMinions)
                {
                    // 5. 创建召唤物（安全、标准、多人兼容）
                    int proj = Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item),  // 来源：物品使用（正确不报错）
                        player.Center,                   // 生成位置：玩家中心
                        Vector2.Zero,                    // 移动速度：静止生成
                        ModContent.ProjectileType<RainbowSummon>(), // 召唤物实体
                        Item.damage,                    // 伤害
                        Item.knockBack,                 // 击退
                        player.whoAmI                   // 归属玩家
                    );

                    // 6. 必须设置：标记为仆从（吃鞭子、正确继承属性）
                    Main.projectile[proj].minion = true;
                    Main.projectile[proj].originalDamage = Item.damage;
                }
            }
        }



    }

        

    public  class WhipDamage : DamageClass
    {
        public WhipAdvancedDamageHandler handler = new WhipAdvancedDamageHandler();

        public  int GetDamage(int DamageTypeId = 0)
        {
            GameStage stage = GetCurrentGameStage();
            return handler.WhipDamage((int)stage, DamageTypeId);
        }
    }
    public class WhipAdvancedDamageHandler : GlobalNPC
    {
        // Damage Type Id 
        public const int BasicDamageTypeId = 0;
        public const int AdvancedDamageTypeId = 1;
        public  int WhipDamage(int stage,int DamageType,int basicDamage = 10)
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
            else if (stage >= (int)GameStage.PostMoonLord)
            {
                return basicDamage + (stage - 10)*5;
            }
            return 0;
            
        }
        
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 1. 检查击中者是不是召唤物 (Minion)
            if (projectile.minion)
            {
                // 2. 检查 NPC 身上有没有原版 Buff (比如万花筒)
                if (npc.HasBuff(BuffID.RainbowWhipNPCDebuff))
                {
                    // 3. 修改伤害 (直接修改 modifiers)
                    // 方案 A: 增加固定数值 (比如 +100)
                    modifiers.FlatBonusDamage += WhipDamage(stage: (int)GetCurrentGameStage(), DamageType: AdvancedDamageTypeId ); 

                }
            }
        }

    }

    

    

}