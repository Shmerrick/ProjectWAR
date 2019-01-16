using Common;
using FrameWork;
using GameData;
using NLog;
using System;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer.World.BattleFronts.Keeps
{
    public class KeepNpcCreature : IComparable<KeepNpcCreature>
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public RegionMgr Region;
        public KeepCreature Creature;
        public Keep_Creature Info;
        public BattleFrontKeep Keep;

        public KeepNpcCreature(RegionMgr region, Keep_Creature info, BattleFrontKeep keep)
        {
            Region = region;
            Info = info;
            Keep = keep;
        }

        public int CompareTo(KeepNpcCreature other)
        {
            if (other == null) return 1;
            return Info.WaypointGUID.CompareTo(other.Info.WaypointGUID);
        }

        public void SpawnGuard(Realms realm)
        {
            if (Creature != null)
            {
                Creature.Destroy();
                Creature = null;

           }

            if (realm != Realms.REALMS_REALM_NEUTRAL)
            {
                if (Info.DestroId == 0 && realm == Realms.REALMS_REALM_DESTRUCTION)
                    _logger.Trace($"Creature Id = 0, no spawning");
                else
                {
                    Creature_proto proto = CreatureService.GetCreatureProto(realm == Realms.REALMS_REALM_ORDER ? Info.OrderId : Info.DestroId);
                    
                    if (proto == null)
                    {
                        Log.Error("KeepNPC", "No FlagGuard Proto");
                        return;
                    }
                    _logger.Trace($"Spawning Guard {proto.Name} ({proto.Entry})");

                    Creature_spawn spawn = new Creature_spawn();
                    spawn.BuildFromProto(proto);
                    spawn.WorldO = Info.O;
                    spawn.WorldX = Info.X;
                    spawn.WorldY = Info.Y;
                    spawn.WorldZ = Info.Z;
                    spawn.ZoneId = Info.ZoneId;

                    Creature = new KeepCreature(spawn, this, Keep)
                    {
                        WaypointGUID = Convert.ToUInt32(Info.WaypointGUID)
                    };

                    Region.AddObject(Creature, spawn.ZoneId);
                }
                
            }
        }

        public void SpawnGuardNear(Realms realm, KeepNpcCreature nearPatrol)
        {
            if (Creature != null)
            {
                Creature.Destroy();
                Creature = null;
            }

            if (realm != Realms.REALMS_REALM_NEUTRAL)
            {
                Creature_proto proto = CreatureService.GetCreatureProto(realm == Realms.REALMS_REALM_ORDER ? Info.OrderId : Info.DestroId);

                _logger.Trace($"Spawning Guard Near {proto.Name} ({proto.Entry})");

                if (proto == null)
                {
                    Log.Error("KeepNPC", "No FlagGuard Proto");
                    return;
                }

                Creature_spawn spawn = new Creature_spawn();
                spawn.BuildFromProto(proto);
                spawn.WorldO = nearPatrol.Info.O;
                spawn.WorldX = nearPatrol.Creature.WorldPosition.X + (nearPatrol.Info.X - Info.X);
                spawn.WorldY = nearPatrol.Creature.WorldPosition.Y + (nearPatrol.Info.Y - Info.Y);
                //ushort height = (ushort)ClientFileMgr.GetHeight(nearPatrol.Info.ZoneId, spawn.WorldX, spawn.WorldY);
                //spawn.WorldZ = Info.IsPatrol ? ((height <= 0) ? Info.Z : height) : Info.Z;
                spawn.WorldZ = nearPatrol.Creature.WorldPosition.Z;
                spawn.ZoneId = nearPatrol.Info.ZoneId;

                Creature = new KeepCreature(spawn, this, Keep)
                {
                    WaypointGUID = Convert.ToUInt32(Info.WaypointGUID),
                    NearAiInterface = nearPatrol.Creature.AiInterface
                };

                Region.AddObject(Creature, spawn.ZoneId);
            }
        }

        public void DespawnGuard()
        {
            if (Creature != null)
            {
                Creature.Destroy();
                Creature = null;
            }
        }

        public class KeepCreature : Creature
        {
            private readonly KeepNpcCreature _flagGrd;
            private readonly BattleFrontKeep _keep;
            /// <summary>Incoming damage scaler from 0.25 to 1<summary>
            private volatile float _damageScaler = 1f;
            public AIInterface NearAiInterface = null;

            public KeepCreature(Creature_spawn spawn, KeepNpcCreature flagGrd, BattleFrontKeep keep) : base(spawn)
            {
                _keep = keep;
                _flagGrd = flagGrd;
                IsKeepLord = flagGrd.Info.KeepLord;
                IsPatrol = flagGrd.Info.IsPatrol;

                EvtInterface.AddEventNotify(EventName.OnReceiveDamage, OnReceiveDamage);
            }

            public KeepNpcCreature returnflag()
            {
                return _flagGrd;
            }

            public override void OnLoad()
            {
                base.OnLoad();

                ScaleLord(_keep.Rank);
                // buff lord with multipler 3 //TODO: rework needed (morale abilities does dmg through the scaler etc)
                if (IsKeepLord)
                    Health *= 3;

                if (WaypointGUID > 0)
                {
                    AiInterface.Waypoints = WaypointService.GetNpcWaypoints(WaypointGUID);
                    foreach (var wp in AiInterface.Waypoints)
                    {
                        wp.X = Convert.ToUInt32(wp.X + WaypointService.ShuffleWaypointOffset(5, 15));
                        wp.X = Convert.ToUInt32(wp.X + WaypointService.ShuffleWaypointOffset(5, 15));
                    }
                }

                if (NearAiInterface != null)
                {
                    AiInterface.IsWalkingBack = NearAiInterface.IsWalkingBack;
                    AiInterface.NextAllowedMovementTime = NearAiInterface.NextAllowedMovementTime;
                    AiInterface.Ended = NearAiInterface.Ended;
                    AiInterface.Started = NearAiInterface.Started;

                    if (NearAiInterface.State == AiState.MOVING)
                    {
                        if (!AiInterface.IsWalkingBack)
                            AiInterface.CurrentWaypointID = NearAiInterface.CurrentWaypointID - 1;
                        else
                            AiInterface.CurrentWaypointID = NearAiInterface.CurrentWaypointID + 1;
                    }
                    else
                    {
                        AiInterface.CurrentWaypointID = NearAiInterface.CurrentWaypointID;
                        AiInterface.SetNextWaypoint(TCPManager.GetTimeStampMS());
                    }

                    NearAiInterface = null;
                }
            }

            public override void Update(long tick)
            {
                base.Update(tick);

                if (WaypointGUID > 0 && AiInterface != null && AiInterface.Waypoints != null && AiInterface.Waypoints.Count > 0)
                {
                    AiInterface.Update(tick);
                }
            }

            public override bool ReceiveDamage(Unit caster, uint damage, float hatredScale = 1f, uint mitigation = 0)
            {
                if (_keep.KeepStatus == KeepStatus.KEEPSTATUS_LOCKED)
                    return false;

                if (_flagGrd.Info.KeepLord)
                {
                    damage = (uint)(damage * _damageScaler);
                }

                return base.ReceiveDamage(caster, damage, hatredScale);
            }

            public override bool ReceiveDamage(Unit caster, AbilityDamageInfo damageInfo)
            {
                if (_keep.KeepStatus == KeepStatus.KEEPSTATUS_LOCKED)
                    return false;

                if (_flagGrd.Info.KeepLord)
                {
                    damageInfo.Mitigation += damageInfo.Damage * (1 - _damageScaler);
                    damageInfo.Damage *= _damageScaler;
                }

                return base.ReceiveDamage(caster, damageInfo);
            }

            public bool OnReceiveDamage(Object sender, object args)
            {
                _keep.OnKeepNpcAttacked(PctHealth);

                if (_flagGrd.Info.KeepLord)
                    _keep.OnKeepLordAttacked(PctHealth);

                if (_flagGrd.Creature.Spawn.Proto.CreatureType == (int)GameData.CreatureTypes.SIEGE)
                    _keep.OnKeepSiegeAttacked(PctHealth);

                return false;
            }

            protected override void SetDeath(Unit killer)
            {
                Health = 0;

                States.Add((byte)CreatureState.Dead);

                PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH, 12);
                Out.WriteUInt16(Oid);
                Out.WriteByte(1);
                Out.WriteByte(0);
                Out.WriteUInt16(killer.IsPet() ? killer.GetPet().Owner.Oid : killer.Oid);
                Out.Fill(0, 6);
                DispatchPacket(Out, true);

                AbtInterface.Cancel(true);
                ScrInterface.OnDie(this);


                BuffInterface.RemoveBuffsOnDeath();

                EvtInterface.Notify(EventName.OnDie, this, killer);

                Pet pet = killer as Pet;
                Player credited = (pet != null) ? pet.Owner : (killer as Player);

                if (credited != null)
                    HandleDeathRewards(credited);

                AiInterface.ProcessCombatEnd();

                SetRespawnTimer();

                EvtInterface.RemoveEventNotify(EventName.OnReceiveDamage, OnReceiveDamage);

                if (!_flagGrd.Info.KeepLord)
                {
                    _keep.OnKeepNpcAttacked(0);
                    return;
                }

                /*Log.Info(_keep.Info.Name, (_keep.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " keep lord slain by " + killer.Name + " of " + (killer.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"));*/

                if (_keep.Realm == killer.Realm)
                {
                    /*if (_flagGrd.Info.KeepLord)
                        Log.Info(_keep.Info.Name, (_keep.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " keep lord respawned.");*/
                    _logger.Debug($"Kill request from own realm {killer.Name} {_keep.Realm}");
                    _flagGrd.Creature = new KeepCreature(Spawn, _flagGrd, _keep);
                    Region.AddObject(_flagGrd.Creature, Spawn.ZoneId);
                    Destroy();
                }
                else
                {
                    _keep.OnLordKilled();
                }
            }

            protected override void SetRespawnTimer()
            {
                if (!_flagGrd.Info.KeepLord)
                {
                    if (Spawn.Proto.CreatureType == (int)GameData.CreatureTypes.SIEGE)
                        EvtInterface.AddEvent(RezUnit, (20 - (_keep.Rank * 3)) * 60000, 1); // 5-20 minute respawn period.
                    else
                        EvtInterface.AddEvent(RezUnit, 6 * 60000, 1); // 6 minute resurrection period.
                }
            }

            private static readonly Tuple<ushort, int>[] AbilityRankRequirements =
            {
                new Tuple<ushort, int>(13626, 0), // Cleave
                new Tuple<ushort, int>(14867, 0), // Iron Body
                new Tuple<ushort, int>(5575, 0), // Enfeebling Shout
                new Tuple<ushort, int>(5347, 1), // Bestial Flurry
                new Tuple<ushort, int>(14867, 2), // Shockwave
                new Tuple<ushort, int>(5576, 3), // Enfeeble
                new Tuple<ushort, int>(14900, 4), // Clip Tendon
                new Tuple<ushort, int>(13627, 4), // Armour Destruction
                new Tuple<ushort, int>(5568, 5), // Whirlwind
            };

            /// <summary>
            /// Scales the lord depending on enemy population.
            /// </summary>
            /// <param name="enemyPlayercount">Maximum number of enemies in short history.</param>
            public void ScaleLord(int playerCount)
            {
                if (AbtInterface.NPCAbilities == null)
                    return;

                float scaler;
                if (playerCount >= BattleFrontConstants.MAX_LORD_SCALER_POP)
                    scaler = 1f - BattleFrontConstants.MAX_LORD_SCALER;
                else
                    scaler = 1f - (BattleFrontConstants.MAX_LORD_SCALER * playerCount / BattleFrontConstants.MAX_LORD_SCALER_POP);
                _damageScaler = scaler;
            }



            public override void RezUnit()
            {
                // Keep lord dosent respawn;
                if (_flagGrd.Info.KeepLord)
                    return;

                _flagGrd.Creature = new KeepCreature(Spawn, _flagGrd, _keep);
                Region.AddObject(_flagGrd.Creature, Spawn.ZoneId);
                Destroy();
            }
        }
    }
}