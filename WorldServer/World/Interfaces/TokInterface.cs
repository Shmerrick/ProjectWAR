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



            LoadActionCounters();

            _loaded = true;

            base.Load();
        }

        #region Ward fragment task counters

        /// <summary>Action counter id -> this character's progress on it.</summary>
        private readonly Dictionary<ushort, Character_action_counter> _actionCounters = new Dictionary<ushort, Character_action_counter>();

        /// <summary>
        /// One indexed read per login. characters_action_counters is keyed (CharacterId, AcId),
        /// so CharacterId is the leftmost primary key column and this is a ref lookup rather than
        /// a scan. Kept out of CharMgr's bulk character load, which would need the same list
        /// threaded through six separate paths.
        /// </summary>
        private void LoadActionCounters()
        {
            _actionCounters.Clear();

            uint characterId = GetPlayer().CharacterId;

            IList<Character_action_counter> counters =
                CharMgr.Database.SelectObjects<Character_action_counter>("CharacterId=" + characterId);

            if (counters == null)
                return;

            foreach (Character_action_counter counter in counters)
                if (counter != null && !_actionCounters.ContainsKey(counter.AcId))
                    _actionCounters.Add(counter.AcId, counter);
        }

        /// <summary>Current progress on an action counter.</summary>
        public uint GetActionCounter(ushort acId)
        {
            Character_action_counter counter;
            return _actionCounters.TryGetValue(acId, out counter) ? counter.Count : 0u;
        }

        /// <summary>
        /// Pushes every ward task counter to the client so the fragment pages show real progress
        /// rather than 0. Called once the Tome is loaded, from Player.OnLoad.
        /// </summary>
        public void SendWardTaskCounters()
        {
            if (!_loaded)
                return;

            foreach (Ward_Fragment_Task task in WardTaskService.GetWardTasks())
                SendActionCounterUpdate(task.AcId, GetActionCounter(task.AcId));
        }

        /// <summary>
        /// Advances a ward fragment task counter and awards its Tome entry once the client's
        /// threshold is reached. Ignores ids that are not ward task counters, so callers can fire
        /// events without knowing which are bound.
        ///
        /// The award goes through AddTok, so completing the task also awards its fragment and
        /// cascades to the tier below exactly as an equip route does.
        /// </summary>
        public void IncrementWardTaskCounter(ushort acId, uint amount = 1)
        {
            if (!_loaded || amount == 0)
                return;

            Ward_Fragment_Task task;
            if (!WardTaskService.TryGetWardTask(acId, out task))
                return;

            // Already complete: nothing to count, and the tok would be a no-op anyway.
            if (task.TokEntry != 0 && HasTok(task.TokEntry))
                return;

            Character_action_counter counter;
            if (_actionCounters.TryGetValue(acId, out counter))
            {
                if (counter.Count >= task.Threshold)
                    return;

                counter.Count += amount;
                counter.Dirty = true;
                CharMgr.Database.SaveObject(counter);
            }
            else
            {
                counter = new Character_action_counter
                {
                    CharacterId = GetPlayer().CharacterId,
                    AcId = acId,
                    Count = amount
                };

                _actionCounters.Add(acId, counter);
                CharMgr.Database.AddObject(counter);
            }

            if (counter.Count > task.Threshold)
                counter.Count = task.Threshold;

            SendActionCounterUpdate(acId, counter.Count);

            if (counter.Count < task.Threshold)
                return;

            if (task.TokEntry == 0)
            {
                // A client-defined counter with no tok_infos row to award: AcIds 704, 705 and 709.
                // Recorded rather than silently dropped so the missing rows stay visible.
                Log.Info("WardTask", GetPlayer().Name + " completed ward task counter " + acId + " but it has no Tome entry to award.");
                return;
            }

            AddTok(task.TokEntry);
        }

        #endregion
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
        /// Takes a Tome entry back off a character. Returns true if they held it.
        ///
        /// The Tome is otherwise append-only -- <see cref="Save"/> only ever calls SaveObject, so
        /// dropping a row from <c>_tokUnlocks</c> alone would let it reload at the next login. The
        /// database row is deleted here for that reason.
        ///
        /// Written for the Land of the Dead tombs, which charge glyphs at the door and reset the
        /// player's glyph progress. Do not reach for it anywhere else without a reason: almost
        /// everything in the Tome is a permanent record, and revoking one silently rewrites a
        /// player's history. It deliberately does not touch ward fragments, tome tactic counters or
        /// the bestiary, none of which are reversible, so it must not be pointed at an entry that
        /// feeds them.
        /// </summary>
        public bool RemoveTok(ushort Entry)
        {
            Character_tok unlock;
            if (!_tokUnlocks.TryGetValue(Entry, out unlock))
                return false;

            _tokUnlocks.Remove(Entry);

            if (unlock != null)
                CharMgr.Database.DeleteObject(unlock);

            SendTokRemoved(Entry);
            return true;
        }

        /// <summary>
        /// Tells the client an entry is no longer held, by sending the ordinary Tome update with
        /// its count set to zero.
        ///
        /// UNVERIFIED. No capture in the corpus shows a Tome entry being revoked, so the zero-count
        /// reading of this field is inference from the shape of <see cref="SendTok"/>, not evidence.
        /// The server-side state is correct either way -- the entry is gone from memory and from the
        /// database, so a relog shows the truth. If the client turns out to ignore this, the fix is
        /// here and nowhere else.
        /// </summary>
        private void SendTokRemoved(ushort Entry)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_TOK_ENTRY_UPDATE);
            Out.WriteUInt32(1);
            Out.WriteUInt16(Entry);
            Out.WriteByte(0); // Count: held -> not held.
            Out.WriteByte(0); // Do not print an unlock announcement for a removal.
            Out.WriteByte(0);

            GetPlayer().SendPacket(Out);
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
                            GrantSetCompletion(tokItemUnlock2.TokUnlock2);
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

            // Bestiary entries marked with the client's fragment icon advance a tome tactic line.
            AwardTomeTacticFragment(Entry);

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
        /// <summary>
        /// Advances the tome tactic fragment counter this Tome entry feeds, and marks any tactic
        /// tier whose threshold the new total reaches as unlocked.
        ///
        /// Called for every Tome entry awarded; entries that are not fragments return immediately,
        /// so callers do not need to know which are bound. The tier unlock goes through AddTok, so
        /// the Section 26 row is stored, announced and XP-awarded exactly like any other entry.
        /// That cannot recurse: Section 26 entries are not fragments, so they return at the guard.
        /// </summary>
        private void AwardTomeTacticFragment(ushort tokEntry)
        {
            ushort acId;
            if (!TomeTacticService.TryGetFragmentLine(tokEntry, out acId))
                return;

            Tome_Tactic_Line line;
            if (!TomeTacticService.TryGetLine(acId, out line))
            {
                Log.Error("TomeTactic", "Tome entry " + tokEntry + " is bound to tactic counter "
                    + acId + " but no such line is loaded; fragment not counted.");
                return;
            }

            Character_action_counter counter;
            if (_actionCounters.TryGetValue(acId, out counter))
            {
                counter.Count++;
                counter.Dirty = true;
                CharMgr.Database.SaveObject(counter);
            }
            else
            {
                counter = new Character_action_counter
                {
                    CharacterId = GetPlayer().CharacterId,
                    AcId = acId,
                    Count = 1
                };

                _actionCounters.Add(acId, counter);
                CharMgr.Database.AddObject(counter);
            }

            SendActionCounterUpdate(acId, counter.Count);

            // Tiers ascend, and a player can cross more than one at once only if thresholds were
            // mis-seeded, but award every tier the total now satisfies so a gap cannot strand one.
            for (int tier = 1; tier <= 3; tier++)
            {
                if (counter.Count < line.ThresholdForTier(tier))
                    break;

                ushort tierTok = line.TokEntryForTier(tier);
                if (tierTok != 0 && !HasTok(tierTok))
                    AddTok(tierTok);
            }
        }

        /// <summary>
        /// Rebuilds every tome tactic fragment counter from the Tome entries this character
        /// actually holds, awards any tier the resulting totals already satisfy, and pushes the
        /// counters to the client.
        ///
        /// Needed in two cases. Characters who unlocked bestiary entries before tome tactics
        /// existed hold the fragments but have no counters, so without this their progress would
        /// only start from the next fragment earned. And the .alltoks bulk grant deliberately
        /// bypasses AddTok, so nothing would advance the counters there either.
        ///
        /// Idempotent: the counter is derived from held entries, so re-running produces the same
        /// totals. It only ever raises a counter -- a value already at or above the derived total
        /// is left alone rather than clawed back, since a fragment could legitimately have been
        /// counted from a source this does not know about.
        /// </summary>
        /// <returns>The number of line counters changed.</returns>
        public int RecomputeTomeTacticCounters()
        {
            if (!_loaded)
                return 0;

            int changed = 0;

            foreach (Tome_Tactic_Line line in TomeTacticService.GetLines())
            {
                IList<ushort> fragments = TomeTacticService.GetFragmentsForLine(line.AcId);

                uint held = 0;
                for (int i = 0; i < fragments.Count; ++i)
                    if (HasTok(fragments[i]))
                        ++held;

                Character_action_counter counter;
                if (_actionCounters.TryGetValue(line.AcId, out counter))
                {
                    if (counter.Count >= held)
                        continue;

                    counter.Count = held;
                    counter.Dirty = true;
                    CharMgr.Database.SaveObject(counter);
                }
                else
                {
                    if (held == 0)
                        continue;

                    counter = new Character_action_counter
                    {
                        CharacterId = GetPlayer().CharacterId,
                        AcId = line.AcId,
                        Count = held
                    };

                    _actionCounters.Add(line.AcId, counter);
                    CharMgr.Database.AddObject(counter);
                }

                ++changed;
                SendActionCounterUpdate(line.AcId, counter.Count);

                for (int tier = 1; tier <= 3; tier++)
                {
                    if (counter.Count < line.ThresholdForTier(tier))
                        break;

                    ushort tierTok = line.TokEntryForTier(tier);
                    if (tierTok != 0 && !HasTok(tierTok))
                        AddTok(tierTok);
                }
            }

            return changed;
        }

        /// <summary>
        /// Pushes every tome tactic line counter so the Tome's fragment pages show real progress.
        /// </summary>
        public void SendTomeTacticCounters()
        {
            if (!_loaded)
                return;

            foreach (Tome_Tactic_Line line in TomeTacticService.GetLines())
                SendActionCounterUpdate(line.AcId, GetActionCounter(line.AcId));
        }

        /// <summary>How many bulk-granted Tome rows to accumulate before forcing a save.</summary>
        private const int BulkGrantFlushSize = 500;

        /// <summary>
        /// Pushes this character's real action counters to the client: every bestiary species
        /// counter, the total-kills counter, and every ward task counter.
        ///
        /// Replaces the old .tokbestiary behaviour, which sent SendActionCounterUpdate(i, i) for
        /// i in 1..999 -- telling the client counter 5 was 5 and counter 700 was 700, persisting
        /// nothing and corrupting the displayed progress of every bestiary, ward and tome tactic
        /// counter at once.
        /// </summary>
        /// <returns>The number of counters pushed.</returns>
        public int ResendActionCounters()
        {
            if (!_loaded)
                return 0;

            int sent = 0;

            foreach (Character_tok_kills kills in _tokKillCount.Values)
            {
                if (kills == null || kills.NPCEntry == 0)
                    continue;

                SendActionCounterUpdate(kills.NPCEntry, kills.Count);
                sent++;
            }

            foreach (Ward_Fragment_Task task in WardTaskService.GetWardTasks())
            {
                SendActionCounterUpdate(task.AcId, GetActionCounter(task.AcId));
                sent++;
            }

            foreach (Tome_Tactic_Line line in TomeTacticService.GetLines())
            {
                SendActionCounterUpdate(line.AcId, GetActionCounter(line.AcId));
                sent++;
            }

            return sent;
        }

        /// <summary>
        /// Developer bulk award behind .alltoks. Deliberately does not route through AddTok.
        /// That path, driven over the ~12,000 rows of tok_infos, sent one F_TOK_ENTRY_UPDATE per
        /// entry, re-materialised Info.Toks on every call (an O(n) rebuild inside an O(n) loop,
        /// roughly 72M list operations), awarded XP ~12,000 times, and left every insert to the
        /// save pump, which batches all dirty objects into a single unbounded transaction.
        ///
        /// Item and token rewards are intentionally skipped: a developer revealing the Tome does
        /// not want 49 Bestial Tokens and every item reward materialising in their bags.
        /// </summary>
        /// <returns>The number of Tome entries newly granted.</returns>
        public int GrantAllToks()
        {
            if (!_loaded)
            {
                Log.Error("ToKSystem", "Tried to bulk-grant Toks when the system wasn't loaded.");
                return 0;
            }

            if (TokService._Toks == null)
                return 0;

            Player player = GetPlayer();
            byte realm = player.Info.Realm;
            uint characterId = player.CharacterId;

            int granted = 0;
            int sinceFlush = 0;

            foreach (Tok_Info info in TokService._Toks.Values)
            {
                if (info == null || _tokUnlocks.ContainsKey(info.Entry))
                    continue;

                if (info.Realm != 0 && info.Realm != realm)
                    continue;

                Character_tok tok = new Character_tok
                {
                    TokEntry = info.Entry,
                    CharacterId = characterId,
                    Count = 1
                };

                _tokUnlocks.Add(info.Entry, tok);
                TrackWardFragment(info.Entry);
                CharMgr.Database.AddObject(tok);
                granted++;

                if (++sinceFlush >= BulkGrantFlushSize)
                {
                    CharMgr.Database.ForceSave();
                    sinceFlush = 0;
                }
            }

            if (sinceFlush > 0)
                CharMgr.Database.ForceSave();

            // Once, rather than once per entry.
            player.Info.Toks = _tokUnlocks.Values.ToList();

            // Bypassing AddTok also bypassed the fragment logic, so the tactic counters would sit
            // at 0 while every tactic showed as unlocked. Derive them from what was just granted.
            RecomputeTomeTacticCounters();

            // One bitmap packet instead of ~12,000 individual entry updates.
            SendAllToks();

            return granted;
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

            // Bestiary_ID is the client's per-species action counter id, not the creature subtype
            // (migration 54). It was NULL for every row until then, which the ORM read back as 0:
            // every species shared bucket 0, the client's per-species counter never moved, and the
            // milestone ladder below ran off one global kill count. Subtype 68 "Hammerer" has no
            // client species and so legitimately has no counter -- counting it would put us back
            // in the shared bucket, so it is skipped rather than written to counter 0.
            if (TB.Bestiary_ID == 0)
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

            // Total kill counter. Load() seeds 495, but a character whose seeding failed would
            // previously leave "kills" null here and NRE on the send below, killing the whole
            // kill-credit path for that character.
            Character_tok_kills totalKills;
            if (_tokKillCount.TryGetValue(495, out totalKills) && totalKills != null)
            {
                totalKills.Count++;
                totalKills.Dirty = true;
                CharMgr.Database.SaveObject(totalKills);
                SendActionCounterUpdate(495, totalKills.Count);
            }

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

            // A milestone cell holds one or more tok entries, e.g. "3007;10501". String.Split never
            // returns an empty array, so the old else branch was dead, and UInt16.Parse threw on any
            // malformed or empty cell -- aborting kill credit mid-award for every later kill of that
            // species. Parse defensively and log the bad cell instead (AGENTS.md rule 4).
            foreach (string st in tok.Split(';'))
            {
                string entryText = st.Trim();
                if (entryText.Length == 0)
                    continue;

                ushort tokEntry;
                if (!ushort.TryParse(entryText, out tokEntry))
                {
                    Log.Error("TokInterface", "Bestiary species " + type + " milestone " + kill
                        + " has unparsable tok entry '" + entryText + "'; skipped.");
                    continue;
                }

                AddTok(tokEntry);
            }
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

                    foreach (Item_Info setItem in currentSet)
                    {
                        if (HasTok(setItem.TokUnlock))
                            count--;
                    }

                    if (count == 0)
                        GrantSetCompletion(item.TokUnlock2);
                }
            }
        }

        /// <summary>
        /// Awards a set-completion unlock and the title it carries, for a character who has
        /// just been found to hold every piece of an armour set.
        /// </summary>
        /// <remarks>
        /// Both halves are optional and each is skipped on its own. tok_infos.Rewards names
        /// the title, and it is 0 for a set whose title mapping was never filled in -- two of
        /// the twenty-one sets, Doomflayer and Warpforged, were in that state. TokService.GetTok(0)
        /// returns null, and dereferencing it aborted Player.OnLoad before RenInterface.Load,
        /// so every later SendRenown then failed too and the character could not enter the
        /// world at all. See BUG-155.
        /// </remarks>
        private void GrantSetCompletion(ushort setTokEntry)
        {
            if (setTokEntry == 0 || HasTok(setTokEntry))
                return;

            Tok_Info setInfo = TokService.GetTok(setTokEntry);

            if (setInfo == null)
            {
                Log.Error("TokInterface", "Set completion unlock " + setTokEntry + " has no tok_infos row; not awarded to " + _Owner.Name + ".");
                return;
            }

            SendTok(setTokEntry, true);

            Character_tok setTok = new Character_tok
            {
                TokEntry = setTokEntry,
                CharacterId = GetPlayer().CharacterId,
                Count = 1
            };

            _tokUnlocks.Add(setTokEntry, setTok);
            TrackWardFragment(setTokEntry);
            GetPlayer().AddXp(setInfo.Xp, false, false);
            CharMgr.Database.AddObject(setTok);

            // The title the set carries. A set with no title mapping still awards the set
            // unlock above; only the title is skipped.
            ushort titleEntry = (ushort)setInfo.Rewards;

            if (titleEntry == 0 || HasTok(titleEntry))
                return;

            Tok_Info titleInfo = TokService.GetTok(titleEntry);

            if (titleInfo == null)
            {
                Log.Error("TokInterface", "Set completion unlock " + setTokEntry + " names title " + titleEntry + ", which has no tok_infos row; title not awarded to " + _Owner.Name + ".");
                return;
            }

            SendTok(titleEntry, true);

            Character_tok titleTok = new Character_tok
            {
                TokEntry = titleEntry,
                CharacterId = GetPlayer().CharacterId,
                Count = 1
            };

            _tokUnlocks.Add(titleEntry, titleTok);
            TrackWardFragment(titleEntry);
            GetPlayer().AddXp(titleInfo.Xp, false, false);
            CharMgr.Database.AddObject(titleTok);
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
