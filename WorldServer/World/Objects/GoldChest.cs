using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects.PublicQuests;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{

    static class RandomEnumerable
    {
        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.RandomElementUsing<T>(new Random());
        }

        public static T RandomElementUsing<T>(this IEnumerable<T> enumerable, Random rand)
        {
            if (enumerable.Count() <= 0)
            {
                return default(T);
            }
            int index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }
    }

    public class GoldBag
    {
        public uint bagWon;
        public byte careerLine;
        public string plrName;
        public GoldBag(uint OurBagWon, byte OurPlrCareer, string OurPlrName)
        {
            bagWon = OurBagWon;
            careerLine = OurPlrCareer;
            plrName = OurPlrName;
        }
    }

    public class GoldChest : GameObject
    {
        const int white = 0, green = 1, blue = 2, purple = 3, gold = 4;
        double goldChance = 0.01, purpChance = 0.05, blueChance = 0.1, greenChance = 0.15, whiteChance = 0.2;

        private readonly byte[] _bags =
        {
            1, // wht
            0, // grn
            0, // blu
            0, // prp
            0  // gld
        };

        private readonly int[] _bpools =
        {
            0,
            1,
            2,
            3,
            4
        };

        private readonly byte[] _availableBags = new byte[5];

        List<KeyValuePair<uint, ContributionInfo>> _preRoll;
        List<KeyValuePair<uint, ContributionInfo>> _postRoll;
        List<Characters_bag_pools> _bagPools;
        Dictionary<uint, GoldBag> _lootBags = new Dictionary<uint, GoldBag>();

        private readonly PQuest_Info _publicQuestInfo;

        public GoldChest(GameObject_spawn spawn, PQuest_Info info, ref Dictionary<uint, ContributionInfo> players, float bagCountMod, RegionMgr region)
        {
            Spawn = spawn;
            _publicQuestInfo = info;
            //Note: _amountOfBags is the original calulation for generating bags, testing math.min to see if this is the cause of the empty bags (clientside limitations)
            int _amountOfBags = (int)(players.Count * bagCountMod);
            if (info.PQType == 2 && WorldMgr.WorldSettingsMgr.GetGenericSetting(16) == 0)
            {
                //foreach (ContributionInfo contrib in players.Values)
                //{
                //    RollForPersonalBag(contrib, 1f, players, region);

                //}
                EvtInterface.AddEvent(Destroy, 180 * 1000, 1);
                // AssignPersonalLoot(player);
            }
            else  //PQs still use the original roll system
            {
                GenerateLootBags(Math.Min(_amountOfBags, 24));
                // Note - if there are more than 42 players, the lowest rarity bag should be green.

#warning if there are more than 114 players, 25 bags will be awarded. Nalgol has the PQ boards restricted to 24 players, for an unknown reason. This could cause something to break.

                AssignLoot(players);

                foreach (KeyValuePair<uint, ContributionInfo> playerRoll in players)
                    Scoreboard(playerRoll.Value, _preRoll.IndexOf(playerRoll), _postRoll.IndexOf(playerRoll));

                EvtInterface.AddEvent(Destroy, 180 * 1000, 1);

            }
        }

        private void GenerateLootBags(int playerCount)
        {
            //generate lootbags
            // RB   5/14/2016   Establish minimums based on type.        
            switch (_publicQuestInfo.PQType)
            {
                // RB   5/14/2016   PvE PQs have bag counts based on rarity and random chance.
                case 1:
                    switch ((PublicQuestDifficulty)(_publicQuestInfo.PQDifficult - 1))
                    {
                        case PublicQuestDifficulty.Easy:
                            _bags[gold] = 0;
                            if ((StaticRandom.Instance.NextDouble() * 100d) < (5 + playerCount / 2))
                                _bags[gold]++;
                            _bags[purple] = 0;
                            if ((StaticRandom.Instance.NextDouble() * 100d) < (10 + playerCount / 2))
                                _bags[purple]++;
                            _bags[blue] = 0;
                            if ((StaticRandom.Instance.NextDouble() * 100d) < (25 + playerCount))
                                _bags[blue]++;
                            _bags[green] = 2;
                            _bags[white] = 2;
                            break;
                        case PublicQuestDifficulty.Medium:
                            _bags[gold] = 0;
                            _bags[purple] = 0;
                            if ((StaticRandom.Instance.NextDouble() * 100d) < (10 + (playerCount / 2)))
                                _bags[gold]++;
                            if ((StaticRandom.Instance.NextDouble() * 100d) < (15 + (playerCount / 2)))
                                _bags[purple]++;
                            _bags[blue] = 1;
                            if ((StaticRandom.Instance.NextDouble() * 100d) < (25 + playerCount))
                                _bags[blue]++;
                            _bags[green] = 3;
                            _bags[white] = 1;
                            break;
                        case PublicQuestDifficulty.Hard:
                            _bags[gold] = 1;
                            if ((StaticRandom.Instance.NextDouble() * 100d) < (25 + (playerCount / 2)))
                                _bags[gold]++;
                            _bags[purple] = 0;
                            if ((StaticRandom.Instance.NextDouble() * 100d) < (50 + (playerCount / 2)))
                                _bags[purple]++;
                            _bags[blue] = 2;
                            _bags[green] = 3;
                            _bags[white] = 0;
                            break;
                        default:
                            _bags[gold] = 0;
                            break;
                    }
                    break;

                case 2:
                    _bags[gold] = 1;
                    _bags[purple] = 0;
                    if ((StaticRandom.Instance.NextDouble() * 100d) < 50)
                        _bags[purple] = 1;
                    _bags[blue] = 1;
                    _bags[green] = 1;
                    _bags[white] = 1;

                    // For every 6 players, roll for an additional bag.
                    for (int i = 0; i < (playerCount / 6); i++)
                    {
                        double bagRoll = (StaticRandom.Instance.NextDouble() * 100d);

                        // Green bags, no maximum.
                        if (bagRoll < 50)
                            _bags[green]++;
                        // Blue bags, max 4. Cascade bag down through rarities if max is reached.
                        else if (bagRoll < 80)
                        {
                            if (_bags[blue] < 4)
                                _bags[blue]++;
                            else
                                _bags[green]++;
                        }
                        // Purple bags, max 2. Cascade bag down through rarities if max is reached.
                        else if (bagRoll < 90)
                        {
                            if (_bags[purple] < 2)
                                _bags[purple]++;
                            else
                            {
                                if (_bags[blue] < 4)
                                    _bags[blue]++;
                                else
                                    _bags[green]++;
                            }
                        }
                        // Gold bags, max 2. Cascade bag down through rarities if max is reached.
                        else
                        {
                            if (_bags[gold] < 2)
                                _bags[gold]++;
                            else
                            {
                                if (_bags[purple] < 2)
                                    _bags[purple]++;
                                else
                                {
                                    if (_bags[blue] < 4)
                                        _bags[blue]++;
                                    else
                                        _bags[green]++;
                                }
                            }
                        }

                    }
                    break;
            }

            if (_publicQuestInfo.PQType == 2)
                Log.Success("Player Count", playerCount.ToString());

            if (playerCount > 24)
                playerCount = 24;

            int bagcount = _bags[gold] + _bags[purple] + _bags[blue] + _bags[green] + _bags[white];

            if (_publicQuestInfo.PQType == 2)
                Log.Success("Bag Count", bagcount.ToString());

            // White bags are filler, if fewer than 12 bags were rewarded. (fewer than 42 players present)
            while (bagcount < playerCount / 2)
            {
                _bags[white]++;
                bagcount++;
            }
        }

        // Sevetar - commented out as it contains legacy RVR calls. Kept in as it might include useful logic
        //private void RollForPersonalBag(ContributionInfo player, float bagCountMod, Dictionary<uint, ContributionInfo> players, RegionMgr region)
        //{

        //    for (int i = 0; i < 5; ++i)
        //    {
        //        _availableBags[i] = 0;
        //    }
        //    for (int i = 0; i < 5; ++i)
        //    {
        //        _bags[i] = 0;
        //    }
        //    ProximityBattleFront bf = null;
        //    int aaoMult = 0;
        //    bool isBonusAppliedAndConsumed = true;
        //    Realms aaoRealm = Realms.REALMS_REALM_NEUTRAL;
        //    Player targPlayer = Player.GetPlayer(player.PlayerCharId);
        //    Character targCharacter = CharMgr.GetCharacter(player.PlayerCharId, true);
        //    if (region != null && region.Bttlfront != null && region.Bttlfront is ProximityBattleFront)
        //    {
        //        bf = region.Bttlfront as ProximityBattleFront;
        //        if (bf != null)
        //        {
        //            aaoMult = Math.Abs(bf._againstAllOddsMult);
        //            if (aaoMult != 0)
        //                aaoRealm = bf._againstAllOddsMult > 0 ? Realms.REALMS_REALM_DESTRUCTION : Realms.REALMS_REALM_ORDER;
        //        }

        //        if (targPlayer != null)
        //        {
        //            //T2 16-19, use bonus rolls
        //            if (region.GetTier() == 2)
        //            {
        //                if (targPlayer.Level < 16 || targPlayer.Level > 19)
        //                {
        //                    isBonusAppliedAndConsumed = false;
        //                }
        //            }
        //            //T3 20-29, use bonus rolls
        //            if (region.GetTier() == 3)
        //            {
        //                if (targPlayer.Level < 20 || targPlayer.Level > 29)
        //                {
        //                    isBonusAppliedAndConsumed = false;
        //                }
        //            }

        //            if (region.GetTier() == 4)
        //            {
        //                if (targPlayer.Level < 30)
        //                {
        //                    isBonusAppliedAndConsumed = false;
        //                }
        //            }
        //        }
        //    }
        //    //Which side is outnumbered?
        //    float aaoMultOutnumberedSide = aaoMult / 2.5f;
        //    //Which side is outnumbering the other side?
        //    float aaoMultOutnumberingSide = aaoMult / 2.5f;

        //    //Being outnumbered has a cap of 2, 10% bonus to your roll.
        //    if (aaoMultOutnumberedSide > 1f)
        //        aaoMultOutnumberedSide = 1f;

        //    //Being utnumbering has a cap of 40% penalty.
        //    if (aaoMultOutnumberingSide > 4f)
        //        aaoMultOutnumberingSide = 4f;

        //    //Divide by 10f to get sane multipliers for rolls
        //    aaoMultOutnumberedSide = aaoMultOutnumberedSide / 10f;
        //    aaoMultOutnumberingSide = aaoMultOutnumberingSide / 10f;

        //    const double goldChance = 0.01, purpChance = 0.05, blueChance = 0.1, greenChance = 0.15, whiteChance = 0.2;
        //    _preRoll = players.OrderByDescending(plrs => plrs.Value.BaseContribution).ToList();
        //    float acv = _preRoll.Sum(plrs => plrs.Value.BaseContribution) / _preRoll.Count;
        //    //handle roll value
        //    if (player.OptOutType == 1)
        //    {
        //        player.RandomBonus = 1;
        //        if (targPlayer != null)
        //        {
        //            targPlayer.SendLocalizeString(_publicQuestInfo.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_PUBLIC_QUEST_OPT_OUT_APPLIED);
        //        }
        //    }
        //    else if (player.BaseContribution > acv * .10)
        //    {
        //        player.RandomBonus = (ushort)RandomMgr.Next(0, 1000);

        //        int temporaryBonus = (int)player.RandomBonus;
        //        if (aaoRealm != Realms.REALMS_REALM_NEUTRAL && aaoMult != 0)
        //        {
        //            if (targPlayer != null)
        //            {
        //                targPlayer.SendClientMessage("Your roll has been adjusted due to your army's size.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        //            }
        //            if (player.PlayerRealm == aaoRealm)
        //            {
        //                temporaryBonus += (int)(player.RandomBonus * (aaoMultOutnumberedSide));
        //            }
        //            else
        //            {
        //                temporaryBonus -= (int)(player.RandomBonus * (aaoMultOutnumberingSide));
        //            }

        //        }
        //        player.RandomBonus = (uint)(Math.Max(2, temporaryBonus));
        //        if (targPlayer != null)
        //            targPlayer.SendClientMessage("You roll " + player.RandomBonus + ".", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        //    }
        //    else
        //    {
        //        player.RandomBonus = 1;
        //    }

        //    float contribFactor = player.BaseContribution > acv ? player.BaseContribution / acv : 1f;
        //    float malus = 0;

        //    //get player bag pools

        //    if (targCharacter != null)
        //    {
        //        if (targCharacter.Bag_Pools == null)
        //            targCharacter.Bag_Pools = new List<Characters_bag_pools>();

        //        _bagPools = targCharacter.Bag_Pools.OrderByDescending(bgpools => bgpools.Bag_Type).ToList();
        //        if (_bagPools.Count == 0)
        //        {
        //            foreach (int pool in _bpools)
        //            {
        //                Characters_bag_pools _bagPool = new Characters_bag_pools((int)player.PlayerCharId, pool, 0);
        //                targCharacter.Bag_Pools.Add(_bagPool);
        //                CharMgr.Database.AddObject(_bagPool);
        //            }
        //            _bagPools = targCharacter.Bag_Pools.OrderByDescending(bgpools => bgpools.Bag_Type).ToList();
        //        }
        //    }

        //    //roll for each bag type
        //    foreach (Characters_bag_pools pool in _bagPools)
        //    {
        //        player.ContributionBonus = (uint)pool.BagPool_Value;
        //        if (player.RandomBonus > 1)
        //        {
        //            if (pool.Bag_Type == 4)
        //            {
        //                if ((player.RandomBonus - malus + (isBonusAppliedAndConsumed ? pool.BagPool_Value : 0)) >= 1000 - (1000 * (WorldMgr.WorldSettingsMgr.GetGenericSetting(12) > 0 ? WorldMgr.WorldSettingsMgr.GetGenericSetting(12) / 1000d : goldChance * contribFactor)))
        //                {
        //                    _bags[gold] = 1;
        //                    if (isBonusAppliedAndConsumed)
        //                    {
        //                        if (targPlayer != null)
        //                        {
        //                            targPlayer.SendClientMessage("Your gold bag bonus roll has been consumed.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        //                        }
        //                        pool.BagPool_Value = 0;
        //                    }
        //                    if (targPlayer != null)
        //                    {
        //                        targPlayer.SendClientMessage("You have won a gold loot bag!", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        //                    }

        //                    break;
        //                }
        //                else
        //                {
        //                    if (isBonusAppliedAndConsumed)
        //                        pool.BagPool_Value += (int)((player.RandomBonus - (0) + pool.BagPool_Value) * .01);
        //                }
        //            }
        //            else if (pool.Bag_Type == 3)
        //            {
        //                if ((player.RandomBonus - malus + (isBonusAppliedAndConsumed ? pool.BagPool_Value : 0)) >= 1000 - (1000 * (WorldMgr.WorldSettingsMgr.GetGenericSetting(13) > 0 ? WorldMgr.WorldSettingsMgr.GetGenericSetting(13) / 1000d : purpChance * contribFactor)))
        //                {
        //                    _bags[purple] = 1;
        //                    if (isBonusAppliedAndConsumed)
        //                    {
        //                        if (targPlayer != null)
        //                        {
        //                            targPlayer.SendClientMessage("Your purple bag bonus roll has been consumed.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        //                        }
        //                        pool.BagPool_Value = 0;
        //                    }
        //                    if (targPlayer != null)
        //                        targPlayer.SendClientMessage("You have won a purple loot bag!", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

        //                    break;
        //                }
        //                else
        //                {
        //                    if (isBonusAppliedAndConsumed)
        //                        pool.BagPool_Value += (int)((player.RandomBonus - (0) + pool.BagPool_Value) * .02);
        //                }
        //            }
        //            else if (pool.Bag_Type == 2)
        //            {
        //                if ((player.RandomBonus + -malus + (isBonusAppliedAndConsumed ? pool.BagPool_Value : 0)) >= 1000 - (1000 * (WorldMgr.WorldSettingsMgr.GetGenericSetting(14) > 0 ? WorldMgr.WorldSettingsMgr.GetGenericSetting(14) / 1000d : blueChance * contribFactor)))
        //                {
        //                    _bags[blue] = 1;
        //                    if (isBonusAppliedAndConsumed)
        //                    {
        //                        if (targPlayer != null)
        //                        {
        //                            targPlayer.SendClientMessage("Your blue loot bag bonus roll has been consumed.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        //                            pool.BagPool_Value = 0;
        //                        }
        //                    }
        //                    if (targPlayer != null)
        //                        targPlayer.SendClientMessage("You have won a blue loot bag!", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        //                    break;
        //                }
        //                else
        //                {
        //                    if (isBonusAppliedAndConsumed)
        //                        pool.BagPool_Value += (int)((player.RandomBonus - (0) + pool.BagPool_Value) * .03);
        //                }
        //            }
        //            else if (pool.Bag_Type == 1)
        //            {
        //                if ((player.RandomBonus - malus + (isBonusAppliedAndConsumed ? pool.BagPool_Value : 0)) >= 1000 - (1000 * (WorldMgr.WorldSettingsMgr.GetGenericSetting(15) > 0 ? WorldMgr.WorldSettingsMgr.GetGenericSetting(15) / 1000d : greenChance * contribFactor)))
        //                {
        //                    _bags[green] = 1;
        //                    if (isBonusAppliedAndConsumed)
        //                    {
        //                        if (targPlayer != null)
        //                            targPlayer.SendClientMessage("Your green loot bag bonus roll has been consumed.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        //                        pool.BagPool_Value = 0;
        //                    }
        //                    if (targPlayer != null)
        //                        targPlayer.SendClientMessage("You have won a green loot bag!", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        //                    break;
        //                }
        //                else
        //                {
        //                    if (isBonusAppliedAndConsumed)
        //                        pool.BagPool_Value += (int)((player.RandomBonus - (0) + pool.BagPool_Value) * .04);
        //                }
        //            }
        //            else if (pool.Bag_Type == 0)
        //            {
        //                _bags[white] = 1;
        //                if (targPlayer != null)
        //                    targPlayer.SendClientMessage("You have won a white loot bag!", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        //                pool.BagPool_Value = 0;
        //            }
        //        }
        //    }
        //    foreach (Characters_bag_pools pool in _bagPools)
        //    {
        //        CharMgr.Database.SaveObject(pool);
        //    }

        //    for (int i = 0; i < 5; ++i)
        //    {
        //        _availableBags[i] = _bags[i];
        //    }


        //    byte bagWon = GetWonBagType(player.OptOutType == 2);
        //    if (bagWon == 0)
        //    {
        //        if (targPlayer != null)
        //            targPlayer.SendClientMessage("You have not contributed enough to this zone's capture, and thus have not rolled.");
        //        return;
        //    }
        //    //Log.Success("Winner", player.PlayerName + " Realm: " + player.PlayerRealm + " Bag Type: " + bagWon.ToString() + "Roll: " + player.RandomBonus + " Contrib: " + player.BaseContribution + " Avg Contrib: " + acv + " BonusConsumed: " + player.ContributionBonus);
        //    player.BagWon = bagWon;
        //    _lootBags.Add(player.PlayerCharId, new GoldBag(PublicQuest.GetBag(player.BagWon), player.PlayerCareerLine, player.PlayerName));
        //    PersonalScoreboard(player, bagWon);
        //}

        private void AssignLoot(Dictionary<uint, ContributionInfo> players)
        {
            _preRoll = players.OrderByDescending(plrs => plrs.Value.BaseContribution).ToList();

            ushort currentContribution = 750;

            // Handle contribution and random bonuses
            foreach (var kvpair in _preRoll)
            {
                ContributionInfo plrInfo = kvpair.Value;

                Player targPlayer = Player.GetPlayer(plrInfo.PlayerCharId);

                if (plrInfo.OptOutType == 1)
                {
                    plrInfo.RandomBonus = 1;
                    if (targPlayer != null)
                        targPlayer.SendLocalizeString(_publicQuestInfo.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_PUBLIC_QUEST_OPT_OUT_APPLIED);
                }
                /*else if (_publicQuestInfo.PQType == 2 && (_publicQuestInfo.PQTier == 2 || _publicQuestInfo.PQTier == 3) && plrInfo.Player.Level > 30)
                {
                    plrInfo.RandomBonus = 1;
                    plrInfo.Player.SendClientMessage("You have been skipped for the loot roll because your rank is too high.", ChatLogFilters.CHATLOGFILTERS_SAY);
                }*/
                else if (plrInfo.BaseContribution > 0)
                {
                    plrInfo.RandomBonus = (ushort)RandomMgr.Next(0, 750);
                    if (currentContribution > 45)
                    {
                        plrInfo.ContributionBonus = currentContribution;
                        if (currentContribution > 100)
                            currentContribution -= 50;
                        else if (currentContribution > 50)
                            currentContribution -= 5;
                    }
                }
                else
                    plrInfo.RandomBonus = 1;
            }

            _postRoll = players.OrderByDescending(plrs => plrs.Value.PersistenceBonus + plrs.Value.RandomBonus + plrs.Value.ContributionBonus).ToList();

            for (int i = 0; i < 5; ++i)
            {
                _availableBags[i] = _bags[i];
            }

            foreach (KeyValuePair<uint, ContributionInfo> contribution in _postRoll)
            {
                byte bagWon = GetWonBagType(contribution.Value.OptOutType == 2);

                // No more bags to be won at this index or any after it.
                if (bagWon == 0)
                    break;
                Log.Success("Winner", contribution.Value.PlayerName + " Realm: " + contribution.Value.PlayerRealm + " Bag Type: " + bagWon.ToString());

                contribution.Value.BagWon = bagWon;
                _lootBags.Add(contribution.Key, new GoldBag(PublicQuest.GetBag(contribution.Value.BagWon), contribution.Value.PlayerCareerLine, contribution.Value.PlayerName));
            }
        }

        public static void Create(RegionMgr region, PQuest_Info info, ref Dictionary<uint, ContributionInfo> players, float bagCountMod)
        {
            if (region == null)
            {
                Log.Error("GoldChest", "Attempt to create for NULL region");
                return;
            }

            if (info == null)
            {
                Log.Error("GoldChest", "NULL PQuest in Region " + region);
                return;
            }

            if (bagCountMod == 0.0f)
                return;

            GameObject_proto proto = GameObjectService.GetGameObjectProto(188);

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldO = 0,
                WorldY = info.GoldChestWorldY,
                WorldZ = info.GoldChestWorldZ,
                WorldX = info.GoldChestWorldX,
                ZoneId = info.ZoneId
            };

            spawn.BuildFromProto(proto);

            GoldChest chest = new GoldChest(spawn, info, ref players, bagCountMod, region);

            region.AddObject(chest, info.ZoneId);
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                foreach (KeyValuePair<uint, GoldBag> loot in _lootBags)
                {
                    Character_mail mail = new Character_mail
                    {
                        Guid = CharMgr.GenerateMailGuid(),
                        CharacterId = loot.Key,
                        SenderName = "Public Quest",
                        ReceiverName = loot.Value.plrName,
                        SendDate = (uint)TCPManager.GetTimeStamp(),
                        Title = "Public Quest Loot",
                        Content = "You won a Public Quest Loot Bag",
                        Money = 0,
                        Opened = false
                    };

                    //Mail.CharacterIdSender = plr.CharacterId;
                    MailItem item = GenerateBag(loot.Key);
                    if (item != null)
                    {
                        mail.Items.Add(item);
                        CharMgr.AddMail(mail);
                    }
                }
            }
            catch (NullReferenceException)
            {
                Log.Error("GoldChest", "Failed to mail loot.");
            }

            _lootBags = new Dictionary<uint, GoldBag>();

            base.Dispose();
        }

        public override void SendInteract(Player player, InteractMenu menu)
        {
            switch (menu.Menu)
            {
                case 15: // Close the loot
                    return;

                case 13: // Retrieve all items
                    TakeAll(player);
                    break;

                case 12: // Retrieve an item
                    TakeLoot(player);
                    break;
            }

            GoldBag bag;
            if (_lootBags.TryGetValue(player.CharacterId, out bag))
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 32);
                Out.WriteByte(4);
                Out.WriteByte(1);
                Out.WriteByte(1);
                Item.BuildItem(ref Out, null, ItemService.GetItem_Info(bag.bagWon), null, 0, 1);
                player.SendPacket(Out);
            }
            else
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 32);
                Out.WriteByte(4);
                Out.WriteByte(0);
                player.SendPacket(Out);
            }
        }

        public void TakeLoot(Player plr)
        {
            Character chara = CharMgr.GetCharacter(plr.CharacterId, false);

            MailItem items = GenerateBag(chara.CharacterId);

            ItemResult result = plr.ItmInterface.CreateItem(ItemService.GetItem_Info(items.id), 1, items.talisman, items.primary_dye, items.secondary_dye, false);

            if (result == ItemResult.RESULT_OK)
                _lootBags.Remove(plr.CharacterId);
            else if (result == ItemResult.RESULT_MAX_BAG)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_OVERAGE_CANT_LOOT);

        }

        public void TakeAll(Player player)
        {
            if (player.ItmInterface.GetTotalFreeInventorySlot() < 1)
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_OVERAGE_CANT_LOOT);
            else
                TakeLoot(player);
        }

        public MailItem GenerateBag(uint characterId)
        {
            GoldBag bag;
            byte bagtype = 0;
            if (!_lootBags.TryGetValue(characterId, out bag))
            {
                Log.Error("Bag ERROR: ", "There is no bag for player of ID: " + characterId);
                return null;
            }

            switch (bag.bagWon)
            {
                case 9940: bagtype = 1; break;
                case 9941: bagtype = 2; break;
                case 9942: bagtype = 3; break;
                case 9943: bagtype = 4; break;
                case 9980: bagtype = 5; break;
            }


            // need to generate loot talsimans are the items pri dye the crafting id secondary dye the money


            List<Talisman> items = new List<Talisman>();

            var results = from pqitems in PQuestService._PQLoot
                          where ((pqitems.PQType == _publicQuestInfo.PQType) && (pqitems.Career & (1 << bag.careerLine - 1)) != 0) && pqitems.Bag == bagtype && (pqitems.Chapter == _publicQuestInfo.Chapter || pqitems.PQTier == _publicQuestInfo.PQTier || pqitems.PQEntry == _publicQuestInfo.Entry)
                          select pqitems;


            if (results.Count() == 0)
            {
                Log.Error("Bag ERROR: ", "SQL returned 0 items for: " + bag.plrName + ", SQL statement: where PQType ==" + _publicQuestInfo.PQType + ", AND bitwise & " + (1 << bag.careerLine - 1) + ", AND bagtype " + bagtype + " AND chapter " + _publicQuestInfo.Chapter + " OR PQTeir " + _publicQuestInfo.PQTier + " OR PQEntry " + _publicQuestInfo.Entry);
            }

            uint itemid = 0;

            PQuest_Loot loot = null;

            if (results.Count() > 0)
            {
                loot = results.RandomElementUsing(StaticRandom.Instance);
            }

            if (loot != null)
                itemid = loot.ItemID;

            if (itemid > 0)
                items.Add(new Talisman(itemid, 1, 0, 0));


            // This is generating medallion rewards inside bags
            if (_publicQuestInfo.PQType == 2)
            {
                uint chestitem = 0;
                switch (_publicQuestInfo.PQTier)
                {
                    case 2: chestitem = 208470; break; //Was Scout now war crest
                    case 3: chestitem = 208470; break; //Was soldier now war cres
                    case 4: chestitem = 208470; break; // This was officer medallion previously, now it is conqueror
                    case 5: chestitem = 208470; break; //was conq
                    case 6: chestitem = 208470; break; //was fused inv

#warning need to add chests for fort city and so on

                }
                if (Constants.DoomsdaySwitch > 0 && (_publicQuestInfo.PQTier == 2 || _publicQuestInfo.PQTier == 3))
                {
                    chestitem = 208470; //was conq crest now war crest
                }

                items.Add(new Talisman(chestitem, (byte)(bagtype == 4 ? 5 : bagtype), 0, 0));

            }



            ushort money = (ushort)(100 * _publicQuestInfo.PQDifficult * _publicQuestInfo.Chapter * bagtype);
            return new MailItem(bag.bagWon, items, _publicQuestInfo.PQCraftingBag, money, 1);

        }
        public byte GetWonBagType(bool optOutGold)
        {
            for (int i = gold; i >= 0; --i)
            {
                if (_availableBags[i] > 0 && (!optOutGold || i != gold))
                {
                    --_availableBags[i];
                    return (byte)(i + 1);
                }
            }

            return 0;

            /*
            int cutoffIndex = 0;

            for (int i = gold; i >= 0; --i)
            {
                cutoffIndex += _bags[i];

                if (cutoffIndex >= pos)
                    return (byte)(i + 1);
            }

            return 0;
            */

            /*
            if (_bags[gold] >= pos)
                return 5;
            if (_bags[gold] + _bags[purple] >= pos)
                return 4;
            if (_bags[blue] + _bags[purple] + _bags[gold] >= pos)
                return 3;
            if (_bags[green] + _bags[blue] + _bags[purple] + _bags[gold] >= pos)
                return 2;
            if (_bags[white] + _bags[green] + _bags[blue] + _bags[purple] + _bags[gold] >= pos)
                return 1;
            return 0;
            */
        }

        public override void SendMeTo(Player plr)
        {
            // Log.Info("STATIC", "Creating static oid=" + Oid + " name=" + Name + " x=" + Spawn.WorldX + " y=" + Spawn.WorldY + " z=" + Spawn.WorldZ + " doorID=" + Spawn.DoorId);
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(VfxState); //ie: red glow, open door, lever pushed, etc

            Out.WriteUInt16((ushort)Spawn.WorldO);
            Out.WriteUInt16((ushort)Spawn.WorldZ);
            Out.WriteUInt32((uint)Spawn.WorldX);
            Out.WriteUInt32((uint)Spawn.WorldY);
            Out.WriteUInt16((ushort)Spawn.DisplayID);

            Out.WriteByte((byte)(Spawn.GetUnk(0) >> 8));

            // Get the database if the value hasnt been changed (currently only used for keep doors)
            if (Realm == Realms.REALMS_REALM_NEUTRAL)
                Out.WriteByte((byte)(Spawn.GetUnk(0) & 0xFF));
            else
                Out.WriteByte((byte)Realm);

            Out.WriteUInt16(Spawn.GetUnk(1));
            Out.WriteUInt16(Spawn.GetUnk(2));
            Out.WriteByte(Spawn.Unk1);

            int flags = Spawn.GetUnk(3);

            if (Realm != Realms.REALMS_REALM_NEUTRAL && !IsInvulnerable)
                flags |= 8; // Attackable (stops invalid target errors)

            LootContainer loots = LootsMgr.GenerateLoot(this, plr, 1);
            if ((loots != null && loots.IsLootable()) || (plr.QtsInterface.PublicQuest != null) || plr.QtsInterface.GameObjectNeeded(Spawn.Entry) || Spawn.DoorId != 0)
            {
                flags |= 4; // Interactable
            }

            Out.WriteUInt16((ushort)flags);
            Out.WriteByte(Spawn.Unk2);
            Out.WriteUInt32(Spawn.Unk3);
            Out.WriteUInt16(Spawn.GetUnk(4));
            Out.WriteUInt16(Spawn.GetUnk(5));
            Out.WriteUInt32(Spawn.Unk4);

            Out.WritePascalString(Name);

            if (Spawn.DoorId != 0)
            {
                Out.WriteByte(0x04);

                Out.WriteUInt32(Spawn.DoorId);
            }
            else
                Out.WriteByte(0x00);

            plr.SendPacket(Out);

            base.SendMeTo(plr);
        }

        public void Scoreboard(ContributionInfo playerRoll, int preIndex, int postIndex)
        {

            Player targPlayer = Player.GetPlayer(playerRoll.PlayerCharId);

            if (targPlayer == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_PQLOOT_TRIGGER, 1723);
            Out.WriteStringBytes(_publicQuestInfo.Name);
            Out.Fill(0, 24 - _publicQuestInfo.Name.Length);
            Out.WriteByte(_bags[gold]);  // gold
            Out.WriteByte(_bags[purple]);
            Out.WriteByte(_bags[blue]);
            Out.WriteByte(_bags[green]);
            Out.WriteByte(_bags[white]); // white
            Out.Fill(0, 3);

            WritePreRolls(Out);

            Out.WriteStringBytes(playerRoll.PlayerName);
            Out.Fill(0, 24 - playerRoll.PlayerName.Length);
            Out.Fill(0, 2);
            Out.WriteUInt16R((ushort)playerRoll.RandomBonus);
            Out.WriteUInt16R((ushort)playerRoll.ContributionBonus);
            Out.WriteUInt16R((ushort)playerRoll.PersistenceBonus);
            Out.WriteUInt16((ushort)(preIndex + 1)); // place

            WritePostRolls(Out);

            Out.WriteUInt16((ushort)(postIndex + 1)); // place
            Out.WriteStringBytes(playerRoll.PlayerName);
            Out.Fill(0, 24 - playerRoll.PlayerName.Length);
            Out.Fill(0, 2);
            Out.WriteUInt16R((ushort)playerRoll.RandomBonus);
            Out.WriteUInt16R((ushort)playerRoll.ContributionBonus);
            Out.WriteUInt16R((ushort)playerRoll.PersistenceBonus);
            Out.WriteByte(1);  // ???
            Out.WriteByte(playerRoll.BagWon);  // bag won

            Out.Fill(0, 2);
            //Out.WriteUInt16(TIME_PQ_RESET);
            Out.WriteByte(0);
            Out.WriteByte(3);


            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(1);
            Out.Fill(0, 27);
            //
            // no clue yet seams to be if you didnt won anything you get that item


            /*
            Out.WritePacketString(@"|d4 c0 01 |...d............|     
            |57 61 72 20 43 72 65 73 74 00 00 00 00 00 00 00 |War Crest.......|
            |00 00 00 00 00 00 00 00 00 00 00                |...........     |
            ");
            */

            targPlayer.SendPacket(Out);
            // Info.SendCurrentStage(plr);
        }  //  d4 c0 01

        public void PersonalScoreboard(ContributionInfo playerRoll, byte bagWon)
        {
            Player targPlayer = Player.GetPlayer(playerRoll.PlayerCharId);

            if (targPlayer == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_PQLOOT_TRIGGER, 1723);
            Out.WriteStringBytes(_publicQuestInfo.Name);
            Out.Fill(0, 24 - _publicQuestInfo.Name.Length);
            Out.WriteByte(_bags[gold]);  // gold
            Out.WriteByte(_bags[purple]);
            Out.WriteByte(_bags[blue]);
            Out.WriteByte(_bags[green]);
            Out.WriteByte(_bags[white]); // white
            Out.Fill(0, 3);
            ContributionInfo curRoll = playerRoll;

            Out.WriteStringBytes(targPlayer.Name);
            Out.Fill(0, 24 - targPlayer.Name.Length);
            Out.Fill(0, 2);
            Out.WriteUInt16R((ushort)curRoll.RandomBonus);
            Out.WriteUInt16R((ushort)curRoll.ContributionBonus);
            Out.WriteUInt16R((ushort)curRoll.PersistenceBonus);
            for (int i = 1; i < 24; i++)
                Out.Fill(0, 32);

            Out.WriteStringBytes(targPlayer.Name);
            Out.Fill(0, 24 - targPlayer.Name.Length);
            Out.Fill(0, 2);
            Out.WriteUInt16R((ushort)playerRoll.RandomBonus);
            Out.WriteUInt16R((ushort)playerRoll.ContributionBonus);
            Out.WriteUInt16R((ushort)playerRoll.PersistenceBonus);
            Out.WriteUInt16(1); // place
            Out.WriteStringBytes(targPlayer.Name);
            Out.Fill(0, 24 - targPlayer.Name.Length);
            Out.Fill(0, 2);
            Out.WriteUInt16R((ushort)curRoll.RandomBonus);
            Out.WriteUInt16R((ushort)curRoll.ContributionBonus);
            Out.WriteUInt16R((ushort)curRoll.PersistenceBonus);
            Out.WriteByte(1);  // ???
            Out.WriteByte(curRoll.BagWon);  // bag won
            for (int i = 1; i < 24; i++)
                Out.Fill(0, 34);  // i just send empty once here

            Out.WriteUInt16(1); // place
            Out.WriteStringBytes(targPlayer.Name);
            Out.Fill(0, 24 - targPlayer.Name.Length);
            Out.Fill(0, 2);
            Out.WriteUInt16R((ushort)playerRoll.RandomBonus);
            Out.WriteUInt16R((ushort)playerRoll.ContributionBonus);
            Out.WriteUInt16R((ushort)playerRoll.PersistenceBonus);
            Out.WriteByte(1);  // ???
            Out.WriteByte(playerRoll.BagWon);  // bag won

            Out.Fill(0, 2);
            //Out.WriteUInt16(TIME_PQ_RESET);
            Out.WriteByte(0);
            Out.WriteByte(3);


            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(1);
            Out.Fill(0, 27);
            //
            // no clue yet seams to be if you didnt won anything you get that item


            /*
            Out.WritePacketString(@"|d4 c0 01 |...d............|     
            |57 61 72 20 43 72 65 73 74 00 00 00 00 00 00 00 |War Crest.......|
            |00 00 00 00 00 00 00 00 00 00 00                |...........     |
            ");
            */
            targPlayer.SendPacket(Out);
            // Info.SendCurrentStage(plr);
        }  //  d4 c0 01

        private void WritePreRolls(PacketOut Out)
        {
            int maxCount = Math.Min(24, _preRoll.Count);

            for (int i = 0; i < maxCount; i++)
            {
                ContributionInfo curRoll = _preRoll[i].Value;

                Out.WriteStringBytes(curRoll.PlayerName);
                Out.Fill(0, 24 - curRoll.PlayerName.Length);
                Out.Fill(0, 2);
                Out.WriteUInt16R((ushort)curRoll.RandomBonus);
                Out.WriteUInt16R((ushort)curRoll.ContributionBonus);
                Out.WriteUInt16R((ushort)curRoll.PersistenceBonus);
            }

            if (maxCount < 24)
                for (int i = maxCount; i < 24; i++)
                    Out.Fill(0, 32);
        }

        private void WritePersonalPreRolls(PacketOut Out)
        {
            int maxCount = Math.Min(24, 1);

            for (int i = 0; i < maxCount; i++)
            {
                ContributionInfo curRoll = _preRoll[i].Value;

                Out.WriteStringBytes(curRoll.PlayerName);
                Out.Fill(0, 24 - curRoll.PlayerName.Length);
                Out.Fill(0, 2);
                Out.WriteUInt16R((ushort)curRoll.RandomBonus);
                Out.WriteUInt16R((ushort)curRoll.ContributionBonus);
                Out.WriteUInt16R((ushort)curRoll.PersistenceBonus);
            }

            if (maxCount < 24)
                for (int i = maxCount; i < 24; i++)
                    Out.Fill(0, 32);
        }

        private void WritePostRolls(PacketOut Out)
        {
            int maxCount = Math.Min(24, _postRoll.Count);

            for (int i = 0; i < maxCount; i++)
            {
                ContributionInfo curRoll = _postRoll[i].Value;

                Out.WriteStringBytes(curRoll.PlayerName);
                Out.Fill(0, 24 - curRoll.PlayerName.Length);
                Out.Fill(0, 2);
                Out.WriteUInt16R((ushort)curRoll.RandomBonus);
                Out.WriteUInt16R((ushort)curRoll.ContributionBonus);
                Out.WriteUInt16R((ushort)curRoll.PersistenceBonus);
                Out.WriteByte(1);  // ???
                Out.WriteByte(curRoll.BagWon);  // bag won
            }

            if (maxCount < 24)
                for (int i = maxCount; i < 24; i++)
                    Out.Fill(0, 34);  // i just send empty once here
        }
    }
}