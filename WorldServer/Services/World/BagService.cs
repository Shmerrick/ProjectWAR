
using Common;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service]
    public class BagService : ServiceBase
    {

        public static Dictionary<uint, Characters_bag_pools> _BagPools;

       /* [LoadingFunction(true)]
        public static void LoadBag_Pools()
        {
            Log.Debug("WorldMgr", "Loading Characters_bag_pools...");

            _BagPools = Database.MapAllObjects<uint, Characters_bag_pools>("CharacterId");

            Log.Success("LoadBag_Pools", "Loaded " + _BagPools.Count + " Characters_bag_pools");
        }*/

        public static Characters_bag_pools GetBagType(ushort Bag_Type)
        {
            List<Characters_bag_pools> BagPools = new List<Characters_bag_pools>();

            foreach (Characters_bag_pools bagPool in _BagPools.Values)
                if (bagPool.Bag_Type == Bag_Type)
                    return bagPool;
            return null;
        }
    }
}
