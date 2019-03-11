using System;
using SystemData;
using FrameWork;
using WorldServer.Managers;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    interface IPetCareerInterface
    {
        void SummonPet(ushort petID);
        void Notify_PetDown();
        Pet myPet
        {
            get;
            set;
        }

        byte AIMode
        {
            get;
            set;
        }
    }

    public enum EArchetype
    {
        ARCHETYPE_Tank,
        ARCHETYPE_DPS,
        ARCHETYPE_Healer
    }

    /// <summary>
    /// Handles career-based interactions with the ability system.
    /// </summary>
    public abstract class CareerInterface : BaseInterface
    {
        protected Player myPlayer;

        private bool _experimentalMode;

        public bool ExperimentalMode
        {
            get { return _experimentalMode; }
            set
            {
                _experimentalMode = false;
                myPlayer._Value.ExperimentalMode = false;

                CharMgr.Database.SaveObject(myPlayer._Value);
            }
        }

        protected byte BUFF_ADD = 1;
        protected byte BUFF_REMOVE = 2;

        protected CareerInterface(Player player)
        {
            myPlayer = player;
            _lastResourceTime = TCPManager.GetTimeStampMS();
        }

        /// <summary>
        /// Returns a new CareerInterface tailored to the career ID.
        /// </summary>
        public static CareerInterface GetInterfaceFor(Player Plr, ushort careerID)
        {
            switch (careerID)
            {
                case 1:
                    return new CareerInterface_Ironbreaker(Plr);
                case 2:
                    return new CareerInterface_SlayerChoppa(Plr);
                case 3:
                    return new CareerInterface_RPZealot(Plr);
                case 4:
                    return new CareerInterface_EngineerMagus(Plr);
                case 5:
                    return new CareerInterface_BlackOrc(Plr);
                case 6:
                    return new CareerInterface_SlayerChoppa(Plr);
                case 7:
                    return new CareerInterface_AMShaman(Plr);
                case 8:
                    return new CareerInterface_SquigHerder(Plr);
                case 9:
                    return new CareerInterface_WHWE(Plr);
                case 10:
                    return new CareerInterface_KnightChosen(Plr);
                case 11:
                    return new CareerInterface_BWSorc(Plr);
                case 12:
                    return new CareerInterface_WPDoK(Plr);
                case 13:
                    return new CareerInterface_KnightChosen(Plr);
                case 14:
                    return new CareerInterface_Marauder(Plr);
                case 15:
                    return new CareerInterface_RPZealot(Plr);
                case 16:
                    return new CareerInterface_EngineerMagus(Plr);
                case 17:
                    return new CareerInterface_Swordmaster(Plr);
                case 18:
                    return new CareerInterface_ShadowWarrior(Plr);
                case 19:
                    return new CareerInterface_WhiteLion(Plr);
                case 20:
                    return new CareerInterface_AMShaman(Plr);
                case 21:
                    return new CareerInterface_Blackguard(Plr);
                case 22:
                    return new CareerInterface_WHWE(Plr);
                case 23:
                    return new CareerInterface_WPDoK(Plr);
                case 24:
                    return new CareerInterface_BWSorc(Plr);
                default:
                    return new CareerInterface_BlackOrc(Plr);
            }
        }

        public virtual bool SetExperimentalMode(bool fullExplanation)
        {
            myPlayer.SendClientMessage("This class has no experimental modifications to activate.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

            ExperimentalMode = false;

            return false;
        }

        public virtual void DisplayChangeList()
        {
            myPlayer.SendClientMessage("This career has not undergone any changes.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        }

        #region Resource Management

        protected byte _lastResource, _careerResource, _maxResource;
        public byte CareerResource => _careerResource;

        protected long _lastResourceTime, _nextCheckTime = TCPManager.GetTimeStampMS() + 1000;
        protected long _resourceTimeout = 10000;
        internal Pet myPet;

        public virtual void Notify_PlayerLoaded()
        {
        }

        public virtual void NotifyClientLoaded()
        {
            
        }

        public virtual void SetResource(byte amount, bool blockEvent)
        {
            if (amount != 0)
                _lastResourceTime = TCPManager.GetTimeStampMS();

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceSet, _careerResource, ref amount);

            _lastResource = _careerResource;
            _careerResource = amount;

            SendResource();
        }

        public virtual bool AddResource(byte amount, bool blockEvent)
        {
            _lastResourceTime = TCPManager.GetTimeStampMS();

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceGained, _careerResource, ref amount);

            if (_careerResource == _maxResource)
                return false;
            _lastResource = _careerResource;
            _careerResource = (byte)Math.Min(_maxResource, _careerResource + amount);

            SendResource();

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);

            return true;
        }

        public virtual bool AddResourceOverride(byte amount, bool blockEvent, bool noTimer)
        {
            _lastResourceTime = TCPManager.GetTimeStampMS();

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceGained, _careerResource, ref amount);

            if (_careerResource == _maxResource)
                return false;
            _lastResource = _careerResource;
            _careerResource = (byte)Math.Min(_maxResource, _careerResource + amount);

            SendResource();

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);

            return true;
        }

        public virtual bool ConsumeResource(byte amount, bool blockEvent)
        {
            _lastResource = _careerResource;

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceLost, _lastResource, ref amount);

            if (_careerResource < amount)
            {
                _careerResource = 0;
                SendResource();

                return false;
            }

            _careerResource = (byte)Math.Max(0, _careerResource - amount);
            SendResource();

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);

            return true;
        }

        public override void Update(long tick)
        {
            if (tick < _nextCheckTime || _resourceTimeout == 0)
                return;

            if (_careerResource != 0 && tick > _lastResourceTime + _resourceTimeout)
                SetResource(0, false);

            _nextCheckTime = tick + 1000;
        }

        public virtual bool HasResource(byte amount)
        {
            return _careerResource >= amount;
        }

        public virtual bool HasResourceRange(int min, int max)
        {
            return min <= _careerResource && max >= _careerResource;
        }

        public virtual byte GetCurrentResourceLevel(byte which)
        {
            return _careerResource;
        }

        public virtual byte GetCurrentResourceLevelForClass(byte which)
        {
            return _careerResource;
        }

        public virtual byte GetLevelForResource(byte res, byte which)
        {
            return res;
        }

        public virtual byte GetResourceLevelMax(byte which)
        {
            return _maxResource;
        }

        public abstract void SendResource();

        public virtual Unit GetTargetOfInterest()
        {
            return null;
        }

        public virtual void NotifyPanicked()
        {
            SetResource(0, false);
        }

        #endregion
        /// <summary>
        /// Gets the archetype of the player associated with this interface,
        /// considering his current build if necessary (for am/sham/dok/wp for example).
        /// </summary>
        /// <returns>Archetype enum value</returns>
        public virtual EArchetype GetArchetype()
        {
            return EArchetype.ARCHETYPE_Tank;
        }

        /// <summary>
        /// Queried by the modifier check "Experimental Mode" to determine whether a given ability should be modified.
        /// </summary>
        public virtual bool ExperimentalModeCheckAbility(AbilityInfo abInfo)
        {
            return _experimentalMode;
        }

        public virtual void ExperimentalModeModifyBuff(BuffInfo buffInfo, Unit target)
        {

        }

        public virtual void ExperimentalModeModifyAbility(AbilityInfo abInfo)
        {
        }

        public virtual void NotifyInitialized()
        {
            if (myPlayer._Value.ExperimentalMode)
                SetExperimentalMode(false);
        }
    }
}
