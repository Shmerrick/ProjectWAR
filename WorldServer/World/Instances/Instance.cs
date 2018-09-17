using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Objects;

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
        public List<string> _Players = new List<string>();
        private List<GameObject> _Objects = new List<GameObject>();
        private Dictionary<uint,List<InstanceSpawn>> _Spawns = new Dictionary<uint, List<InstanceSpawn>>();
        private Dictionary<uint, List<InstanceBossSpawn>> _BossSpawns = new Dictionary<uint, List<InstanceBossSpawn>>();
        private List<uint> GroupsinCombat = new List<uint>();
        private List<Respawn> Respawns = new List<Respawn>();
        private EventInterface _evtInterface;
        private bool _running;
        public ushort ZoneID;
        byte Realm;
        public Instance_Lockouts Lockout = null;
        private int closetime;
        public byte state;
        public byte updatestate;
        public bool Encounterinprogress = false;

        public Instance(ushort zoneid, ushort id, byte realm, Instance_Lockouts lockouts)
        {
            Lockout = lockouts;
            ID = id;
            ZoneID = zoneid;
            Realm = realm;
            Region = new RegionMgr(zoneid, ZoneService.GetZoneRegion(zoneid),"", new ApocCommunications());
            InstanceService._InstanceInfo.TryGetValue(zoneid,out Info);
            LoadBossSpawns();
            LoadSpawns(); // todo get the saved progress from group
            _running = true;
            _evtInterface = new EventInterface();
            closetime = (TCPManager.GetTimeStamp() + 7200);

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

                if (_Objects.Count > 0)
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
                checkcombatgroups();
                checkrespawns();
                checkinstanceempty();

                Thread.Sleep(2000);
            }
        }

        private void checkinstanceempty()
        {
            if (Region.Players.Count > 0)
                closetime = TCPManager.GetTimeStamp() + (int)Info.LockoutTimer * 60;
			
            if (closetime < TCPManager.GetTimeStamp())
            {
                Log.Success("Closing Instance", "Instance ID " + ID + "  Map: " + Info.Name);
                Region.Stop();
                WorldMgr.InstanceMgr.closeInstance(this,ID);
                _running = false;
            }
        }

        private void checkrespawns()
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

        private void checkcombatgroups()
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

        public void AddPlayer(Player player)
        {
            lock (_Players)
            {
                if (!_Players.Contains(player.Name))
                {
                    _Players.Add(player.Name);
                }
            }

            player.InstanceID = ID;

            // player._Value.AddLogout("60;"+(TCPManager.GetTimeStamp()+34000)+";1;4");
            // player.SendLockouts();

            Region.AddObject(player, ZoneID, true);
        }
		
        public void AddPlayer(Player player, Zone_jump jump)
        {
            lock (_Players)
            {
                if (!_Players.Contains(player.Name))
                {
                    _Players.Add(player.Name);
                }
            }

            player.InstanceID = ID;

			if (jump != null)
			{
				player.Teleport(Region, jump.ZoneID, jump.WorldX, jump.WorldY, jump.WorldZ, jump.WorldO);
			}
			else
			{
				player.Teleport(Region, player._Value.ZoneId, (uint)player._Value.WorldX, (uint)player._Value.WorldY, (ushort)player._Value.WorldZ, (ushort)player._Value.WorldO);
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
			if (Lockout == null) // instance hasn't got any lockouts
			{
				Lockout = new Instance_Lockouts
				{
					InstanceID = "~" + boss.InstanceID + ":" + (TCPManager.GetTimeStamp() + Info.LockoutTimer * 60),
					Bosseskilled = boss.BossID.ToString()
				};
				InstanceService._InstanceLockouts.Add(Lockout.InstanceID, Lockout);
				Lockout.Dirty = true;
				WorldMgr.Database.AddObject(Lockout);
			}
			else // instance has got already lockouts
			{
				Lockout.Bosseskilled += ":" + boss.BossID;
				Lockout.Dirty = true;
				WorldMgr.Database.SaveObject(Lockout);
			}

			foreach (Player pl in Region.Players)
			{
				pl._Value.AddLockout(Lockout);
				pl.SendLockouts();
			}
			Encounterinprogress = false;
		}

        private void LoadBossSpawns()
        {
            List<Instance_Boss_Spawn> Obj;

            List<uint> deadbossids = new List<uint>();

            if (Lockout != null)
				for (int i = 0; i < Lockout.Bosseskilled.Split(';').Count();i++)
				{
					deadbossids.Add(uint.Parse(Lockout.Bosseskilled.Split(';')[i].Split(':')[1]));
				}
			
            InstanceService._InstanceBossSpawns.TryGetValue(Info.Entry, out Obj);
			
            if (Obj == null)
                return;

            foreach (var obj in Obj)
            {
                if (obj.Realm == 0 || obj.Realm == this.Realm)
                {
                    if (deadbossids.Contains(obj.BossID))
                        continue;

                    if (obj.ZoneID != ZoneID)
                        continue;
					
                    Creature_spawn spawn = new Creature_spawn();
                    spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                    spawn.BuildFromProto(CreatureService.GetCreatureProto((uint)obj.Entry));
                    spawn.WorldO = (int)obj.WorldO;
                    spawn.WorldY = obj.WorldY;
                    spawn.WorldZ = obj.WorldZ;
                    spawn.WorldX = obj.WorldX;
                    spawn.ZoneId = obj.ZoneID;
                    spawn.Enabled = 1;
					
                    InstanceBossSpawn IS = new InstanceBossSpawn(spawn, obj.SpawnGroupID, obj.BossID, obj.InstanceID, this);

                    if (obj.SpawnGroupID > 0)
                    {
                        List<InstanceBossSpawn> spawns;
                        _BossSpawns.TryGetValue(obj.SpawnGroupID, out spawns);
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
            List<Instance_Spawn> Obj;
           
            InstanceService._InstanceSpawns.TryGetValue(ZoneID, out Obj);

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
				
                if (obj.Realm == 0 || obj.Realm == this.Realm)
                {
                    Creature_spawn spawn = new Creature_spawn();
                    spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                    spawn.BuildFromProto(CreatureService.GetCreatureProto((uint)obj.Entry));
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
                        List<InstanceSpawn> spawns;
                        _Spawns.TryGetValue(obj.SpawnGroupID, out spawns);
                        if (spawns == null)
                        {
                            spawns = new List<InstanceSpawn>();
                        }
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
                    List<InstanceSpawn> spawns = new List<InstanceSpawn>();
                    _Spawns.TryGetValue(GroupID, out spawns);
                    foreach(InstanceSpawn sp in spawns )
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
                    List<InstanceBossSpawn> spawns = new List<InstanceBossSpawn>();
                    _BossSpawns.TryGetValue(GroupID, out spawns);
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

                List<InstanceSpawn> spawns;
                _Spawns.TryGetValue(GroupID, out spawns);
                for(int i =0;i < spawns.Count ; i++)
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

                List<InstanceBossSpawn> spawns;
                _BossSpawns.TryGetValue(GroupID, out spawns);
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
    }
}
