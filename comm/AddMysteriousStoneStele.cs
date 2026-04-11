using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.GameContent.Biomes;

namespace Light_and_Shadow.comm
{
    public class LivingTreeSteleGenerator : ModSystem
    {
        public static List<int> LivingTreeXPositions = new List<int>();

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int vanillaTreePassIndex = tasks.FindIndex(t => t.Name == "Living Trees");

            if (vanillaTreePassIndex != -1)
            {
                tasks.Insert(vanillaTreePassIndex + 1, new PassLegacy("Living Tree Stele Shrine", (progress, config) =>
                {
                    progress.Message = "Generating Living Trees";
                    GenerateLivingTreesWithStele();
                }));
            }

            int finalCleanupIndex = tasks.FindIndex(t => t.Name == "Final Cleanup");
            if (finalCleanupIndex != -1)
            {
                tasks.Insert(finalCleanupIndex, new PassLegacy("Generate Stele Shrines", (progress, config) =>
                {
                    progress.Message = "Generating Stele Shrines";

                    foreach (int x in LivingTreeXPositions)
                    {
                        SpawnSteleUnderTree(x);
                    }
                    SpawnSteleUnderTree(Main.maxTilesX / 2 - 120); // 中心剑冢
                    LivingTreeXPositions.Clear();
                }));
            }
        }

        private void GenerateLivingTreesWithStele()
        {
            var rand = WorldGen.genRand;
            int centerAvoidDistance = 200;
            double worldSizeScale = (double)Main.maxTilesX / 4200.0;

            int treeCount = rand.Next(0, (int)(2.0 * worldSizeScale) + 1);
            if (treeCount == 0)
                treeCount++;

            if (Main.drunkWorld)
                treeCount += (int)(2.0 * worldSizeScale);
            else if (Main.tenthAnniversaryWorld)
                treeCount += (int)(3.0 * worldSizeScale);
            else if (Main.remixWorld)
                treeCount += (int)(2.0 * worldSizeScale);

            for (int i = 0; i < treeCount; i++)
            {
                bool success = false;
                int attempts = 0;

                while (!success && attempts < Main.maxTilesX / 2)
                {
                    attempts++;
                    int x = rand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);

                    if (WorldGen.tenthAnniversaryWorldGen && !WorldGen.remixWorldGen)
                        x = rand.Next((int)(Main.maxTilesX * 0.15), (int)(Main.maxTilesX * 0.85));

                    if (Math.Abs(x - Main.maxTilesX / 2) < centerAvoidDistance)
                        continue;

                    int y = FindSurfaceHeight(x);
                    if (y < 150)
                        continue;

                    if (!IsValidLivingTreeTerrain(x, y))
                        continue;

                    success = WorldGen.GrowLivingTree(x, y);

                    if (success)
                    {
                        LivingTreeXPositions.Add(x);
                    }
                }
            }

            Main.tileSolid[TileID.LeafBlock] = false;
        }

        private int FindSurfaceHeight(int x)
        {
            int y = 0;
            while (y < Main.worldSurface && !Main.tile[x, y].HasTile)
                y++;

            if (Main.tile[x, y].TileType == TileID.Dirt)
                y--;

            return y;
        }

        private bool IsValidLivingTreeTerrain(int centerX, int centerY)
        {
            for (int x = centerX - 50; x < centerX + 50; x++)
            {
                for (int y = centerY - 50; y < centerY + 50; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (!tile.HasTile) continue;

                    switch (tile.TileType)
                    {
                        case TileID.SnowBlock:
                        case TileID.IceBlock:
                        case TileID.Sand:
                        case TileID.CorruptSandstone:
                        case TileID.Crimsand:
                        case TileID.HallowSandstone:
                        case TileID.JungleGrass:
                        case TileID.Mud:
                            return false;
                    }
                }
            }
            return true;
        }

        private void SpawnSteleUnderTree(int treeX)
        {
            int centerX = treeX;
            int startY = (int)GenVars.worldSurface + WorldGen.genRand.Next(50, 100);
            int centerY = startY;

            // 1. 尝试生成官方剑冢
            EnchantedSwordBiome swordShrine = GenVars.configuration.CreateBiome<EnchantedSwordBiome>();
            Point origin = new Point(centerX, startY);

            bool placed = false;
            int retry = 0;
            while (!placed && retry < 10)
            {
                placed = swordShrine.Place(origin, GenVars.structures);
                retry++;
            }

            // 2. 寻找平台
            int platformY = 0;
            centerY = startY;
            for (int i = 0; i < 60; i++)
            {
                centerY++;
                Tile below = Framing.GetTileSafely(centerX, centerY + 1);
                if (below.HasTile && Main.tileSolid[below.TileType])
                {
                    platformY = centerY;
                    break;
                }
            }

            // 如果没找到平台 → 直接指定一个安全位置
            if (platformY == 0)
                platformY = startY + 25;

            for (int x = -2; x <= 2; x++)
            {
                for (int y = 0; y <= 2; y++)
                {
                    int px = centerX + x;
                    int py = platformY + y;
                    if (WorldGen.InWorld(px, py))
                    {
                        WorldGen.KillTile(px, py, false, false, true);
                        WorldGen.KillWall(px, py, false);
                        WorldGen.PlaceTile(px, py, TileID.Dirt, true, true);
                    }
                }
            }

            for (int x = -1; x <= 0; x++)
            {
                for (int y = -2; y <= 0; y++)
                {
                    int cx = centerX + x;
                    int cy = platformY + y;
                    if (WorldGen.InWorld(cx, cy))
                    {
                        WorldGen.KillTile(cx, cy, false, false, true);
                        WorldGen.KillWall(cx, cy, false);
                    }
                }
            }

            for (int y = platformY - 2; y > (int)GenVars.worldSurface - 15; y--)
            {
                WorldGen.KillTile(centerX, y, false, false, true);
                WorldGen.KillWall(centerX, y, false);

                if (WorldGen.genRand.NextBool())
                {
                    WorldGen.KillTile(centerX - 1, y, false, false, true);
                    WorldGen.KillWall(centerX - 1, y, false);
                }
                if (WorldGen.genRand.NextBool())
                {
                    WorldGen.KillTile(centerX + 1, y, false, false, true);
                    WorldGen.KillWall(centerX + 1, y, false);
                }
            }
            WorldGen.PlaceObject(centerX, platformY, ModContent.TileType<Content.Tiles.MysteriousStoneStele>(), true);
        }


    }
}