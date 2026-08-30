using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Land of the Dead expedition commands under .lotd</summary>
    internal class LotdCommands
    {
        [CommandAttribute(EGmLevel.GM, "Shows the Land of the Dead expedition tracker state. Usage: .lotd status")]
        public static void Status(Player plr, string unused = null)
        {
            SendCsr(plr, LotdService.GetStatusSummary());

            if (plr != null)
                SendCsr(plr, "Your realm can currently reach the expedition: "
                             + (LotdService.CanRealmAccessLotd(plr.Realm) ? "yes" : "no"));
        }

        [CommandAttribute(EGmLevel.Developer, "Opens the expedition for a realm. Usage: .lotd unlock <1=Order|2=Destruction>")]
        public static void Unlock(Player plr, int realmId)
        {
            Realms realm = (Realms)realmId;

            if (!LotdService.ForceUnlock(realm))
            {
                SendCsr(plr, "Could not unlock. Use 1 for Order or 2 for Destruction, and confirm the tracker loaded with .lotd status");
                return;
            }

            SendCsr(plr, "Expedition opened for " + realm + ". " + LotdService.GetStatusSummary());
        }

        [CommandAttribute(EGmLevel.Developer, "Returns the expedition to the accumulating race. Usage: .lotd reset")]
        public static void Reset(Player plr, string unused = null)
        {
            if (!LotdService.ForceReset())
            {
                SendCsr(plr, "Could not reset; the tracker is not loaded.");
                return;
            }

            SendCsr(plr, "Expedition race reset. " + LotdService.GetStatusSummary());
        }

        [CommandAttribute(EGmLevel.Developer, "Awards resource points as a battlefront lock would. Usage: .lotd award <1=Order|2=Destruction> <points>")]
        public static void Award(Player plr, int realmId, int points)
        {
            Realms realm = (Realms)realmId;

            if (!LotdService.ForceAwardPoints(realm, points))
            {
                SendCsr(plr, "Could not award points. Use 1 for Order or 2 for Destruction and a positive point value.");
                return;
            }

            SendCsr(plr, LotdService.GetStatusSummary());
        }
    }
}
