using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Items.Placeable
{
    public class ShadowCrystal : ModItem
    {   
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Placeable.ShadowCrystalTile>());
        }
    }
}