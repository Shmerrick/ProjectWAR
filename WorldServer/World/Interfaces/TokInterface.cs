using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class TokInterface : BaseInterface
    {
        private readonly Dictionary<ushort, Character_tok> _tokUnlocks = new Dictionary<ushort, Character_tok>();
        private readonly Dictionary<ushort, Character_tok_kills> _tokKillCount = new Dictionary<ushort, Character_tok_kills>();
        private readonly byte[] _wardFragments = new byte[5];
        private readonly byte[] _effectiveWardFragments = new byte[5];

        private bool _loaded;

        public void Load(List<Character_tok> toks, List<Character_tok_kills> toksKills)
        {
            Array.Clear(_wardFragments, 0, _wardFragments.Length);
            Array.Clear(_effectiveWardFragments, 0, _effectiveWardFragments.Length);

            if (toks != null)
            {
                if (_tokUnlocks.Count > 0)
                {
                    Log.Error(_Owner.Name, "ToK system was loaded multiple times!");
                    _tokUnlocks.Clear();
                }

                foreach (Character_tok tok in toks)
                {
                    if (!_tokUnlocks.ContainsKey(tok.TokEntry))
                    {
                        _tokUnlocks.Add(tok.TokEntry, tok);
                        TrackWardFragment(tok.TokEntry);
                    }
                }

            }

            if (toksKills != null)
            {
                if (_tokKillCount.Count > 0)
                {
                    Log.Error(_Owner.Name, "ToKKill system was loaded multiple times!");
                    toksKills.Clear();
                }

                foreach (Character_tok_kills tok in toksKills)
                {
                    if (!_tokKillCount.ContainsKey(tok.NPCEntry))
                        _tokKillCount.Add(tok.NPCEntry, tok);
                }
            }

            Character_tok_kills kills;
            if (!_tokKillCount.TryGetValue(495, out kills))
            {
                uint totalcount = 0;

                foreach (KeyValuePair<ushort, Character_tok_kills> k in _tokKillCount)
                {
                    totalcount += k.Value.Count;
                }
                kills = new Character_tok_kills
                {
                    NPCEntry = 495,
                    CharacterId = GetPlayer().CharacterId,
                    Count = totalcount
                };
                _tokKillCount.Add(495, kills);
                GetPlayer().Info.TokKills = _tokKillCount.Values.ToList();
                CharMgr.Database.AddObject(kills);
            }



            _loaded = true;

            base.Load();
        }
        public override void Save()
        {
            foreach (KeyValuePair<ushort, Character_tok> Kp in _tokUnlocks)
                CharMgr.Database.SaveObject(Kp.Value);
        }

        public bool HasTok(ushort Entry)
        {
            return _tokUnlocks.ContainsKey(Entry);
        }

        /// <summary>
        /// Returns the permanent fragments that satisfy the requested ward tier.
        /// A completed higher ward satisfies every lower tier.
        /// </summary>
        public byte GetWardFragmentCount(WardTier wardTier)
        {
            int wardIndex = (int)wardTier - 1;
            if (wardIndex < 0 || wardIndex >= _effectiveWardFragments.Length)
                return 0;

            return _effectiveWardFragments[wardIndex];
        }

        /// <summary>
        /// Repairs ward progress the character earned but never received, in two directions:
        /// a completed task whose fragment was never awarded, and a held fragment that never
        /// completed task 2 on the tier below. Called once after Load, from Player.OnLoad.
        ///
        /// Ward tasks became fragment-awarding only after characters could already complete
        /// them, so a character may hold the task with no fragment to show for it. Load itself
        /// cannot repair this: it tracks entries 7600-7624 and a task is not one of them.
        /// Equipment-based repair is not sufficient either, because a task may have been
        /// completed by a route that leaves nothing worn.
        ///
        /// This only ever adds. Earned fragments are permanent and are never recalculated from
        /// current equipment, per the 1.4.8 target in docs/WARD_SYSTEM.md.
        /// </summary>
        public void BackfillWardFragments()
        {
            if (!_loaded)
                return;

            // Collected first: AddTok mutates _tokUnlocks, which cannot be done while
            // enumerating it. Bounded by the number of toks the character holds.
            List<ushort> missing = null;

            foreach (KeyValuePair<ushort, Character_tok> held in _tokUnlocks)
            {
                // A completed task whose fragment was never awarded.
                ushort fragmentEntry;
                if (TokService.TryGetWardFragmentForTask(held.Key, out fragmentEntry) && !_tokUnlocks.ContainsKey(fragmentEntry))
                {
                    if (missing == null)
                        missing = new List<ushort>();

                    if (!missing.Contains(fragmentEntry))
                        missing.Add(fragmentEntry);
                }

                // A held fragment that never completed task 2 on the tier below. Granting the
                // task awards that fragment, which cascades down the remaining tiers.
                ushort lowerTaskEntry;
                if (TokService.TryGetLowerWardTaskForFragment(held.Key, out lowerTaskEntry) && !_tokUnlocks.ContainsKey(lowerTaskEntry))
                {
                    if (missing == null)
                        missing = new List<ushort>();

                    if (!missing.Contains(lowerTaskEntry))
                        missing.Add(lowerTaskEntry);
                }
            }

            if (missing == null)
                return;

            for (int i = 0; i < missing.Count; ++i)
                AddTok(missing[i], false, false);

            Log.Info("TokInterface", _Owner.Name + " backfilled " + missing.Count + " ward unlock(s) from progress already earned.");
        }

        public void AddToks(string Toks)
        {
            if (!_loaded)
            {
                Log.Error("ToKSystem", "Tried to add ToK when system wasn't loaded.\n" + Environment.StackTrace);
                return;
            }

            if (!string.IsNullOrEmpty(Toks))
            {
                ushort tok;

                string[] tmp = Toks.Split(';');
                if (tmp.Length > 0)
                {
                    foreach (string st in tmp)
                    {
                        if (ushort.TryParse(st, out tok))
                            AddTok(tok);
                    }
                }
                else if (ushort.TryParse(Toks, out tok))
                    AddTok(tok);
            }
        }

        public void AddTok(Tok_Info Info)
        {
            if (!_loaded)
            {
                Log.Error("ToKSystem", "Tried to add ToK when system wasn't loaded.\n" + Environment.StackTrace);
                return;
            }

            if (Info != null)
                AddTok(Info.Entry);
        }
        // variable itemEquipedToK checks if this ToK was triggered by equiping item. If it is true it was, otherwise it is false
        // announce controls the client's "new Tome entry" ticker; help tips suppress it because
        // the tip window is already their notification.
        public void AddTok(ushort Entry, bool itemEquipedToK = false, bool announce = true)
        {
            // Resolved before the early return below. A character may already hold a ward
            // fragment task while still missing the fragment it awards: the task could be
            // unlocked long before the fragment cascade existed, and re-equipping the item
            // reaches this method with the task already held. Returning early there would
            // leave the ward permanently short, and would silently defeat the login backfill
            // in ItmInterface.GrantEquippedItemUnlocks, which grants through this method.
            // The recursion is one level deep and cannot go further: a fragment award has
            // task digit 0, so it is never itself a task and never resolves here.
            ushort wardFragmentEntry;
            bool isWardFragmentTask = TokService.TryGetWardFragmentForTask(Entry, out wardFragmentEntry);

            if (HasTok(Entry))
            {
                if (isWardFragmentTask && !HasTok(wardFragmentEntry))
                    AddTok(wardFragmentEntry, false, announce);

                // A fragment held from before the cross-tier cascade existed still owes the
                // tier below its task 2. Resolved here for the same reason as above.
                ushort heldLowerWardTaskEntry;
                if (TokService.TryGetLowerWardTaskForFragment(Entry, out heldLowerWardTaskEntry) && !HasTok(heldLowerWardTaskEntry))
                    AddTok(heldLowerWardTaskEntry, false, announce);

                return;
            }

            if (!_loaded)
            {
                Log.Error("ToKSystem", "Tried to add ToK when system wasn't loaded.\n" + Environment.StackTrace);
                return;
            }

            Tok_Info Info = TokService.GetTok(Entry);

            if (Info == null)
            {
                if (isWardFragmentTask)
                    Log.Error("TokInterface", "Ward task " + Entry + " has no tok_infos row; fragment not awarded for " + _Owner.Name + ".");

                return;
            }

            if (Info.Realm != 0 && Info.Realm != _Owner.GetPlayer().Info.Realm)
            {
                if (isWardFragmentTask)
                    Log.Error("TokInterface", "Ward task " + Entry + " is realm " + Info.Realm + " but " + _Owner.Name + " is realm " + _Owner.GetPlayer().Info.Realm + "; fragment not awarded.");

                return;
            }


            SendTok(Entry, announce);

            Character_tok Tok = new Character_tok
            {
                TokEntry = Entry,
                CharacterId = GetPlayer().CharacterId,
                Count = 1
            };

            _tokUnlocks.Add(Entry, Tok);
            TrackWardFragment(Entry);
            GetPlayer().AddXp(Info.Xp, false, false);

            // This checks if ToK we are adding is a part of larger ToK, for example title
            // "Sovereign Trinket" is part of title "The Sovereign"
            if (itemEquipedToK)
            {
                // Selects item we equiped from DB
                Item_Info tokItemUnlock2 = WorldMgr.Database.SelectObject<Item_Info>("career=" + GetPlayer().Info.CareerFlags + " AND TokUnlock=" + Entry);

                if (tokItemUnlock2 != null && tokItemUnlock2.TokUnlock != 0 && tokItemUnlock2.TokUnlock2 != 0)
                {
                    // Selects secondary ToK we want to setup if we completed full set
                    IList<Item_Info> tokItems = WorldMgr.Database.SelectObjects<Item_Info>("career=" + GetPlayer().Info.CareerFlags + " AND TokUnlock2 = " + tokItemUnlock2.TokUnlock2);
                    int count = tokItems.Count();

                    // If there is more than 0 items with complete set unlock we proceed
                    if (count > 0)
                    {
                        foreach (Item_Info tokItem in tokItems)
                        {
                            if (HasTok(tokItem.TokUnlock))
                            {
                                count--;
                            }
                        }
                        // If we have all required unlocks count = 0 and we can proceed 
                        if (count == 0)
                        {
                            // Tok is send to player
                            SendTok((ushort)tokItemUnlock2.TokUnlock2, true);

                            Character_tok Tok2 = new Character_tok
                            {
                                TokEntry = (ushort)tokItemUnlock2.TokUnlock2,
                                CharacterId = GetPlayer().CharacterId,
                                Count = 1
                            };

                            Tok_Info InfoSetTok = TokService.GetTok((ushort)tokItemUnlock2.TokUnlock2);

                            // ToK is added to the book
                            _tokUnlocks.Add((ushort)tokItemUnlock2.TokUnlock2, Tok2);
                            TrackWardFragment((ushort)tokItemUnlock2.TokUnlock2);
                            GetPlayer().AddXp(InfoSetTok.Xp, false, false);

                            // Adding reward from final ToK - Title
                            SendTok((ushort)InfoSetTok.Rewards, true);

                            Character_tok Tok2Title = new Character_tok
                            {
                                TokEntry = (ushort)InfoSetTok.Rewards,
                                CharacterId = GetPlayer().CharacterId,
                                Count = 1
                            };

                            Tok_Info TokInfoTitle = TokService.GetTok((ushort)InfoSetTok.Rewards);

                            _tokUnlocks.Add((ushort)InfoSetTok.Rewards, Tok2Title);
                            TrackWardFragment((ushort)InfoSetTok.Rewards);
                            GetPlayer().AddXp(TokInfoTitle.Xp, false, false);

                            //ToKs saved in DB :)
                            CharMgr.Database.AddObject(Tok2);
                            CharMgr.Database.AddObject(Tok2Title);
                        }
                    }
                }
            }

            if (Info.Rewards > 0)
            {
                // this will be used for future additions like the tome tactics and gear to buy
                if (Info.Rewards == 1)
                {
                    GetPlayer().ItmInterface.CreateItem(80001, 1);   // Betial Token
                }
            }

            GetPlayer().Info.Toks = _tokUnlocks.Values.ToList();

            CharMgr.Database.AddObject(Tok);

            // Completing any one of a ward fragment's tasks awards that fragment. Resolved at
            // the top of this method so the already-held case is handled there too.
            if (isWardFragmentTask)
                AddTok(wardFragmentEntry, false, announce);

            // Task 2 of a fragment is "acquire the same fragment of the next ward up", so
            // earning this fragment completes that task one tier down and awards the fragment
            // below it, which repeats until tier 1. Termination is guaranteed: each step moves
            // strictly one sigil tier down, so the chain is at most four deep (Supreme to
            // Lesser), and any fragment already held returns at the top of this method.
            ushort lowerWardTaskEntry;
            if (TokService.TryGetLowerWardTaskForFragment(Entry, out lowerWardTaskEntry))
                AddTok(lowerWardTaskEntry, false, announce);
        }
        public void SendAllToks()
        {

            PacketOut Out = new PacketOut((byte)Opcodes.F_TOK_ENTRY_UPDATE, 1509);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteUInt16(1500);
            Out.WriteByte(0);
            Out.WriteByte(0);

            byte flags = 0;
            if (Program.Config.DiscoverAll)
            {
                Out.Fill(0xFF, 1500);
            }
            else
            {
                for (ushort i = 0; i < 1500 * 8; i++)
                {
                    if (_tokUnlocks.ContainsKey(i))
                        flags |= (byte)(1 << ((byte)(i % 8)));

                    if (i % 8 == 7)
                    {
                        Out.WriteByte(flags);
                        flags = 0;
                    }
                }
            }
            GetPlayer().SendPacket(Out);
        }

        /// <summary>
        /// Unlocks every help tip configured for a trigger that the player has not seen yet.
        /// Tips are ordinary Tome unlocks, so <see cref="AddTok"/> persists them and each tip is
        /// therefore shown once per character.
        /// </summary>
        /// <param name="trigger">The server event that fired.</param>
        /// <param name="triggerValue">
        /// Event parameter, matched against Help_Tip.TriggerValue. A configured value of zero
        /// matches anything.
        /// </param>
        public void FireHelpTips(HelpTipTrigger trigger, uint triggerValue = 0)
        {
            if (!_loaded)
                return;

            Player player = GetPlayer();

            if (player == null)
                return;

            List<Help_Tip> tips = HelpTipService.GetTips(trigger);

            for (int i = 0; i < tips.Count; ++i)
            {
                Help_Tip tip = tips[i];

                if (tip.TriggerValue != 0 && tip.TriggerValue != triggerValue)
                    continue;

                if (tip.MaxRank != 0 && player.Level > tip.MaxRank)
                    continue;

                if (HasTok(tip.TokEntry))
                    continue;

                // Help tips announce themselves through the tip window, so the Tome ticker is
                // suppressed to avoid a second notification for the same unlock.
                AddTok(tip.TokEntry, false, false);
            }
        }

        public void SendTok(ushort Entry, bool Print)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_TOK_ENTRY_UPDATE);
            Out.WriteUInt32(1);
            Out.WriteUInt16(Entry);
            Out.WriteByte(1);
            Out.WriteByte((byte)(Print ? 1 : 0));

            // Final byte is the client's help tip category. A non-zero value on an entry that is
            // not a help tip pops an empty tip window: EA_HelpTips resolves the title and body
            // from its HelpTipNames and HelpTipDescriptions string tables with (Entry - 11799)
            // and finds nothing there.
            Out.WriteByte(HelpTipService.GetTipType(Entry));

            GetPlayer().SendPacket(Out);
        }

        public void SendBestiary(ref PacketOut Out)
        {
            // total kills  01 EF 00 00 C5 17
            Out.WriteUInt32((UInt32)_tokKillCount.Count);
            foreach (KeyValuePair<ushort, Character_tok_kills> entry in _tokKillCount)
            {
                Out.WriteUInt16(entry.Key);
                Out.WriteUInt32(entry.Value.Count);
            }
        }

        public void SendActionCounterUpdate(ushort Subtype, uint Count)
        {

            PacketOut Out = new PacketOut((byte)Opcodes.F_ACTION_COUNTER_UPDATE, 11);
            Out.WriteUInt16(Subtype);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteUInt32(Count);
            _Owner.GetPlayer().SendPacket(Out);

        }

        public void AddKill(ushort type)
        {
            Tok_Bestiary TB = TokService.GetTokBestiary(type);
            if (TB == null)
                return;

            Character_tok_kills kills;
            if (_tokKillCount.TryGetValue(TB.Bestiary_ID, out kills))
            {
                kills.Count++;
                kills.Dirty = true;
                CharMgr.Database.SaveObject(kills);
            }

            else
            {
                kills = new Character_tok_kills
                {
                    NPCEntry = TB.Bestiary_ID,
                    CharacterId = GetPlayer().CharacterId,
                    Count = 1
                };
                _tokKillCount.Add(TB.Bestiary_ID, kills);
                GetPlayer().Info.TokKills = _tokKillCount.Values.ToList();
                CharMgr.Database.AddObject(kills);
            }
            uint kill = kills.Count;

            //Log.Info("creature type", "" + type+"  bestid "+ TB.Bestiary_ID + " kills "+ kill);

            SendActionCounterUpdate(TB.Bestiary_ID, kill);

            // total kill counter

            if (_tokKillCount.TryGetValue(495, out kills))
            {
                kills.Count++;
                kills.Dirty = true;
                CharMgr.Database.SaveObject(kills);
            }
            SendActionCounterUpdate(495, kills.Count);

            string tok;

            if (kill == 100000 && TB.Kill100000 != null)
                tok = TB.Kill100000;
            else if (kill == 10000 && TB.Kill10000 != null)
                tok = TB.Kill10000;
            else if (kill == 1000 && TB.Kill1000 != null)
                tok = TB.Kill1000;
            else if (kill == 100 && TB.Kill100 != null)
                tok = TB.Kill100;
            else if (kill == 25 && TB.Kill25 != null)
                tok = TB.Kill25;
            else if (kill == 1 && TB.Kill1 != null)
                tok = TB.Kill1;
            else
                return;

            string[] tmp = tok.Split(';');
            if (tmp.Length > 0)
            {
                foreach (string st in tmp)
                {
                    AddTok(UInt16.Parse(st));
                }
            }
            else
                AddTok(UInt16.Parse(tok));
        }

        public void CheckTokKills(ushort type, uint count)
        {
            Tok_Bestiary TB = TokService.GetTokBestiary(type);
            if (TB == null)
                return;

            uint kill = count;

            string tok;

            if (kill >= 1 && TB.Kill1 != null)
            {
                tok = TB.Kill1;
                string[] tmp = tok.Split(';');
                if (tmp.Length > 0)
                {
                    foreach (string st in tmp)
                    {
                        FixTokKills(UInt16.Parse(st));
                    }
                }
                else
                    FixTokKills(UInt16.Parse(tok));
            }

            if (kill >= 25 && TB.Kill25 != null)
            {
                tok = TB.Kill25;
                string[] tmp = tok.Split(';');
                if (tmp.Length > 0)
                {
                    foreach (string st in tmp)
                    {
                        FixTokKills(UInt16.Parse(st));
                    }
                }
                else
                    FixTokKills(UInt16.Parse(tok));
            }

            if (kill >= 100 && TB.Kill100 != null)
            {
                tok = TB.Kill100;
                string[] tmp = tok.Split(';');
                if (tmp.Length > 0)
                {
                    foreach (string st in tmp)
                    {
                        FixTokKills(UInt16.Parse(st));
                    }
                }
                else
                    FixTokKills(UInt16.Parse(tok));
            }

            if (kill >= 1000 && TB.Kill1000 != null)
            {
                tok = TB.Kill1000;
                string[] tmp = tok.Split(';');
                if (tmp.Length > 0)
                {
                    foreach (string st in tmp)
                    {
                        FixTokKills(UInt16.Parse(st));
                    }
                }
                else
                    FixTokKills(UInt16.Parse(tok));
            }

            if (kill >= 10000 && TB.Kill10000 != null)
            {
                tok = TB.Kill10000;
                string[] tmp = tok.Split(';');
                if (tmp.Length > 0)
                {
                    foreach (string st in tmp)
                    {
                        FixTokKills(UInt16.Parse(st));
                    }
                }
                else
                    FixTokKills(UInt16.Parse(tok));
            }

            if (kill >= 100000 && TB.Kill100000 != null)
            {
                tok = TB.Kill100000;
                string[] tmp = tok.Split(';');
                if (tmp.Length > 0)
                {
                    foreach (string st in tmp)
                    {
                        FixTokKills(UInt16.Parse(st));
                    }
                }
                else
                    FixTokKills(UInt16.Parse(tok));
            }
        }

        private void FixTokKills(ushort Entry)
        {
            Tok_Info Info = TokService.GetTok(Entry);

            if (Info == null)
                return;

            if (Info.Realm != 0 && Info.Realm != _Owner.GetPlayer().Info.Realm)
                return;

            Character_tok Tok = new Character_tok
            {
                TokEntry = Entry,
                CharacterId = GetPlayer().CharacterId,
                Count = 1
            };

            if (Tok == null)
                return;

            if (!HasTok(Entry))
            { 
                SendTok(Entry, true);

                GetPlayer().AddXp(Info.Xp, false, false);

                _tokUnlocks.Add(Entry, Tok);
                TrackWardFragment(Entry);

                GetPlayer().Info.Toks = _tokUnlocks.Values.ToList();
                
                CharMgr.Database.AddObject(Tok);
            }
        }

        public void FixTokItems()
        {
            //IList<Item_Info> tokItems = WorldMgr.Database.SelectObjects<Item_Info>("career=" + GetPlayer().Info.CareerFlags + " AND TokUnlock2 = " + item.Info.TokUnlock2);
            List<Item_Info> tokItems = new List<Item_Info>();

            for (ushort i = 10; i<35; i++)
            {
                if (i != 29 && i != 30)
                { 
                    Item item = GetPlayer().ItmInterface.GetItemInSlot(i);
                    if (item != null)
                        tokItems.Add(WorldMgr.Database.SelectObject<Item_Info>("entry =" + item.Info.Entry));
                }
            }

            foreach (Item_Info item in tokItems)
            {
                if (item != null && item.TokUnlock2 != 0 && !HasTok(item.TokUnlock2))
                {
                    IList<Item_Info> currentSet = WorldMgr.Database.SelectObjects<Item_Info>("career=" + GetPlayer().Info.CareerFlags + " AND TokUnlock2 = " + item.TokUnlock2);

                    int count = currentSet.Count();

                    foreach (Item_Info itm in currentSet)
                    {
                        if (count > 0)
                        {
                            foreach (Item_Info setItem in currentSet)
                            {
                                if (HasTok(setItem.TokUnlock))
                                    count--;
                            }
                        }

                        if (count == 0 && !HasTok(itm.TokUnlock2))
                        {
                            // Tok is send to player
                            SendTok((ushort)item.TokUnlock2, true);

                            Character_tok Tok2 = new Character_tok
                            {
                                TokEntry = (ushort)item.TokUnlock2,
                                CharacterId = GetPlayer().CharacterId,
                                Count = 1
                            };

                            Tok_Info InfoSetTok = TokService.GetTok((ushort)item.TokUnlock2);

                            // ToK is added to the book
                            _tokUnlocks.Add((ushort)item.TokUnlock2, Tok2);
                            TrackWardFragment((ushort)item.TokUnlock2);
                            GetPlayer().AddXp(InfoSetTok.Xp, false, false);

                            // Adding reward from final ToK - Title
                            SendTok((ushort)InfoSetTok.Rewards, true);

                            Character_tok Tok2Title = new Character_tok
                            {
                                TokEntry = (ushort)InfoSetTok.Rewards,
                                CharacterId = GetPlayer().CharacterId,
                                Count = 1
                            };

                            Tok_Info TokInfoTitle = TokService.GetTok((ushort)InfoSetTok.Rewards);

                            _tokUnlocks.Add((ushort)InfoSetTok.Rewards, Tok2Title);
                            TrackWardFragment((ushort)InfoSetTok.Rewards);
                            GetPlayer().AddXp(TokInfoTitle.Xp, false, false);

                            // ToKs saved in DB :)
                            CharMgr.Database.AddObject(Tok2);
                            CharMgr.Database.AddObject(Tok2Title);
                        }
                        
                    }
                }
            }
        }

        private void TrackWardFragment(ushort tokEntry)
        {
            const ushort firstWardFragment = 7600;
            const ushort lastWardFragment = 7624;
            const int fragmentsPerWard = 5;

            if (tokEntry < firstWardFragment || tokEntry > lastWardFragment)
                return;

            int wardIndex = (tokEntry - firstWardFragment) / fragmentsPerWard;
            if (_wardFragments[wardIndex] < fragmentsPerWard)
                _wardFragments[wardIndex]++;

            bool higherWardComplete = false;
            for (int index = _wardFragments.Length - 1; index >= 0; --index)
            {
                _effectiveWardFragments[index] = higherWardComplete
                    ? (byte)fragmentsPerWard
                    : _wardFragments[index];

                if (_wardFragments[index] == fragmentsPerWard)
                    higherWardComplete = true;
            }
        }
    }
}
