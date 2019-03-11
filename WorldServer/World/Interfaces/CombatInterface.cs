using FrameWork;
using GameData;
using WorldServer.World.Objects;

namespace WorldServer.World.Interfaces
{
    public class AggroInfo
    {
        public AggroInfo(ushort oid)
        {
            Oid = oid;
        }

        public ushort Oid;

        public ulong Hatred;
        public ulong DamageReceived;

        public ulong HealingDealt;
        public ulong HealingReceived;
        public ulong HealingTotal;
        public long HealingReceivedTime = 0;

        public ulong GetHate()
        {
            return Hatred;
        }

        public void ResetHeals()
        {
            HealingDealt = 0;
            HealingReceived = 0;
            HealingTotal = 0;
            HealingReceivedTime = 0;
        }
        public void ResetDamage()
        {
            Hatred = 0;
            DamageReceived = 0;
        }
        public void Reset()
        {
            ResetDamage();
            ResetHeals();
        }
    }

    public enum AiState
    {
        STANDING = 0,
        MOVING = 1,
        FIGHTING = 2,
        BACK = 3,
    }

    public abstract class CombatInterface : BaseInterface
    {
        protected Unit UnitOwner;

        /// <summary>
        /// A player who will receive credit for damage inflicted by the owner of this interface.
        /// </summary>
        public Player CreditedPlayer;
        protected long NextAttackTime;
        public bool IsPvp { get; set; }

        public bool IsInCombat { get; protected set; }
        public long CombatLeaveTime { get; protected set; }
        public bool IsAttacking { get; set; }

        public ushort[] Targets = new ushort[7];

        public override void SetOwner(Object owner)
        {
            _Owner = owner;
            UnitOwner = (Unit)_Owner;
        }

        public static CombatInterface GetInterfaceFor(Unit unit)
        {
            if (unit is Player)
                return new CombatInterface_Player();
            if (unit is Pet)
                return new CombatInterface_Pet();
            return new CombatInterface_Npc();
        }

        #region Combat Entry/Exit

        public void RefreshCombatTimer()
        {
            if (!IsInCombat)
            {
                IsInCombat = true;
                _Owner.EvtInterface.Notify(EventName.OnEnterCombat, _Owner, null);
                UnitOwner.DispatchUpdateState((byte)StateOpcode.Combat, 1, 0);

                // Clearing heal hate on combat enter
                //if (CreditedPlayer != null)
                    //CreditedPlayer.HealAggros.Clear();
            }

            #if DEBUG && ABILITY_DEVELOPMENT
            CombatLeaveTime = TCPManager.GetTimeStampMS() + 1000;
            #else
            CombatLeaveTime = TCPManager.GetTimeStampMS() + 10000;
            #endif
        }

        public void LeaveCombat()
        {
            if (IsInCombat)
            {
                IsInCombat = false;
                _Owner.EvtInterface.Notify(EventName.OnLeaveCombat, _Owner, null);

                UnitOwner.ClearTrackedDamage();

                // We are clearing aggros from NPC leaving combat
                // UnitOwner.AiInterface.CurrentBrain.Aggros.Clear();

                UnitOwner.DispatchUpdateState((byte)StateOpcode.Combat, 0, 0);
            }

            IsAttacking = false;

            // Clearing heal hate on combat leave
            if (CreditedPlayer != null)
                CreditedPlayer.HealAggros = new System.Collections.Generic.Dictionary<ushort, AggroInfo>();
        }

        public override void Update(long tick)
        {
            if (IsInCombat && CombatLeaveTime < tick)
                LeaveCombat();
        }

        #endregion

        #region Targets

        public abstract void SetTarget(ushort oid, TargetTypes targetType);
        public abstract bool HasTarget(TargetTypes type);
        public abstract Unit GetTarget(TargetTypes type);
        public abstract Unit GetCurrentTarget();

        #endregion

        public long LastInteractionTime { get; protected set; }

        #region Events

