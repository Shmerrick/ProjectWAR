using SystemData;
using GameData;

namespace WorldServer
{
    class CareerInterface_KnightChosen : CareerInterface
    {
        public CareerInterface_KnightChosen(Player player) : base(player)
        {
            
        }

        public override void SendResource()
        {
        }

        public override void NotifyInitialized()
        {
            myPlayer.SendClientMessage("This class has modifications. Enter the command \".ab changelist\" to see the changelist.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        }

        public override void DisplayChangeList()
        {
            if (myPlayer.Info.CareerLine == (int)CareerLine.CAREERLINE_KNIGHT)
            {
                myPlayer.SendClientMessage("Global changes to Knight of the Blazing Sun:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ It is no longer necessary to twist auras. You may run 3 at a time.");
                myPlayer.SendClientMessage("- The tactic Destroy Confidence is no longer available.");
                myPlayer.SendClientMessage("- The tactic Dirty Tricks improves group critical hit rate by 5% rather than 10%.");
                myPlayer.SendClientMessage("- The tactic Encouraged Aim requires a greatsword in order to work.");
                myPlayer.SendClientMessage("- The tactic Bellow Commands increases your AP regeneration rate by 15.");
            }

            else
            {
                myPlayer.SendClientMessage("Global changes to Chosen:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ It is no longer necessary to twist auras. You may run 3 at a time.");
                myPlayer.SendClientMessage("- The tactic Bellow Commands increases your AP regeneration rate by 15.");
            }
        }
    }
}
