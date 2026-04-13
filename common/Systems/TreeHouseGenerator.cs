using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Light_and_Shadow.common.Systems
{
    // 必须是public！
    public class TreeHouseGenerator : ModSystem
    {
        private List<BlockData> structureData;
        private int structureWidth;
        private int structureHeight;

        public override void Load()
        {
            Mod.Logger.Info("🔧 TreeHouseGenerator 开始加载...");
            try
            {
                var assembly = GetType().Assembly;
                // 打印所有嵌入资源，验证路径是否正确
                var allResources = assembly.GetManifestResourceNames();
                Mod.Logger.Info("📋 所有嵌入资源列表：\n" + string.Join("\n", allResources));

                var resourceName = "Light_and_Shadow.Structure.TreeHouse.qotstruct";
                Mod.Logger.Info($"🔍 尝试读取资源：{resourceName}");

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new FileNotFoundException($"❌ 资源流为空！请检查路径和嵌入设置");

                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        structureWidth = reader.ReadInt32();
                        structureHeight = reader.ReadInt32();
                        Mod.Logger.Info($"📏 读取到结构尺寸：宽={structureWidth}, 高={structureHeight}");

                        structureData = new List<BlockData>();
                        for (int y = 0; y < structureHeight; y++)
                        {
                            for (int x = 0; x < structureWidth; x++)
                            {
                                ushort tile = reader.ReadUInt16();
                                ushort wall = reader.ReadUInt16();
                                structureData.Add(new BlockData(tile, wall));
                            }
                        }
                    }
                }
                Mod.Logger.Info($"✅ 树屋结构加载成功！总格子={structureData.Count}");
            }
            catch (Exception e)
            {
                Mod.Logger.Error($"❌ 树屋结构加载失败：{e.Message}\n完整堆栈：{e.StackTrace}");
                structureData = null;
            }
        }

        // 强制插入生成任务，不依赖Surface
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            Mod.Logger.Info("🔧 正在插入树屋生成任务...");
            tasks.Add(new PassLegacy("TreeHouse", (progress, config) =>
            {
                progress.Message = "正在生成树屋...";
                Mod.Logger.Info("✅ 树屋生成任务已执行！");
                GenerateTreeHouse();
            }));
            totalWeight += 1;
        }

        private void GenerateTreeHouse()
        {
            if (structureData == null || structureData.Count == 0)
            {
                Mod.Logger.Error("❌ 结构数据为空，无法生成树屋！");
                return;
            }

            // 强制生成在世界中央，绝对能找到位置
            int centerX = Main.maxTilesX / 2;
            int surfaceY = FindSurface(centerX);
            int spawnY = surfaceY - structureHeight - 15; // 抬高15格，避免贴地
            int spawnX = centerX - structureWidth / 2;

            Mod.Logger.Info($"✅ 树屋生成位置: X={spawnX}, Y={spawnY}");
            PlaceStructure(spawnX, spawnY);
            Mod.Logger.Info("✅ 树屋生成完成！");
        }

        private void PlaceStructure(int startX, int startY)
        {
            for (int x = 0; x < structureWidth; x++)
            {
                for (int y = 0; y < structureHeight; y++)
                {
                    int wx = startX + x;
                    int wy = startY + y;
                    if (!WorldGen.InWorld(wx, wy)) continue;

                    // QoT 正确索引：y * width + x
                    int index = y * structureWidth + x;
                    if (index < 0 || index >= structureData.Count) continue;

                    BlockData data = structureData[index];

                    if (data.tile != 0)
                    {
                        WorldGen.KillTile(wx, wy, false, false, true);
                        WorldGen.PlaceTile(wx, wy, data.tile, true, true);
                    }
                    if (data.wall != 0)
                    {
                        WorldGen.PlaceWall(wx, wy, data.wall, true);
                    }
                }
            }
        }

        private int FindSurface(int x)
        {
            int y = 10;
            while (y < Main.maxTilesY - 20 && !Main.tile[x, y].HasTile)
                y++;
            return y;
        }

        private struct BlockData
        {
            public ushort tile;
            public ushort wall;
            public BlockData(ushort t, ushort w) { tile = t; wall = w; }
        }
    }
}