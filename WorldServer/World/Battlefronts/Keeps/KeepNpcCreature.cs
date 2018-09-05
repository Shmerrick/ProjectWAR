using System;
using System.Xml.Schema;
using Common;
using FrameWork;
using GameData;
using WorldServer.World.BattleFronts;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer.World.BattleFronts.Keeps
{
    public class KeepNpcCreature
    {
        public RegionMgr Region;
        public KeepCreature Creature;
        public Keep_Creature Info;
        public Keep Keep;

        public KeepNpcCreature(RegionMgr region, Keep_Creature info, Keep keep)
        {
            Region = region;
            Info = info;
            Keep = keep;
        }

        public void SpawnGuard(Realms realm)
        {
            if (Creature != null)
            {
                Creature.Destroy();
                Creature = null;

                /*if (Info.KeepLord)
                    Log.Info(Keep.Info.Name, (Keep.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " keep lord disposed.");*/
            }

            if (realm != Realms.REALMS_REALM_NEUTRAL)
            {
                Creature_proto proto = CreatureService.GetCreatureProto(realm == Realms.REALMS_REALM_ORDER ? Info.OrderId : Info.DestroId);
                if (proto == null)
                {
                    Log.Error("KeepNPC", "No FlagGuard Proto");
                    return;
                }

                Creature_spawn spawn = new Creature_spawn();
                spawn.BuildFromProto(proto);
                spawn.WorldO = Info.O;
                /*
                if (proto.CreatureType == 32 && Info.Y < 70000)
                {
                    WorldMgr.Database.DeleteObject(Info);
                    Keep_Creature newInfo = new Keep_Creature
                    {
                        KeepId = Info.KeepId,
                        ZoneId = Info.ZoneId,
                        OrderId = Info.OrderId,
                        DestroId = Info.DestroId,
                        X = Info.X,
                        Y = ZoneMgr.CalculWorldPosition(WorldMgr.GetZone_Info(Info.ZoneId), 0, (ushort) Info.Y, 0).Y,
                        Z = Info.Z,
                        O = Info.O
                    };
                    WorldMgr.Database.AddObject(newInfo);

                    Info = newInfo;
                }*/
                spawn.WorldY = Info.Y;
                spawn.WorldZ = Info.Z;
                spawn.WorldX = Info.X;
                spawn.ZoneId = Info.ZoneId;

                Creature = new KeepCreature(spawn, this, Keep);

                /*if (Info.KeepLord)
                    Log.Info(Keep.Info.Name, (Keep.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " keep lord spawned.");*/
                Region.AddObject(Creature, spawn.ZoneId);
            }
        }

        public class KeepCreature : Creature
        {
            private readonly KeepNpcCreature _flagGrd;
            private readonly Keep _keep;
            /// <summary>Incoming damage scaler from 0.25 to 1<summary>
            private volatile float _damageScaler = 1f;

            public KeepCreature(Creature_spawn spawn, KeepNpcCreature flagGrd, Keep keep) : base(spawn)
            {
                _keep = keep;
                _flagGrd = flagGrd;
                IsKeepLord = flagGrd.Info.KeepLord;

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
            }

            public override bool ReceiveDamage(Unit caster, uint damage, float hatredScale = 1f, uint mitigation = 0)
            {
                if (_keep.KeepStatus == KeepStatus.KEEPSTATUS_LOCKED)
                    return false;

                if (_flagGrd.Info.KeepLord)
                {
                    if (_keep.LastMessage < Keep.KeepMessage.Inner0)
                        return false;
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
                    if (_keep.LastMessage < Keep.KeepMessage.Inner0)
                        return false;

                    damageInfo.Mitigation += damageInfo.Damage * (1 - _damageScaler);
                    damageInfo.Damage *= _damageScaler;
                }

                return base.ReceiveDamage(caster, damageInfo);
            }

            public bool OnReceiveDamage(Object sender, object args)
            {
                if (_flagGrd.Info.KeepLord || _flagGrd.Creature.Spawn.Proto.CreatureType == (int)GameData.CreatureTypes.SIEGE)
                {
                    _keep.ResetSafeTimer();

                    if (_flagGrd.Info.KeepLord)
                        _keep.OnKeepLordAttacked(PctHealth);
                }

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
                    return;

                /*Log.Info(_keep.Info.Name, (_keep.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " keep lord slain by " + killer.Name + " of " + (killer.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"));*/

                if (_keep.Realm == killer.Realm)
                {
                    /*if (_flagGrd.Info.KeepLord)
                        Log.Info(_keep.Info.Name, (_keep.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " keep lord respawned.");*/
                    _flagGrd.Creature = new KeepCreature(Spawn, _flagGrd, _keep);
                    Region.AddObject(_flagGrd.Creature, Spawn.ZoneId);
                    Destroy();
                }

                else
                    _keep.OnKeepLordDied();
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

            public void ScaleLordVP(int vp)
            {
                var tH = this.TotalHealth;
                uint tHBuffed = 0;
                bool is100wobuff = false;
                switch (vp)
                {
                    case 0:

                        //200% buff(totalhealth * 3)
                        tHBuffed = (tH * 3);
                        is100wobuff = (this.Health == tH) ? true : false;
                        if (is100wobuff)
                        {
                            this.Health = tHBuffed;
                        }

                        break;

                    case 2500:
                        tHBuffed = (tH * 2);
                        is100wobuff = (this.Health == tH * 3 || this.Health > tHBuffed)
                            ? true
                            : false;
                        if (is100wobuff)
                        {
                            this.Health = tHBuffed;
                        }

                        break;

                    case 4000:
                        tHBuffed = (tH);
                        is100wobuff = (this.Health == tH * 2 || this.Health > tHBuffed)
                            ? true
                            : false;
                        if (is100wobuff)
                        {
                            this.Health = tHBuffed;
                        }

                        break;
                }
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