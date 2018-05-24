using GameData;
using System.Collections.Generic;

namespace WorldServer.World.Battlefronts
{
    /// <summary>
    /// Static class holding the whole server RvR campain of battlefronts.
    /// </summary>
    public class BattlefrontList
    {

        private BattlefrontList() { }

        // TODO set to private and create search methods (instead of lamda queries)

        /// <summary>RegionManagers indexed by "tier-1"</summary>
        public static List<IBattlefront>[] Battlefronts = { new List<IBattlefront>(), new List<IBattlefront>(), new List<IBattlefront>(), new List<IBattlefront>() };
        /// <summary>Active RegionManagers indexed by "tier-1"</summary>
        public static IBattlefront[] ActiveFronts = new IBattlefront[4];

        /// <summary>
        /// Registers the given battlefront.
        /// </summary>
        /// <param name="front"></param>
        /// <remarks>public for legacy purpose</remarks>
        public static void AddBattlefront(IBattlefront front, int tier)
        {
            Battlefronts[tier - 1].Add(front);
        }

        /// <summary>
        /// Gets the active battlefront depending on a player's level.
        /// </summary>
        /// <param name="level">level from 1 to 40 (may be out of bounds)</param>
        /// <returns>Active battlefront (unique per tier)</returns>
        public static IBattlefront GetActiveFront(byte level)
        {
            if (level <= Constants.MaxTierLevel[0]) // 15
                return ActiveFronts[0];
            if (level <= Constants.MaxTierLevel[2]) // 30
                return ActiveFronts[2];
            return ActiveFronts[3]; // 40
        }
    }
}
