using System;
using SystemData;
using Common;
using WorldServer.Scenarios;

namespace WorldServer
{
    class CareerInterface_WhiteLion : CareerInterface, IPetCareerInterface
    {
        public Pet myPet { get; set; }

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

        public override void NotifyInitialized()
        {
            myPlayer.SendClientMessage("This class has modifications. Enter the command \".ab changelist\" to see the changelist.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        }

        public override void DisplayChangeList()
        {
            myPlayer.SendClientMessage("Global changes to White Lion:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("- Pounce's cooldown is 10 seconds, and it will grant a 3 second 35% speed boost to the White Lion when they land. This boost will be removed if the White Lion uses any ability.");
            myPlayer.SendClientMessage("- AOE splash damage no longer lands on Lions, AOEs can be anchored off them and they will take damage from an AOE if they are the primary target but they wont melt from splash any more so they will be more survivable UNLESS attacked directly where they will be weaker unless significant investment is place into the Guardian tree.");
            myPlayer.SendClientMessage("- Pet Wounds, Toughness, Weaponskill, Strength and Initiative are being reduced.");
            myPlayer.SendClientMessage("- You can build these stats back into the Lion pet with points in the various trees, the more points you have in the appropriate trees the more stats your Lion gains. The higher points in the tree give more to your Lion pet than the lower points do so investment brings benefits past the abilities in them.");
            myPlayer.SendClientMessage("- Points in Path of the Hunter grants Strength and Weaponskill to your Lion.");
            myPlayer.SendClientMessage("- Points in Path of the Axeman grants Toughness and Initiative to your Lion.");
            myPlayer.SendClientMessage("- Points in the Guardian Tree grants Wounds, Toughness, Weaponskill, Strength and Initiative.");

            myPlayer.SendClientMessage("Path of the Axeman:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("- Trained to Threaten - This ability now boosts the Lion pets Toughness and Wounds by 50% of the players Strength gained from items. The stance allows for the augmentation of the 'harassment' pet using dps main stat (as he should be dealing damage) and should prevent a full guardian built and spec'd WL from swapping to this stance and making an uber tank pet.");

            myPlayer.SendClientMessage("Path of the Guardian:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("- Trained to Kill - This ability now boosts the Lion pets Strength by the Wounds the player has from items and Weaponskill from the Toughness that a player has from items. The stance now correctly benefits those who play support to the Lion pet as opposed to giving the most benefit to those who go full on dps.");
            myPlayer.SendClientMessage("- 3 point tactic - Tactic - Furious Mending - Heal value increased.");
            myPlayer.SendClientMessage("- 5 point ability - Echoing Roar.");
            myPlayer.SendClientMessage("- 7 point tactic - Baited Trap - The armour increase additional from this tactic will now stack with potions.");
            myPlayer.SendClientMessage("- 9 point ability - Brutal Pounce.");
            myPlayer.SendClientMessage("- 11 point tactic - Stalker - This tactic now grants your Lion an increase in their Strength and Toughness (160 each stat at lvl 40).");
            myPlayer.SendClientMessage("- 13 point ability - Leonine Frenzy - Internal Cooldown on damage procs reduced to 1s, scales off the players Wounds.");
            myPlayer.SendClientMessage("- 15 point ability - Rampage.");
        }
    }
}
