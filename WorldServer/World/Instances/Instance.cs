using Common;
using FrameWork;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Objects.Instances;

namespace WorldServer
{
    public class Respawn
    {
        public long timer;
        public uint group;

        public Respawn(long timer1,uint group1)
        {
            timer = timer1;
            group = group1;
        }
    }

    public class Instance
    {
        public ushort ID { get; set; }
        public Instance_Info Info;
        public RegionMgr Region;
        public List<Player> Players = new List<Player>();
        private List<GameObject> _Objects = new List<GameObject>();
        private Dictionary<uint,List<InstanceSpawn>> _Spawns = new Dictionary<uint, List<InstanceSpawn>>();
        private Dictionary<uint, List<InstanceBossSpawn>> _BossSpawns = new Dictionary<uint, List<InstanceBossSpawn>>();
        private List<uint> GroupsinCombat = new List<uint>();
        private List<Respawn> Respawns = new List<Respawn>();
        private EventInterface _evtInterface;
        private bool _running;
        public ushort ZoneID;
        readonly byte Realm;
        public Instance_Lockouts Lockout = null;
        private int closetime;
        public byte state;
        public byte updatestate;
        public bool Encounterinprogress = false;

        public uint CurrentBossId { get; set; } = 0;

        public Instance(ushort zoneid, ushort id, byte realm, Instance_Lockouts lockouts)
        {
            Lockout = lockouts;
            ID = id;
            ZoneID = zoneid;
            Realm = realm;
            Region = new RegionMgr(zoneid, ZoneService.GetZoneRegion(zoneid),"", new ApocCommunications());
            InstanceService._InstanceInfo.TryGetValue(zoneid, out Info);
            LoadBossSpawns();
            LoadSpawns(); // todo get the saved progress from group
            _running = true;
            _evtInterface = new EventInterface();
            closetime = (TCPManager.GetTimeStamp() + 7200);

            // instancing stuff
            InstanceService.SaveLockoutInstanceID(ZoneID + ":" + ID, Lockout);

            new Thread(Update).Start();
			
            Log.Success("Opening Instance","Instance ID "+ID+"  Map: "+Info.Name);
            if (zoneid == 179)
            {
                foreach (var p in GameObjectService.GameObjectSpawns.Where(e => e.Value.ZoneId == 179))
                {
                    if (p.Value.Entry == 98908)
                    {
                        GameObject go = new GameObject(p.Value);

                        _Objects.Add(go);
                        Region.AddObject(go,zoneid,true);

                    }
                }

                if (Info != null &&  Info.Objects.Count > 0)
                    LoadObjects();
                _evtInterface.AddEvent(UpdatePendulums, 7000, 0);
            }
        }
        
        public void UpdatePendulums()
        {
            updatestate++;

            if (updatestate == 0)
                updatestate = 1;
            Log.Success("updatestate", "  " + updatestate);
            if (updatestate % 4 != 0)
                return;

            Log.Info("update pendulum", " " + state);
            foreach (var p in _Objects.Where(e => e.Name == "Pendulum").ToList())
            {
                p.UpdateVfxState((byte)state);
            }
            state++;
            if (state > 6)
                state = 4;
        }

        public void Update()
        {
            while (_running)
            {
                Checkcombatgroups();
                Checkrespawns();
                Checkinstanceempty();
                CheckPlayers();

                Thread.Sleep(500);
            }
        }

        private void CheckPlayers()
        {
            lock (Players)
            {
                for (int i = 0; i < Players.Count; i++)
                    if (Players[i].IsDisposed)
                    {
                        Players.RemoveAt(i);
                        InstanceService.SavePlayerIDs(ZoneID + ":" + ID, Players);
                    }
            }
        }

        private void Checkinstanceempty()
        {
            if (Region.Players.Count > 0)
                closetime = TCPManager.GetTimeStamp() + (int)Info.LockoutTimer * 60;
			
            if (closetime < TCPManager.GetTimeStamp())
            {
                Log.Success("Closing Instance", "Instance ID " + ID + "  Map: " + Info.Name);
                Region.Stop();
                WorldMgr.InstanceMgr.CloseInstance(this, ID);
                _running = false;
            }
        }

