using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using System.Collections.Generic;

namespace Light_and_Shadow.Content.Tiles 
{ 

    public class MysteriousStoneStele : ModTile
    {
        public override void SetStaticDefaults()
        {

            // 1. 基础方块属性（可挖、不透明、不挡光、可 smart 交互）
            Main.tileSolidTop[Type] = false; // 顶部不是实心（可站）
            Main.tileBlockLight[Type] = false; // 不挡光
            Main.tileNoSunLight[Type] = false;
            Main.tileLighted[Type] = false;

            Main.tileMergeDirt[Type] = false;
            Main.tileSpelunker[Type] = false;

            // 整体化
            Main.tileFrameImportant[Type] = true;

            // 2. 核心：TileObjectData 定义 2×3 结构（最关键部分）
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1); // 从1格模板复制，再改
            TileObjectData.newTile.Width = 2;       // 横向占2格（X轴）
            TileObjectData.newTile.Height = 3;      // 纵向占3格（Y轴）
            TileObjectData.newTile.Origin = new Point16(1, 2); // 原点：第2列(X=1)、第3行(Y=2)（底部中间）
            TileObjectData.newTile.CoordinateWidth = 16;       // 单格贴图宽度（像素）
            TileObjectData.newTile.CoordinateHeights = [16, 16, 16];      // 单格贴图高度（像素）
            TileObjectData.newTile.CoordinatePadding = 2;     // 帧之间的间距（像素，默认2）
            TileObjectData.newTile.StyleHorizontal = true;      // 多格帧**横向排布**（贴图从左到右切）
            TileObjectData.newTile.StyleMultiplier = 1;        // 风格数量（1种就1）
            TileObjectData.newTile.StyleWrapLimit = 1;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0); // 底部必须贴实心方块
            // TileObjectData.newTile.AnchorInvalidTiles = new int[] { 19, 427, 435, 436, 437, 438, 439 }; // 不能放平台上
            
            TileObjectData.newTile.AnchorBottom = new AnchorData(
                AnchorType.SolidTile,        // 仅实心方块
                2,                           // 底部 2 格都要实心
                0
            );

            // 注册结构到游戏
            TileObjectData.addTile(Type);

            // 3. 其他：地图显示、掉落、交互
            AddMapEntry(new Color(200, 180, 160), CreateMapEntryName()); // 地图颜色+名称
            //RegisterItemDrop(ModContent.ItemType<Content.Items.RainbowCrystal>()); // (!!多格不可用)挖掉掉落对应物品
            //AdjTiles = new int[] { TileID.Statues }; // (会覆盖掉落!!)归类为雕像
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            // 直接返回要掉落的物品，支持多物品、概率掉落
            yield return new Item(ModContent.ItemType<Items.RainbowCrystal>());
        }

        // 可选：自定义放置条件（比如只能放实心地面、不能重叠）
        public override bool CanPlace(int i, int j)
        {
            for (int x = 0; x < 1; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Tile tile = Framing.GetTileSafely(i + x, j - y);
                    if (tile.HasTile || tile.IsHalfBlock || tile.TileType == TileID.Platforms) return false;
                }
            }
            // 底部两个格子必须是实心方块（不能是平台、半砖）
            for (int x = 0; x < 1; x++)
            {
                Tile below = Framing.GetTileSafely(i + x, j + 1);

                // 必须有方块 + 不是平台 + 不是半砖
                if (!below.HasTile
                    || below.TileType == TileID.Platforms
                    || below.IsHalfBlock)
                {
                    return false;
                }
            }
            return true;
        }

        // 放置后调用（官方方法）
        public override void PlaceInWorld(int i, int j, Item item)
        {
            SoundEngine.PlaySound(SoundID.Dig, new Vector2(i * 16, j * 16));
        }

    }

}