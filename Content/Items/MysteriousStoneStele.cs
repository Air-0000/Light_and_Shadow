using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamity_OverHaul_Patch.Content.Items
{
    public class MysteriousStoneStele : ModItem
    {   
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Diamond);
        }
    }
}