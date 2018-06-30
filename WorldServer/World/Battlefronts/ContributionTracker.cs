using Common;
using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using SystemData;
using NLog;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.BattleFronts.Keeps;
using WorldServer.World.Objects.PublicQuests;


namespace WorldServer.World.BattleFronts
{
    internal class ContributionTracker
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private const float RENOWN_CONTRIBUTION_FACTOR = 0.1f;
        private const int CONTRIB_ELAPSE_INTERVAL = 60 * 60; // 1 hour of no contribution forfeits.

        private readonly int _tier;
        private readonly RegionMgr _region;

        private float WinnerShare = 1f;
        private float LoserShare = 0.1f;

        /// <summary>
        /// Used to compare RegionManagers within a tier, to catch out zone dodging nitwits.
        /// </summary>
        public ulong TotalContribFromRenown;

        protected readonly Dictionary<uint, ContributionInfo> PlayerContributions = new Dictionary<uint, ContributionInfo>();

        public ContributionTracker(int tier, RegionMgr region)
        {
            _tier = tier;
            _region = region;

            Reset();
        }

        /// <summary>
        /// <para>Adds contribution for a player. This is based on renown earned and comes from 4 sources at the moment:</para>
        /// <para>- Killing players.</para>
        /// <para>- Objective personal capture rewards.</para>
        /// <para>- Objective defense tick rewards.</para>
        /// <para>- Destroying siege weapons.</para>
        /// </summary>
        public void AddContribution(Player plr, uint contribution)
        {
            if (!plr.ValidInTier(_tier, true))
                return;

            ContributionInfo contrib;

            if (!PlayerContributions.TryGetValue(plr.CharacterId, out contrib))
            {
                contrib = new ContributionInfo(plr);
                PlayerContributions.Add(plr.CharacterId, contrib);
            }

            float contributionScaler;

            // Better rewards depending on group organization status.
            if (plr.WorldGroup == null)
            {
                contributionScaler = 1f;
                contrib.ActiveTimeEnd = TCPManager.GetTimeStamp() + 90; // 1.5 minutes for solo
            }
            else
            {
                int memberCount = plr.WorldGroup.TotalMemberCount;
                contributionScaler = 1f + memberCount * 0.05f;
                contrib.ActiveTimeEnd = TCPManager.GetTimeStamp() + 90 + (10 * memberCount); // 4.5 minutes for full warband
            }

            uint contribGain = (uint)(contribution * RENOWN_CONTRIBUTION_FACTOR * contributionScaler);
            contrib.BaseContribution += contribGain;
            TotalContribFromRenown += contribGain;
        }

        private readonly List<KeyValuePair<uint, ContributionInfo>> _toRemove = new List<KeyValuePair<uint, ContributionInfo>>(8);

