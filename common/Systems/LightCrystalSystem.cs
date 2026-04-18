using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using System.Collections.Generic;
using System.Threading;

namespace Light_and_Shadow.Content.Tiles.Placeable
{
    public class LightCrystalOreSystem : ModSystem
    {
        public static LocalizedText LightCrystalOrePassMessage { get; private set; }

        public override void SetStaticDefaults() {
            LightCrystalOrePassMessage = Mod.GetLocalization($"WorldGen.{nameof(LightCrystalOrePassMessage)}");
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
            int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));

            if (ShiniesIndex != -1) {
                tasks.Insert(ShiniesIndex + 1, new LightCrystalOrePass("Light Crystal Ores", 237.4298f));
            }
        }
    }

    public class LightCrystalOrePass : GenPass
    {
        public LightCrystalOrePass(string name, float loadWeight) : base(name, loadWeight) {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
            progress.Message = LightCrystalOreSystem.LightCrystalOrePassMessage.Value;

            for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 6E-05); k++) {
                int x = WorldGen.genRand.Next(0, Main.maxTilesX);
                int y = WorldGen.genRand.Next((int)GenVars.worldSurfaceLow, Main.maxTilesY);

                WorldGen.TileRunner(x, y, WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(2, 6), (ushort)ModContent.TileType<LightCrystalTile>());
            }
        }
    }
}