using SystemData;
using FrameWork;
using GameData;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class CombatInterface_Player : CombatInterface
    {
        private Player _player;

        public bool MoveAndShoot { get; set; }

        private float _autoAttackSpeedBonus
            => _player.StsInterface.GetBonusStat(Stats.AutoAttackSpeed) / 100f;

        private float _autoAttackSpeedReduction
            => _player.StsInterface.GetReducedStat(Stats.AutoAttackSpeed) / 100f;

        public override void SetOwner(Object owner)
        {
            _Owner = owner;
            UnitOwner = _Owner as Unit;
            _player = _Owner as Player;
            CreditedPlayer = _player;
        }

        public override void Update(long tick)
        {
            if (IsInCombat && CombatLeaveTime < tick)
            {
                LeaveCombat();
                return;
            }

            if (!_player.CanAutoAttack)
                return;

            DisablePvp(tick, false);

            if (!IsAttacking || NextAttackTime > tick)
                return;

            Unit target = GetCurrentTarget();

            if (target == null || !CanAttack(_player, target))
                return;

            NextAttackTime += 100;

            if (target.IsStaggered || _player.IsMounted || _player.AbtInterface.IsCasting() || !_player.IsObjectInFront(target, 140))
                return;

            if (_player.IsInCastRange(target, 5))
            {
                _player.Strike(target);
                NextAttackTime = tick + (ushort)((_player.ItmInterface.GetAttackTime(EquipSlot.MAIN_HAND) * 10) / (1 + _autoAttackSpeedBonus - _autoAttackSpeedReduction));
            }
            else if ((!_player.IsMoving || MoveAndShoot) && 
                (_player.Info.CareerLine != 8 || _player.CrrInterface.CareerResource != 1) && // Squig Armor
                (_player.Info.CareerLine != 18 || _player.CrrInterface.CareerResource != 3) && // Shadow Warrior Assault Stance
                _player.ItmInterface.GetItemInSlot((ushort)EquipSlot.RANGED_WEAPON) != null && 
                _player.IsInCastRange(target, (uint)(90 + _player.StsInterface.GetBonusStat(Stats.Range)))) // Ranged
            {
                if (!_player.LOSHit(target))
                    NextAttackTime += 1000;
                else
                {
                    _player.Strike(target, EquipSlot.RANGED_WEAPON);
                    NextAttackTime = tick + (ushort)((_player.ItmInterface.GetAttackTime(EquipSlot.RANGED_WEAPON) * 10) / (1 + _autoAttackSpeedBonus - _autoAttackSpeedReduction));
                }
            }
        }

        #region Targets

        public void SendTarget(ushort targetID, SwitchTargetTypes type)
        {
            var t = (byte)type;
            var Out = new PacketOut((byte)Opcodes.F_SET_TARGET, 6);
            Out.WriteUInt16(targetID); //targetID
            Out.WriteUInt16(_player.Oid);
            Out.WriteByte((byte)type);
            Out.WriteByte(0);
            _player.DispatchPacket(Out, false);

          //  _player.DebugMessage("Send Target " + type);
        }
        public override void SetTarget(ushort oid, TargetTypes targetType)
        {

            if (targetType == TargetTypes.TARGETTYPES_TARGET_ALLY && oid == 0)
                SendTarget(oid, SwitchTargetTypes.FRIENDLY_CLEAR);
            else if (targetType == TargetTypes.TARGETTYPES_TARGET_ALLY && oid != 0)
                SendTarget(oid, SwitchTargetTypes.FRIENDLY_SET);
            if (targetType == TargetTypes.TARGETTYPES_TARGET_ENEMY && oid == 0)
                SendTarget(oid, SwitchTargetTypes.ENEMY_CEAR);
            else if (targetType == TargetTypes.TARGETTYPES_TARGET_ENEMY && oid != 0)
                SendTarget(oid, SwitchTargetTypes.ENEMY_SET);
 
            if (targetType == 0)    // this is needed cause if you select an frendly target then a enemytarget the enemy target is type 0
            {
                if (oid == Targets[2])
                    return;
                Object newTarget = _Owner.Region.GetObject(oid);
                var newTargetUnit = newTarget as Unit;
                if (newTargetUnit != null)
                {
                    if (newTarget is Creature || newTarget is Player)
                    {
                        if (((Player)_Owner).Faction != newTargetUnit.Faction)
                            Targets[2] = oid;
                    }
                }
            }
            else
            {
                if (Targets[(int)targetType] == oid)
                    return;
                Targets[(int)targetType] = oid;
            }

            if ((int)targetType == 0 || (int)targetType == 2)
                IsAttacking = false;

            if (oid != 0)
            {
                Unit unit = _Owner.Region.GetObject(oid) as Unit;
                unit?.BuffInterface.SendBuffList(_player, false);
            }
        }

        public override bool HasTarget(TargetTypes type)
        {
            return Targets[(int)type] != 0;
        }
        public override Unit GetTarget(TargetTypes type)
        {
            if (_Owner?.Region == null)
                return null;

            Unit u = null;
            ushort oid = Targets[(int)type];
            if (oid != 0)
                u = _Owner.Region.GetObject(oid) as Unit;
            return u;
        }
        public override Unit GetCurrentTarget()
        {
            ushort oid;

            for (int i = 0; i < 7; ++i)
            {
                oid = Targets[i];
                if (oid != 0)
                {
                    Unit u = _Owner.Region.GetObject(oid) as Unit;

                    if (u != null)
                        return u;
                }
            }

            return null;
        }

        #endregion

        #region Events

        public override void OnAttacked(Unit attacker)
        {
            RefreshCombatTimer();
        }

        public override void OnTakeDamage(Unit fighter, uint damage, float hatredMod, uint mitigation = 0)
        {
            RefreshCombatTimer();

            _Owner.EvtInterface.Notify(EventName.OnReceiveDamage, fighter, null);

            if (fighter is Player && _Owner != fighter)
                ResetPvpTime();

            LastInteractionTime = TCPManager.GetTimeStampMS();
        }

        public override void OnDealDamage(Unit victim, uint damageCount)
        {
            RefreshCombatTimer();

            _Owner.EvtInterface.Notify(EventName.OnDealDamage, victim, damageCount);

            if (victim is Player && _Owner != victim)
                ResetPvpTime();

            LastInteractionTime = TCPManager.GetTimeStampMS();
        }

        public override void OnDealHeal(Unit target, uint damageCount)
        {
            if (target != _Owner && target.CbtInterface.IsInCombat)
                RefreshCombatTimer();
            _Owner.EvtInterface.Notify(EventName.OnDealHeal, target, damageCount);

            var player = target as Player;
            if (player != null && player.CbtInterface.IsPvp)
                ResetPvpTime();

            if (target != _Owner && IsInCombat && !(target is Pet))
                LastInteractionTime = TCPManager.GetTimeStampMS();
        }

        public override void OnTakeHeal(Unit caster)
        {
            _Owner.EvtInterface.Notify(EventName.OnReceiveHeal, caster, null);

            if (caster != _Owner && IsInCombat)
                LastInteractionTime = TCPManager.GetTimeStampMS();
        }

        public override void OnTargetDie(Unit victim)
        {
            _Owner.EvtInterface.Notify(EventName.OnTargetDie, victim, null);
        }

        #endregion

        #region PVP

        public long NextAllowedDisable;

        public void TogglePvPFlag()
        {
            if (IsPvp)
            {
                if (NextAllowedDisable == 0)
                    NextAllowedDisable = TCPManager.GetTimeStampMS() + 10 * 60 * 1000;

                _Owner.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.TEXT_RVR_UNFLAG);
            }
            else
                EnablePvp();
        }

        public void EnablePvp()
        {
            if (_player.Companion != null && _player.Companion.IsVanity)
            {
                _player.Companion.RemoveVanityPet();
                _player.SendClientMessage("Your vanity pet has been dismissed as you are now marked for RvR.", ChatLogFilters.CHATLOGFILTERS_ABILITY_ERROR);
            }

            if (IsPvp)
                return;

            NextAllowedDisable = 0;
            IsPvp = true;
            _Owner.GetPlayer().SetPVPFlag(true);
            _Owner.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.TEXT_RVR_FLAG);
        }

        public void DisablePvp(long tick, bool force)
        {
            if (!IsPvp) 
                return;

            if (!force && (NextAllowedDisable >= tick || NextAllowedDisable == 0 || _player.CurrentArea == null || _player.CurrentArea.IsRvR))
                return;

            NextAllowedDisable = 0;
            IsPvp = false;
            _player.SetPVPFlag(false);
            _player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.TEXT_YOU_ARE_NO_LONGER_RVR_FLAGGED);
        }

        public void ResetPvpTime()
        {
            EnablePvp();

            NextAllowedDisable = 0;
        }

        #endregion

        public override bool CanAttackPlayer(Player plr)
        {
            return plr.CbtInterface.IsPvp;
        }
    }
}