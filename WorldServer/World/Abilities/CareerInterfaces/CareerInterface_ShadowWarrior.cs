using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    class CareerInterface_ShadowWarrior : CareerInterface
    {
        public CareerInterface_ShadowWarrior(Player player) : base(player)
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
                default: return true;
            }
        }

        public override void SetResource(byte amount, bool blockEvent)
        {
            if (_careerResource == amount || amount == 0)
                return;
            _lastResource = _careerResource;
            _careerResource = amount;
            SendResource();
        }

        public override bool AddResource(byte amount, bool blockEvent)
        {
            return true;
        }

        public override bool ConsumeResource(byte amount, bool blockEvent)
        {
            return true;
        }

        public override void SendResource()
        {
        }

        public override EArchetype GetArchetype()
        {
            return EArchetype.ARCHETYPE_DPS;
        }
    }
}
