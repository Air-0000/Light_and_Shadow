using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using Terraria.Enums;
using ReLogic.OS;
using Light_and_Shadow.Common;
using Microsoft.Xna.Framework;

namespace Light_and_Shadow.Common.Systems
{
    public class TreeHouseGenerator : ModSystem
    {
        public static List<Point> StructureSpawnPoints = new List<Point>();

        private QoLStructure structure;
        private bool isDataLoaded = false;
        private Dictionary<int, int> dirtCountPerY = new Dictionary<int, int>();
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

                    if (structure?.StructureDatas == null || structure.StructureDatas.Count == 0)
                    {
                        Mod.Logger.Warn("❌ 没有加载到任何方块数据！");
                        return;
                    }

                    Point spawnPoint = StructureSpawnPoints.Count > 0
                        ? StructureSpawnPoints[0]
                        : new Point(Main.maxTilesX / 2, 100);

                    // 【自动适配地表】根据X动态获取地面Y
                    int surfaceY = GetSurfaceGroundY(spawnPoint.X) - 82;


                    int groundY = GetSurfaceGroundY(spawnPoint.X);
                    if (groundY != (int)Main.worldSurface + 200)
                    {
                        Mod.Logger.Info($"✅ 找到纯土块地表，树屋生成于 Y:{groundY}");
                        PlaceStructure(spawnPoint.X, surfaceY);
                    }
                    else
                    {
                        Mod.Logger.Info($"❌ 无纯土块地面 或 进入洞穴层，跳过树屋生成");
                    }


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
                string structureFilePath = Path.Combine(basePath, "tModLoader", "ModSources", Mod.Name, "Structure", "TreeHouse.qotstruct");
                string fullPath = Path.GetFullPath(structureFilePath);

                Mod.Logger.Info($"📂 加载路径: {fullPath}");

                if (!File.Exists(fullPath))
                {
                    Mod.Logger.Error($"❌ 文件不存在: {fullPath}");
                    return;
                }

                structure = new QoLStructure(fullPath);

                Mod.Logger.Info($"📐 元数据: Width={structure.Width}, Height={structure.Height}, Origin=({structure.OriginX},{structure.OriginY})");
                Mod.Logger.Info($"📊 总方块数: {structure.StructureDatas.Count}");
                Mod.Logger.Info($"📋 Mod方块/墙壁条目: {structure.entries.Count}, typeMaping: {structure.typeMaping.Count}");
                foreach (var entry in structure.entries)
                {
                    Mod.Logger.Info($"   条目: {entry.Key} -> {entry.Value}");
                }

