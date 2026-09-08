using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldServer.World.Objects.Instances;
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
            {
                SendCsr(plr, "Your realm can currently reach the expedition: "
                             + (LotdService.CanRealmAccessLotd(plr.Realm) ? "yes" : "no"));
                SendCsr(plr, LotdService.GetTaxiDiagnostic(plr));
            }
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

        /// <summary>
        /// Lists the open lair copies and says, for each, whether the calling player could invade it
        /// at this instant.
        ///
        /// Invadability is a live answer, not a property of the instance: run this, flip the
        /// expedition with .lotd unlock, and run it again -- the same copies change from safe to
        /// invadable with nothing about them having changed. That is the behaviour to check, and it
        /// is why nothing caches it.
        /// </summary>
        [CommandAttribute(EGmLevel.GM, "Lists open Land of the Dead lair copies and who can invade them. Usage: .lotd instances")]
        public static void Instances(Player plr, string unused = null)
        {
            if (plr == null)
                return;

            SendCsr(plr, LotdService.GetStatusSummary());
            SendCsr(plr, "Your realm holds the expedition: "
                         + (LotdService.CanRealmAccessLotd(plr.Realm) ? "yes -- you may invade" : "no -- you may not invade"));

            int listed = 0;

            foreach (ushort zoneId in new ushort[] { 179, 241, 242, 243, 244 })
            {
                foreach (Instance instance in WorldMgr.InstanceMgr.GetOpenInstances(zoneId))
                {
                    ++listed;
                    string owner = instance.OwningRealm == 0
                        ? "unclaimed"
                        : ((Realms)instance.OwningRealm).ToString();

                    SendCsr(plr, "  " + (instance.Info != null ? instance.Info.Name : "zone " + zoneId)
                                 + " id " + instance.ID
                                 + " owner " + owner
                                 + " players " + (instance.Players != null ? instance.Players.Count : 0)
                                 + (WorldMgr.InstanceMgr.CanBeInvadedBy(instance, plr) ? " -- INVADABLE by you" : ""));
                }
            }

            if (listed == 0)
                SendCsr(plr, "  No lair copies are open.");
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
