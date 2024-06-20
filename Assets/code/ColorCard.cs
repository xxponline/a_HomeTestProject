using UnityEngine;

namespace code
{
    public enum ColorCardRewardType
    {
        None,
        Player,
        AI
    }
    
    public class ColorCard
    {
        public ColorCard(ColorCardRewardType rewardType, Color ballColor)
        {
            RewardType = rewardType;
            BallColor = ballColor;
        }

        public ColorCardRewardType RewardType { get; private set; }
        public Color BallColor { get; private set; }
    }
}