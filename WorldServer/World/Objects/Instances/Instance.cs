using Common;
using FrameWork;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects.Instances.Bastion_Stairs;
using WorldServer.World.Objects.Instances.Bilerot_Burrow;
using WorldServer.World.Objects.Instances.Bloodwrought_Enclave;
using WorldServer.World.Objects.Instances.Hunters_Vale;
using WorldServer.World.Objects.Instances.Mount_Gunbad;
using WorldServer.World.Objects.Instances.SacellumDungeonsWestWingSacellum1;
using WorldServer.World.Objects.Instances.SacellumDungeonsWestWingSacellum2;
using WorldServer.World.Objects.Instances.SacellumDungeonsWestWingSacellum3;
using WorldServer.World.Objects.Instances.SigmarCrypts;
using WorldServer.World.Objects.Instances.The_Lost_Vale;
using WorldServer.World.Objects.Instances.TheSewersofAltdorfWing1Sewers2;
using WorldServer.World.Objects.Instances.TheSewersofAltdorfWing2Sewers3;
using WorldServer.World.Objects.Instances.TheSewersofAltdorfWing3Sewers;
using WorldServer.World.Objects.Instances.TomboftheVultureLord;
using WorldServer.World.Objects.Instances.Tombs;
using WorldServer.World.Objects.Instances.WarpbladeTunnels1;
using WorldServer.World.Objects.Instances.WarpbladeTunnels2;

namespace WorldServer.World.Objects.Instances
{
    public class Respawn
    {
        public long timer;
        public uint group;

        public Respawn(long timer1, uint group1)
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
        private Dictionary<uint, List<InstanceSpawn>> _Spawns = new Dictionary<uint, List<InstanceSpawn>>();
        private Dictionary<uint, List<InstanceBossSpawn>> _BossSpawns = new Dictionary<uint, List<InstanceBossSpawn>>();
        private List<uint> GroupsinCombat = new List<uint>();
        private List<Respawn> Respawns = new List<Respawn>();
        private EventInterface _evtInterface;
        private bool _running;
        public ushort ZoneID;
        private readonly byte Realm;
        public Instance_Lockouts Lockout = null;
        private int closetime;
        public byte state;
        public byte updatestate;
        public bool EncounterInProgress = false;

        public uint CurrentBossId { get; set; } = 0;

