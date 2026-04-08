
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ObjectData;
using Terraria.Enums;

namespace Calamity_OverHaul_Patch.Content.Tiles
{
    public class ShadowAnvil : ModTile
    {
        public override void SetStaticDefaults()
        {

            // 基础属性
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false; // 如果不发光就设为 false
            TileID.Sets.IgnoredByNpcStepUp[Type] = true; // NPC不会试图跳上这个家具

            // 本地化名称
            AddMapEntry(new Color(100, 50, 150), CreateMapEntryName());

            // 定义物体结构 (1x1 大小)
            Main.tileFrameImportant[Type] = true;//方块为多格
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 0);//平台或固体上放置,2格
            TileObjectData.addTile(Type);

            // 注册为制作站 
            AdjTiles = new int[] { TileID.Anvils };
            
            Main.tileFrameImportant[Type] = true;

        }

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