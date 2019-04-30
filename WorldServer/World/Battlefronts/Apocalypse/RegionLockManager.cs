using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class RegionLockManager
    {
        public RegionMgr Region { get; set; }

        protected readonly EventInterface _EvtInterface = new EventInterface();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool RegionLocked { get; set; }

        public RegionLockManager(RegionMgr region)
        {
            Region = region;
            RegionLocked = false;
        }

        public bool IsLocked()
        {
            return RegionLocked;
        }

        public void Update(long tick)
        {
            _EvtInterface.Update(tick);
        }


        public void Start()
        {
            Logger.Info($"Starting Region Lock for Region : {Region.RegionId}");
            _EvtInterface.AddEvent(EndRegionLock, 90000, 1);
            RegionLocked = true;
            var playersToAnnounceTo = Player._Players.Where(x => !x.IsDisposed
                                                                 && x.IsInWorld()
                                                                 && x.ScnInterface.Scenario == null
                                                                 && x.Region.RegionId == this.Region.RegionId);

            foreach (var player in playersToAnnounceTo)
            {
                player.SendClientMessage($"{Region.RegionName} is in ruins, but it shall rise again!");
            }
        }

        private void EndRegionLock()
        {
            Logger.Info($"Ending Region Lock for Region : {this.Region.RegionId}");
            _EvtInterface.RemoveEvent(EndRegionLock);
            RegionLocked = false;
            var playersToAnnounceTo = Player._Players.Where(x => !x.IsDisposed
                                                                 && x.IsInWorld()
                                                                 && x.ScnInterface.Scenario == null
                                                                 && x.Region.RegionId == this.Region.RegionId);

            //foreach (var player in playersToAnnounceTo)
            //{
            //    player.SendClientMessage($"{Region.RegionName} has recovered and is available for battle!");
            //}
        }
    }
}
