using Terraria;
using Terraria.ModLoader;

namespace Light_and_Shadow.common.Players // 改成你的MOD名
{
    public class SummonStagePlayer : ModPlayer
    {
        // 自定义：额外召唤栏
        public int extraMinions;

        // 每帧刷新阶段，决定加多少栏
        public override void PostUpdateBuffs()
        {
            // 按游戏阶段设置额外栏位
            if (!Main.hardMode)
                extraMinions = 0; // 肉前 +0
            else if (Main.hardMode && !NPC.downedPlantBoss)
                extraMinions = 1; // 肉后 +1
            else if (NPC.downedPlantBoss)
                extraMinions = 3; // 花后 +3
        }

        // ✅ 1.4.4 唯一正确：叠加召唤栏
        public override void PostUpdateEquips()
        {
            Player.maxMinions += extraMinions;
        }
    }
}