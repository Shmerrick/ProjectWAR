using System.Collections.Generic;
using Common;
using FrameWork;
using WorldServer.NetWork;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class TacticsInterface : BaseInterface
    {
        private Player _myPlayer;

        /// <summary> A list of all tactic entries the player has active.</summary>
        private readonly List<ushort> _activeTactics = new List<ushort>();
        /// <summary> A list of all tactic entries the player has which have applied modifiers.</summary>
        private readonly HashSet<ushort> _modifyingTactics = new HashSet<ushort>();
        /// <summary> A list of all the active tactic buffs.</summary>
        private readonly List<NewBuff> _activeBuffs = new List<NewBuff>();

        /// <summary>Modifiers which run on all abilities as they begin casting.</summary>
        private readonly List<AbilityModifier> _generalPreCastModifiers = new List<AbilityModifier>();
        /// <summary>Modifiers which run on all abilities as they finish casting.</summary>
        private readonly List<AbilityModifier> _generalModifiers = new List<AbilityModifier>();
        /// <summary>Modifiers which run on all buffs as they are created.</summary>
        private readonly List<AbilityModifier> _generalBuffModifiers = new List<AbilityModifier>();

        /// <summary>Modifiers which run on abilities within a certain mastery tree as they begin casting.</summary>
        private readonly Dictionary<ushort, List<AbilityModifier>> _speclinePreCastModifiers = new Dictionary<ushort, List<AbilityModifier>>();
        /// <summary>Modifiers which run on abilities within a certain mastery tree as they finish casting.</summary>
        private readonly Dictionary<ushort, List<AbilityModifier>> _speclineModifiers = new Dictionary<ushort, List<AbilityModifier>>();
        /// <summary>Modifiers which run on buffs within a certain mastery tree as they are created.</summary>
        private readonly Dictionary<ushort, List<AbilityModifier>> _speclineBuffModifiers = new Dictionary<ushort, List<AbilityModifier>>();

        /// <summary>Modifiers which run on abilities of a particular entry as they begin casting.</summary>
        private readonly Dictionary<ushort, List<AbilityModifier>> _abilityPreCastModifiers = new Dictionary<ushort, List<AbilityModifier>>();
        /// <summary>Modifiers which run on abilities of a particular entry as they finish casting.</summary>
        private readonly Dictionary<ushort, List<AbilityModifier>> _abilityModifiers = new Dictionary<ushort, List<AbilityModifier>>();
        /// <summary>Modifiers which run on buffs of a particular entry as they are created.</summary>
        private readonly Dictionary<ushort, List<AbilityModifier>> _buffModifiers = new Dictionary<ushort, List<AbilityModifier>>();

        #region Tactic Add/Remove

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_TACTICS, (int)eClientState.Playing, "onTACTICS")]
        public static void F_TACTICS(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null || !cclient.Plr.IsInWorld())
                return;

            Player plr = cclient.Plr;

            List<ushort> tactics = new List<ushort>();

            byte unk1 = packet.GetUint8(); // always seems to be 3?
            byte numberofTactics = packet.GetUint8();

            for (int i = 0; i < numberofTactics; i++)
                tactics.Add(packet.GetUint16());


            plr.TacInterface.HandleTactics(tactics);
        }

        public override void SetOwner(Object owner)
        {
            _Owner = owner;
            _myPlayer = owner as Player;
        }

        public void LoadTactics()
        {
            var list = new List<ushort>();

            if (_myPlayer._Value.Tactic1 != 0 && !list.Contains(_myPlayer._Value.Tactic1))
                list.Add(_myPlayer._Value.Tactic1);
            if (_myPlayer._Value.Tactic2 != 0 && !list.Contains(_myPlayer._Value.Tactic2))
                list.Add(_myPlayer._Value.Tactic2);
            if (_myPlayer._Value.Tactic3 != 0 && !list.Contains(_myPlayer._Value.Tactic3))
                list.Add(_myPlayer._Value.Tactic3);
            if (_myPlayer._Value.Tactic4 != 0 && !list.Contains(_myPlayer._Value.Tactic4))
                list.Add(_myPlayer._Value.Tactic4);

            HandleTactics(list);
        }

        public void ReloadTactics()
        {
            List<ushort> tacList = new List<ushort>();
            bool sendTacticUpdate = false;

            if (_myPlayer._Value.Tactic1 != 0 && !tacList.Contains(_myPlayer._Value.Tactic1))
                tacList.Add(_myPlayer._Value.Tactic1);
            if (_myPlayer._Value.Tactic2 != 0 && !tacList.Contains(_myPlayer._Value.Tactic2))
                tacList.Add(_myPlayer._Value.Tactic2);
            if (_myPlayer._Value.Tactic3 != 0 && !tacList.Contains(_myPlayer._Value.Tactic3))
                tacList.Add(_myPlayer._Value.Tactic3);
            if (_myPlayer._Value.Tactic4 != 0 && !tacList.Contains(_myPlayer._Value.Tactic4))
                tacList.Add(_myPlayer._Value.Tactic4);

            int maxAllowedTactics = _myPlayer.AdjustedLevel / 10;

            while (tacList.Count > maxAllowedTactics)
            {
                tacList.RemoveAt(tacList.Count - 1);
                sendTacticUpdate = true;
            }

            foreach (NewBuff buff in _activeBuffs)
            {
                buff.BuffHasExpired = true;

                if (_modifyingTactics.Contains(buff.Entry))
                {
                    _modifyingTactics.Remove(buff.Entry);

                    List<ushort> toRemove = new List<ushort>();

                    if (AbilityMgr.HasPreCastModifiers(buff.Entry))
                    {
                        foreach (AbilityModifier mod in AbilityMgr.GetAbilityPreCastModifiers(buff.Entry))
                        {
                            if (mod.Affecting == 0)
                                _generalPreCastModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                            else if (mod.Affecting <= 3)
                                _speclinePreCastModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                            else
                                toRemove.Add(mod.Affecting);
                        }

                        foreach (ushort rem in toRemove)
                            _abilityPreCastModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);

                        toRemove.Clear();
                    }

                    if (AbilityMgr.HasModifiers(buff.Entry))
                    {
                        foreach (AbilityModifier mod in AbilityMgr.GetAbilityModifiers(buff.Entry))
                        {
                            if (mod.Affecting == 0)
                                _generalModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                            else if (mod.Affecting <= 3)
                                _speclineModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                            else
                                toRemove.Add(mod.Affecting);
                        }

                        foreach (ushort rem in toRemove)
                            _abilityModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);

                        toRemove.Clear();
                    }

                    if (AbilityMgr.HasBuffModifiers(buff.Entry))
                    {
                        foreach (AbilityModifier mod in AbilityMgr.GetBuffModifiers(buff.Entry))
                        {
                            if (mod.Affecting == 0)
                                _generalBuffModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                            else if (mod.Affecting <= 3)
                                _speclineBuffModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                            else
                                toRemove.Add(mod.Affecting);
                        }

                        foreach (ushort rem in toRemove)
                            _buffModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);
                    }
                }

                _activeTactics.Remove(buff.Entry);
            }

            _activeBuffs.Clear();

            foreach (ushort id in tacList)
            {
                if (id == 0 || _activeTactics.Contains(id))
                    continue;
                BuffInfo b = AbilityMgr.GetBuffInfo(id);

                if (b == null)
                {
                    _myPlayer.SendClientMessage("Nonexistent tactic: " + id + " " + AbilityMgr.GetAbilityNameFor(id));
                    continue;
                }

                if (!_myPlayer.AbtInterface.IsValidTactic(id))
                {
                    _myPlayer.SendClientMessage("Invalid tactic: " + id + " " + AbilityMgr.GetAbilityNameFor(id));
                    sendTacticUpdate = true;
                    continue;
                }

                if (!string.IsNullOrEmpty(b.AuraPropagation))
                    _myPlayer.BuffInterface.QueueBuff(new BuffQueueInfo(_myPlayer, _myPlayer.AbtInterface.GetMasteryLevelFor(AbilityMgr.GetMasteryTreeFor(b.Entry)), b, BuffEffectInvoker.CreateAura, RegisterTacticBuff));
                else _myPlayer.BuffInterface.QueueBuff(new BuffQueueInfo(_myPlayer, _myPlayer.AbtInterface.GetMasteryLevelFor(AbilityMgr.GetMasteryTreeFor(b.Entry)), b, RegisterTacticBuff));
                _activeTactics.Add(id);
            }

            // Update the saved list for the server
            for (int i = 0; i < 4; ++i)
                _myPlayer._Value.SetTactic((byte)(i + 1), i < _activeTactics.Count ? _activeTactics[i] : (ushort)0);

            if (sendTacticUpdate)
                SendTactics();
        }

        public void HandleTactics(List<ushort> tacList)
        {
            if (_myPlayer.CbtInterface.IsInCombat)
            {
                SendTactics();
                return;
            }

            bool sendTacticUpdate = false;

            List<NewBuff> removeList = new List<NewBuff>();

            _myPlayer._Value.SetTactic(1, 0);
            _myPlayer._Value.SetTactic(2, 0);
            _myPlayer._Value.SetTactic(3, 0);
            _myPlayer._Value.SetTactic(4, 0);

            int maxAllowedTactics = _myPlayer.AdjustedLevel / 10;

            while (tacList.Count > maxAllowedTactics)
            {
                tacList.RemoveAt(tacList.Count - 1);
                sendTacticUpdate = true;
            }

            foreach (var buff in _activeBuffs)
            {
                if (!tacList.Contains(buff.Entry))
                {
                    buff.BuffHasExpired = true;

                    if (_modifyingTactics.Contains(buff.Entry))
                    {
                        _modifyingTactics.Remove(buff.Entry);

                        List<ushort> toRemove = new List<ushort>();

                        if (AbilityMgr.HasPreCastModifiers(buff.Entry))
                        {
                            foreach (AbilityModifier mod in AbilityMgr.GetAbilityPreCastModifiers(buff.Entry))
                            {
                                if (mod.Affecting == 0)
                                    _generalPreCastModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                                else if (mod.Affecting <= 3)
                                    _speclinePreCastModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                                else
                                    toRemove.Add(mod.Affecting);
                            }

                            foreach (ushort rem in toRemove)
                                _abilityPreCastModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);

                            toRemove.Clear();
                        }

                        if (AbilityMgr.HasModifiers(buff.Entry))
                        {
                            foreach (AbilityModifier mod in AbilityMgr.GetAbilityModifiers(buff.Entry))
                            {
                                if (mod.Affecting == 0)
                                    _generalModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                                else if (mod.Affecting <= 3)
                                    _speclineModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                                else
                                    toRemove.Add(mod.Affecting);
                            }

                            foreach (ushort rem in toRemove)
                                _abilityModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);

                            toRemove.Clear();
                        }

                        if (AbilityMgr.HasBuffModifiers(buff.Entry))
                        {
                            foreach (AbilityModifier mod in AbilityMgr.GetBuffModifiers(buff.Entry))
                            {
                                if (mod.Affecting == 0)
                                    _generalBuffModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                                else if (mod.Affecting <= 3)
                                    _speclineBuffModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                                else
                                    toRemove.Add(mod.Affecting);
                            }

                            foreach (ushort rem in toRemove)
                                _buffModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);
                        }
                    }

                    removeList.Add(buff);
                    _activeTactics.Remove(buff.Entry);
                }
            }

            foreach (var buff in removeList)
            {
                _activeBuffs.Remove(buff);
            }

            foreach (ushort id in tacList)
            {
                if (id == 0 || _activeTactics.Contains(id))
                    continue;
                BuffInfo b = AbilityMgr.GetBuffInfo(id);

                if (b == null)
                {
                    _myPlayer.SendClientMessage("Nonexistent tactic: " + id + " " + AbilityMgr.GetAbilityNameFor(id));
                    continue;
                }

                if (!_myPlayer.AbtInterface.IsValidTactic(id))
                {
                    _myPlayer.SendClientMessage("Invalid tactic: " + id + " " + AbilityMgr.GetAbilityNameFor(id));
                    sendTacticUpdate = true;
                    continue;
                }

                if (!string.IsNullOrEmpty(b.AuraPropagation))
                    _myPlayer.BuffInterface.QueueBuff(new BuffQueueInfo(_myPlayer, _myPlayer.AbtInterface.GetMasteryLevelFor(AbilityMgr.GetMasteryTreeFor(b.Entry)), b, BuffEffectInvoker.CreateAura, RegisterTacticBuff));
                else _myPlayer.BuffInterface.QueueBuff(new BuffQueueInfo(_myPlayer, _myPlayer.AbtInterface.GetMasteryLevelFor(AbilityMgr.GetMasteryTreeFor(b.Entry)), b, RegisterTacticBuff));
                _activeTactics.Add(id);
            }


            // Update the saved list for the server
            for (int i = 0; i < 4; ++i)
                _myPlayer._Value.SetTactic((byte)(i + 1), i < _activeTactics.Count ? _activeTactics[i] : (ushort)0);

            if (sendTacticUpdate)
                SendTactics();
        }

        /// <summary>
        /// Verifies that the player can legitimately assign the requested tactic.
        /// </summary>
        public void ValidateTactics()
        {
            List<NewBuff> removeList = new List<NewBuff>();
            foreach (var buff in _activeBuffs)
            {
                if (!_myPlayer.AbtInterface.IsValidTactic(buff.Entry))
                {
                    buff.BuffHasExpired = true;

                    if (_modifyingTactics.Contains(buff.Entry))
                    {
                        _modifyingTactics.Remove(buff.Entry);

                        List<ushort> toRemove = new List<ushort>();

                        if (AbilityMgr.HasPreCastModifiers(buff.Entry))
                        {
                            foreach (AbilityModifier mod in AbilityMgr.GetAbilityPreCastModifiers(buff.Entry))
                            {
                                if (mod.Affecting == 0)
                                    _generalPreCastModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                                else if (mod.Affecting <= 3)
                                    _speclinePreCastModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                                else
                                    toRemove.Add(mod.Affecting);
                            }

                            foreach (ushort rem in toRemove)
                                _abilityPreCastModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);

                            toRemove.Clear();
                        }

                        if (AbilityMgr.HasModifiers(buff.Entry))
                        {
                            foreach (AbilityModifier mod in AbilityMgr.GetAbilityModifiers(buff.Entry))
                            {
                                if (mod.Affecting == 0)
                                    _generalModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                                else if (mod.Affecting <= 3)
                                    _speclineModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                                else
                                    toRemove.Add(mod.Affecting);
                            }

                            foreach (ushort rem in toRemove)
                                _abilityModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);

                            toRemove.Clear();
                        }

                        if (AbilityMgr.HasBuffModifiers(buff.Entry))
                        {
                            foreach (AbilityModifier mod in AbilityMgr.GetBuffModifiers(buff.Entry))
                            {
                                if (mod.Affecting == 0)
                                    _generalBuffModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                                else if (mod.Affecting <= 3)
                                    _speclineBuffModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                                else
                                    toRemove.Add(mod.Affecting);
                            }

                            foreach (ushort rem in toRemove)
                                _buffModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);
                        }
                    }

                    removeList.Add(buff);
                    _activeTactics.Remove(buff.Entry);
                }
            }

            foreach (var buff in removeList)
                _activeBuffs.Remove(buff);

            SendTactics();
        }

        /// <summary>
        /// Sends the new tactic list to the client.
        /// </summary>
        public void SendTactics()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_TACTICS, 16);
            Out.WriteByte(3);
            Out.WriteByte((byte)_activeTactics.Count);
            foreach (ushort tactic in _activeTactics)
                Out.WriteUInt16(tactic);

            _myPlayer.SendPacket(Out);

        }

        /// <summary>
        /// Registers the tactic's ability and buff modifiers.
        /// </summary>
        public void RegisterTacticBuff(NewBuff b)
        {
            bool bAdded = false;
            _activeBuffs.Add(b);

            #region PreCast
            if (AbilityMgr.HasPreCastModifiers(b.Entry))
            {
                List<AbilityModifier> tacPrecastModifiers = AbilityMgr.GetAbilityPreCastModifiers(b.Entry);

                foreach (AbilityModifier mod in tacPrecastModifiers)
                {
                    if (mod.Affecting == 0)
                        _generalPreCastModifiers.Add(mod);

                    else if (mod.Affecting <= 3)
                    {
                        if (!_speclinePreCastModifiers.ContainsKey((ushort)(mod.Affecting - 1)))
                            _speclinePreCastModifiers.Add((ushort)(mod.Affecting - 1), new List<AbilityModifier>());
                        _speclinePreCastModifiers[(byte) (mod.Affecting - 1)].Add(mod);

                    }

                    else
                    {
                        if (!_abilityPreCastModifiers.ContainsKey(mod.Affecting))
                            _abilityPreCastModifiers.Add(mod.Affecting, new List<AbilityModifier>());

                        _abilityPreCastModifiers[mod.Affecting].Add(mod);
                    }

                    for (AbilityModifierEffect effect = mod.Effect; effect != null; effect = effect.nextMod)
                        b.AddBuffParameter(effect.BuffLine, effect.PrimaryValue);
                }

                bAdded = true;
            }

            #endregion

            #region Cast

            if (AbilityMgr.HasModifiers(b.Entry))
            {
                List<AbilityModifier> tacModifiers = AbilityMgr.GetAbilityModifiers(b.Entry);

                foreach (AbilityModifier mod in tacModifiers)
                {
                    if (mod.Affecting == 0)
                        _generalModifiers.Add(mod);

                    else if (mod.Affecting <= 3)
                    { 
                        if (!_speclineModifiers.ContainsKey((ushort) (mod.Affecting - 1)))
                            _speclineModifiers.Add((ushort) (mod.Affecting - 1), new List<AbilityModifier>());
                        _speclineModifiers[(byte) (mod.Affecting - 1)].Add(mod);
                    }

                    else
                    {
                        if (!_abilityModifiers.ContainsKey(mod.Affecting))
                            _abilityModifiers.Add(mod.Affecting, new List<AbilityModifier>());

                        _abilityModifiers[mod.Affecting].Add(mod);
                    }

                    for (AbilityModifierEffect effect = mod.Effect; effect != null; effect = effect.nextMod)
                        b.AddBuffParameter(effect.BuffLine, effect.PrimaryValue);
                }

                bAdded = true;
            }

            #endregion

            #region Buff
            if (AbilityMgr.HasBuffModifiers(b.Entry))
            {
                List<AbilityModifier> tacBuffModifiers = AbilityMgr.GetBuffModifiers(b.Entry);

                foreach (AbilityModifier mod in tacBuffModifiers)
                {
                    if (mod.Affecting == 0)
                        _generalBuffModifiers.Add(mod);

                    else if (mod.Affecting <= 3)
                    {
                        if (!_speclineBuffModifiers.ContainsKey((ushort)(mod.Affecting - 1)))
                            _speclineBuffModifiers.Add((ushort)(mod.Affecting - 1), new List<AbilityModifier>());
                        _speclineBuffModifiers[(byte)(mod.Affecting - 1)].Add(mod);
                    }

                    else
                    {
                        if (!_buffModifiers.ContainsKey(mod.Affecting))
                            _buffModifiers.Add(mod.Affecting, new List<AbilityModifier>());

                        _buffModifiers[mod.Affecting].Add(mod);
                    }

                    for (AbilityModifierEffect effect = mod.Effect; effect != null; effect = effect.nextMod)
                        b.AddBuffParameter(effect.BuffLine, effect.PrimaryValue);
                }

                bAdded = true;
            }

            #endregion

            if (bAdded)
                _modifyingTactics.Add(b.Entry);
        }

        /// <summary>
        /// Registers a given buff's ability and buff modifiers.
        /// </summary>
        public void RegisterGeneralBuff(NewBuff buff)
        {
            #region PreCast
            if (AbilityMgr.HasPreCastModifiers(buff.Entry))
            {
                List<AbilityModifier> abilityPreCastModifiers = AbilityMgr.GetAbilityPreCastModifiers(buff.Entry);

                foreach (AbilityModifier mod in abilityPreCastModifiers)
                {
                    if (mod.Affecting == 0)
                        _generalPreCastModifiers.Add(mod);

                    else if (mod.Affecting <= 3)
                    {
                        if (!_speclinePreCastModifiers.ContainsKey((ushort)(mod.Affecting - 1)))
                            _speclinePreCastModifiers.Add((ushort)(mod.Affecting - 1), new List<AbilityModifier>());
                        _speclinePreCastModifiers[(byte)(mod.Affecting - 1)].Add(mod);

                    }

                    else
                    {
                        if (!_abilityPreCastModifiers.ContainsKey(mod.Affecting))
                            _abilityPreCastModifiers.Add(mod.Affecting, new List<AbilityModifier>());

                        _abilityPreCastModifiers[mod.Affecting].Add(mod);
                    }

                    for (AbilityModifierEffect effect = mod.Effect; effect != null; effect = effect.nextMod)
                        buff.AddBuffParameter(effect.BuffLine, effect.PrimaryValue);
                }
            }

            #endregion

            #region Cast

            if (AbilityMgr.HasModifiers(buff.Entry))
            {
                List<AbilityModifier> abilityModifiers = AbilityMgr.GetAbilityModifiers(buff.Entry);

                foreach (AbilityModifier mod in abilityModifiers)
                {
                    if (mod.Affecting == 0)
                        _generalModifiers.Add(mod);

                    else if (mod.Affecting <= 3)
                    {
                        if (!_speclineModifiers.ContainsKey((ushort)(mod.Affecting - 1)))
                            _speclineModifiers.Add((ushort)(mod.Affecting - 1), new List<AbilityModifier>());
                        _speclineModifiers[(byte)(mod.Affecting - 1)].Add(mod);
                    }

                    else
                    {
                        if (!_abilityModifiers.ContainsKey(mod.Affecting))
                            _abilityModifiers.Add(mod.Affecting, new List<AbilityModifier>());

                        _abilityModifiers[mod.Affecting].Add(mod);
                    }


                    for (AbilityModifierEffect effect = mod.Effect; effect != null; effect = effect.nextMod)
                        buff.AddBuffParameter(effect.BuffLine, effect.PrimaryValue);
                }
            }

            #endregion

            #region Buff
            if (AbilityMgr.HasBuffModifiers(buff.Entry))
            {
                List<AbilityModifier> buffModifiers = AbilityMgr.GetBuffModifiers(buff.Entry);

                foreach (AbilityModifier mod in buffModifiers)
                {
                    if (mod.Affecting == 0)
                        _generalBuffModifiers.Add(mod);

                    else if (mod.Affecting <= 3)
                    {
                        if (!_speclineBuffModifiers.ContainsKey((ushort)(mod.Affecting - 1)))
                            _speclineBuffModifiers.Add((ushort)(mod.Affecting - 1), new List<AbilityModifier>());
                        _speclineBuffModifiers[(byte)(mod.Affecting - 1)].Add(mod);
                    }

                    else
                    {
                        if (!_buffModifiers.ContainsKey(mod.Affecting))
                            _buffModifiers.Add(mod.Affecting, new List<AbilityModifier>());

                        _buffModifiers[mod.Affecting].Add(mod);
                    }


                    for (AbilityModifierEffect effect = mod.Effect; effect != null; effect = effect.nextMod)
                        buff.AddBuffParameter(effect.BuffLine, effect.PrimaryValue);
                }
            }

            #endregion
        }

        public void UnregisterGeneralBuff(NewBuff buff)
        {
            List<ushort> toRemove = new List<ushort>();

            #region PreCast
            if (AbilityMgr.HasPreCastModifiers(buff.Entry))
            {
                foreach (AbilityModifier mod in AbilityMgr.GetAbilityPreCastModifiers(buff.Entry))
                {
                    if (mod.Affecting == 0)
                        _generalPreCastModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                    else if (mod.Affecting <= 3)
                        _speclinePreCastModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                    else
                        toRemove.Add(mod.Affecting);
                }

                foreach (ushort rem in toRemove)
                    _abilityPreCastModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);

                toRemove.Clear();
            }

            #endregion

            #region Cast

            if (AbilityMgr.HasModifiers(buff.Entry))
            {
                foreach (AbilityModifier mod in AbilityMgr.GetAbilityModifiers(buff.Entry))
                {
                    if (mod.Affecting == 0)
                        _generalModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                    else if (mod.Affecting <= 3)
                        _speclineModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                    else
                        toRemove.Add(mod.Affecting);
                }

                foreach (ushort rem in toRemove)
                    _abilityModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);

                toRemove.Clear();
            }

            #endregion

            #region Buff

            if (AbilityMgr.HasBuffModifiers(buff.Entry))
            {
                foreach (AbilityModifier mod in AbilityMgr.GetBuffModifiers(buff.Entry))
                {
                    if (mod.Affecting == 0)
                        _generalBuffModifiers.RemoveAll(fmod => fmod.Source == buff.Entry);
                    else if (mod.Affecting <= 3)
                        _speclineBuffModifiers[(byte)(mod.Affecting - 1)].RemoveAll(fmod => fmod.Source == buff.Entry);
                    else
                        toRemove.Add(mod.Affecting);
                }

                foreach (ushort rem in toRemove)
                    _buffModifiers[rem].RemoveAll(fmod => fmod.Source == buff.Entry);
            }

            #endregion
        }

        #endregion

        public void ModifyInitials(AbilityInfo abInfo)
        {
            if (_generalPreCastModifiers.Count != 0)
            {
                foreach (var modifier in _generalPreCastModifiers)
                    modifier.ModifyAbility(_myPlayer, abInfo);
            }

            if (abInfo.ConstantInfo.MasteryTree != 0 && _speclinePreCastModifiers.ContainsKey((byte)(abInfo.ConstantInfo.MasteryTree - 1)))
            {
                foreach (var modifier in _speclinePreCastModifiers[(byte)(abInfo.ConstantInfo.MasteryTree - 1)])
                    modifier.ModifyAbility(_myPlayer, abInfo);
            }

            if (_abilityPreCastModifiers.ContainsKey(abInfo.Entry))
            {
                foreach (var modifier in _abilityPreCastModifiers[abInfo.Entry])
                    modifier.ModifyAbility(_myPlayer, abInfo);
            }
        }

        public void ModifyFinals(AbilityInfo abInfo)
        {
            if (_generalModifiers.Count != 0)
            {
                foreach (var modifier in _generalModifiers)
                    modifier.ModifyAbility(_myPlayer, abInfo);
            }

            if (abInfo.ConstantInfo.MasteryTree != 0 && _speclineModifiers.ContainsKey((byte)(abInfo.ConstantInfo.MasteryTree - 1)))
            {
                foreach (var modifier in _speclineModifiers[(byte)(abInfo.ConstantInfo.MasteryTree - 1)])
                    modifier.ModifyAbility(_myPlayer, abInfo);
            }

            if (_abilityModifiers.ContainsKey(abInfo.Entry))
            {
                foreach (var modifier in _abilityModifiers[abInfo.Entry])
                    modifier.ModifyAbility(_myPlayer, abInfo);
            }

            if (_myPlayer.CrrInterface.ExperimentalMode)
                _myPlayer.CrrInterface.ExperimentalModeModifyAbility(abInfo);
        }

        public void ModifyBuff(BuffInfo buffInfo, Unit target)
        {
            if (_generalBuffModifiers.Count != 0)
            {
                foreach (var modifier in _generalBuffModifiers)
                    modifier.ModifyBuff(_myPlayer, target, buffInfo);
            }

            if (buffInfo.MasteryTree != 0 && _speclineBuffModifiers.ContainsKey((byte)(buffInfo.MasteryTree - 1)))
            {
                foreach (var modifier in _speclineBuffModifiers[(byte)(buffInfo.MasteryTree - 1)])
                    modifier.ModifyBuff(_myPlayer, target, buffInfo);
            }

            if (_buffModifiers.ContainsKey(buffInfo.Entry))
            {
                foreach (var modifier in _buffModifiers[buffInfo.Entry])
                    modifier.ModifyBuff(_myPlayer, target, buffInfo);
            }

            if (_myPlayer.CrrInterface.ExperimentalMode)
                _myPlayer.CrrInterface.ExperimentalModeModifyBuff(buffInfo, target);
        }
    }
}
