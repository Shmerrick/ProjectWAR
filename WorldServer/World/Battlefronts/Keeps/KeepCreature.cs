using Common;
using FrameWork;
using GameData;
using NLog;
using System;
using SystemData;
using Common.Database.World.Battlefront;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Battlefronts.Keeps
{
    public class KeepCreature : Creature
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public KeepNpcCreature FlagGuard;
        private readonly BattleFrontKeep _keep;
        /// <summary>Incoming damage scaler from 0.25 to 1<summary>
        private volatile float _damageScaler = 1f;
        public AIInterface NearAiInterface = null;

        public KeepCreature(Creature_spawn spawn, KeepNpcCreature flagGuard, BattleFrontKeep keep) : base(spawn)
        {
            _keep = keep;
            FlagGuard = flagGuard;
            IsKeepLord = flagGuard.Info.KeepLord;
            IsPatrol = flagGuard.Info.IsPatrol;

            EvtInterface.AddEventNotify(EventName.OnReceiveDamage, OnReceiveDamage);
        }

        public KeepNpcCreature returnflag()
        {
            return FlagGuard;
        }

        public override void OnLoad()
        {
            base.OnLoad();

            // ScaleLord(_keep.Rank);
            // buff lord with multipler 3 //TODO: rework needed (morale abilities does dmg through the scaler etc)
            // Upgrade keep lord health
            if (IsKeepLord)
                Health *= 4;
        }

        public override void Update(long msTick)
        {
            base.Update(msTick);

            if (WaypointGUID > 0 && AiInterface != null && AiInterface.Waypoints != null && AiInterface.Waypoints.Count > 0)
            {
                AiInterface.Update(msTick);
            }
        }

        public override bool ReceiveDamage(Unit caster, uint damage, float hatredScale = 1f, uint mitigation = 0)
        {
            if (_keep.KeepStatus == KeepStatus.KEEPSTATUS_LOCKED)
                return false;

            if (FlagGuard.Info.KeepLord)
            {
                damage = (uint)(damage * _damageScaler);
            }

            return base.ReceiveDamage(caster, damage, hatredScale);
        }

        

        public override bool ReceiveDamage(Unit caster, AbilityDamageInfo damageInfo)
        {
            if (_keep.KeepStatus == KeepStatus.KEEPSTATUS_LOCKED)
                return false;

            if (FlagGuard.Info.KeepLord)
            {
                damageInfo.Mitigation += damageInfo.Damage * (1 - _damageScaler);
                damageInfo.Damage *= _damageScaler;
            }

            return base.ReceiveDamage(caster, damageInfo);
        }

        public bool OnReceiveDamage(Object sender, object args)
        {
            _keep.OnKeepNpcAttacked(PctHealth);

            if (FlagGuard.Info.KeepLord)
                _keep.OnKeepLordAttacked(PctHealth);

            if (FlagGuard.Creature.Spawn.Proto.CreatureType == (int)GameData.CreatureTypes.SIEGE)
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

            if (!FlagGuard.Info.KeepLord)
            {
                _keep.OnKeepNpcAttacked(0);
                return;
            }

            /*Log.Info(_keep.Info.Name, (_keep.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " keep lord slain by " + killer.Name + " of " + (killer.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"));*/

            if (_keep.Realm == killer.Realm)
            {
                /*if (FlagGuard.Info.KeepLord)
                    Log.Info(_keep.Info.Name, (_keep.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " keep lord respawned.");*/
                _logger.Debug($"Kill request from own realm {killer.Name} {_keep.Realm}");
                FlagGuard.Creature = new KeepCreature(Spawn, FlagGuard, _keep);
                Region.AddObject(FlagGuard.Creature, Spawn.ZoneId);
                Destroy();
            }
            else
            {
                _keep.OnLordKilled();
            }
        }

        protected override void SetRespawnTimer()
        {
            if (!FlagGuard.Info.KeepLord)
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
        public void ScaleLord(int defenderPlayerCount)
        {
            if (AbtInterface.NPCAbilities == null)
                return;

            var oldRank = this.Rank;

            if (defenderPlayerCount > 40)
            {
                this.Rank = 1;
            }
            else
            {
                if (defenderPlayerCount > 20)
                {
                    this.Rank = 2;
                }
                else
                {
                    this.Rank = 3;
                }
            }


            if (oldRank > this.Rank)
            {
                Say("I grow weaker!", ChatLogFilters.CHATLOGFILTERS_SHOUT);
            }
            else if (oldRank < this.Rank)
            {
                Say("I grow stronger!", ChatLogFilters.CHATLOGFILTERS_SHOUT);
            }
            _logger.Trace($"Lord Rank scaled to {this.Rank} population {defenderPlayerCount}");
        }



        public override void RezUnit()
        {
            // Keep lord dosent respawn;
            if (FlagGuard.Info.KeepLord)
                return;

            FlagGuard.Creature = new KeepCreature(Spawn, FlagGuard, _keep);
            Region.AddObject(FlagGuard.Creature, Spawn.ZoneId);
            Destroy();
        }
    }
}
