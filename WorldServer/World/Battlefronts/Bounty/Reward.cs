namespace WorldServer.World.Battlefronts.Bounty
{
        public class Reward
        {
            public int RenownBand { get; set; }
            public int InsigniaItemId { get; set; }
            public int BaseMoney { get; set; }
            public int InsigniaCount { get; set; }
            public int BaseRP { get; set; }
            public int BaseXP { get; set; }
            public int BaseInf { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return $"Reward RRBand : {RenownBand}. {InsigniaCount}x{InsigniaItemId}, Money:{BaseMoney}, RP:{BaseRP}, XP:{BaseXP}, Inf:{BaseInf} for {Description}";
            }

            public static int DetermineRenownBand(int playerRenownLevel)
            {
                // Add extra bounds. 
                if (playerRenownLevel == 0)
                    return 10;
                if (playerRenownLevel >= 100)
                    return 100;

                if (playerRenownLevel % 10 == 0) return playerRenownLevel;
                return (10 - playerRenownLevel % 10) + playerRenownLevel;
            }
        }
}
