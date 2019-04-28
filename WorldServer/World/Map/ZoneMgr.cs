using System;
using System.Collections.Generic;
using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.World.Positions;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Map
{
    public class ZoneMgr
    {
        public ushort ZoneId;
        public Zone_Info Info;
        public ClientZoneInfo ClientInfo;
        public RegionMgr Region;
        public static int LOW_FIGHT = 50;
        public static int MEDIUM_FIGHT = 200;
        public static int LARGE_FIGHT = 600;
        public bool Running;

        // List of pqs in the zone
        public List<PublicQuest> PQuests = new List<PublicQuest>();

        private int[,] _heatmap = new int[64,64];
        // list of hot spots in the zone
        public List<HotSpot> HotSpots = new List<HotSpot>();

        public List<Player> Players = new List<Player>();

        public ZoneMgr(RegionMgr Region, Zone_Info Info)
        {
            this.Region = Region;
            ZoneId = Info.ZoneId;
            this.Info = Info;
            Running = true;
            ClientInfo = ClientFileMgr.GetZoneInfo(Info.ZoneId);
        }
        public void Stop()
        {
            Log.Debug("ZoneMgr", "[" + ZoneId + "] Stop");
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
        /*
        public bool Run(long Tick)
        {
            if (!Running)
                return false;

            int i = 0;
            lock (_Objects)
            {
                UpdateAnnounces(Tick);

            }

            return true;
        }*/

        #endregion

        #region HotSpots
        public void AddHotspotDamage(int zoneX, int zoneY)
        {
            int x = (int)Point2D.Clamp(zoneX / 1024, 0, 63);
            int y = (int)Point2D.Clamp(zoneY / 1024, 0, 63);
            if(_heatmap[x, y] <= LARGE_FIGHT+1000)
                _heatmap[x,y]++;
        }
        public List<Tuple<Point3D, int>> GetHotSpots()
        {
            List<Tuple<Point3D, int>> HotSpots = new List<Tuple<Point3D, int>>();

            for (int x = 0; x < 64; x++)
                for (int y = 0; y < 64; y++)
                {
                    if (_heatmap[x, y] >= LOW_FIGHT )
                    {
                        HotSpots.Add(new Tuple<Point3D, int>(new Point3D(x * 1024 + 512, y*1024 + 512,0), (int)(_heatmap[x, y])));
                    }
                }
            return HotSpots;
        }

        public void DecayHotspots(int amount = 25)
        {

            for (int x = 0; x < 64; x++)
                for (int y = 0; y < 64; y++)
                {
                    if (_heatmap[x, y] > 0)
                    {
                      
                        if(_heatmap[x, y] > MEDIUM_FIGHT)
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
            List<Tuple<Point3D, int>> HotSpots = GetHotSpots();
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_HOT_SPOT);
            Out.WriteByte((byte)HotSpots.Count);
            Out.WriteByte(3); // 3 - multipul hotspots
            Out.WriteUInt16(ZoneId);

            for (int i = 0; i < HotSpots.Count; i++)
            {
                Out.WriteByte((byte)i);
                Out.WriteUInt16((ushort)HotSpots[i].Item1.X);
                Out.WriteUInt16((ushort)HotSpots[i].Item1.Y);

                if (HotSpots[i].Item2 >= LARGE_FIGHT)
                    Out.WriteByte(0); // 24 or more
                else if (HotSpots[i].Item2 >= MEDIUM_FIGHT)
                    Out.WriteByte(2); // 16 or more
                else
                    Out.WriteByte(1); // 8 or more
            }

            if (Plr == null)
                lock(Players)
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

        public ushort CalculPin(uint WorldPos, bool x)
        {
            return ZoneService.CalculPin(Info, (int)WorldPos, x);
        }

        #endregion

        #region Announces

        public int CurrentAnnounce = 0;
        public long NextAnnounce = 0;

        public void UpdateAnnounces(long Tick)
        {/*
            if (NextAnnounce <= Tick)
            {
                TimedAnnounce Announce = WorldMgr.GetNextAnnounce(ref CurrentAnnounce, ZoneId);
                if (Announce == null)
                    NextAnnounce = Tick + 30000;
                else
                {
                    foreach (Player Plr in _Players)
                    {
                        if (Plr == null)
                            continue;

                        if (Announce.Realm == 0 || (byte)Plr.Realm == Announce.Realm)
                        {
                            Plr.SendMessage(0, Announce.SenderName, Announce.Message, (SystemData.ChatLogFilters)Announce.Type);
                        }
                    }

                    NextAnnounce = Tick + Announce.NextTime;
                }
            }*/
        }

        #endregion

        #region Statics

        #endregion
    }
}
