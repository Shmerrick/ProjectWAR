using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer.World.Scripting.Items
{
    [GeneralScript(true, "PotionScript", 0, 0)]
    public class PotionScript : AGeneralScript
    {
        /*
        public override void OnWorldPlayerEvent(string EventName, Player Plr, object Data)
        {
            if (EventName != "USE_ITEM" || !(Data is Item))
                return;

            Item item = Data as Item;

            if (item.Info.Type == 31)
            {
                if (item.Info.ScriptName == "HealthPotion") // Potion
                {
                    Plr.DealHeal(Plr, (uint)item.Info.MinRank * 100);
                    Plr.ItmInterface.DeleteItem(item.SlotId, 1, true);
                }
            }
        }*/
    }
}
