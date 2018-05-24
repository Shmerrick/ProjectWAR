using Common;
using FrameWork;
using System.Collections.Generic;
using System.Linq;

namespace WorldServer.Services.World
{
    [Service]
    public class DyeService : ServiceBase
    {
        public static List<Dye_Info> _Dyes = new List<Dye_Info>();

        [LoadingFunction(true)]
        public static void LoadDyes()
        {
            _Dyes = new List<Dye_Info>();

            Log.Debug("WorldMgr", "Loading Dye_Info...");

            IList<Dye_Info> Dyes = Database.SelectAllObjects<Dye_Info>();

            int Count = 0;
            foreach (Dye_Info Dye in Dyes)
            {
                if (!_Dyes.Contains(Dye))
                    _Dyes.Add(Dye);

                ++Count;
            }
            _Dyes.Sort((a, b) => a.Price.CompareTo(b.Price));

            Log.Success("WorldMgr", "Loaded " + Count + " Dyes");
        }

        /// <summary>
        /// Gets the existing dyes sorted by price.
        /// </summary>
        /// <returns>List of dyes sorted by price, never null</returns>
        public static List<Dye_Info> GetDyes()
        {
            return _Dyes;
        }
    }
}
