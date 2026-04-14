using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.ModLoader.IO;
using Light_and_Shadow.Content;
using Terraria.DataStructures;
using ReLogic.OS;

namespace Light_and_Shadow.Common.Systems
{
    public class TreeHouseGenerator : ModSystem
    {
        public static List<Point> StructureSpawnPoints = new List<Point>();

        private struct TileData
        {
            public int X;
            public int Y;
            public ushort TileType;
            public ushort WallType;
            public short TileFrameX;
            public short TileFrameY;
        }

        private List<TileData> tiles = new List<TileData>();
        private Dictionary<ushort, ushort> typeMapping = new Dictionary<ushort, ushort>();
        private int structureWidth = 0;
        private int structureHeight = 0;
        private int originX = 0;
        private int originY = 0;
        private bool isDataLoaded = false;

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int cleanupIndex = tasks.FindIndex(t => t.Name == "Final Cleanup");

            if (cleanupIndex != -1)
            {
                tasks.Insert(cleanupIndex, new PassLegacy("Place TreeHouse", (progress, config) =>
                {
                    progress.Message = "Placing Tree House";

                    if (!isDataLoaded) 
                    {
                        LoadStructureData();
                    }

                    if (tiles.Count == 0)
                    {
                        Mod.Logger.Warn("❌ 没有加载到任何方块数据！");
                        return;
                    }

                    Point spawnPoint = StructureSpawnPoints.Count > 0 
                        ? StructureSpawnPoints[0] 
                        : new Point(Main.maxTilesX / 2, 100);

                    PlaceStructure(spawnPoint.X, spawnPoint.Y);
                    StructureSpawnPoints.Clear();
                }));
            }
        }

