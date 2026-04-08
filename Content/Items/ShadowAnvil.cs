using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamity_OverHaul_Patch.Content.Items
{
    public class ShadowAnvil : ModItem
    {
        public override void SetDefaults()
        {

            Item.DefaultToPlaceableTile(ModContent.TileType<Content.Tiles.ShadowAnvil>());

        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.CraftingObjects;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.WorkBenches)
                .AddIngredient(ModContent.ItemType<RainbowCrystal>(), 1)
                .AddIngredient(ItemID.StoneBlock, 10)
                .Register();
        }
    }
}