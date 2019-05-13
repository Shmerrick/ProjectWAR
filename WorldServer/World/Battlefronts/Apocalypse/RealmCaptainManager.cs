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
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public static class RealmCaptainManager
    {
        public static int MARK_PLAYER_REALM_CAPTAIN = 1;
        public static int UNMARK_PLAYER_REALM_CAPTAIN = 0;
        public static int REALM_CAPTAIN_TELL_CHANCE = 100;
        public static int REALM_CAPTAIN_MINIMUM_CONTRIBUTION = 100;
        public static int REALM_CAPTAIN_MINIMUM_PLAYERS = 30;

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


                    MarkPlayerAsRealmCaptain(status.DestructionRealmCaptain, Player._Players, UNMARK_PLAYER_REALM_CAPTAIN);
                    MarkPlayerAsRealmCaptain(status.OrderRealmCaptain, Player._Players, UNMARK_PLAYER_REALM_CAPTAIN);

                    status.RemoveAsRealmCaptain(status.DestructionRealmCaptain);
                    status.RemoveAsRealmCaptain(status.OrderRealmCaptain);

                    RemoveRealmCaptainBuffs(status.DestructionRealmCaptain);
                    RemoveRealmCaptainBuffs(status.OrderRealmCaptain);

                    // Destruction
                    if (realmCaptains[0] != null)
                    {
                        status.SetAsRealmCaptain(realmCaptains[0]);

                        MarkPlayerAsRealmCaptain(realmCaptains[0], zonePlayers, MARK_PLAYER_REALM_CAPTAIN);

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
                        MarkPlayerAsRealmCaptain(realmCaptains[1], zonePlayers, MARK_PLAYER_REALM_CAPTAIN);
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

        private static void RemoveRealmCaptainBuffs(Player player)
        {
            if (player == null)
                return;
            player.BuffInterface.RemoveBuffByEntry(28115);
            player.BuffInterface.RemoveBuffByEntry(28116);
            player.BuffInterface.RemoveBuffByEntry(28117);
            player.BuffInterface.RemoveBuffByEntry(28118);
        }

        public static void MarkPlayerAsRealmCaptain(Player player, List<Player> playersToAnnounce, int upDown)
        {
            if (player == null) return;
            if (playersToAnnounce == null) return;

            if (upDown == MARK_PLAYER_REALM_CAPTAIN)
                player.EffectStates.Add((byte)ObjectEffectState.OBJECTEFFECTSTATE_CARRYING_BANNER);
            if (upDown == UNMARK_PLAYER_REALM_CAPTAIN)
                player.EffectStates.Remove((byte)ObjectEffectState.OBJECTEFFECTSTATE_CARRYING_BANNER);


            foreach (var announce in playersToAnnounce)
            {
                //announce.DispatchPacket(Out, true);
                player.SendMeTo(announce);
            }

            var Out = new PacketOut((byte)Opcodes.F_OBJECT_EFFECT_STATE);

            Out.WriteUInt16(player.Oid);
            Out.WriteByte(1);
            Out.WriteByte((byte)ObjectEffectState.OBJECTEFFECTSTATE_CARRYING_BANNER);
            Out.WriteByte((byte)(upDown));
            Out.WriteByte(0);

            player.DispatchPacket(Out, true);
        }


        public static bool IsPlayerRealmCaptain(uint characterId)
        {
            var status = WorldMgr.UpperTierCampaignManager.GetActiveCampaign().ActiveBattleFrontStatus;
            return status.DestructionRealmCaptain?.CharacterId == characterId ||
                   status.OrderRealmCaptain?.CharacterId == characterId;
        }

        public static void ApplyRealmCaptainBuff(Player plr, ushort infoSpellId)
        {
            if (plr == null)
                return;
            // Remove any other RC buffs
            plr.BuffInterface.RemoveBuffByEntry(28115);
            plr.BuffInterface.RemoveBuffByEntry(28116);
            plr.BuffInterface.RemoveBuffByEntry(28117);
            plr.BuffInterface.RemoveBuffByEntry(28118);

            plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.Level,
                AbilityMgr.GetBuffInfo(infoSpellId)));
        }
    }
}
