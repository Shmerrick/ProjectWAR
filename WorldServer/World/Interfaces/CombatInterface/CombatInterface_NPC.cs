using FrameWork;
using GameData;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class CombatInterface_Npc : CombatInterface
    {
        /// <summary>The player who first attacked this monster.</summary>
        public Player FirstStriker { get; set; }

        public Unit CurrentTarget { get; protected set; }

        protected AIInterface AIInterface;

        public bool IsFighting => IsInCombat || (AIInterface != null && AIInterface.State == AiState.FIGHTING);

        public override bool Load()
        {
            AIInterface = UnitOwner.AiInterface;
            CurrentTarget = null;
            return base.Load();
        }

        public override void Update(long tick)
        {
            if (CombatLeaveTime < tick && IsInCombat)
            { 
                LeaveCombat();
                Creature p = UnitOwner as Creature;
                if (p != null && p.Spawn != null)
                {
                    byte animId = p.Spawn.Emote;
                    AnimateCreature(p, animId);
                }
            }
        }

        public virtual void TryAttack()
        {
            Creature p = UnitOwner as Creature;

            if (p == null)
                return;

            //Added this to allow clearing NPC emote if combat starts
            if (p.Spawn.Emote != 0)
                AnimateCreature(p, 0);

            long tick = TCPManager.GetTimeStampMS();

            if (NextAttackTime >= tick || (CurrentTarget != null && CurrentTarget.IsStaggered))
                return;

            if (p.Ranged > 0)
            {
                if (!p.IsInCastRange(CurrentTarget, p.Ranged))
                    return;
                p.Strike(CurrentTarget, p.IsInCastRange(CurrentTarget, (uint)p.BaseRadius) ? EquipSlot.MAIN_HAND : EquipSlot.RANGED_WEAPON);
                NextAttackTime = tick + 1500 + StaticRandom.Instance.Next(0, 1000);
            }

            else
            {
                if (!p.IsInCastRange(CurrentTarget, (uint)(0.5f + p.BaseRadius)))
                    return;
                p.Strike(CurrentTarget);
                NextAttackTime = tick + 1500 + StaticRandom.Instance.Next(0, 1000);
            }
        }

        #region Targets

        /// <summary>Sets the current target from an object ID. Used by players and siege.</summary>
        public override void SetTarget(ushort oid, TargetTypes targetType)
        {
            if (Targets[(int)targetType] == oid)
                return;
            Targets[(int)targetType] = oid;

            CurrentTarget = (Unit) _Owner.Region?.GetObject(oid);
        }

        public void SetTarget(Unit target, TargetTypes targetType)
        {
            if (target == null)
            {
                Targets[(int) targetType] = 0;
                CurrentTarget = null;
            }

            else
            {
                Targets[(int) targetType] = target.Oid;

                CurrentTarget = target;
            }
        }

        public override bool HasTarget(TargetTypes type) => CurrentTarget != null;

        public override Unit GetTarget(TargetTypes type) => CurrentTarget;

        public override Unit GetCurrentTarget() => CurrentTarget;

        #endregion

        #region Events

        public override void OnAttacked(Unit attacker)
        {
            AIInterface.ProcessAttacked(attacker);

            RefreshCombatTimer();

            if (FirstStriker == null)
                FirstStriker = attacker as Player;
        }

        public override void OnTakeDamage(Unit fighter, uint damage, float hatredMod, uint mitigation = 0)
        {
            AIInterface.ProcessTakeDamage(fighter, damage, hatredMod, mitigation);

            RefreshCombatTimer();

            // RB   6/13/2016   Send damage as an argument
            _Owner.EvtInterface.Notify(EventName.OnReceiveDamage, fighter, damage);
        }

        public override void OnDealDamage(Unit victim, uint damageCount)
        {
            AIInterface.ProcessInflictDamage(victim);

            RefreshCombatTimer();

            _Owner.EvtInterface.Notify(EventName.OnDealDamage, victim, damageCount);
        }

        public override void OnDealHeal(Unit target, uint damageCount)
        {
            _Owner.EvtInterface.Notify(EventName.OnDealHeal, target, damageCount);
        }

        public override void OnTakeHeal(Unit caster)
        {
            _Owner.EvtInterface.Notify(EventName.OnReceiveHeal, caster, null);
        }

        public override void OnTargetDie(Unit victim)
        {
            _Owner.EvtInterface.Notify(EventName.OnTargetDie, victim, null);
        }

        #endregion

        public override bool CanAttackPlayer(Player plr)
        {
            return true;
        }

        #region NPC Anim

        public static void AnimateCreature(Creature creature, byte animId)
        {

            var Out = new PacketOut((byte)Opcodes.F_ANIMATION);

            Out.WriteUInt16(creature.Oid);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteUInt16((ushort)animId);

            creature.DispatchPacket(Out, true);
        }


        #endregion
    }
}