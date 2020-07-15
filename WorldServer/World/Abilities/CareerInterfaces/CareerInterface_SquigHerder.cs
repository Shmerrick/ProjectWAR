using Common;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    class CareerInterface_SquigHerder : CareerInterface, IPetCareerInterface
    {
        private Pet _myPet;
        public new Pet myPet { get { return _myPet; } set { _myPet = value; } }

        private byte _AIMode = 5;
        public byte AIMode { get { return _AIMode; } set { _AIMode = value; } }

        private ushort _currentPetID;

        public CareerInterface_SquigHerder(Player player) : base(player)
        {
            _resourceTimeout = 0;
            _careerResource = 2;
        }

        public override bool HasResource(byte amount)
        {
            return (amount == CareerResource || amount == 0);
        }

        // Used for Squig Armor
        public override void SetResource(byte amount, bool blockEvent)
        {
            if (amount == 0)
                return;

            _careerResource = amount; //_careerResource == 1 ? (byte)2 : (byte)1;
        }

        public override void SendResource()
        {

        }

        private bool _summoning;
        public void SummonPet(ushort myID)
        {
            if (_summoning)
                return;
            try
            {
                _summoning = true; // Happens when pet is automatically reset after zone change
                if (myPet != null)
                {
                    myPet.ReceiveDamage(myPet, uint.MaxValue);
                    myPet = null;
                }
                if (myPlayer.Zone == null)
                    return;

                _currentPetID = myID;

                Creature_proto Proto = new Creature_proto { Faction = 129 };

                switch (myID)
                {
                    case 1841:
                        Proto.Name = myPlayer.Name + "'s Squig";
                        Proto.Model1 = 136;
                        Proto.Ranged = 0;
                        break;
                    case 1842:
                        Proto.Name = myPlayer.Name + "'s Horned Squig";
                        Proto.Model1 = 137;
                        Proto.Ranged = 0;
                        break;
                    case 1843:
                        Proto.Name = myPlayer.Name + "'s Gas Squig";
                        Proto.Model1 = 139;
                        Proto.Ranged = 100;
                        break;
                    case 1844:
                        Proto.Name = myPlayer.Name + "'s Spiked Squig";
                        Proto.Model1 = 138;
                        Proto.Ranged = 100;
                        break;
                    case 1845:
                        Proto.Name = myPlayer.Name + "'s Battle Squig";
                        Proto.Model1 = 140;
                        Proto.Ranged = 0;
                        break;
                    case 2828:
                        Proto.Name = myPlayer.Name + "'s Horned Squig";
                        Proto.Model1 = 137;
                        Proto.Ranged = 0;
                        break;
                    case 2829:
                        Proto.Name = myPlayer.Name + "'s Gas Squig";
                        Proto.Model1 = 139;
                        Proto.Ranged = 100;
                        break;
                }

                Creature_spawn Spawn = new Creature_spawn();


                Proto.MinScale = 50;
                Proto.MaxScale = 50;
                Spawn.BuildFromProto(Proto);
                Spawn.WorldO = myPlayer._Value.WorldO;
                Spawn.WorldY = myPlayer._Value.WorldY;
                Spawn.WorldZ = myPlayer._Value.WorldZ;
                Spawn.WorldX = myPlayer._Value.WorldX;
                Spawn.ZoneId = myPlayer.Zone.ZoneId;
                Spawn.Icone = 18;
                Spawn.WaypointType = 0;
                Spawn.Proto.MinLevel = Spawn.Proto.MaxLevel = myPlayer.EffectiveLevel;

                if (Spawn.Proto.MinLevel > 40)
                { 
                    Spawn.Proto.MinLevel = 40;
                    Spawn.Proto.MaxLevel = 40;
                }

                _myPet = new Pet(myID, Spawn, myPlayer, _AIMode, false, true);

                if (myID == 1844)
                    myPet.BuffInterface.QueueBuff(new BuffQueueInfo(myPet, 1, AbilityMgr.GetBuffInfo(14)));

                myPlayer.Region.AddObject(_myPet, Spawn.ZoneId);

                myPlayer.BuffInterface.NotifyPetEvent(myPet);
            }
            finally
            {
                _summoning = false;
            }
        }

        public void Notify_PetDown()
        {
            #warning FIXME - implement with InvokeCooldown
            // For I Got Lots
            if (myPlayer.BuffInterface.GetBuff(1862, null) == null)
                myPlayer.AbtInterface.SetCooldown(_currentPetID, 30000);
            myPlayer.BuffInterface.NotifyPetEvent(myPet);
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
