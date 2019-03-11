using System.Collections.Generic;
using System.Linq;
using FrameWork;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Map
{
    public class HotSpot : Object
    {
        public enum HotSpotState
        {
            NONE,
            HOTSPOT_24,
            HOTSPOT_16,
            HOTSPOT_8
        }

        public HotSpotState State = HotSpotState.NONE;
        public Dictionary<Player, long> players = new Dictionary<Player, long>();
        public static long LastPlayers = 0;

        // time players are missing before being removed
        public static int PLAYER_EXPIRE = 60;

        public HotSpot() : base()
        {
            EvtInterface.AddEvent(UpdatePlayers, 20000, 0);
        }

        public void AddPlayer(Player Plr)
        {
            long Tick = TCPManager.GetTimeStampMS();
            if (players.Keys.Contains(Plr))
                players[Plr] = Tick;
            else
                players.Add(Plr, Tick);
        }

        public void UpdatePlayers()
        {
            if (Zone == null)
            {
                Destroy();
                return;
            }

            long Tick = TCPManager.GetTimeStampMS();
            players = players.Where(kv => (Tick - kv.Value) < PLAYER_EXPIRE * 1000).ToDictionary(x => x.Key, v => v.Value);

            if (players.Count == 0 && !IsDisposed)
            {
                Destroy();
                return;
            }

            HotSpotState newState;
            if (players.Count >= 24)
                newState = HotSpotState.HOTSPOT_24;
            else if (players.Count >= 16)
                newState = HotSpotState.HOTSPOT_16;
            else if (players.Count >= 8)
                newState = HotSpotState.HOTSPOT_8;
            else
                newState = HotSpotState.NONE;

            if (newState != State)
            {
                State = newState;
                Zone.SendHotSpots(null);
            }
        }

        public override void OnLoad()
        {
            base.OnLoad();
            IsActive = true;
        }

        public override void Dispose()
        {
            EvtInterface.RemoveEvent(UpdatePlayers);
            base.Dispose();
        }
    }
}