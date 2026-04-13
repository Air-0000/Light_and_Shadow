using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Items.Stuffs
{
    public class MysteriousStonePowder : ModItem
    {   
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            ItemID.Sets.SortingPriorityMaterials[Type] = 70; // 设置物品在物品栏中的排序为70 (宝石之后)
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.Gray;
            Item.material = true;
        }
    }
}