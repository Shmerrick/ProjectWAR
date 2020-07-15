using System;
using Common;
using FrameWork;
using GameData;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    class CareerInterface_EngineerMagus : CareerInterface, IPetCareerInterface
    {
        private byte _powerBonus = 4;
        private byte _maxStack = 5;

        private int rangeMax = 25;

        private float _rangeBonusPct1 = 0f;
        private readonly float _rangeBonusPct2Pet = 0f;
        private readonly float _rangeReductionPct3 = 0f;

        private readonly float _radiusBonusPctMaster = 50f/8f;
        private readonly ushort _dodgeDisruptBonus = 0;
        private readonly ushort _dodgeDisruptBonusPet = 0;

        private readonly ushort _resourceID;

        public new Pet myPet { get; set; }

        public byte AIMode { get; set; } = 5;

        public CareerInterface_EngineerMagus(Player player) : base(player)
        {
            if (myPlayer.Info.CareerLine == (byte) CareerLine.CAREERLINE_ENGINEER)
                _resourceID = 1070;
            else
                _resourceID = 1072;

            _resourceTimeout = 0;
        }

        /*
        public override bool SetExperimentalMode(bool fullExplanation)
        {
            if (myPlayer.GmLevel < 1)
            { 
                myPlayer.SendClientMessage("This career has no experimental modifications to activate.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return false;
            }

            ExperimentalMode = !ExperimentalMode;

            Notify_PlayerLoaded();

            if (!fullExplanation && ExperimentalMode)
            {
				_rangeBonusPct1 = 20f/8f;	
                myPlayer.SendClientMessage("Experimental mode for Engineer and Magus is currently enabled.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return true;
            }

            if (_boostStacks > 0)
            {
                RemoveBonuses();
                _boostStacks = 0;
                SendBoostRemoval();
            }

            if (ExperimentalMode)
            {
                _rangeBonusPct1 = 20f/8f;
                myPlayer.SendClientMessage("Experimental mode for Engineer and Magus has been enabled.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("General changes:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("- The range bonus for Rifleman and Havoc maxes at 20% instead of 40%.");
            }

            else
            {
                myPlayer.SendClientMessage("Experimental mode has been disabled.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                _rangeBonusPct1 = 40f / 8f;
            }

            return true;
        }*/

        public override void NotifyClientLoaded()
        {
            if (myPlayer.Info.CareerLine == (byte)CareerLine.CAREERLINE_MAGUS)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(475); // Magus' badge
                Out.WriteByte(1); // disc effect
                Out.WriteByte(0);
                Out.WriteByte(1);
                Out.WriteByte(0);

                myPlayer.SendPacket(Out);

                Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(475); // Magus' badge
                Out.WriteByte(6); // unk
                Out.WriteByte(0);
                Out.WriteByte(1);
                Out.WriteByte(0);

                myPlayer.SendPacket(Out);

                Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(475); // Magus' badge
                Out.WriteByte(7); // unk
                Out.WriteByte(0);
                Out.WriteByte(1);
                Out.WriteByte(0);

                myPlayer.SendPacket(Out);
            }
        }

        public override bool HasResource(byte amount)
        {
            return _boostStacks > 0 && CareerResource == amount;
        }

        public override bool HasResourceRange(int min, int max)
        {
            return min <= _careerResource && max >= _careerResource && _boostStacks > 0;
        }

        public override void SetResource(byte amount, bool blockEvent)
        {
        }

        public override bool AddResource(byte amount, bool blockEvent)
        {
            return false;
        }

        public override bool ConsumeResource(byte amount, bool blockEvent)
        {
            return false;
        }

        // For boost stacks
        public override byte GetCurrentResourceLevel(byte which)
        {
            return (byte)_boostStacks;
        }

        public override void SendResource()
        {

        }

        private long _lastTick;
        private int _boostStacks;

        public override void Update(long tick)
        {
            if (tick - _lastTick < 2000)
                return;

            if (_boostStacks == 0)
            {
                if (myPet == null || !myPet.ObjectWithinRadiusFeet(myPlayer, 25))
                    return;

                _boostStacks++;
                AddBonuses();
            }

            else if (_boostStacks <= _maxStack)
            {   
                // Stack decay
                if (myPet == null || !myPet.ObjectWithinRadiusFeet(myPlayer, _careerResource == 2 ? 80 : 25))
                {
                    RemoveBonuses();

                    int stackMod = 1; //myPet == null ? 2 : 1;

                    if (_boostStacks <= stackMod)
                    {
                        _boostStacks = 0;
                        SendBoostRemoval();
                    }

                    else
                    {
                        _boostStacks = _boostStacks - stackMod;
                        AddBonuses();
                    }
                }

                else if (_boostStacks < _maxStack && (_careerResource != 2 || myPet.ObjectWithinRadiusFeet(myPlayer, 40)))
                { 
                    RemoveBonuses();
                    _boostStacks++;
                    AddBonuses();
                }
            }

            _lastTick = tick;
        }

        private void SwitchPetBonusType(int newResource)
        {
            if (newResource == _careerResource)
                return;

            if (_boostStacks > 0)
            {
                RemoveBonuses();
                _boostStacks = 0;
                SendBoostRemoval();
            }

            _careerResource = (byte)newResource;
        }

        public override void NotifyPanicked()
        {
           
        }

        private void RemoveBonuses()
        {
            myPlayer.StsInterface.RemoveBonusMultiplier(Stats.OutgoingDamagePercent, _powerBonus * 0.01f * _boostStacks, BuffClass.Career);

            switch (_careerResource)
            {
                case 1:
                    // Master range increase
                    myPlayer.StsInterface.RemoveBonusMultiplier(Stats.Range, Math.Min(rangeMax * 0.01f, _rangeBonusPct1 * 0.01f * _boostStacks), BuffClass.Tactic);
                    // Pet range increase
                    myPet?.StsInterface.RemoveBonusMultiplier(Stats.Range, Math.Min(rangeMax * 0.01f, _rangeBonusPct1 * 0.01f * _boostStacks), BuffClass.Tactic);
                    break;
                case 2:
                    // Pet range increase
                    myPet?.StsInterface.RemoveBonusMultiplier(Stats.Range, _rangeBonusPct2Pet * 0.01f * _boostStacks, BuffClass.Tactic);
                    break;
                case 3:
                    // Master range reduction
                    myPlayer.StsInterface.RemoveReducedMultiplier(Stats.Range, 1f - _rangeReductionPct3 * 0.01f * _boostStacks, BuffClass.Tactic);

                    // Master AoE radius increase
                    myPlayer.StsInterface.RemoveBonusMultiplier(Stats.Radius, _radiusBonusPctMaster * 0.01f * _boostStacks, BuffClass.Tactic);

                    // Master AoE cap increase
                    myPlayer.StsInterface.RemoveBonusStat(Stats.EffectBuff, (ushort)(2 * _boostStacks), BuffClass.Tactic);

                    // Master dodge/disrupt increase
                    myPlayer.StsInterface.RemoveBonusStat(Stats.Evade, (ushort)(_dodgeDisruptBonus * _boostStacks), BuffClass.Tactic);
                    myPlayer.StsInterface.RemoveBonusStat(Stats.Disrupt, (ushort)(_dodgeDisruptBonus * _boostStacks), BuffClass.Tactic);

                    // Pet dodge/disrupt increase
                    myPet?.StsInterface.RemoveBonusStat(Stats.Evade, (ushort)(_dodgeDisruptBonusPet * _boostStacks), BuffClass.Tactic);
                    myPet?.StsInterface.RemoveBonusStat(Stats.Disrupt, (ushort)(_dodgeDisruptBonusPet * _boostStacks), BuffClass.Tactic);
                    break;
            }
        }

        private void AddBonuses()
        {
            myPlayer.StsInterface.AddBonusMultiplier(Stats.OutgoingDamagePercent, _powerBonus * 0.01f * _boostStacks, BuffClass.Career);
            
            switch (_careerResource)
            {
                case 1:
                    // Master range increase
                    myPlayer.StsInterface.AddBonusMultiplier(Stats.Range, Math.Min(rangeMax * 0.01f, _rangeBonusPct1 * 0.01f * _boostStacks), BuffClass.Tactic);
                    // Pet range increase
                    myPet?.StsInterface.AddBonusMultiplier(Stats.Range, Math.Min(rangeMax * 0.01f, _rangeBonusPct1 * 0.01f * _boostStacks), BuffClass.Tactic);
                    break;
                case 2:
                    // Pet range increase
                    myPet?.StsInterface.AddBonusMultiplier(Stats.Range, _rangeBonusPct2Pet * 0.01f * _boostStacks, BuffClass.Tactic);
                    break;
                case 3:
                    // Master range reduction
                    myPlayer.StsInterface.AddReducedMultiplier(Stats.Range, 1f - _rangeReductionPct3 * 0.01f * _boostStacks, BuffClass.Tactic);

                    // Master AoE radius increase
                    myPlayer.StsInterface.AddBonusMultiplier(Stats.Radius, _radiusBonusPctMaster * 0.01f * _boostStacks, BuffClass.Tactic);

                    // Master AoE cap increase
                    myPlayer.StsInterface.AddBonusStat(Stats.EffectBuff, (ushort)(2 * _boostStacks), BuffClass.Tactic);

                    // Master dodge/disrupt increase
                    myPlayer.StsInterface.AddBonusStat(Stats.Evade, (ushort)(_dodgeDisruptBonus * _boostStacks), BuffClass.Tactic);
                    myPlayer.StsInterface.AddBonusStat(Stats.Disrupt, (ushort)(_dodgeDisruptBonus * _boostStacks), BuffClass.Tactic);

                    // Pet dodge/disrupt increase
                    myPet?.StsInterface.AddBonusStat(Stats.Evade, (ushort)(_dodgeDisruptBonusPet * _boostStacks), BuffClass.Tactic);
                    myPet?.StsInterface.AddBonusStat(Stats.Disrupt, (ushort)(_dodgeDisruptBonusPet * _boostStacks), BuffClass.Tactic);
                    break;
            }

            SendBoostUpdate();
        }

        private void SendBoostUpdate()
        {
            // DAMAGE
            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 18);
            Out.WriteByte(1);
            Out.WriteByte(1); // update
            Out.WriteUInt16(0);
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(255); // buffID - some number I pulled out of the air
            Out.WriteByte(0);
            Out.WriteUInt16R(_resourceID);
            Out.WriteZigZag(0);
            Out.WriteUInt16R(_Owner.Oid);

            Out.WriteByte(2);

            Out.WriteByte(0);
            Out.WriteZigZag(_boostStacks);
            Out.WriteByte(1);
            Out.WriteZigZag(_powerBonus * _boostStacks);

            Out.WriteByte(0);
            ((Player)_Owner).DispatchPacket(Out, true);

            // RANGE

            if (_careerResource == 1 || _careerResource == 3)
            {
                Out = new PacketOut((byte) Opcodes.F_INIT_EFFECTS, 18);
                Out.WriteByte(1);
                Out.WriteByte(1); // update
                Out.WriteUInt16(0);
                Out.WriteUInt16(_Owner.Oid);
                Out.WriteByte(254); // buffID - some number I pulled out of the air
                Out.WriteByte(0);
                Out.WriteUInt16R(10354); // true magic
                Out.WriteZigZag(0);
                Out.WriteUInt16R(_Owner.Oid);

                Out.WriteByte(1);
                Out.WriteByte(0);
                if (_careerResource == 1)
                    Out.WriteZigZag((int)Math.Min(rangeMax, _rangeBonusPct1 *  _boostStacks));
                else
                    Out.WriteZigZag(-(int)(_rangeBonusPct1 * _boostStacks));

                Out.WriteByte(0);
                ((Player) _Owner).DispatchPacket(Out, true);
            }
        }

        private void SendBoostRemoval()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 12);
            Out.WriteByte(1);
            Out.WriteByte(BUFF_REMOVE);
            Out.WriteUInt16(0);
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(255); // buffID
            Out.WriteByte(0);
            Out.WriteUInt16R(_resourceID);
            Out.WriteByte(0);
            ((Player)_Owner).DispatchPacket(Out, true);

            if (_careerResource == 1 || _careerResource == 3)
            {
                Out = new PacketOut((byte) Opcodes.F_INIT_EFFECTS, 12);
                Out.WriteByte(1);
                Out.WriteByte(BUFF_REMOVE);
                Out.WriteUInt16(0);
                Out.WriteUInt16(_Owner.Oid);
                Out.WriteByte(254); // buffID
                Out.WriteByte(0);
                Out.WriteUInt16R(10354);
                Out.WriteByte(0);
                ((Player) _Owner).DispatchPacket(Out, true);
            }
        }

        public void SummonPet(ushort myID)
        {
            if (myPet != null)
            {
                myPet.Destroy();
                myPet = null;
            }

            Creature_proto proto = new Creature_proto();

            switch(myID)
            {
                case 8474:
                    proto.Name = myPlayer.Name + "'s Pink Horror^M";
                    proto.Model1 = 141;
                    proto.Faction = 129;
                    proto.Ranged = 100;
                    SwitchPetBonusType(1);
                    SpawnPet(proto);

                    break;
                case 8476:
                    proto.Name = myPlayer.Name + "'s Blue Horror^M";
                    proto.Model1 = 142;
                    proto.Faction = 129;
                    proto.Ranged = 30;
                    SwitchPetBonusType(3);
                    SpawnPet(proto);

                    break;
                case 8478:
                    proto.Name = myPlayer.Name + "'s Flamer^M";
                    proto.Model1 = 143;
                    proto.Faction = 129;
                    proto.Ranged = 65;
                    SwitchPetBonusType(2);
                    SpawnPet(proto);

                    break;
                case 1511:
                    proto.Name = myPlayer.Name + "'s Gun Turret^N";
                    proto.Model1 = 145;
                    proto.Faction = 65;
                    proto.Ranged = 100;
                    SwitchPetBonusType(1);
                    SpawnPet(proto);

                    break;
                case 1518:
                    proto.Name = myPlayer.Name + "'s Bombardment Turret^N";
                    proto.Model1 = 146;
                    proto.Faction = 65;
                    proto.Ranged = 65;
                    SwitchPetBonusType(2);
                    SpawnPet(proto);

                    break;
                case 1526:
                    proto.Name = myPlayer.Name + "'s Flame Turret^N";
                    proto.Model1 = 147;
                    proto.Faction = 65;
                    proto.Ranged = 30;
                    SwitchPetBonusType(3);
                    SpawnPet(proto);

                    break;


                case 8535:
                    proto.Name = myPlayer.Name + "'s Firewyrm^N";
                    proto.Model1 = 144;   // firewyrm  
                    proto.Faction = 129;  //dest
                    proto.Ranged = 5;
                    SwitchPetBonusType(2);
                    SpawnPet(proto);

                    break;
                default:
                    throw new Exception("Engineer/Magus: Requested pet ID "+myID+" not found for SummonPet");
            }
        }

        private void SpawnPet(Creature_proto proto)
        {
            Creature_spawn spawn = new Creature_spawn();

            proto.MinScale = 50;
            proto.MaxScale = 50;
            spawn.BuildFromProto(proto);
            spawn.WorldO = myPlayer._Value.WorldO;
            spawn.WorldY = myPlayer._Value.WorldY;
            spawn.WorldZ = myPlayer._Value.WorldZ;
            spawn.WorldX = myPlayer._Value.WorldX;
            spawn.ZoneId = myPlayer.Zone.ZoneId;
            spawn.Icone = 18;
            spawn.WaypointType = 0;
            spawn.Proto.MinLevel = spawn.Proto.MaxLevel = myPlayer.EffectiveLevel;

            if (spawn.Proto.MinLevel > 40)
            { 
                spawn.Proto.MinLevel = 40;
                spawn.Proto.MaxLevel = 40;
            }

            myPet = new Pet(0, spawn, myPlayer, AIMode, true, true);
            myPet.StsInterface.Speed = 0;
            if (_boostStacks > 0)
            {
                switch (_careerResource)
                {
                    case 1:
                        // Pet range increase
                        myPet.StsInterface.AddBonusMultiplier(Stats.Range, _rangeBonusPct1 * 0.01f * _boostStacks, BuffClass.Tactic);
                        break;
                    case 2:
                        // Pet range increase
                        myPet.StsInterface.AddBonusMultiplier(Stats.Range, _rangeBonusPct2Pet * 0.01f * _boostStacks, BuffClass.Tactic);
                        break;
                    case 3:
                        // Pet dodge/disrupt increase
                        myPet?.StsInterface.AddBonusStat(Stats.Evade, (ushort)(_dodgeDisruptBonusPet * _boostStacks), BuffClass.Tactic);
                        myPet?.StsInterface.AddBonusStat(Stats.Disrupt, (ushort)(_dodgeDisruptBonusPet * _boostStacks), BuffClass.Tactic);
                        break;
                }
            }
            myPet.UpdateSpeed();
            myPlayer.Region.AddObject(myPet, spawn.ZoneId);

            myPlayer.BuffInterface.NotifyPetEvent(myPet);
        }

        public void Notify_PetDown()
        {
            myPet = null;
            myPlayer.BuffInterface.NotifyPetEvent(myPet);

            // Penalize 4 stacks for pet death / cycle
            if (_boostStacks > 0)
            {
                RemoveBonuses();

                if (_boostStacks <= 4)
                {
                    SendBoostRemoval();
                    _boostStacks = 0;
                }
                else
                {
                    _boostStacks -= 4;
                    AddBonuses();
                }
            }
        }

        public override void Stop()
        {
            if (myPet != null)
            {
                myPet.Destroy();
                myPet = null;
            }

            base.Stop();
        }

        public override Unit GetTargetOfInterest()
        {
            return myPet;
        }

        public override EArchetype GetArchetype()
        {
            return EArchetype.ARCHETYPE_DPS;
        }   
    }
}
