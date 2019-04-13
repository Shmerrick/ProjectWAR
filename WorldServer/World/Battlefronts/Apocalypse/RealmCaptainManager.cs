using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemData;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Managers;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Map;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public static class RealmCaptainManager
    {
        public static int SCALE_MODEL_UP = 1;
        public static int SCALE_MODEL_DOWN = 0;
        public static int REALM_CAPTAIN_TELL_CHANCE = 10;
        public static int REALM_CAPTAIN_MINIMUM_CONTRIBUTION = 50;
        public static int REALM_CAPTAIN_MINIMUM_PLAYERS = 5;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void DetermineCaptains(IBattleFrontManager battleFrontManager, RegionMgr region)
        {
            var status = battleFrontManager.GetActiveCampaign().ActiveBattleFrontStatus;
            lock (status)
            {

                Logger.Trace($"Checking for new Realm Captains...");
                if (status.RegionId == region.RegionId)
                {
                    var zonePlayers = Player._Players.Where(x => !x.IsDisposed
                                                                 && x.IsInWorld()
                                                                 && x.CbtInterface.IsPvp
                                                                 && x.ScnInterface.Scenario == null
                                                                 && x.ZoneId == status.ZoneId).ToList();

                    if (zonePlayers.Count < REALM_CAPTAIN_MINIMUM_PLAYERS)
                    {
                        Logger.Trace($"Zone Players = {zonePlayers} - not enough for a Realm Captain to spawn");
                        return;
                    }

                    var realmCaptains = status.ContributionManagerInstance.GetHigestContributors(
                        REALM_CAPTAIN_MINIMUM_CONTRIBUTION, zonePlayers);


                    ScaleModel(status.DestructionRealmCaptain, zonePlayers, SCALE_MODEL_DOWN);
                    ScaleModel(status.OrderRealmCaptain, zonePlayers, SCALE_MODEL_DOWN);

                    status.RemoveAsRealmCaptain(status.DestructionRealmCaptain);
                    status.RemoveAsRealmCaptain(status.OrderRealmCaptain);


                    // Destruction
                    if (realmCaptains[0] != null)
                    {
                        status.SetAsRealmCaptain(realmCaptains[0]);

                        ScaleModel(realmCaptains[0], zonePlayers, SCALE_MODEL_UP);

                        if (StaticRandom.Instance.Next(100) < REALM_CAPTAIN_TELL_CHANCE)
                        {
                            foreach (var player in zonePlayers.Where(x => x.Realm == Realms.REALMS_REALM_ORDER))
                            {
                                player.SendMessage(
                                    $"A captain has emerged from the ranks of the enemy. Take the head of {realmCaptains[0].Name}!",
                                    ChatLogFilters.CHATLOGFILTERS_RVR);
                            }
                        }
                    }
                    // Order
                    if (realmCaptains[1] != null)
                    {
                        status.SetAsRealmCaptain(realmCaptains[1]);
                        ScaleModel(realmCaptains[1], zonePlayers, SCALE_MODEL_UP);
                        if (StaticRandom.Instance.Next(100) < REALM_CAPTAIN_TELL_CHANCE)
                        {
                            foreach (var player in zonePlayers.Where(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION))
                            {
                                player.SendMessage(
                                    $"A captain has emerged from the ranks of the enemy. Take the head of {realmCaptains[1].Name}!",
                                    ChatLogFilters.CHATLOGFILTERS_RVR);
                            }
                        }
                    }

                }
            }
        }

        public static void ScaleModel(Player player, List<Player> playersToAnnounce, int upDown)
        {
            if (player == null) return;
            if (playersToAnnounce == null) return;

            if (upDown == SCALE_MODEL_UP)
                player.EffectStates.Add((byte)ObjectEffectState.OBJECTEFFECTSTATE_SCALE_UP);
            if (upDown == SCALE_MODEL_DOWN)
                player.EffectStates.Remove((byte)ObjectEffectState.OBJECTEFFECTSTATE_SCALE_UP);


            foreach (var announce in playersToAnnounce)
            {
                //announce.DispatchPacket(Out, true);
                player.SendMeTo(announce);
            }
        }


        public static bool IsPlayerRealmCaptain(uint characterId)
        {
            var status = WorldMgr.UpperTierCampaignManager.GetActiveCampaign().ActiveBattleFrontStatus;
            return status.DestructionRealmCaptain?.CharacterId == characterId ||
                   status.OrderRealmCaptain?.CharacterId == characterId;
        }

        public static void ApplyRealmCaptainBuff(Player plr, ushort infoSpellId)
        {
            // Remove any other RC buffs
            plr.BuffInterface.RemoveBuffByEntry(28115);
            plr.BuffInterface.RemoveBuffByEntry(28116);
            plr.BuffInterface.RemoveBuffByEntry(28118);

            plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.Level,
                AbilityMgr.GetBuffInfo(infoSpellId)));
        }
    }
}
