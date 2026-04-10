using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Items
{
    public class RainbowCrystal : ModItem
    {   
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Diamond);
            Item.DefaultToPlaceableTile(ModContent.TileType<Content.Tiles.RainbowCrystalTile>());
        }
    }
}