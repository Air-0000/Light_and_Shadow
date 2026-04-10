using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Calamity_OverHaul_Patch.Content.Tiles
{
    public class RainbowCrystalTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            // 基础属性
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;       // 不是实心方块
            Main.tileMergeDirt[Type] = true;    // 能和泥土融合
            Main.tileBlockLight[Type] = false;  // 不挡光
            Main.tileLighted[Type] = true;      // 会发光
            Main.tileShine2[Type] = true;       // 宝石闪光效果

            // 大小：1x1 单个宝石方块
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(200, 180, 160), CreateMapEntryName()); 
            // 挖掘硬度
            //MinPick = 40; // 金镐以上能挖
        }

        // 发光（和原版宝石一样）
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // 彩虹光，你可以自己改颜色
            r = 0.8f;
            g = 0.4f;
            b = 0.9f;
        }

        // 掉落物品
        // 正确写法，重写官方虚方法
        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            // 直接返回要掉落的物品，支持多物品、概率掉落
            yield return new Item(ModContent.ItemType<Items.RainbowCrystal>());
        }
    }
}