        public abstract void OnAttacked(Unit attacker);
        public abstract void OnTakeDamage(Unit fighter, uint damage, float hatredMod, uint mitigation = 0);
        public abstract void OnDealDamage(Unit victim, uint damage);
        public abstract void OnDealHeal(Unit target, uint damageCount);
        public abstract void OnTakeHeal(Unit caster);
        public abstract void OnTargetDie(Unit victim);

        #endregion

        #region Defense Timers

        private const long DEFENSE_TIMEOUT = 5000;

        // Block, Parry, Dodge, Disrupt
        private readonly long[] _defenseTimers = new long[4];
        private readonly long[] _defendedAgainstTimers = new long[4];

        public void SetDefenseTimer(byte defenseType)
        {
            _defenseTimers[defenseType - 4] = TCPManager.GetTimeStampMS();
        }

        public void SetDefendedAgainstTimer(byte defenseType)
        {
            _defendedAgainstTimers[defenseType - 4] = TCPManager.GetTimeStampMS();
        }

        public bool HasDefended(int defenseFlags)
        {
            byte count = 0;

            while (defenseFlags != 0)
            {
                if ((defenseFlags & 1) > 0 && TCPManager.GetTimeStampMS() - _defenseTimers[count] <= DEFENSE_TIMEOUT)
                    return true;
                ++count;
                defenseFlags = defenseFlags >> 1;
            }

            return false;
        }

        public bool WasDefendedAgainst(int defenseFlags)
        {
            byte count = 0;

            while (defenseFlags != 0)
            {
                if ((defenseFlags & 1) > 0 && TCPManager.GetTimeStampMS() - _defendedAgainstTimers[count] <= DEFENSE_TIMEOUT)
                    return true;
                ++count;
                defenseFlags = defenseFlags >> 1;
            }

            return false;
        }
        #endregion

        public static TargetTypes GetTargetType(Unit a, Unit b)
        {
            if (b == null)
                return TargetTypes.TARGETTYPES_TARGET_NONE;

            if (a == b)
                return TargetTypes.TARGETTYPES_TARGET_SELF;

            if (IsEnemy(a, b))
                return TargetTypes.TARGETTYPES_TARGET_ENEMY;

            if (IsFriend(a, b))
                return TargetTypes.TARGETTYPES_TARGET_ALLY;

            return TargetTypes.TARGETTYPES_TARGET_NONE;
        }

        public static bool IsEnemy(Unit a, Unit b)
        {
            if (a == null || b == null)
                return false;

            if (a.Faction == 64 && b.Faction == 64 && a is Player && b is Player)
                return true;

            return a.Realm != b.Realm;
        }

        public static bool IsFriend(Unit a, Unit b)
        {
            if (a == null || b == null)
                return false;

            if (a == b)
                return true;

            if (a.Faction == 64 || b.Faction == 64)
                return false;

            if (b is GameObject)
                return false;

            return a.Realm == b.Realm;
        }

        public static bool CanAttack(Unit attacker, Unit victim)
        {
            if (attacker == null || victim == null)
                return false;

            if (victim is GameObject && victim.GetGameObject().IsAttackable == 0)
                return false;

            if (victim is GameObject && (victim.Realm == Realms.REALMS_REALM_NEUTRAL || victim.Realm == attacker.Realm))
                return false;

            if (victim is GameObject && attacker is Creature && !(attacker is Pet) && !(attacker is Siege))
                return false;

            if (attacker.IsDisposed || victim.IsDisposed)
                return false;

            if (attacker == victim)
                return false;

            if (attacker.IsDead || victim.IsDead)
                return false;

            if (!IsEnemy(attacker, victim))
                return false;

            if (!attacker.IsInWorld() || !victim.IsInWorld())
                return false;

            if (!attacker.IsVisible || attacker.IsInvulnerable)
                return false;

            if (!victim.IsVisible || victim.IsInvulnerable)
                return false;

            Player player = victim as Player;

            return player == null || attacker.CbtInterface.CanAttackPlayer(player);
        }

        public void ResetAttackTimer()
        {
            NextAttackTime = 0;
        }

        public abstract bool CanAttackPlayer(Player plr);
    }
}