        private void LoadStructureData()
        {
            if (isDataLoaded) return;

            try
            {
                string basePath = GetTerrariaPath();
                string structureFilePath = Path.Combine(basePath, "tModLoader", "ModSources", Mod.Name, "Structure", "TreeHouse1.qotstruct");
                string fullPath = Path.GetFullPath(structureFilePath);

                Mod.Logger.Info($"📂 尝试加载: {fullPath}");

                if (!File.Exists(fullPath))
                {
                    Mod.Logger.Error($"❌ 文件不存在: {fullPath}");
                    return;
                }

                TagCompound tag = TagIO.FromFile(fullPath);

                structureWidth = tag.Get<short>("Width");
                structureHeight = tag.Get<short>("Height");
                originX = tag.Get<short>("OriginX");
                originY = tag.Get<short>("OriginY");

                Mod.Logger.Info($"📐 元数据: Width={structureWidth}, Height={structureHeight}, Origin=({originX},{originY})");

                // 🔑 关键修复：正确读取列表
                typeMapping.Clear();
                if (tag.ContainsKey("EntriesName") && tag.ContainsKey("EntriesType"))
                {
                    var entriesNameList = tag.GetList<string>("EntriesName");
                    var entriesTypeList = tag.GetList<ushort>("EntriesType");

                    Mod.Logger.Info($"📋 加载方块映射: {entriesNameList.Count} 个Mod方块");

                    for (int i = 0; i < entriesNameList.Count && i < entriesTypeList.Count; i++)
                    {
                        string fullName = entriesNameList[i];
                        ushort typeIndex = entriesTypeList[i];

                        Mod.Logger.Debug($"  处理: {fullName} -> index {typeIndex}");

                        // 判断是方块还是墙壁
                        if (fullName.EndsWith("t"))  // 方块
                        {
                            string modTileName = fullName.Substring(0, fullName.Length - 1);
                            if (ModContent.TryFind<ModTile>(modTileName, out var modTile))
                            {
                                typeMapping[typeIndex] = modTile.Type;
                                Mod.Logger.Debug($"  ✅ 映射方块: {modTileName} (index {typeIndex}) -> {modTile.Type}");
                            }
                            else
                            {
                                Mod.Logger.Warn($"  ❌ 未找到Mod方块: {modTileName}");
                            }
                        }
                        else if (fullName.EndsWith("w"))  // 墙壁
                        {
                            string modWallName = fullName.Substring(0, fullName.Length - 1);
                            if (ModContent.TryFind<ModWall>(modWallName, out var modWall))
                            {
                                typeMapping[typeIndex] = modWall.Type;
                                Mod.Logger.Debug($"  ✅ 映射墙壁: {modWallName} (index {typeIndex}) -> {modWall.Type}");
                            }
                            else
                            {
                                Mod.Logger.Warn($"  ❌ 未找到Mod墙壁: {modWallName}");
                            }
                        }
                    }
                }
                else
                {
                    Mod.Logger.Warn($"⚠️ 找不到 EntriesName/EntriesType");
                }

                if (tag.TryGet("StructureData", out object structureObj) && structureObj is System.Collections.IList dataList)
                {
                    Mod.Logger.Info($"📊 总方块数: {dataList.Count}");

                    tiles.Clear();

                    int stride = structureHeight + 1;
                    int processedCount = 0;
                    int validCount = 0;
                    int modTileCount = 0;

                    for (int index = 0; index < dataList.Count; index++)
                    {
                        if (!(dataList[index] is TagCompound tileTag))
                            continue;

                        processedCount++;

                        TileDefinition def = TileDefinition.DeserializeData(tileTag);

                        int x = index % stride;
                        int y = index / stride;
                        int swappedX = y;
                        int swappedY = x;

                        int relX = swappedX - originX;
                        int relY = swappedY - originY;

                        // 转换方块ID
                        ushort finalTileType = 0;
                        ushort finalWallType = 0;

                        // 在 LoadStructureData 中，修改这部分：

                        if (def.TileIndex >= 0)
                        {
                            // 检查是否在原版方块范围内
                            if (def.TileIndex < TileID.Count)
                            {
                                // 原版方块，直接使用
                                finalTileType = (ushort)def.TileIndex;
                            }
                            else
                            {
                                // Mod方块，需要映射
                                Mod.Logger.Debug($"检测到Mod方块ID: {def.TileIndex} (TileID.Count={TileID.Count})");
                                
                                if (typeMapping.TryGetValue((ushort)def.TileIndex, out var mappedType))
                                {
                                    finalTileType = mappedType;
                                    modTileCount++;
                                    if (index < 50)
                                    {
                                        Mod.Logger.Debug($"Index {index}: Mod方块 {def.TileIndex} -> {mappedType}");
                                    }
                                }
                                else
                                {
                                    Mod.Logger.Warn($"⚠️ 方块映射失败: {def.TileIndex}");
                                }
                            }
                        }

                        if (def.WallIndex >= 0)
                        {
                            // 检查是否在原版墙壁范围内
                            if (def.WallIndex < WallID.Count)
                            {
                                // 原版墙壁，直接使用
                                finalWallType = (ushort)def.WallIndex;
                            }
                            else
                            {
                                // Mod墙壁，需要映射
                                if (typeMapping.TryGetValue((ushort)def.WallIndex, out var mappedType))
                                {
                                    finalWallType = mappedType;
                                }
                                else
                                {
                                    Mod.Logger.Warn($"⚠️ 墙壁映射失败: {def.WallIndex}");
                                }
                            }
                        }

                        tiles.Add(new TileData
                        {
                            X = relX,
                            Y = relY,
                            TileType = finalTileType,
                            WallType = finalWallType,
                            TileFrameX = def.TileFrameX,
                            TileFrameY = def.TileFrameY
                        });

                        if (def.TileIndex >= 0 || def.WallIndex >= 0)
                            validCount++;
                    }

                    isDataLoaded = true;
                    Mod.Logger.Info($"✅ 加载完成: {validCount}/{processedCount}");
                    Mod.Logger.Info($"   Mod方块数: {modTileCount}");

                    if (tiles.Count > 0)
                    {
                        int minX = int.MaxValue, maxX = int.MinValue;
                        int minY = int.MaxValue, maxY = int.MinValue;

                        foreach (var t in tiles)
                        {
                            minX = Math.Min(minX, t.X);
                            maxX = Math.Max(maxX, t.X);
                            minY = Math.Min(minY, t.Y);
                            maxY = Math.Max(maxY, t.Y);
                        }

                        Mod.Logger.Info($"📍 坐标范围: X({minX}~{maxX}) Y({minY}~{maxY})");
                    }
                }
            }
            catch (Exception e)
            {
                Mod.Logger.Error($"❌ 加载失败: {e.Message}");
                Mod.Logger.Error(e.StackTrace);
            }
        }

