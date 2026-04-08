using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Calamity_OverHaul_Patch.Content.Tiles
{
    public class MysteriousStoneStele : ModTile
    {
        public override void SetStaticDefaults()
        {
            // 1. 基础属性：定义它为实心背景块
            Main.tileSolid[Type] = true;          // ✅ 实心：可以站人，可以挡水
            Main.tileMergeDirt[Type] = true;      // ✅ 与泥土融合：边缘会自动变成泥土纹理
            Main.tileBlockLight[Type] = true;     // 阻挡光线
            Main.tileLighted[Type] = true;        // 自身发光

            // 2. 地图显示
            AddMapEntry(new Color(100, 50, 150), CreateMapEntryName());

            // 3. 核心：设置挖掘掉落
            // 告诉游戏：挖这个方块，掉落 RainbowCrystal 物品
            RegisterItemDrop(ModContent.ItemType<Items.RainbowCrystal>());
            
            // 4. 可选：设置挖掘所需的最低镐力
            // 例如：需要 55% 镐力（金镐级别）才能挖
            // MineResist = 2f; // 挖掘阻力
            // MinPick = 55;    // 最低镐力
        }

        // 5. 可选：发光效果
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer && Main.rand.NextBool(5))
            {
                // 产生紫色粒子
                Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustID.PurpleTorch);
            }
        }
    }
}