using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Light_and_Shadow.Content.Items.Stuffs;

namespace Light_and_Shadow.Content.Items.Blocks
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
                .AddIngredient(ModContent.ItemType<MysteriousStonePowder>(), 1)
                .AddRecipeGroup(RecipeGroupID.IronBar, 10)
                .Register();
        }
    }
}