        private void Checkrespawns()
        {
            lock (Respawns)
            {

                for (int i = 0; i < Respawns.Count; i++)
                {
                    if (Respawns[i].timer  < TCPManager.GetTimeStampMS())
                    {
                        RespawnInstanceGroup(Respawns[i].group,true);
                        Respawns.RemoveAt(i);
                    }
                    else
                        break;
                }
            }
        }

        private void Checkcombatgroups()
        {

            if (_Spawns == null || _Spawns.Count == 0)
                return;

            lock (GroupsinCombat)
            {
                for (int i = 0; i < GroupsinCombat.Count; i++)
                {
                    List<InstanceSpawn> sp = new List<InstanceSpawn>();
                    _Spawns.TryGetValue(GroupsinCombat[i], out sp);
                    ushort death = 0;

                    foreach (InstanceSpawn IS in sp)
                    {
                        if (IS.IsDead)
                            death++;
                    }
                    if (death == sp.Count)
                    {
                        Respawns.Add(new Respawn(TCPManager.GetTimeStampMS() + (Info.TrashRespawnTimer * 1000), GroupsinCombat[i]));
                        GroupsinCombat.Remove(GroupsinCombat[i]);
                        i--;
                    }
                }
            }
        }

		public int GetBossCount()
		{
			return _BossSpawns.Count;
		}
		
