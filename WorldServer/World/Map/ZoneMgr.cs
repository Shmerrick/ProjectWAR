
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Common;
using FrameWork;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.Services.World;

namespace WorldServer
{
    public class ZoneMgr
    {
        public UInt16 ZoneId;
        public Zone_Info Info;
        public ClientZoneInfo ClientInfo;
        public RegionMgr Region;
        public static Int32 LOW_FIGHT = 100;
        public static Int32 MEDIUM_FIGHT = 400;
        public static Int32 LARGE_FIGHT = 1600;

        // List of pqs in the zone
        public List<PublicQuest> PQuests = new List<PublicQuest>();

        private Int32[,] _heatmap = new Int32[64, 64];

        // list of hot spots in the zone
        public List<HotSpot> HotSpots = new List<HotSpot>();

        public List<Player> Players = new List<Player>();

        public ZoneMgr(RegionMgr Region, Zone_Info Info)
        {
            this.Region = Region;
            ZoneId = Info.ZoneId;
            this.Info = Info;
            ClientInfo = ClientFileMgr.GetZoneInfo(Info.ZoneId);
        }

        #region Objects

        public void AddObject(Object obj)
        {
            if (obj.Zone == this)
            {
                Log.Error("ZoneMgr", "Object Already in zone : " + ZoneId);
                return;
            }

            obj.SetZone(this);

            if (obj is PublicQuest)
                PQuests.Add((PublicQuest)obj);

            if (obj is HotSpot)
                HotSpots.Add((HotSpot)obj);

            if (obj is Player)
                lock (Players)
                    Players.Add((Player)obj);

            obj.LastRangeCheck = new Point2D();
        }

        public void RemoveObject(Object obj)
        {
            if (obj.Zone == this)
            {
                obj.ClearZone();
            }

            if (obj is Player)
                lock (Players)
                    Players.Remove((Player)obj);

            if (obj is PublicQuest)
                PQuests.Remove((PublicQuest)obj);

            if (obj is HotSpot)
                HotSpots.Remove((HotSpot)obj);
        }

        #endregion

        #region HotSpots

        public void AddHotspotDamage(Int32 zoneX, Int32 zoneY)
        {
            Int32 x = (Int32)Point2D.Clamp(zoneX / 1024, 0, 63);
            Int32 y = (Int32)Point2D.Clamp(zoneY / 1024, 0, 63);
            if (_heatmap[x, y] <= LARGE_FIGHT + 1000)
                _heatmap[x, y]++;
        }

        public List<Tuple<Point3D, Int32>> GetHotSpots()
        {
            var HotSpots = new List<Tuple<Point3D, Int32>>();

            for (Int32 x = 0; x < 64; x++)
                for (Int32 y = 0; y < 64; y++)
                {
                    if (_heatmap[x, y] >= LOW_FIGHT)
                    {
                        HotSpots.Add(new Tuple<Point3D, Int32>(new Point3D(x * 1024 + 512, y * 1024 + 512, 0), (Int32)(_heatmap[x, y])));
                    }
                }
            return HotSpots;
        }

        public void DecayHotspots(Int32 amount = 25)
        {

            for (Int32 x = 0; x < 64; x++)
                for (Int32 y = 0; y < 64; y++)
                {
                    if (_heatmap[x, y] > 0)
                    {

                        if (_heatmap[x, y] > MEDIUM_FIGHT)
                            _heatmap[x, y] -= amount * 3;
                        else
                            _heatmap[x, y] -= amount;

                        if (_heatmap[x, y] < 0)
                            _heatmap[x, y] = 0;
                    }
                }
        }

        public void SendHotSpots(Player Plr)
        {
            List<Tuple<Point3D, Int32>> HotSpots = GetHotSpots();
            var Out = new PacketOut((Byte)Opcodes.F_UPDATE_HOT_SPOT);
            Out.WriteByte((Byte)HotSpots.Count);
            Out.WriteByte(3); // 3 - multipul hotspots
            Out.WriteUInt16(ZoneId);

            for (Int32 i = 0; i < HotSpots.Count; i++)
            {
                Out.WriteByte((Byte)i);
                Out.WriteUInt16((UInt16)HotSpots[i].Item1.X);
                Out.WriteUInt16((UInt16)HotSpots[i].Item1.Y);

                if (HotSpots[i].Item2 >= LARGE_FIGHT)
                    Out.WriteByte(0); // 24 or more
                else if (HotSpots[i].Item2 >= MEDIUM_FIGHT)
                    Out.WriteByte(2); // 16 or more
                else
                    Out.WriteByte(1); // 8 or more
            }

            if (Plr == null)
                lock (Players)
                    foreach (Player pPlr in Players)
                    {
                        if (pPlr == null || pPlr.IsDisposed || !pPlr.IsInWorld())
                            continue;

                        pPlr.SendPacket(Out);
                    }
            else
                Plr.SendPacket(Out);
        }

        #endregion

        #region Range

        public UInt16 CalculPin(UInt32 WorldPos, Boolean x) => ZoneService.CalculPin(Info, (Int32)WorldPos, x);

        #endregion
    }
}
