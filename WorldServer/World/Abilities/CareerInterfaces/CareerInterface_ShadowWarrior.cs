using SystemData;

namespace WorldServer
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

        public override void NotifyInitialized()
        {
            myPlayer.SendClientMessage("This class has modifications. Enter the command \".ab changelist\" to see the changelist.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        }

        public override void DisplayChangeList()
        {
            myPlayer.SendClientMessage("Global changes to Shadow Warrior:", SystemData.ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("- Leading Shots improves critical hit rate by 8% instead of 15%.");
            myPlayer.SendClientMessage("The Path of Assault:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("- Disabled ranged AutoAttack when in Assault stance.");
            myPlayer.SendClientMessage("- Increased Melee AA speed proc to 33%");
            myPlayer.SendClientMessage("- Increased Melee AA damage while in Assault stance by 65%.");
            myPlayer.SendClientMessage("- Swift Strikes will now grant a movement speed boost for the duration of the ability being channeled.");
            myPlayer.SendClientMessage("- Merciless Soldier Tactic redesigned:");
            myPlayer.SendClientMessage("Removed AP reduction");
            myPlayer.SendClientMessage("Direct damage Assault melee attacks (Opportunistic Strike, Grim Slash, Exploit weakness, Counterstrike, Brutal assault, Sweeping Slash) will do 50% extra critical damage. The DoT (Draw Blood) and channel ability (Swift Strikes) have a 25% increased chance to critically hit. (technical reasons for the split functionality).");

            myPlayer.SendClientMessage("The Path of the Skirmisher:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("- Flanking shot is now core.");
            myPlayer.SendClientMessage("- Barrage is now 5 point ability.");
            myPlayer.SendClientMessage("- Shadow Sting is now 9 point ability.");
            myPlayer.SendClientMessage("- Eye Shot is now 13 point ability.");

            myPlayer.SendClientMessage("The Path of the Scout:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("- Fell the weak gains its additional damage modifier if the target has 30% of their HPs or below (up from 20%).");
            myPlayer.SendClientMessage("- Reduced the added AP cost of the No Quarter tactic to 13 (down from 20).");

        }
    }
}
