using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Tiles.Placeable
{
    public class ShadowCrystalTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 420;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 975;
            Main.tileMergeDirt[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            AddMapEntry(new Color(60, 40, 100), CreateMapEntryName());

            DustType = DustID.Shadowflame;
            HitSound = SoundID.Tink;
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            yield return new Item(ModContent.ItemType<Items.Placeable.ShadowCrystal>());
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // 暗紫黑光
            r = 0.2f;
            g = 0.0f;
            b = 0.5f;
        }
    }
}