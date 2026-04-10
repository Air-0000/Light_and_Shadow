using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Light_and_Shadow.Content.Items
{
    public class MysteriousStoneStele : ModItem
    {
        public override void SetDefaults()
        {
            // 基础物品属性
            Item.width = 24;
            Item.height = 32;
            Item.maxStack = 99;
            Item.rare = ItemRarityID.Blue;

            // 放置核心：指向你的 Tile
            Item.DefaultToPlaceableTile(tileIDToPlace: ModContent.TileType<Content.Tiles.MysteriousStoneStele>());

            Item.placeStyle = 0;

            // 使用方式
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 10;
            Item.useAnimation = 10;
        }
    }
}
