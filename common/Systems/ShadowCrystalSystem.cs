using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Light_and_Shadow.Content.Tiles.Placeable
{
    public class ShadowCrystalOreSystem : ModSystem
    {
        public static LocalizedText ShadowCrystalOrePassMessage { get; private set; }

        public override void SetStaticDefaults()
        {
            ShadowCrystalOrePassMessage = Mod.GetLocalization($"Systems.WorldGen.{nameof(ShadowCrystalOrePassMessage)}");
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));

            if (ShiniesIndex != -1)
            {
                tasks.Insert(ShiniesIndex + 1, new ShadowCrystalOrePass("Shadow Crystal Ores", 237.4298f));
            }
        }
    }

    public class ShadowCrystalOrePass : GenPass
    {
        public ShadowCrystalOrePass(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = ShadowCrystalOreSystem.ShadowCrystalOrePassMessage.Value;

            for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 6E-05); k++)
            {
                int x = WorldGen.genRand.Next(0, Main.maxTilesX);
                int y = WorldGen.genRand.Next((int)GenVars.worldSurfaceLow, Main.maxTilesY);

                WorldGen.TileRunner(x, y, WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(2, 6), (ushort)ModContent.TileType<ShadowCrystalTile>());
            }
        }
    }
}