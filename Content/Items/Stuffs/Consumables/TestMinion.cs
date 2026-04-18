using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Light_and_Shadow.Content.Items.Stuffs.Consumables
{
    public class TestMinion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.consumable = false; // 不消耗物品
            Item.UseSound = SoundID.Item4;
            Item.rare = ItemRarityID.Pink;
        }

        public override bool CanUseItem(Player player)
        {
            // 防止连续快速点击
            return player.itemTime == 0;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            var modPlayer = player.GetModPlayer<MinionSlotPlayer>();

            if (player.altFunctionUse == 2)
            {
                // 右键：重置为0
                modPlayer.extraMinionSlotsFromThisItem = 0;
                Main.NewText($"仆从栏加成已重置！当前总仆从栏：{player.maxMinions} 格", Color.Orange);
            }
            else
            {
                // 左键：每次 +1
                modPlayer.extraMinionSlotsFromThisItem += 1;
                Main.NewText($"仆从栏 +1！当前总仆从栏：{player.maxMinions} 格", Color.LightGreen);
            }

            return true;
        }

        // 允许右键使用
        public override bool AltFunctionUse(Player player) => true;

        public override void AddRecipes()
        {
            // 你可以自己改配方
            CreateRecipe()
                .AddIngredient(ItemID.Diamond, 5)
                .AddIngredient(ItemID.SoulofLight, 3)
                .AddIngredient(ItemID.SoulofNight, 3)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    // ==============================================
    // 存储玩家数据：额外仆从栏
    // ==============================================
    public class MinionSlotPlayer : ModPlayer
    {
        public int extraMinionSlotsFromThisItem;

        public override void ResetEffects()
        {
            // 每帧重置，防止残留
            // 这里不清空，因为是永久加成
        }

        public override void UpdateEquips()
        {
            // 给玩家加仆从栏
            Player.maxMinions += extraMinionSlotsFromThisItem;
        }
    }
}