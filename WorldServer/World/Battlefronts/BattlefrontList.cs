using GameData;
using System.Collections.Generic;

namespace WorldServer.World.BattleFronts
{
    /// <summary>
    /// Static class holding the whole server RvR campain of BattleFronts.
    /// </summary>
    public class BattleFrontList
    {

        private BattleFrontList() { }

        // TODO set to private and create search methods (instead of lamda queries)

        /// <summary>RegionManagers indexed by "tier-1"</summary>
        public static List<IBattleFront>[] BattleFronts = { new List<IBattleFront>(), new List<IBattleFront>(), new List<IBattleFront>(), new List<IBattleFront>() };
        /// <summary>Active RegionManagers indexed by "tier-1"</summary>
        public static IBattleFront[] ActiveFronts = new IBattleFront[4];

        /// <summary>
        /// Registers the given BattleFront.
        /// </summary>
        /// <param name="front"></param>
        /// <remarks>public for legacy purpose</remarks>
        public static void AddBattleFront(IBattleFront front, int tier)
        {
            BattleFronts[tier - 1].Add(front);
        }

        /// <summary>
        /// Gets the active BattleFront depending on a player's level.
        /// </summary>
        /// <param name="level">level from 1 to 40 (may be out of bounds)</param>
        /// <returns>Active BattleFront (unique per tier)</returns>
        public static IBattleFront GetActiveFront(byte level)
        {
            if (level <= Constants.MaxTierLevel[0]) // 15
                return ActiveFronts[0];
            if (level <= Constants.MaxTierLevel[2]) // 30
                return ActiveFronts[2];
            return ActiveFronts[3]; // 40
        }
    }
}
