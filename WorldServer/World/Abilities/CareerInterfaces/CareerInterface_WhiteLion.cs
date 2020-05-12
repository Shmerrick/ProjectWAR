using System;
using Common;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    class CareerInterface_WhiteLion : CareerInterface, IPetCareerInterface
    {
        public new Pet myPet { get; set; }

        // This is variable that store current pet health
        uint currentHealth;

        public byte AIMode { get; set; } = 5;

        public string myPetName { get; set; }

        private static readonly ushort[] _petTrainBuffs = { 3954, 3955, 3951 };

        public CareerInterface_WhiteLion(Player player) : base(player)
        {
            _resourceTimeout = 0;
        }

        public override bool HasResource(byte amount)
        {
            return true;
        }

        // Used to set up pet buffs which should be reapplied if the lion's resummoned
        public override void SetResource(byte amount, bool blockEvent)
        {
            if (amount == 0 || amount == _careerResource)
                return;
            _careerResource = amount;

            if (myPet != null && !myPet.IsDead)
            {
                currentHealth = myPet.Health;
                myPet.BuffInterface.QueueBuff(new BuffQueueInfo(myPlayer, myPlayer.EffectiveLevel, AbilityMgr.GetBuffInfo(_petTrainBuffs[amount - 1], myPlayer, myPet)));
                myPet.EvtInterface.AddEvent(SetHealth, 100, 1);
            }
        }

        private void SetHealth()
        {
            if (myPet != null)
                myPet.Health = currentHealth;
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
                    myPet.Destroy();
                    myPet = null;
                }
                if (myPlayer.Zone == null)
                    return;

                ushort model1;
                byte faction;
                string name;

                if (myPlayer.Realm == GameData.Realms.REALMS_REALM_ORDER)
                {
                    model1 = (ushort)(132 + ((myPlayer.Level - 1) * 0.1f));
                    faction = 65;
                    if (myPlayer.Level < 16)
                        name = "Lion Cub";
                    else
                        name = "War Lion";
                }
                else
                {
                    Random rand = new Random();
                    switch (rand.Next(1, 4))
                    {
                        case 1:
                            model1 = 1156;
                            faction = 129;
                            name = "War Manticore";
                            break;
                        case 2:
                            model1 = 1142;
                            faction = 129;
                            name = "War Scorpion";
                            break;
                        case 3:
                            model1 = 1086;
                            faction = 129;
                            name = "Vicious Harpy";
                            break;
                        default:
                            model1 = 1272;
                            faction = 129;
                            name = "Hydra Wyrmling";
                            break;
                    }
                    
                }

                if (myPlayer.Info.PetModel != 0)
                {
                    model1 = myPlayer.Info.PetModel;
                }

                if (!String.IsNullOrEmpty(myPlayer.Info.PetName))
                    myPetName = myPlayer.Info.PetName;

                Creature_proto Proto = new Creature_proto
                {
                    Name = string.IsNullOrEmpty(myPetName) ? name : myPetName,
                    Ranged = 0,
                    Faction = faction,
                    Model1 = model1
                };

                Creature_spawn Spawn = new Creature_spawn();
                if (model1 == 1272)
                {
                    Proto.MinScale = 15;
                    Proto.MaxScale = 15;
                }
                else
                {
                    Proto.MinScale = 50;
                    Proto.MaxScale = 50;
                }
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

                myPet = new Pet(myID, Spawn, myPlayer, AIMode, false, true);

                myPlayer.Region.AddObject(myPet, Spawn.ZoneId);

                if (_careerResource != 0)
                {
                    //currentHealth = myPet.Health;
                    myPet.BuffInterface.QueueBuff(new BuffQueueInfo(myPlayer, myPlayer.EffectiveLevel, AbilityMgr.GetBuffInfo(_petTrainBuffs[_careerResource - 1], myPlayer, myPet)));
                    //myPet.EvtInterface.AddEvent(SetHealth, 100, 1);
                }

                myPlayer.BuffInterface.NotifyPetEvent(myPet);
            }
            finally
            {
                _summoning = false;
            }
        }

        public void Notify_PetDown()
        {
            if (myPet != null)
            {
                myPlayer.AbtInterface.SetCooldown(9159, 15000);
                myPlayer.BuffInterface.NotifyPetEvent(myPet);
                myPet = null;
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