        internal void TickContribution(long curTimeSeconds)
        {
            foreach (KeyValuePair<uint, ContributionInfo> kV in PlayerContributions)
            {
                var x = kV.Value;
                if (kV.Value.ActiveTimeEnd > curTimeSeconds)
                {
                    kV.Value.BaseContribution += (uint)(125 * _tier * RENOWN_CONTRIBUTION_FACTOR);
                }

                else if (curTimeSeconds - kV.Value.ActiveTimeEnd > CONTRIB_ELAPSE_INTERVAL)
                    _toRemove.Add(kV);
            }

            if (_toRemove.Count > 0)
            {
                Item_Info medallionInfo = ItemService.GetItem_Info((uint)(208399 + _tier));

                uint rpCap = (uint)(_tier * 7000);

                uint maxContrib = GetMaxContribution(PlayerContributions);

                foreach (var kVr in _toRemove)
                {
                    // Convert contribution to XP/RP based on current loss rates.
                    float contributionFactor = Math.Min(1f, kVr.Value.BaseContribution / (maxContrib * 0.7f));

                    uint rp = (uint)(Math.Min(rpCap, maxContrib * 1.5f * LoserShare * contributionFactor));
                    uint xp = rp * 5;
                    ushort medallionCount = (ushort)Math.Min(12, rp / (450 * _tier));

                    Player player = Player.GetPlayer(kVr.Key);

                    if (player != null)
                    {
                        player.SendClientMessage("You have received a reward for your contributions to a recent battle.", ChatLogFilters.CHATLOGFILTERS_RVR);

                        if (player.Region == _region)
                        {
                            player.AddXp(xp, false, false);
                            player.AddRenown(rp, false);

                            if (medallionCount > 0 && player.ItmInterface.CreateItem(medallionInfo, medallionCount) == ItemResult.RESULT_OK)
                                player.SendLocalizeString(new[] { medallionInfo.Name, medallionCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }

                        else
                        {
                            player.AddPendingXP(xp);
                            player.AddPendingRenown(rp);

                            if (medallionCount > 0)

                            {
                                Character_mail medallionMail = new Character_mail
                                {
                                    Guid = CharMgr.GenerateMailGuid(),
                                    CharacterId = player.CharacterId,
                                    CharacterIdSender = player.CharacterId,
                                    SenderName = player.Name,
                                    ReceiverName = player.Name,
                                    SendDate = (uint)TCPManager.GetTimeStamp(),
                                    Title = "Medallion Reward",
                                    Content = "You've received a medallion reward for your participation in a recent battle.",
                                    Money = 0,
                                    Opened = false
                                };
                                medallionMail.Items.Add(new MailItem(medallionInfo.Entry, medallionCount));
                                CharMgr.AddMail(medallionMail);
                            }
                        }
                    }

                    else
                    {
                        Character chara = CharMgr.GetCharacter(kVr.Key, false);

                        if (chara != null && chara.Value != null)
                        {
                            chara.Value.PendingXp += xp;
                            chara.Value.PendingRenown += rp;

                            CharMgr.Database.SaveObject(chara.Value);

                            if (medallionCount > 0)
                            {
                                Character_mail medallionMail = new Character_mail
                                {
                                    Guid = CharMgr.GenerateMailGuid(),
                                    CharacterId = chara.CharacterId,
                                    CharacterIdSender = chara.CharacterId,
                                    SenderName = chara.Name,
                                    ReceiverName = chara.Name,
                                    SendDate = (uint)TCPManager.GetTimeStamp(),
                                    Title = "Medallion Reward",
                                    Content = "You've received a medallion reward for your participation in a recent battle.",
                                    Money = 0,
                                    Opened = false
                                };
                                medallionMail.Items.Add(new MailItem(medallionInfo.Entry, medallionCount));
                                CharMgr.AddMail(medallionMail);
                            }
                        }
                    }

                    PlayerContributions.Remove(kVr.Key);
                }
                _toRemove.Clear();
            }
        }

        /// <summary>
        /// Gets a ream players contribution.
        /// </summary>
        /// <returns>Contribution infos indexed by character id</returns>
        public Dictionary<uint, ContributionInfo> GetContributorsFromRealm(Realms realm)
        {
            Dictionary<uint, ContributionInfo> newDic = new Dictionary<uint, ContributionInfo>();

            foreach (var contrib in PlayerContributions.Values)
            {
                if (contrib.PlayerRealm == realm)
                    newDic.Add(contrib.PlayerCharId, contrib);
            }

            return newDic;
        }

        [Obsolete] // Should remove ?
        public List<ContributionInfo> GetContributionListFromRealm(Realms realm)
        {
            List<ContributionInfo> newList = new List<ContributionInfo>();

            foreach (var contrib in PlayerContributions.Values)
            {
                if (contrib.PlayerRealm == realm)
                    newList.Add(contrib);
            }

            return newList;
        }

        /// <summary>
        /// Resets the contribution state.
        /// </summary>
        public void Reset()
        {
            PlayerContributions.Clear();
            TotalContribFromRenown = (ulong)(_tier * 50);
        }

        /// <summary>
        /// Create a gold chest in the given keep.
        /// </summary>
        /// <param name="keep">Taken keep</param>
        /// <param name="realm">Realm that gained control of the keep</param>
        internal void CreateGoldChest(Realms realm)
        {
            Dictionary<uint, ContributionInfo> contributors = GetContributorsFromRealm(realm);
            _logger.Debug($"Creating Gold Chest -- not implemented. Contributor Count = {contributors.Count}");
            if (contributors.Count > 0)
            {
                //Log.Info("BattleFront", $"Creating gold chest for {keep.Info.Name} for {contributors.Count} {((Realms)keep.Info.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")} contributors");
                //GoldChest.Create(_region, keep.Info.PQuest, ref contributors, (Realms)keep.Info.Realm == realm ? WinnerShare : LoserShare);
            }
        }

        /// <summary>
        /// Rewards players based on their contribution, converting it to XP, RP, Influence and Medallions.
        /// </summary>
        internal void HandleLockReward(Realms realm, float winnerRewardScale, string lockMessage, int zoneId, int tier)
        {
            /*
            Some general notes on this.

            The ticker, if kicked consistently by a player during the course of one hour, will grant contribution which converts to 3000 RP over 60 minutes.
            Rewards are based on a player's contribution performance relative to the best player on their realm.
            The RP from a lock cannot exceed Tier * 5000, and by extension, the XP cannot exceed Tier * 15000, 
            the Influence cannot exceed Tier * 500 and there can be no more than 6 medallions issued.

            The rewards are scaled by the proximity of a zone to the enemy fortress.

            The rewards are also scaled by the relative BattleFront scaler, which cripples rewards for players 
            refusing to fight in the most hotly contested zone.

            For the losing side, the reward is also scaled by the % of rewards, linked to the Victory Point pool.

            To receive the max losing reward, the top contributor on the losing realm must have contribution greater than 1/3 of the top attacker contribution.
            */

            string zoneName;
            if (zoneId == 0)
                zoneName = _region.ZonesInfo[0].Name + " and " + _region.ZonesInfo[1].Name;
            else
                zoneName = ZoneService.GetZone_Info((ushort)zoneId).Name;


            uint xpCap = (uint)(tier * 19000);
            uint rpCap = (uint)(tier * 10000);
            ushort infCap = (ushort)(tier * 2000);

            #region Init winner rewards
            Dictionary<uint, ContributionInfo> winnerContrib = GetContributorsFromRealm(realm);

            uint winMaxContrib = GetMaxContribution(winnerContrib);
            //Log.Info(zoneName, $"Winner contributor count : {winnerContrib.Count} Max contribution: {winMaxContrib}");

            uint winRP = (uint)(winMaxContrib * 1.5 * winnerRewardScale * BattleFrontConstants.LOCK_REWARD_SCALER);
            uint winXP = winRP * 4;
            ushort winInf = (ushort)(winRP * 0.25f);
            ushort winMedallionCount = (ushort)Math.Min(20, winRP / (450 * tier));

            //Log.Info(zoneName, $"Lock XP: {winXP} RP: {winRP} Inf: {winInf} Medals: {winMedallionCount}");

            #endregion

            #region Init loser rewards

            Dictionary<uint, ContributionInfo> loserContrib = GetContributorsFromRealm(realm == Realms.REALMS_REALM_ORDER ? Realms.REALMS_REALM_DESTRUCTION : Realms.REALMS_REALM_ORDER);

            uint loserMaxContrib = GetMaxContribution(loserContrib);

            //Log.Info(zoneName, $"Loser contributor count : {loserContrib.Count} Max contribution: {loserMaxContrib}");

            uint lossRP = (uint)(winRP * LoserShare * Math.Min(1f, (loserMaxContrib * 3) / (float)winMaxContrib));
            uint lossXP = lossRP * 5;
            ushort lossInf = (ushort)(lossRP * 0.35f);
            ushort lossMedallionCount = (ushort)Math.Min(15, winMedallionCount * LoserShare);

            //Log.Info(zoneName, $"Lock XP: {lossXP} RP: {lossRP} Inf: {lossInf} Medallions: {lossMedallionCount}");

            #endregion

            Item_Info medallionInfo = ItemService.GetItem_Info((uint)(208399 + tier));
            Item_Info T3Token = ItemService.GetItem_Info(2165);
            Item_Info T4Token = ItemService.GetItem_Info(2166);

            ushort tokenCount = 2;

            foreach (var kV in PlayerContributions)
            {
                Player plr = Player.GetPlayer(kV.Key);

                Character chara = plr != null ? plr.Info : CharMgr.GetCharacter(kV.Key, false);

                if (chara == null)
                    continue;

                if (plr != null)
                {
                    plr.SendLocalizeString(lockMessage, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                    plr.SendLocalizeString(lockMessage, realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE, Localized_text.CHAT_TAG_DEFAULT);
                    // AAO multiplier needs to be multiplied with 20 to get the AAO that player sees. 
                    // AAO mult is the global value for the server to grab the difference in size of the teams while AAOBonus is the players individual bonus
                    int aaoBuff = Convert.ToInt32(plr.AAOBonus);
                    if (aaoBuff < 0.1)
                        tokenCount = 1;
                    if (aaoBuff >= 2)
                        tokenCount = 3;
                    if (aaoBuff >= 3)
                        tokenCount = 4;
                    if (aaoBuff >= 4)
                        tokenCount = 5;
                }

                ContributionInfo contrib = kV.Value;

                if (chara.Realm == (int)realm)
                {
                    if (winRP == 0)
                        continue;

                    float contributionFactor = Math.Min(1f, contrib.BaseContribution / (winMaxContrib * 0.7f));

                    string contributionDesc;

                    if (contributionFactor > 0.75f)
                        contributionDesc = "valiant";
                    else if (contributionFactor > 0.5f)
                        contributionDesc = "stalwart";
                    else if (contributionFactor > 0.25f)
                        contributionDesc = "modest";
                    else
                        contributionDesc = "small";

                    plr?.SendClientMessage($"Your realm has captured {zoneName}, and you have been rewarded for your {contributionDesc} effort!", ChatLogFilters.CHATLOGFILTERS_RVR);

                    if (plr != null)
                    {
                        if (plr.Region == _region)
                        {
                            plr.AddXp(Math.Min(xpCap, (uint)(winXP * contributionFactor)), false, false);
                            if (plr.AAOBonus > 1)
                            {
                                plr.AddRenown(Math.Min(rpCap, (uint)(winRP * contributionFactor + (tokenCount * 100))), false, RewardType.ZoneKeepCapture, zoneName);
                                if (plr.CurrentArea != null)
                                {
                                    ushort influenceId = realm == Realms.REALMS_REALM_ORDER ? (ushort)plr.CurrentArea.OrderInfluenceId : (ushort)plr.CurrentArea.DestroInfluenceId;
                                    plr.AddInfluence(influenceId, Math.Min(infCap, (ushort)(winInf * contributionFactor + (tokenCount * 100))));
                                }
                            }
                            else
                            {
                                plr.AddRenown(Math.Min(rpCap, (uint)(winRP * contributionFactor)), false, RewardType.ZoneKeepCapture, zoneName);
                                if (plr.CurrentArea != null)
                                {
                                    ushort influenceId = realm == Realms.REALMS_REALM_ORDER ? (ushort)plr.CurrentArea.OrderInfluenceId : (ushort)plr.CurrentArea.DestroInfluenceId;
                                    plr.AddInfluence(influenceId, Math.Min(infCap, (ushort)(winInf * contributionFactor)));
                                }
                            }

                        }

                        else
                        {
                            plr.AddPendingXP(Math.Min(xpCap, (uint)(winXP * contributionFactor)));
                            plr.AddPendingRenown(Math.Min(rpCap, (uint)(winRP * contributionFactor)));
                        }
                    }

                    else if (chara.Value != null)
                    {
                        chara.Value.PendingXp += Math.Min(xpCap, (uint)(winXP * contributionFactor));
                        chara.Value.PendingRenown += Math.Min(rpCap, (uint)(winRP * contributionFactor));
                        CharMgr.Database.SaveObject(chara.Value);
                    }

                    ushort resultantCount = (ushort)(winMedallionCount * contributionFactor);
                    /*
                    if (plr != null && plr.AAOBonus > 1 && plr.Region == Region)
                    {
                        resultantCount = Convert.ToUInt16(resultantCount * ((AgainstAllOddsMult * 20)/100));
                    }
                    */
                    if (contributionFactor > 0 && plr != null)
                    {
                        if ((tier == 2 || tier == 3) && plr.ItmInterface.CreateItem(T3Token, tokenCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { T3Token.Name, tokenCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                        if (tier == 4 && plr.ItmInterface.CreateItem(T4Token, tokenCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { T4Token.Name, tokenCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                    }

                    if (resultantCount > 0)
                    {
                        if (plr != null && plr.Region == _region && plr.ItmInterface.CreateItem(medallionInfo, resultantCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { medallionInfo.Name, resultantCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }

                        else
                        {
                            Character_mail medallionMail = new Character_mail
                            {
                                Guid = CharMgr.GenerateMailGuid(),
                                CharacterId = chara.CharacterId,
                                CharacterIdSender = chara.CharacterId,
                                SenderName = chara.Name,
                                ReceiverName = chara.Name,
                                SendDate = (uint)TCPManager.GetTimeStamp(),
                                Title = "Medallion Reward",
                                Content = "You've received a medallion reward for your realm's victory in a recent battle in which you were a participant.",
                                Money = 0,
                                Opened = false
                            };
                            medallionMail.Items.Add(new MailItem(medallionInfo.Entry, resultantCount));
                            if (tier == 2 || tier == 3)
                            {
                                medallionMail.Items.Add(new MailItem(T3Token.Entry, tokenCount));
                            }
                            if (tier == 4)
                            {
                                medallionMail.Items.Add(new MailItem(T4Token.Entry, tokenCount));
                            }
                            CharMgr.AddMail(medallionMail);
                        }
                    }
                }

                else
                {
                    if (lossRP == 0)
                        continue;

                    float scaleFactor = Math.Min(1f, contrib.BaseContribution / (loserMaxContrib * 0.7f));

                    string contributionDesc;

                    if (scaleFactor > 0.75f)
                        contributionDesc = "valiant";
                    else if (scaleFactor > 0.5f)
                        contributionDesc = "stalwart";
                    else if (scaleFactor > 0.25f)
                        contributionDesc = "modest";
                    else
                        contributionDesc = "small";

                    plr?.SendClientMessage($"Though your realm lost {zoneName}, you have been rewarded for your {contributionDesc} effort in defense.", ChatLogFilters.CHATLOGFILTERS_RVR);

                    if (plr != null)
                    {
                        if (plr.Region == _region)
                        {
                            plr.AddXp((uint)Math.Min(xpCap * 0.9f, lossXP * scaleFactor), false, false);
                            if (plr.AAOBonus > 1)
                            {
                                plr.AddRenown((uint)Math.Min(rpCap * 0.9f, lossRP * scaleFactor + (tokenCount * 100)), false, RewardType.ObjectiveDefense, zoneName);
                                if (plr.CurrentArea != null)
                                {
                                    ushort influenceId = realm == Realms.REALMS_REALM_ORDER ? (ushort)plr.CurrentArea.OrderInfluenceId : (ushort)plr.CurrentArea.DestroInfluenceId;
                                    plr.AddInfluence(influenceId, (ushort)Math.Min(infCap * 0.9f, lossInf * scaleFactor + (tokenCount * 100)));
                                }
                            }
                            else
                            {
                                plr.AddRenown((uint)Math.Min(rpCap * 0.9f, lossRP * scaleFactor), false, RewardType.ObjectiveDefense, zoneName);
                                if (plr.CurrentArea != null)
                                {
                                    ushort influenceId = realm == Realms.REALMS_REALM_ORDER ? (ushort)plr.CurrentArea.OrderInfluenceId : (ushort)plr.CurrentArea.DestroInfluenceId;
                                    plr.AddInfluence(influenceId, (ushort)Math.Min(infCap * 0.9f, lossInf * scaleFactor));
                                }
                            }

                        }

                        else
                        {
                            plr.AddPendingXP((uint)Math.Min(xpCap * 0.9f, lossXP * scaleFactor));
                            plr.AddPendingRenown((uint)Math.Min(rpCap * 0.9f, lossRP * scaleFactor));
                        }
                    }

                    else if (chara.Value != null)
                    {
                        chara.Value.PendingXp += (uint)Math.Min(xpCap * 0.9f, lossXP * scaleFactor);
                        chara.Value.PendingRenown += (uint)Math.Min(rpCap * 0.9f, lossRP * scaleFactor);
                        CharMgr.Database.SaveObject(chara.Value);
                    }

                    ushort resultantCount = (ushort)(lossMedallionCount * scaleFactor);
                    /*
                    if (plr != null && plr.AAOBonus > 1 && plr.Region == Region)
                    {
                        resultantCount = Convert.ToUInt16(resultantCount * ((AgainstAllOddsMult * 20)/100));
                    }
                    */
                    if (scaleFactor > 0 && plr != null)
                    {
                        if ((tier == 2 || tier == 3) && plr.ItmInterface.CreateItem(T3Token, tokenCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { T3Token.Name, tokenCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                        if (tier == 4 && plr.ItmInterface.CreateItem(T4Token, tokenCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { T4Token.Name, tokenCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                    }
                    if (resultantCount > 0)
                    {
                        if (plr != null && plr.Region == _region && plr.ItmInterface.CreateItem(medallionInfo, resultantCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { medallionInfo.Name, resultantCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                        else
                        {
                            Character_mail medallionMail = new Character_mail
                            {
                                Guid = CharMgr.GenerateMailGuid(),
                                CharacterId = chara.CharacterId,
                                CharacterIdSender = chara.CharacterId,
                                SenderName = chara.Name,
                                ReceiverName = chara.Name,
                                SendDate = (uint)TCPManager.GetTimeStamp(),
                                Title = "Medallion Reward",
                                Content = "You've received a medallion reward for your participation in a recent battle.",
                                Money = 0,
                                Opened = false
                            };
                            medallionMail.Items.Add(new MailItem(medallionInfo.Entry, resultantCount));
                            if (tier == 2 || tier == 3)
                            {
                                medallionMail.Items.Add(new MailItem(T3Token.Entry, tokenCount));
                            }
                            if (tier == 4)
                            {
                                medallionMail.Items.Add(new MailItem(T4Token.Entry, tokenCount));
                            }
                            CharMgr.AddMail(medallionMail);
                        }
                    }
                }
            }

            PlayerContributions.Clear();
        }

        internal void CreateGoldChest(Keep keep, Realms realm)
        {
            throw new NotImplementedException();
        }

        internal void UpdateLoserShare(int _orderCount, int _destroCount)
        {
            int minPlayerCount = Math.Min(_orderCount, _destroCount);

            if (LoserShare < 0.6f)
                LoserShare += minPlayerCount * 0.01f * 0.015f;
        }

        /// <summary>
        /// Utility method returning maximum contribution in a contriution map.
        /// </summary>
        /// <param name="contribs">Contribution infos indexed by character id</param>
        /// <returns>Maximum contribution or 1</returns>
        private uint GetMaxContribution(Dictionary<uint, ContributionInfo> contribs)
        {
            uint max = 1;
            foreach (ContributionInfo info in contribs.Values)
                if (info.BaseContribution > max)
                    max = info.BaseContribution;
            return max;
        }

    }
}