        public Instance(ushort zoneid, ushort id, byte realm, Instance_Lockouts lockouts)
        {
            Lockout = lockouts;
            ID = id;
            ZoneID = zoneid;
            Realm = realm;
            Region = new RegionMgr(zoneid, ZoneService.GetZoneRegion(zoneid), "", new ApocCommunications());
            InstanceService._InstanceInfo.TryGetValue(zoneid, out Info);
            LoadBossSpawns();
            LoadSpawns(); // todo get the saved progress from group
            _running = true;
            _evtInterface = new EventInterface();
            closetime = (TCPManager.GetTimeStamp() + 7200);

            // instancing stuff
            InstanceService.SaveLockoutInstanceID(ZoneID + ":" + ID, Lockout);

            new Thread(Update).Start();

            Log.Success("Opening Instance", "Instance ID " + ID + "  Map: " + Info.Name);
            // TOVL
            if (zoneid == 179)
            {
                foreach (var p in GameObjectService.GameObjectSpawns.Where(e => e.Value.ZoneId == 179))
                {
                    if (p.Value.Entry == 98908)
                    {
                        GameObject go = new GameObject(p.Value);

                        _Objects.Add(go);
                        Region.AddObject(go, zoneid, true);
                    }
                }

                if (Info != null && Info.Objects.Count > 0)
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
                CheckCombatGroups();
                CheckRespawns();
                CheckInstanceEmpty();
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

        private void CheckInstanceEmpty()
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

        private void CheckRespawns()
        {
            lock (Respawns)
            {
                for (int i = 0; i < Respawns.Count; i++)
                {
                    if (Respawns[i].timer < TCPManager.GetTimeStampMS())
                    {
                        RespawnInstanceGroup(Respawns[i].group, true);
                        Respawns.RemoveAt(i);
                    }
                    else
                        break;
                }
            }
        }

        private void CheckCombatGroups()
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
                    if (sp != null)
                        foreach (InstanceSpawn IS in sp)
                        {
                            if (IS.IsDead)
                                death++;
                        }
                    else
                        return;
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
            EncounterInProgress = false;
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
            List<uint> deadbossIds = new List<uint>();

            if (Lockout != null)
                for (int i = 0; i < Lockout.Bosseskilled.Split(':').Count(); i++)
                {
                    deadbossIds.Add(uint.Parse(Lockout.Bosseskilled.Split(':')[i]));
                }

            InstanceService._InstanceBossSpawns.TryGetValue(Info.Entry, out List<Instance_Boss_Spawn> Obj);

            if (Obj == null)
                return;

            foreach (var obj in Obj)
            {
                if (obj.Realm == 0 || obj.Realm == Realm)
                {
                    if (deadbossIds.Contains(obj.bossId))
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
                    spawn.Level = obj.Level;

                    InstanceBossSpawn IS = null;

                    switch (obj.Entry)
                    {
                        //zone 50
                        case 97425:
                            IS = new SimpleThananTreeLord(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 97441:
                            IS = new SimpleTheCadaithaineLion(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 97430:
                            IS = new SimpleSpiritofKurnous(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 63
                        case 38829:
                            IS = new SimpleGlompdaSquigMasta(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 64
                        case 37967:
                            IS = new SimpleMastaMixa(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 65
                        case 15102:
                            IS = new SimpleArdtaFeed(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 66
                        case 42207:
                            IS = new SimpleWightLordSolithex(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 152
                        case 19409:
                            IS = new SimpleKokritManEater(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 153
                        case 3650:
                            IS = new SimpleBulbousOne(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 3651:
                            IS = new SimpleProtFangchitter(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 3649:
                            IS = new SimpleVermerFangchitter(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 154
                        case 2501335:
                            IS = new SimpleGreySeerQuoltik(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 2501338:
                            IS = new SimpleMasterMoulderSkrot(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 2501340:
                            IS = new SimpleBrauk(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 155
                        case 10256:
                            IS = new SimpleHoarfrost(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 26812:
                            IS = new SimpleSebcrawtheDiscarded(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 26814:
                            IS = new SimpleLorthThunderbelly(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 26815:
                            IS = new SimpleSlorthThunderbelly(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 156
                        case 25721:
                            IS = new SimpleGhalmarRagehorn(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 33180:
                            IS = new SimpleUzhaktheBetrayer(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 33173:
                            IS = new SimpleVultheBloodchosen(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 163
                        case 45084:
                            IS = new SimpleTharlgnan(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 164
                        case 48112:
                            IS = new SimpleLordSlaurith(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 165
                        case 2000751:
                            IS = new SimpleKaarntheVanquisher(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 166
                        case 64106:
                            IS = new SimpleSkullLordVarIthrok(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 169
                        case 33401:
                            IS = new SimpleGoradiantheCreator(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 18762:
                            IS = new SimpleMasterMoulderVitchek(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 173
                        case 33172:
                            IS = new SimpleSnaptailtheBreeder(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 33182:
                            IS = new SimpleGoremane(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 33181:
                            IS = new SimpleViraxiltheBroken(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 176
                        case 2500954:
                            IS = new SimpleTheReaper(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 2500953:
                            IS = new SimpleCryptwebQueen(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 2500952:
                            IS = new SimpleSeraphinePaleEye(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 2500951:
                            IS = new SimpleSisterEudocia(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 2500950:
                            IS = new SimpleArchLectorVerrimus(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 2500949:
                            IS = new SimpleArchLectorZakarai(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 2500948:
                            IS = new SimpleTobiastheFallen(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 2500947:
                            IS = new SimpleNecromancerMalcidious(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 177
                        case 2501325:
                            IS = new SimpleSkivRedwarp(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 2501326:
                            IS = new SimpleWarlockPeenk(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 179
                        case 93757:
                            IS = new SimpleHandofUalatp(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 93814:
                            IS = new SimpleUsiriansKeeper(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 93834:
                            IS = new SimpleHighPriestHerakh1(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //case 93834:
                        //  IS = new SimpleHighPriestHerakh2(spawn,  obj.bossId, obj.InstanceID, this);
                        //  break;
                        //case 93834:
                        //    IS = new SimpleHighPriestHerakh3(spawn,  obj.bossId, obj.InstanceID, this);
                        //    break;
                        case 94102:
                            IS = new SimpleAkiltheShrewd(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 94101:
                            IS = new SimpleJahitheIndignant(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 94103:
                            IS = new SimpleTumainitheHopeless(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 94191:
                            IS = new SimpleHierophantEutrata(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 94389:
                            IS = new SimpleKingAmenemhetumtheVultureLord(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 195
                        case 2000763:
                            IS = new SimpleCuliusEmbervine(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 46995:
                            IS = new SimpleSarlothBloodtouched(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 2000757:
                            IS = new SimpleKorthuktheRaging(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 46205:
                            IS = new SimpleBarakustheGodslayer(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 196
                        case 52594:
                            IS = new SimpleTheBileLord(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 52462:
                            IS = new SimpleSsrydianMorbidae(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 48128:
                            IS = new SimpleBartholomeustheSickly(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 241
                        case 2000772:
                            IS = new SimpleTsekaniHeyafa(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 242
                        case 2000764:
                            IS = new SimpleHapuShebikef(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 243
                        case 2000767:
                            IS = new SimpleBennuApeht(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 244
                        case 2000774:
                            IS = new SimpleSaaKhasef(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //zone 260
                        case 4276:
                            IS = new SimpleTheDeamonicBeast(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 59211:
                            IS = new SimpleAhzranok(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 6821:
                            IS = new SimpleMalghorGreathorn(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 6841:
                            IS = new SimpleHorgulul(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 6843:
                            IS = new SimpleDralel(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 6807:
                            IS = new SimpleChulEarthkeeper(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 6856:
                            IS = new SimpleLargtheDevourer(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 6834:
                            IS = new SimpleButcherGutbeater(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 6850:
                            IS = new SimpleGoraktheAncient(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 6842:
                            IS = new SimpleSarthaintheWorldbearer(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 61601:
                            IS = new SimpleZaarthePainseeker(spawn, obj.bossId, obj.InstanceID, this);
                            break;
                        //case 4276:
                        //    IS = new SimpleTheDarkpromiseBeast2(spawn,  obj.bossId, obj.InstanceID, this);
                        //    break;
                        case 61598:
                            IS = new SimpleSechartheDarkpromiseChieftain(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        case 62147:
                            IS = new SimpleNKariKeeperofSecrets(spawn, obj.bossId, obj.InstanceID, this);
                            break;

                        //REMOVED BOSSES
                        //case 2000899:
                        //    IS = new SimpleFulgurThunderborn(spawn,  obj.bossId, obj.InstanceID, this);
                        //    break;

                        //case 2000901:
                        //    IS = new SimpleTonragThunderborn(spawn,  obj.bossId, obj.InstanceID, this);
                        //    break;
                        default:
                            IS = new InstanceBossSpawn(spawn, obj.bossId, obj.InstanceID, this);
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

            List<uint> deadbossIds = new List<uint>();
            if (Lockout != null)
                for (int i = 0; i < Lockout.Bosseskilled.Split(';').Count(); i++)
                {
                    deadbossIds.Add(uint.Parse(Lockout.Bosseskilled.Split(';')[i].Split(':')[1]));
                }

            if (Obj == null)
                return;

            foreach (var obj in Obj)
            {
                if (deadbossIds.Contains(obj.ConnectedbossId))
                    continue;

                if (obj.Realm == 0 || obj.Realm == Realm)
                {
                    Creature_spawn spawn = new Creature_spawn
                    {
                        Guid = (uint)CreatureService.GenerateCreatureSpawnGUID()
                    };
                    spawn.BuildFromProto(CreatureService.GetCreatureProto(obj.Entry));
                    if (spawn.Proto == null)
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

                    InstanceSpawn IS = new InstanceSpawn(spawn, obj.ConnectedbossId, this);

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
                    foreach (InstanceSpawn sp in spawns)
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
                for (int i = 0; i < spawns.Count; i++)
                {
                    if (spawns[i].IsDead)
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
            EncounterInProgress = false;
        }

        public void DoorOpened(InstanceDoor door)
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