                isDataLoaded = true;
            }
            catch (Exception e)
            {
                Mod.Logger.Error($"❌ 加载失败: {e.Message}");
                Mod.Logger.Error(e.StackTrace);
            }
        }



        /// <summary>
        /// 获取指定X坐标的地表地面Y（向下找第一个实心方块顶部）
        /// </summary>
        private int GetSurfaceGroundY(int x)
        {
            int caveLayerTop = (int)Main.rockLayer;
            int startY = (int)(Main.worldSurface - 200);//地表上方200格开始
            for (int y = startY; y < caveLayerTop-82; y++)
            {
                Tile currentTile = Main.tile[x, y];

                if ((currentTile.TileType == TileID.Dirt || currentTile.TileType == TileID.Grass)) // 只接受纯土块地表
                {
                     return y;
                }
            }
            // 兜底：世界地表高度
            return (int)Main.worldSurface + 200;
        }

        private void PlaceStructure(int centerX, int groundY)
        {
            Mod.Logger.Info($"🏗️ 开始放置 - 锚点: ({centerX}, {groundY})");

            int placedCount = 0;
            int skippedEmpty = 0;
            int skippedOutOfBounds = 0;
            int skippedMultiTile = 0;
            int placedMultiTile = 0;
            int skippedGetTileData = 0;
            int skippedStyle = 0;
            int placeFailed = 0;

            int minPlaceX = int.MaxValue, maxPlaceX = int.MinValue;
            int minPlaceY = int.MaxValue, maxPlaceY = int.MinValue;

            int stride = structure.Height + 1;

            // 记录需要在第二遍处理的多格物块索引
            List<int> multiTileIndices = new List<int>();

            // 第一遍：放置单格物块
            for (int index = 0; index < structure.StructureDatas.Count; index++)
            {
                TileDefinition def = structure.StructureDatas[index];

                int x = index % stride;
                int y = index / stride;
                int swappedX = y;
                int swappedY = x;

                int relX = swappedX - structure.OriginX;
                int relY = swappedY - structure.OriginY;

                int wx = centerX + relX;
                int wy = groundY + relY;

                minPlaceX = Math.Min(minPlaceX, wx);
                maxPlaceX = Math.Max(maxPlaceX, wx);
                minPlaceY = Math.Min(minPlaceY, wy);
                maxPlaceY = Math.Max(maxPlaceY, wy);

                if (def.TileIndex == -1 && def.WallIndex == -1)
                {
                    skippedEmpty++;
                    continue;
                }

                if (wx < 0 || wx >= Main.maxTilesX || wy < 0 || wy >= Main.maxTilesY)
                {
                    skippedOutOfBounds++;
                    continue;
                }

                int tileType = structure.ParseTileType(def);
                int wallType = structure.ParseWallType(def);

                bool hasTile = tileType >= 0 && tileType < TileLoader.TileCount;
                bool hasWall = wallType >= 0 && wallType < WallLoader.WallCount;

                // 调试：统计每个y坐标的土块数量
                if (def.TileIndex == 0 || def.TileIndex == 2 || def.TileIndex == 3)
                {
                    if (!dirtCountPerY.ContainsKey(wy)) dirtCountPerY[wy] = 0;
                    dirtCountPerY[wy]++;
                }

                if (!hasTile && !hasWall)
                {
                    continue;
                }

                if (hasTile)
                {
                    // 检查是否为多格方块
                    TileObjectData tileObjectData = null;
                    for (int tryStyle = 0; tryStyle < 16; tryStyle++)
                    {
                        try
                        {
                            tileObjectData = TileObjectData.GetTileData(tileType, tryStyle, 0);
                            if (tileObjectData != null)
                                break;
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    if (tileObjectData != null && (tileObjectData.CoordinateFullWidth > 18 || tileObjectData.CoordinateFullHeight > 18))
                    {
                        skippedMultiTile++;
                        multiTileIndices.Add(index); // 记录需要第二遍处理的索引
                        continue;
                    }

                    // 计算平台样式
                    int style = 0;
                    if (tileObjectData != null && tileType == TileID.Platforms)
                    {
                        // 从frame计算平台样式
                        int num = def.TileFrameX / tileObjectData.CoordinateFullWidth;
                        int num2 = def.TileFrameY / tileObjectData.CoordinateFullHeight;
                        int num3 = tileObjectData.StyleWrapLimit;
                        if (num3 == 0) num3 = 1;
                        int num4 = (!tileObjectData.StyleHorizontal) ? (num / tileObjectData.StyleLineSkip * num3 + num2) : (num2 / tileObjectData.StyleLineSkip * num3 + num);
                        style = num4 / tileObjectData.StyleMultiplier;
                    }

                    // 使用KillTile清除已有方块
                    WorldGen.KillTile(wx, wy, false, false, true);

                    bool placed = WorldGen.PlaceTile(wx, wy, tileType, mute: true, forced: true, style: style);
                    if (!placed)
                    {
                        placeFailed++;
                    }

                    // 应用斜坡/半砖属性
                    ApplyBlockStyle(wx, wy, def);
                }

                if (hasWall)
                {
                    WorldGen.PlaceWall(wx, wy, wallType, true);
                }

                placedCount++;
            }

            // 第二遍：只处理第一遍记录的多格物块
            foreach (int index in multiTileIndices)
            {
                try
                {
                    TileDefinition def = structure.StructureDatas[index];

                    int x = index % stride;
                    int y = index / stride;
                    int swappedX = y;
                    int swappedY = x;

                int relX = swappedX - structure.OriginX;
                int relY = swappedY - structure.OriginY;

                int wx = centerX + relX;
                int wy = groundY + relY;

                int tileType = structure.ParseTileType(def);

                if (tileType < 0 || tileType >= TileLoader.TileCount)
                    continue;

                if (wx < 0 || wx >= Main.maxTilesX || wy < 0 || wy >= Main.maxTilesY)
                    continue;

                TileObjectData tileObjectData = null;
                bool foundData = false;

                // 尝试多个可能的style值
                for (int tryStyle = 0; tryStyle < 16; tryStyle++)
                {
                    try
                    {
                        var data = TileObjectData.GetTileData(tileType, tryStyle, 0);
                        if (data != null)
                        {
                            tileObjectData = data;
                            foundData = true;
                            break;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (!foundData || tileObjectData == null)
                {
                    skippedGetTileData++;
                    continue;
                }

                int subX = (def.TileFrameX / tileObjectData.CoordinateFullWidth) * tileObjectData.CoordinateFullWidth;
                int subY = (def.TileFrameY / tileObjectData.CoordinateFullHeight) * tileObjectData.CoordinateFullHeight;

                Tile tempTile = new Tile();
                tempTile.TileType = (ushort)tileType;
                tempTile.TileFrameX = (short)subX;
                tempTile.TileFrameY = (short)subY;

                int style = TileFrameToPlaceStyle(tempTile);
                if (style < 0)
                {
                    skippedStyle++;
                    continue;
                }

                subX = def.TileFrameX % tileObjectData.CoordinateFullWidth;
                subY = def.TileFrameY % tileObjectData.CoordinateFullHeight;

                int direction = tileObjectData.Direction switch
                {
                    TileObjectDirection.PlaceLeft => -1,
                    TileObjectDirection.PlaceRight => 1,
                    _ => 0
                };

                if (TileID.Sets.BasicChest[tileType])
                {
                    PlaceChestNoSync(wx, wy, (ushort)tileType, false, style);
                    placedMultiTile++;
                    continue;
                }

                if (TileID.Sets.BasicDresser[tileType])
                {
                    if (TileHelper.Place3x2NoSyncDresser(wx, wy, (ushort)tileType, style))
                        placedMultiTile++;
                    continue;
                }

                if (TryPlaceMultiTile(wx, wy, tileType, style, direction))
                {
                    placedMultiTile++;
                }
                }
                catch (Exception ex)
                {
                    Mod.Logger.Warn($"❌ 处理多格物块时出错: {ex.Message}");
                }
            }

            Mod.Logger.Info($"📍 放置范围: X({minPlaceX}~{maxPlaceX}) Y({minPlaceY}~{maxPlaceY})");
            Mod.Logger.Info($"✅ 放置完成");
            Mod.Logger.Info($"   成功: {placedCount}");
            Mod.Logger.Info($"   多格物块: {placedMultiTile}");
            Mod.Logger.Info($"   空方块: {skippedEmpty}");
            Mod.Logger.Info($"   超出范围: {skippedOutOfBounds}");
            Mod.Logger.Info($"   跳过(多格待处理): {skippedMultiTile}");
            Mod.Logger.Info($"   GetTileData失败: {skippedGetTileData}");
            Mod.Logger.Info($"   Style失败: {skippedStyle}");
            Mod.Logger.Info($"   放置失败: {placeFailed}");
        }

        private static TileObjectData GetTileData(int tileType, int frameX, int frameY)
        {
            if (tileType < 0 || tileType >= TileLoader.TileCount)
                return null;

            try
            {
                var data = TileObjectData.GetTileData(tileType, 0);
                if (data != null)
                    return data;

                return TileObjectData.GetTileData(tileType, -1, 0);
            }
            catch
            {
                return null;
            }
        }

        // 应用斜坡、半砖等属性
        private static void ApplyBlockStyle(int x, int y, TileDefinition def)
        {
            if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                return;

            Tile tile = Main.tile[x, y];
            if (tile == null || !tile.HasTile)
                return;

            var blockType = def.BlockType;

            if (blockType == BlockType.HalfBlock)
            {
                tile.IsHalfBlock = true;
            }
            else if (blockType != BlockType.Solid)
            {
                // 斜坡
                switch (blockType)
                {
                    case BlockType.SlopeUpLeft:
                        tile.Slope = SlopeType.SlopeUpLeft;
                        break;
                    case BlockType.SlopeUpRight:
                        tile.Slope = SlopeType.SlopeUpRight;
                        break;
                    case BlockType.SlopeDownLeft:
                        tile.Slope = SlopeType.SlopeDownLeft;
                        break;
                    case BlockType.SlopeDownRight:
                        tile.Slope = SlopeType.SlopeDownRight;
                        break;
                }
            }

            // 处理促动器
            if (def.HasActuator)
            {
                tile.IsActuated = true;
            }
        }

        private static int TileFrameToPlaceStyle(Tile tile)
        {
            if (tile == null)
                return 0;

            int type = tile.TileType;
            if (type < 0 || type >= TileLoader.TileCount)
                return 0;

            TileObjectData data;
            try
            {
                data = TileObjectData.GetTileData(type, 0);
            }
            catch
            {
                // 如果获取失败，返回0作为默认值
                return 0;
            }

            if (data == null)
                return 0;

            int num = tile.TileFrameX / data.CoordinateFullWidth;
            int num2 = tile.TileFrameY / data.CoordinateFullHeight;
            int num3 = data.StyleWrapLimit;
            if (num3 == 0)
                num3 = 1;

            int styleLineSkip = data.StyleLineSkip;
            int num4 = (!data.StyleHorizontal) ? (num / styleLineSkip * num3 + num2) : (num2 / styleLineSkip * num3 + num);
            int num5 = num4 / data.StyleMultiplier;

            return num5;
        }

        private static bool TryPlaceMultiTile(int x, int y, int tileType, int style, int direction)
        {
            if (WorldGen.PlaceTile(x, y, tileType, mute: true, forced: true, style: style))
            {
                return true;
            }

            if (direction != 0)
            {
                return WorldGen.PlaceTile(x, y, tileType, mute: true, forced: true, style: style);
            }

            return false;
        }

        private static int PlaceChestNoSync(int i, int j, ushort tileType, bool nearSurface, int style)
        {
            return WorldGen.PlaceChest(i, j, tileType, nearSurface, style);
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
