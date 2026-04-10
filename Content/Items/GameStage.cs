using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Items
{
    public class GameStageHelper
    {
        public enum GameStage
        {
            nothing,        // 无
            PostSlimeKing,     // 史莱姆王后
            PostEyeOfCthulhu,  // 眼球后
            PostEvil,   // 双邪后
            PostQueenBee,      // 蜜蜂女王后
            PostSkeletron,     // 骷髅王后
            HardModePrePlantera, // 肉后 
            PostMechanicalBoss, // 机械后 55
            PostPlantera,   // 花后 160
            PostGolem,      // 石巨人后
            PostMoonLord    // 月总后（毕业）
        }

        public static GameStage GetCurrentGameStage()
        {
            if (NPC.downedMoonlord)
                return GameStage.PostMoonLord;
            
            if (NPC.downedGolemBoss)
                return GameStage.PostGolem;
            
            if (NPC.downedPlantBoss)
                return GameStage.PostPlantera;
            if (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3)
                return GameStage.PostMechanicalBoss;
            
            if (Main.hardMode)
                return GameStage.HardModePrePlantera;
            else
            {
                if (NPC.downedBoss3)
                    return GameStage.PostSkeletron;
                
                if (NPC.downedQueenBee)
                    return GameStage.PostQueenBee;
                
                if (NPC.downedBoss2)
                    return GameStage.PostEvil;
                
                if (NPC.downedBoss1)
                    return GameStage.PostEyeOfCthulhu;
                
                if (NPC.downedSlimeKing)
                    return GameStage.PostSlimeKing;
                
                return GameStage.nothing; // 默认返回无
            }
        }
    }
}