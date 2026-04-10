
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ObjectData;
using Terraria.Enums;

namespace Light_and_Shadow.Content.Tiles
{
    public class ShadowAnvil : ModTile
    {   
        public override bool Slope(int i, int j) => false;
        public override void SetStaticDefaults()
        {
            // 基础属性
            Main.tileTable[Type] = true;
            Main.tileSolidTop[Type] = true;
            Main.tileNoAttach[Type] = true; // ✅ 关键：不让方块附着在上面
            Main.tileLavaDeath[Type] = true;
            Main.tileHammer[Type] = false; 
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;
            TileID.Sets.IgnoredByNpcStepUp[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            // 本地化名称
            AddMapEntry(new Color(100, 50, 150), CreateMapEntryName());

            // 2x1 结构
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 0);
            TileObjectData.addTile(Type);

            // 合成站
            AdjTiles = new int[] { TileID.Anvils };


        }
        //11111
        // 发光效果
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer && Main.rand.NextBool(10))
            {
                Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustID.Shadowflame);
            }
        }
        public override void NumDust(int x, int y, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        // 挖的时候：
        // 挖坏 = 1个粒子
        // 成功挖掉 = 3个粒子

    }

}