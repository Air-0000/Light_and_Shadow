using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Items.Placeable
{
    public class LightCrystal : ModItem
    {   
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Placeable.LightCrystalTile>());
        }
    }
}