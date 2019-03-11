using FrameWork;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    class CareerInterface_Marauder : CareerInterface
    {
        private static readonly byte[] _armStates = { 0x1A, 0x19, 0x18 };

        private const long SHIFT_DELAY = 550;

        public CareerInterface_Marauder(Player player) : base(player)
        {
            _resourceTimeout = 0;
        }

        public override bool HasResource(byte amount)
        {
            if (amount < 4)
                return _careerResource == amount;
            switch (amount)
            {
                case 4: return (_careerResource == 1 || _careerResource == 2);
                case 5: return (_careerResource == 2 || _careerResource == 3);
                case 6: return (_careerResource == 1 || _careerResource == 3);
                case 7: return _careerResource != 0;
                default: return true;
            }
        }

        public override void SetResource(byte amount, bool blockEvent)
        {
            if (_careerResource == amount || amount == 0)
                return;
            if (amount == 4)
                amount = 0;
            _lastResource = _careerResource;
            _careerResource = amount;

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);

            _nextArmShiftTime = TCPManager.GetTimeStampMS() + SHIFT_DELAY;
        }

        private long _nextArmShiftTime;

        public override void Update(long tick)
        {
            if (_nextArmShiftTime > 0 && _nextArmShiftTime <= tick)
            {
                SendResource();
                _nextArmShiftTime = 0;
            }

            base.Update(tick);
        }

        public override void SendResource()
        {
            if (_lastResource != 0)
                myPlayer.OSInterface.RemoveEffect(_armStates[(byte)(_lastResource - 1)]);
            if (_careerResource != 0)
                myPlayer.OSInterface.AddEffect(_armStates[(byte)(_careerResource - 1)]);
        }

        public override EArchetype GetArchetype()
        {
            return EArchetype.ARCHETYPE_DPS;
        }
    }
}