        public void AddPlayer(Player player, Zone_jump jump)
        {
            lock (Players)
            {
                if (!Players.Contains(player))
                {
                    Players.Add(player);
                }
                
                player.InstanceID = ZoneID + ":" + ID;

                if (jump != null)
                {
                    player.Teleport(Region, jump.ZoneID, jump.WorldX, jump.WorldY, jump.WorldZ, jump.WorldO);
                }
                else
                {
                    player.Teleport(Region, player._Value.ZoneId, (uint)player._Value.WorldX, (uint)player._Value.WorldY, (ushort)player._Value.WorldZ, (ushort)player._Value.WorldO);
                }

                Region.CheckZone(player);

                InstanceService.SavePlayerIDs(ZoneID + ":" + ID, Players);

                player.SendClientMessage("Instance ID: " + ID, SystemData.ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);

                string players = string.Empty;
                foreach (Player plr in Players)
                    players += plr.Name + ",";
                if (players.EndsWith(","))
                    players = players.Substring(0, players.Length - 1);

                player.SendClientMessage("Registered players: " + players, SystemData.ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
                player.SendClientMessage("Note: Wait for your party leader to get into the instance if you find yourself in another instance ID.", SystemData.ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        private void LoadObjects()
        {
            foreach (var obj in Info.Objects)
            {
				InstanceObject o = null;
				if (obj.DoorID != 0)
					o = new InstanceDoor(this, obj);
				else
					o = new InstanceObject(this, obj);

				Region.AddObject(o, Info.ZoneID, true);
				o.SetZone(Region.GetZoneMgr(Info.ZoneID));
				Region.CheckZone(o);
				_Objects.Add(o);
			}
        }

        public void OnBossDeath(uint GroupID, InstanceBossSpawn boss)
        {
			Encounterinprogress = false;
		}

		public void ApplyLockout(List<Player> subGroup)
		{
            if (Lockout == null) // instance hasn't got any lockouts
			{
				Lockout = new Instance_Lockouts
				{
					InstanceID = "~" + ZoneID + ":" + (TCPManager.GetTimeStamp() + Info.LockoutTimer * 60),
					Bosseskilled = CurrentBossId.ToString()
				};
				InstanceService._InstanceLockouts.Add(Lockout.InstanceID, Lockout);
				Lockout.Dirty = true;
				WorldMgr.Database.AddObject(Lockout);
                InstanceService.SaveLockoutInstanceID(ZoneID + ":" + ID, Lockout);
			}
			else // instance has got already lockouts
			{
                List<string> bossList = Lockout.Bosseskilled.Split(':').Distinct().ToList();
                if (!bossList.Contains(CurrentBossId.ToString()))
                    bossList.Add(CurrentBossId.ToString());
                Lockout.Bosseskilled = string.Empty;
                foreach (string boss in bossList)
                    Lockout.Bosseskilled += ":" + boss;
                if (Lockout.Bosseskilled.StartsWith(":"))
                    Lockout.Bosseskilled = Lockout.Bosseskilled.Substring(1);
                Lockout.Dirty = true;
				WorldMgr.Database.SaveObject(Lockout);
			}

			foreach (Player pl in subGroup)
			{
				pl._Value.AddLockout(Lockout);
				pl.SendLockouts();
			}
		}
		
		private void LoadBossSpawns()
        {
            List<uint> deadbossids = new List<uint>();

            if (Lockout != null)
				for (int i = 0; i < Lockout.Bosseskilled.Split(':').Count();i++)
				{
					deadbossids.Add(uint.Parse(Lockout.Bosseskilled.Split(':')[i]));
				}
			
            InstanceService._InstanceBossSpawns.TryGetValue(Info.Entry, out List<Instance_Boss_Spawn> Obj);
			
            if (Obj == null)
                return;

            foreach (var obj in Obj)
            {
                if (obj.Realm == 0 || obj.Realm == Realm)
                {
                    if (deadbossids.Contains(obj.BossID))
                        continue;

                    if (obj.ZoneID != ZoneID)
                        continue;

                    Creature_spawn spawn = new Creature_spawn
                    {
                        Guid = (uint)CreatureService.GenerateCreatureSpawnGUID()
                    };
                    spawn.BuildFromProto(CreatureService.GetCreatureProto(obj.Entry));
                    spawn.WorldO = (int)obj.WorldO;
                    spawn.WorldY = obj.WorldY;
                    spawn.WorldZ = obj.WorldZ;
                    spawn.WorldX = obj.WorldX;
                    spawn.ZoneId = obj.ZoneID;
                    spawn.Enabled = 1;
                    spawn.Level = spawn.Proto.MinLevel;

					InstanceBossSpawn IS = null;

					switch (obj.Entry)
                    {
                        case 4276:
                            IS = new SimpleTheDeamonicBeast(spawn, obj.SpawnGroupID, obj.BossID, obj.InstanceID, this);
                            break;

                        case 59211:
							IS = new SimpleAhzranok(spawn, obj.SpawnGroupID, obj.BossID, obj.InstanceID, this);
							break;

                        case 6821:
                            IS = new SimpleMalghorGreathorn(spawn, obj.SpawnGroupID, obj.BossID, obj.InstanceID, this);
                            break;

                        case 6841:
                            IS = new SimpleHorgulul(spawn, obj.SpawnGroupID, obj.BossID, obj.InstanceID, this);
                            break;

                        case 6843:
                            IS = new SimpleDralel(spawn, obj.SpawnGroupID, obj.BossID, obj.InstanceID, this);
                            break;
                            
                        //case 2000899:
                        //    IS = new SimpleFulgurThunderborn(spawn, obj.SpawnGroupID, obj.BossID, obj.InstanceID, this);
                        //    break;

                        //case 2000901:
                        //    IS = new SimpleTonragThunderborn(spawn, obj.SpawnGroupID, obj.BossID, obj.InstanceID, this);
                        //    break;

                        default:
							IS = new InstanceBossSpawn(spawn, obj.SpawnGroupID, obj.BossID, obj.InstanceID, this);
							break;
					}

					if (IS == null)
						return;

                    if (obj.SpawnGroupID > 0)
                    {
                        _BossSpawns.TryGetValue(obj.SpawnGroupID, out List<InstanceBossSpawn> spawns);
                        if (spawns == null)
                        {
                            spawns = new List<InstanceBossSpawn>();
                        }
                        spawns.Add(IS);
                        _BossSpawns[obj.SpawnGroupID] = spawns;
                    }
                    Region.AddObject(IS, obj.ZoneID);
                }
            }
        }

        private void LoadSpawns()
        {
            InstanceService._InstanceSpawns.TryGetValue(ZoneID, out List<Instance_Spawn> Obj);

            List<uint> deadbossids = new List<uint>();
            if (Lockout != null)
                for (int i = 0; i < Lockout.Bosseskilled.Split(';').Count(); i++)
                {
                    deadbossids.Add(uint.Parse(Lockout.Bosseskilled.Split(';')[i].Split(':')[1]));
                }

            if (Obj == null)
                return;

            foreach (var obj in Obj)
            {
                if (deadbossids.Contains(obj.ConnectedBossID))
                    continue;
				
                if (obj.Realm == 0 || obj.Realm == Realm)
                {
                    Creature_spawn spawn = new Creature_spawn
                    {
                        Guid = (uint)CreatureService.GenerateCreatureSpawnGUID()
                    };
                    spawn.BuildFromProto(CreatureService.GetCreatureProto(obj.Entry));
                    if(spawn.Proto== null)
                    {
                        Log.Error("Creature Proto not found", " " + obj.Entry);
                        continue;
                    }

                    spawn.WorldO = (int)obj.WorldO;
                    spawn.WorldY = obj.WorldY;
                    spawn.WorldZ = obj.WorldZ;
                    spawn.WorldX = obj.WorldX;
                    spawn.ZoneId = obj.ZoneID;
                    spawn.Enabled = 1;
					
                    InstanceSpawn IS = new InstanceSpawn(spawn, obj.SpawnGroupID, obj.ConnectedBossID,this);

                    if (obj.SpawnGroupID > 0)
                    {
                        _Spawns.TryGetValue(obj.SpawnGroupID, out List<InstanceSpawn> spawns);
                        if (spawns == null)
                            spawns = new List<InstanceSpawn>();
                        spawns.Add(IS);
                        _Spawns[obj.SpawnGroupID] = spawns;
                    }
                    Region.AddObject(IS, obj.ZoneID);
                }
            }
        }

        public void AttackTarget(uint GroupID, Unit Target)
        {
            if (Target == null)
                return;

            lock (GroupsinCombat)
            {
                if (!GroupsinCombat.Contains(GroupID))
                {
                    GroupsinCombat.Add(GroupID);
                    _Spawns.TryGetValue(GroupID, out List<InstanceSpawn> spawns);
                    if (spawns == null)
                        spawns = new List<InstanceSpawn>();
                    foreach (InstanceSpawn sp in spawns )
                    {
                        sp.AiInterface.ProcessCombatStart(Target);
                    }
                }
            }
        }

        public void BossAttackTarget(uint GroupID, Unit Target)
        {
            lock (GroupsinCombat)
            {
                if (!GroupsinCombat.Contains(GroupID))
                {
                    GroupsinCombat.Add(GroupID);
                    _BossSpawns.TryGetValue(GroupID, out List<InstanceBossSpawn> spawns);
                    if (spawns == null)
                        spawns = new List<InstanceBossSpawn>();
                    foreach (InstanceBossSpawn sp in spawns)
                    {
                        sp.AiInterface.ProcessCombatStart(Target);
                    }
                }
            }
        }

        public void RespawnInstanceGroup(uint GroupID, bool rezall = false)
        {
            lock (GroupsinCombat)
            {
                if (!GroupsinCombat.Contains(GroupID) || rezall)
                    return;
                GroupsinCombat.Remove(GroupID);

                _Spawns.TryGetValue(GroupID, out List<InstanceSpawn> spawns);
                if (spawns == null)
                    spawns = new List<InstanceSpawn>();
                for (int i =0;i < spawns.Count ; i++)
                {
                    if(spawns[i].IsDead)
                    {
                        InstanceSpawn IS = spawns[i].RezInstanceSpawn();
                        spawns[i] = IS;
                    }
                }
                _Spawns[GroupID] = spawns;
            }
        }

        public void BossRespawnInstanceGroup(uint GroupID, bool rezall = false)
        {
            lock (GroupsinCombat)
            {
                if (!GroupsinCombat.Contains(GroupID) || rezall)
                    return;
                GroupsinCombat.Remove(GroupID);

                _BossSpawns.TryGetValue(GroupID, out List<InstanceBossSpawn> spawns);
                if (spawns == null)
                    spawns = new List<InstanceBossSpawn>();
                for (int i = 0; i < spawns.Count; i++)
                {
                    if (spawns[i].IsDead)
                    {
                        InstanceBossSpawn IS = spawns[i].RezInstanceSpawn();
                        spawns[i] = IS;
                    }
                }
                _BossSpawns[GroupID] = spawns;
            }
            Encounterinprogress = false;
        }

        public void DoorOpenned(InstanceDoor door)
        {

        }

        public void DoorClosed(InstanceDoor door)
        {

        }

		public void RemoveInstanceObjectOnBossDeath(uint bossId)
		{
			var list = _Objects.Where(x => (x as InstanceObject).Info.EncounterID == bossId).ToList();
			if (list != null && list.Count > 0)
			{
				list.ForEach(x => x.RemoveFromWorld());
			}
		}
    }
}