        private void PlaceStructure(int centerX, int groundY)
        {
            Mod.Logger.Info($"🏗️ 开始放置 - 锚点: ({centerX}, {groundY})");

            int placedCount = 0;
            int skippedEmpty = 0;          // 空方块（TileType=0 && WallType=0）
            int skippedOutOfBounds = 0;    // 超出范围
            int skippedInvalidTile = 0;    // 无效方块ID
            int skippedInvalidWall = 0;    // 无效��壁ID

            int minPlaceX = int.MaxValue, maxPlaceX = int.MinValue;
            int minPlaceY = int.MaxValue, maxPlaceY = int.MinValue;

            for (int i = 0; i < tiles.Count; i++)
            {
                TileData tile = tiles[i];

                int wx = centerX + tile.X;
                int wy = groundY + tile.Y;

                minPlaceX = Math.Min(minPlaceX, wx);
                maxPlaceX = Math.Max(maxPlaceX, wx);
                minPlaceY = Math.Min(minPlaceY, wy);
                maxPlaceY = Math.Max(maxPlaceY, wy);

                // 🔍 调试：详细统计跳过原因
                if (tile.TileType == 0 && tile.WallType == 0)
                {
                    skippedEmpty++;
                    continue;
                }

                if (wx < 0 || wx >= Main.maxTilesX || wy < 0 || wy >= Main.maxTilesY)
                {
                    skippedOutOfBounds++;
                    continue;
                }

                // 检查方块ID是否有效
                bool hasTile = tile.TileType > 0 && tile.TileType < TileLoader.TileCount;
                bool hasWall = tile.WallType > 0 && tile.WallType < WallLoader.WallCount;

                if (!hasTile && !hasWall)
                {
                    if (tile.TileType > 0)
                        skippedInvalidTile++;
                    if (tile.WallType > 0)
                        skippedInvalidWall++;
                    continue;
                }

                try
                {
                    if (hasTile)
                    {
                        WorldGen.PlaceTile(wx, wy, tile.TileType, mute: true, forced: true);
                        
                        Tile mainTile = Main.tile[wx, wy];
                        if (mainTile != null && mainTile.HasTile)
                        {
                            mainTile.TileFrameX = tile.TileFrameX;
                            mainTile.TileFrameY = tile.TileFrameY;
                        }
                    }

                    if (hasWall)
                    {
                        Main.tile[wx, wy].WallType = tile.WallType;
                    }

                    placedCount++;
                }
                catch (Exception e)
                {
                    Mod.Logger.Debug($"放置失败 at ({wx},{wy}): {e.Message}");
                }
            }

            Mod.Logger.Info($"📍 放置范围: X({minPlaceX}~{maxPlaceX}) Y({minPlaceY}~{maxPlaceY})");
            Mod.Logger.Info($"✅ 放置完成");
            Mod.Logger.Info($"   成功: {placedCount}");
            Mod.Logger.Info($"   空方块: {skippedEmpty}");
            Mod.Logger.Info($"   超出范围: {skippedOutOfBounds}");
            Mod.Logger.Info($"   无效方块ID: {skippedInvalidTile}");
            Mod.Logger.Info($"   无效墙壁ID: {skippedInvalidWall}");
        }
        private string GetTerrariaPath()
        {
            if (Platform.IsWindows)
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Terraria");
            else if (Platform.IsOSX)
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "Terraria");
            else
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "Terraria");
        }
    }
}