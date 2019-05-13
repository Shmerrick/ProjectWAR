using System;
using System.Collections.Generic;

namespace Common
{
    public class HonorCalculation
    {
        private Dictionary<int, int> honorLevelReference = new Dictionary<int, int>();
        public const int HONOR_RANK_1 = 1000;
        public const int HONOR_RANK_2 = 2000;
        public const int HONOR_RANK_3 = 4000;
        public const int HONOR_RANK_4 = 8000;



        public HonorCalculation()
        {
            // This list MUST be descending in sequence for this to work ;)
            honorLevelReference.Add(4, HONOR_RANK_4);
            honorLevelReference.Add(3, HONOR_RANK_3);
            honorLevelReference.Add(2, HONOR_RANK_2);
            honorLevelReference.Add(1, HONOR_RANK_1);

        }

        public int GetHonorLevel(int honorPoints)
        {

            foreach (var honorLevel in honorLevelReference)
            {
                if (honorLevel.Value < honorPoints)
                    return honorLevel.Key;
            }
            return 0;
        }

        public static int RoundOffNearestTen(int i)
        {
            return ((int)Math.Round(i / 10.0)) * 10;
        }


        public static int CalculateRank0Percent(ushort infoHonorPoints)
        {
            return RoundOffNearestTen((100 * infoHonorPoints) / HonorCalculation.HONOR_RANK_1);
        }

        public static int CalculateRank1Percent(ushort infoHonorPoints)
        {
            return RoundOffNearestTen((100 * (infoHonorPoints - HonorCalculation.HONOR_RANK_2 + HonorCalculation.HONOR_RANK_1)) / HonorCalculation.HONOR_RANK_1);
        }

        public static int CalculateRank2Percent(ushort infoHonorPoints)
        {
            return RoundOffNearestTen((100 * (infoHonorPoints - HonorCalculation.HONOR_RANK_3 + HonorCalculation.HONOR_RANK_2)) / HonorCalculation.HONOR_RANK_2);
        }
        public static int CalculateRank3Percent(ushort infoHonorPoints)
        {
            return RoundOffNearestTen((100 * (infoHonorPoints - HonorCalculation.HONOR_RANK_4 + HonorCalculation.HONOR_RANK_3)) / HonorCalculation.HONOR_RANK_3);
        }
    }
}
