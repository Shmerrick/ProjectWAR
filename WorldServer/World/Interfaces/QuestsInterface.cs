using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldServer.World.Objects.PublicQuests;
using Item = WorldServer.World.Objects.Item;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class QuestsInterface : BaseInterface
    {
        public uint Entry
        {
            get
            {
                if (_Owner == null)
                    return 0;

                if (_Owner.IsCreature())
                    return _Owner.GetCreature().Entry;
               
                return 0;
            }
        }

        #region Npc

        public bool HasQuestStarter(ushort questID)
        {
            return QuestService.GetStartQuests(Entry).Find(info => info.Entry == questID) != null;
        }

        public bool HasQuestFinisher(ushort questID)
        {
            List<Quest> quests = QuestService.GetFinishersQuests(Entry);
            if(quests != null)
                return QuestService.GetFinishersQuests(Entry).Find(info => info.Entry == questID) != null;

            return false;
        }

        public bool CreatureHasQuestToComplete(Player plr)
        {
            if (Entry == 0)
                return false;

            List<Quest> finisher = QuestService.GetFinishersQuests(Entry);
            if (finisher == null)
                return false;

            return  finisher.Find(q => plr.QtsInterface.CanEndQuest(q)) != null;
        }

        public bool CreatureHasQuestToAchieve(Player plr)
        {
            if (Entry == 0)
                return false;

            List<Quest> finisher = QuestService.GetFinishersQuests(Entry);
            if (finisher == null)
                return false;

            foreach (Quest q in finisher)
            {
                Character_quest cq = plr.QtsInterface.GetQuest(q.Entry);
                if (cq != null && !cq.IsDone())
                    return true;
            }

            return false;
        }

        public bool CreatureHasStartRepeatingQuest(Player plr)
        {
            if (Entry == 0)
                return false;

            List<Quest> starter = QuestService.GetStartQuests(Entry);
            if (starter == null)
                return false;

            foreach(Quest quest in starter)
            {
                if (quest.Repeatable && plr.QtsInterface.CanStartQuest(quest))
                    return true;
            }


            return false;
        }

        public bool CreatureHasStartQuest(Player plr)
        {
            if (Entry == 0)
                return false;

            List<Quest> starter = QuestService.GetStartQuests(Entry);

            return starter?.Find(q => plr.QtsInterface.CanStartQuest(q)) != null;
        }

        public bool HasQuestInteract(Player plr, Creature crea)
        {
            if (Entry == 0)
                return false;

            List<Quest> starter = crea.Spawn.Proto.StartingQuests;
            List<Quest> finisher = crea.Spawn.Proto.FinishingQuests;
            List<Quest> inProgress = starter?.FindAll(info => plr.QtsInterface.HasQuest(info.Entry) && !plr.QtsInterface.HasDoneQuest(info.Entry));

            if (starter != null && starter.FindAll(q => plr.QtsInterface.CanStartQuest(q)).Count > 0)
                return true;

            if (finisher != null && finisher.FindAll(q => plr.QtsInterface.CanEndQuest(q)).Count > 0)
                return true;

            if (inProgress != null && inProgress.Count > 0)
                return true;

            return false;
        }

        public void BuildInteract(Player plr, Creature crea, PacketOut Out)
        {
            List<Quest> starter = crea.Spawn.Proto.StartingQuests;
            List<Quest> finisher = crea.Spawn.Proto.FinishingQuests;
            List<Quest> inProgress = starter != null ? starter.FindAll(info => plr.QtsInterface.HasQuest(info.Entry) && !plr.QtsInterface.HasDoneQuest(info.Entry)) : null;

            Out.WriteUInt32(0);
            Out.WriteUInt16(plr.Oid);

            if (starter != null)
            {
                List<Quest> starts = starter.FindAll(q => plr.QtsInterface.CanStartQuest(q));

                Out.WriteByte((byte)starts.Count);
                foreach (Quest q in starts)
                {
                    Out.WriteByte(0);
                    Out.WriteUInt16(q.Entry);
                    Out.WriteUInt16(0);
                    Out.WritePascalString(q.Name);
                }
            }
            else
                Out.WriteByte(0);

            if (finisher != null)
            {
                List<Quest> finishs = finisher.FindAll(q => plr.QtsInterface.CanEndQuest(q));

                Out.WriteByte((byte)finishs.Count);
                foreach (Quest q in finishs)
                {
                    Out.WriteByte(0);
                    Out.WriteUInt16(q.Entry);
                    Out.WritePascalString(q.Name);
                }
            }
            else if (inProgress != null)
            {
                Out.WriteByte((byte)inProgress.Count);
                foreach (Quest q in inProgress)
                {
                    Out.WriteByte(0);
                    Out.WriteUInt16(q.Entry);
                    Out.WritePascalString(q.Name);
                }
            }
            else
                Out.WriteByte(0);
        }

        #endregion

        #region Players

        public Dictionary<ushort, Character_quest> Quests = new Dictionary<ushort, Character_quest>();

        public void Load(List<Character_quest> quests)
        {
            if (quests == null)
                return;

            foreach (Character_quest quest in quests)
            {
                quest.Quest = QuestService.GetQuest(quest.QuestID);
                if (quest.Quest == null)
                    continue;

                foreach (Character_Objectives obj in quest._Objectives)
                    obj.Objective = QuestService.GetQuestObjective(obj.ObjectiveID);

                // If a quest objective has been deleted in the world db lets remove it from the player
                quest._Objectives = quest._Objectives.FindAll(o => o.Objective != null);

                if (!this.Quests.ContainsKey(quest.QuestID))
                    this.Quests.Add(quest.QuestID, quest);
            }
        }

        public override void Save()
        {
            foreach (KeyValuePair<ushort, Character_quest> kp in Quests)
                CharMgr.Database.SaveObject(kp.Value);

            // Lock? Threadsafe?
            CharMgr.Chars[_Owner.GetPlayer().CharacterId].Quests = Quests.Values.ToList();
        }

        public bool HasQuest(ushort questID)
        {
            if (questID == 0)
                return true;

            return Quests.ContainsKey(questID);
        }

        public bool HasFinishQuest(ushort questID)
        {
            if (questID == 0)
                return true;

            if (!HasQuest(questID))
                return false;

            return GetQuest(questID).IsDone();
        }

        public bool HasDoneQuest(ushort questID)
        {
            if (questID == 0)
                return true;

            if (!HasQuest(questID))
                return false;

            return GetQuest(questID).Done;
        }

        public Character_quest GetQuest(ushort questID)
        {
            Character_quest quest;
            Quests.TryGetValue(questID, out quest);
            return quest;
        }

        public bool CanStartQuest(Quest quest)
        {
            if(GetPlayer() == null)
                return false;

            if (quest == null)
                return false;

            // RB   7/4/2016    Add min/max level and renown rank for quests, allow them to be turned off.
            if (!quest.Active)
            {
                return false;
            }

            if (HasQuest(quest.Entry)
                || HasDoneQuest(quest.Entry)
                || (quest.PrevQuest != 0 && !HasDoneQuest(quest.PrevQuest)))
            {
                return false;
            }

            if (quest.MinLevel > GetPlayer().Level
                || quest.MaxLevel < GetPlayer().Level
                || quest.MinRenown > GetPlayer().RenownRank
                || quest.MaxRenown < GetPlayer().RenownRank)
            {
                return false;
            }

            return true;
        }

        public bool CanEndQuest(Quest quest)
        {
            if (GetPlayer() == null)
                return false;

            if (quest == null)
                return false;

            foreach(Quest_Objectives qo in quest.Objectives)
            {
                if (qo.ObjType == (byte)Objective_Type.QUEST_SPEAK_TO && HasQuest(quest.Entry) && !HasDoneQuest(quest.Entry))
                    return true;
            }

            if (!HasQuest(quest.Entry) || !HasFinishQuest(quest.Entry) || HasDoneQuest(quest.Entry))
                return false;

            return true;
        }

        public bool AcceptQuest(ushort questID)
        {
            return AcceptQuest(QuestService.GetQuest(questID));
        }

        const int MAX_ACTIVE_QUESTS = 60;

        public bool AcceptQuest(Quest quest)
        {
            if (quest == null)
                return false;

            if (!CanStartQuest(quest))
                return false;

            int activeQuestCount = Quests.Values.Count(q => !q.Done);

            if (activeQuestCount >= MAX_ACTIVE_QUESTS)
            {
                GetPlayer().SendClientMessage("Too many quests", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                GetPlayer().SendClientMessage("You have too many quests. Please abandon some of them first.");
                return false;
            }

            Character_quest cQuest = new Character_quest();
            cQuest.QuestID = quest.Entry;
            cQuest.Done = false;
            cQuest.CharacterId = GetPlayer().CharacterId;
            cQuest.Quest = quest;

            foreach(Quest_Objectives qObj in quest.Objectives)
            {
                Character_Objectives cObj = new Character_Objectives();
                cObj.Quest = cQuest;
                cObj._Count = 0;
                cObj.Objective = qObj;
                cObj.ObjectiveID = qObj.Guid;
                cQuest._Objectives.Add(cObj);
            }

            CharMgr.Database.AddObject(cQuest);
            Quests.Add(quest.Entry, cQuest);

            SendQuestState(quest, QuestCompletion.QUESTCOMPLETION_OFFER);

            // This will make objects lootable if they contain a quest object.
            UpdateObjects();

            _Owner.EvtInterface.Notify(EventName.OnAcceptQuest, _Owner, cQuest);
            return true;
        }

        public void AbandonQuest(ushort questID)
        {
            Character_quest quest = GetQuest(questID);
            if (quest == null)
                return;

            Quests.Remove(quest.QuestID);
            SendQuestState(quest.Quest, QuestCompletion.QUESTCOMPLETION_ABANDONED);
            CharMgr.Database.DeleteObject(quest);

            // This will make objects unlootable if they were lootable because of a quest.
            UpdateObjects();

            // Update quest givers around
            foreach (Object obj in _Owner.ObjectsInRange)
            {
                if (obj.IsCreature())
                    obj.GetCreature().SendMeTo(_Owner.GetPlayer());
            }

            _Owner.EvtInterface.Notify(EventName.OnAcceptQuest, _Owner, quest);
        }

        public bool DoneQuest(ushort questID)
        {
             Character_quest quest = GetQuest(questID);
             bool save= true;

            if (quest == null || !quest.IsDone() || quest.Done)
                return false;

            Player plr = GetPlayer();

            Dictionary<Item_Info, uint> choices = GenerateRewards(quest.Quest, plr);

            if (quest.Quest.Choice != null && quest.Quest.Choice.Length > 0 && quest.SelectedRewards.Count == 0)
            {
                return false;
            }

            ushort freeSlots = plr.ItmInterface.GetTotalFreeInventorySlot();
            if (freeSlots < quest.SelectedRewards.Count)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_OVERAGE_CANT_SALVAGE);
                return false;
            }

            foreach (Quest_Objectives obj in quest.Quest.Objectives)
            {
                if ((Objective_Type)obj.ObjType == Objective_Type.QUEST_GET_ITEM 
                    || (Objective_Type)obj.ObjType == Objective_Type.QUEST_USE_ITEM)
                {
                    if (obj.Item != null)
                    {
                        plr.ItmInterface.RemoveQuestItems(obj.Item.Entry);
                    }
                }
                if (quest.Quest.Repeatable)
                {
                    save = false;
                    Quests.Remove(quest.QuestID);
                    SendQuestState(quest.Quest, QuestCompletion.QUESTCOMPLETION_ABANDONED);
                    CharMgr.Database.DeleteObject(quest);

                    // This will make objects unlootable if they were lootable because of a quest.
                    UpdateObjects();

                    // Update quest givers around
                    foreach (Object obje in _Owner.ObjectsInRange)
                    {
                        if (obje.IsCreature())
                            obje.GetCreature().SendMeTo(_Owner.GetPlayer());
                    }

                    _Owner.EvtInterface.Notify(EventName.OnAcceptQuest, _Owner, quest);
                }
            }

            byte num = 0;
            foreach (KeyValuePair<Item_Info, uint> kp in choices)
            {
                if (quest.SelectedRewards.Contains(num))
                {
                    plr.ItmInterface.CreateItem(kp.Key, (ushort)kp.Value);
                }
                ++num;
            }

            plr.AddXp(quest.Quest.Xp, false, false);
            plr.AddMoney(quest.Quest.Gold);

            quest.Done = true;
            quest.Dirty = true;
            quest.SelectedRewards.Clear();
            SendQuestState(quest.Quest, QuestCompletion.QUESTCOMPLETION_DONE);

            if(save)
                CharMgr.Database.SaveObject(quest);


            _Owner.EvtInterface.Notify(EventName.OnDoneQuest, _Owner, quest);
            return true;
        }

        public void FinishQuest(Quest quest)
        {
            if (quest == null)
                return;

            if (!HasFinishQuest(quest.Entry))
                return;
        }

        public void HandleEvent(Objective_Type type, uint entry, int count, bool suppressGroupBroadcast = false)
        {
            Player player = _Owner as Player;

            if (!suppressGroupBroadcast)
            {
                if (player?.PriorityGroup != null)
                {
                    Group currentGroup = player.PriorityGroup;

                    foreach (Player member in currentGroup.GetPlayersCloseTo(player, 150))
                    {
                        if (member != player)
                            member.QtsInterface.HandleEvent(type, entry, count, true);
                    }
                }
            }

             if(_Owner is Pet && _Owner.GetPet().Owner != null)
                 _Owner.GetPet().Owner.QtsInterface.HandleEvent(type, entry, count, true);

            // Check every quest a player has... 
            foreach (KeyValuePair<ushort, Character_quest> questKp in Quests)
            {
                // For each objective in every quest...
                foreach (Character_Objectives objective in questKp.Value._Objectives)
                {
                    if (objective.Objective == null)
                        continue;

                    // RB   7/4/2016    Allow objectives to be completed in an order                 
                    if (objective.Objective.PreviousObj > 0)
                    {
                        Character_Objectives previousObjective = questKp.Value._Objectives.FirstOrDefault(o => o.Objective.Guid == objective.Objective.PreviousObj);
                        if (previousObjective != null && !previousObjective.IsDone())
                            continue;
                    }

                    if (objective.Objective.ObjType == (uint)type && !objective.IsDone())
                    {
                        if(objective.Objective.PQArea > 0)
                        {
                            bool skip = true;

                            if (player.CurrentKeep != null && player.CurrentKeep.Realm == player.Realm && player.CurrentKeep.Info.PQuest != null && player.CurrentKeep.Info.PQuest.Entry == objective.Objective.PQArea)
                                skip = false;

                            else if (PublicQuest != null && PublicQuest.Info.Entry == objective.Objective.PQArea)
                                skip = false;

                            if (skip)
                                continue;
                        }

                        if(!string.IsNullOrEmpty(objective.Objective.inZones))
                        {
                            string[] temp = objective.Objective.inZones.Split(',');
                            if (temp != null && (player.Zone == null || !temp.Contains(""+ player.Zone.ZoneId)))
                                continue;
                        }


                        bool canAdd = false;
                        int newCount = objective.Count;

                        switch (type)
                        {
                            case Objective_Type.QUEST_SPEAK_TO:
                            case Objective_Type.QUEST_KILL_MOB:
                            case Objective_Type.QUEST_PROTECT_UNIT:
                                if (objective.Objective.Creature != null && entry == objective.Objective.Creature.Entry)
                                {
                                    canAdd = true;
                                    newCount += count;
                                }
                                break;
                            case Objective_Type.QUEST_KILL_GO:
                                if (objective.Objective.GameObject != null && entry == objective.Objective.GameObject.Entry)
                                {
                                    canAdd = true;
                                    newCount += count;
                                }
                                break;
                            case Objective_Type.QUEST_KILL_PLAYERS:
                                if (objective.Objective != null)
                                {
                                    int result;

                                    if (int.TryParse(objective.Objective.ObjID, out result))
                                    {
                                        if (result == 0 || ((result >> ((byte)entry - 1)) & 1) == 1)
                                        {
                                            canAdd = true;
                                            newCount += count;
                                        }
                                    }
                                }
                                break;

                            case Objective_Type.QUEST_GET_ITEM:
                            case Objective_Type.QUEST_USE_ITEM:
                                if (objective.Objective.Item != null && entry == objective.Objective.Item.Entry)
                                {
                                    canAdd = true;
                                    newCount = _Owner.GetPlayer().ItmInterface.GetItemCount(entry);
                                }
                                break;

                            case Objective_Type.QUEST_USE_GO:
                                if (objective.Objective.GameObject != null && entry == objective.Objective.GameObject.Entry)
                                {
                                    // This will turn off Interactable flag on clicked GO, some more work can be  
                                    // done with GO despawning and UNKs[3] unk modification
                                    // Default respawn time: 60 seconds
                                    Object target = player.CbtInterface.GetCurrentTarget();
                                    if (target != null)
                                    {
                                        GameObject go = target.GetGameObject();
                                        if (go != null && go.IsGameObject())
                                        {
                                            if (go.Spawn.AllowVfxUpdate == 1) go.VfxState = 1;
                                            go.Interactable = false;
                                            go.EvtInterface.AddEvent(MakeGOInteractable, 60000, 1, target);
                                        }
                                    }

                                    canAdd = true;
                                    newCount += count;
                                }
                                break;

                            case Objective_Type.QUEST_WIN_SCENARIO:
                                if (objective.Objective.Scenario != null && entry == objective.Objective.Scenario.ScenarioId)
                                {
                                    canAdd = true;
                                    newCount += count;
                                }
                                break;

                            case Objective_Type.QUEST_CAPTURE_BO:
                                if (objective.Objective.BattleFrontObjective != null && entry == objective.Objective.BattleFrontObjective.Entry)
                                {
                                    canAdd = true;
                                    newCount += count;
                                }
                                break;

                            case Objective_Type.QUEST_CAPTURE_KEEP:
                                if (objective.Objective.Keep != null && entry == objective.Objective.Keep.KeepId)
                                {
                                    canAdd = true;
                                    newCount += count;
                                }
                                break;

                            default:
                                if (objective.Objective.Guid == entry)
                                {
                                    canAdd = true;
                                    newCount += count;
                                }
                                break;
                        }

                        if (canAdd)
                        {
                            objective.Count = newCount;
                            questKp.Value.Dirty = true;
                            SendQuestUpdate(questKp.Value);
                            CharMgr.Database.SaveObject(questKp.Value);

                            if (objective.IsDone())
                            {
                                Creature finisher;

                                foreach (Object obj in _Owner.ObjectsInRange)
                                {
                                    if (obj.IsCreature())
                                    {
                                        finisher = obj.GetCreature();
                                        if (QuestService.HasQuestToFinish(finisher.Entry, questKp.Value.Quest.Entry))
                                            finisher.SendMeTo(_Owner.GetPlayer());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // This is run by an event handler in HandleEvent method, sets Interactable flag to true
        public void MakeGOInteractable(object target)
        {
            GameObject go = target as GameObject;

            if (go != null && go.IsGameObject())
            {
                if (go.Spawn.AllowVfxUpdate == 1) go.VfxState = 0;
                go.Interactable = true;
            }
        }

        public void SelectRewards(ushort questID, byte num)
        {
            Character_quest quest = GetQuest(questID);
            if (quest == null || !quest.IsDone())
                return;

             if((num & 1) > 0)
                quest.SelectedRewards.Add(0);
             if ((num & 2) > 0)
                 quest.SelectedRewards.Add(1);
             if ((num & 4) > 0)
                 quest.SelectedRewards.Add(2);
             if ((num & 8) > 0)
                 quest.SelectedRewards.Add(3);
        }

        #endregion

        public static void BuildQuestInfo(PacketOut Out, Player plr, Quest q)
        {
            BuildQuestHeader(Out, plr, q, true);

            BuildQuestRewards(Out, plr, q);

            BuildObjectives(Out, q.Objectives);

            Out.WriteByte(0);
        }

        public static void BuildQuestHeader(PacketOut Out, Player plr, Quest q, bool particular)
        {
            Out.WritePascalString(q.Name);

            Out.WriteUInt16((ushort)q.Description.Length);
            Out.WriteStringBytes(q.Description);

            if (particular)
            {
                Out.WriteUInt16((ushort)q.Particular.Length);
                Out.WriteStringBytes(q.Particular);
            }
            Out.WriteByte(1);
            Out.WriteUInt32(q.Gold);
            Out.WriteUInt32(q.Xp);

        }

        public static void BuildQuestInProgress(PacketOut Out, Quest q, bool particular)
        {
            Out.WritePascalString(q.Name);

            if (q.ProgressText.Length > 0)
            {
                Out.WriteUInt16((ushort)q.ProgressText.Length);
                Out.WriteStringBytes(q.ProgressText);
            }
            else
            {
                Out.WriteUInt16((ushort)q.Particular.Length);
                Out.WriteStringBytes(q.Particular);
            }

            Out.WriteByte(1);
        }

        /// <summary>
        /// Writes 13 + quest name length + description length + particular length.
        /// </summary>
        public static void BuildQuestComplete(PacketOut Out, Quest q, bool particular)
        {
            Out.WritePascalString(q.Name);

            if (q.OnCompletionQuest.Length > 0)
            {
                Out.WriteUInt16((ushort)q.OnCompletionQuest.Length);
                Out.WriteStringBytes(q.OnCompletionQuest);
            }
            else
            {
                Out.WriteUInt16((ushort)q.Description.Length);
                Out.WriteStringBytes(q.Description);
            }

            if (particular)
            {
                Out.WriteUInt16((ushort)q.Particular.Length);
                Out.WriteStringBytes(q.Particular);
            }
            Out.WriteByte(1);
            Out.WriteUInt32(q.Gold);
            Out.WriteUInt32(q.Xp);
        }
        public static void BuildQuestRewards(PacketOut Out, Player plr, Quest q)
        {
            Dictionary<Item_Info, uint> choices = GenerateRewards(q, plr);

            Out.WriteByte(Math.Min(q.ChoiceCount,(byte)choices.Count));
            Out.WriteByte(0);
            Out.WriteByte((byte)choices.Count);

            foreach (KeyValuePair<Item_Info, uint> kp in choices)
                Item.BuildItem(ref Out, null, kp.Key,null, 0, (ushort)kp.Value);
        }

        /// <summary>Writes 10 bytes.</summary>
        public static void BuildQuestInteract(PacketOut Out,ushort questID, ushort senderOid, ushort receiverOid)
        {
            Out.WriteUInt16(questID);
            Out.WriteUInt16(0);

            Out.WriteUInt16(senderOid);
            Out.WriteUInt16(0);

            Out.WriteUInt16(receiverOid);
        }

        public void BuildQuest(ushort questID, Player plr)
        {
            Quest q = QuestService.GetQuest(questID);
            if (q == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 14);
            Out.WriteByte(1);
            Out.WriteByte(1);

            BuildQuestInteract(Out, q.Entry, _Owner.Oid, plr.Oid); // 10 bytes

            Out.WriteUInt16(0);

            BuildQuestInfo(Out, plr, q);

            plr.SendPacket(Out);
        }

        /// <summary>Writes 2 bytes.</summary>
        public void BuildQuest(PacketOut Out, Quest q)
        {
            Out.WriteByte(q.ChoiceCount);
            Out.WriteByte(0);
        }

        public static void BuildObjectives(PacketOut Out, List<Quest_Objectives> objs)
        {
            Out.WriteByte((byte)objs.Count);

            List<Quest_Objectives> SortedObjectives = objs.OrderBy(x => x.Entry).ThenBy(x => x.Guid).ToList();

            foreach (Quest_Objectives objective in SortedObjectives)
            {
                Out.WriteByte((byte)objective.ObjCount);
                Out.WritePascalString(objective.Description);
            }
        }

        public static void BuildObjectives(PacketOut Out, List<Character_Objectives> objs)
        {
            Out.WriteByte((byte)objs.Count);

            List<Character_Objectives> SortedObjectives = objs.OrderBy(x => x.Objective.Entry).ThenBy(x => x.Objective.Guid).ToList();

            foreach (Character_Objectives objective in SortedObjectives)
            {
                Out.WriteByte((byte)objective.Count);
                Out.WriteByte((byte)objective.Objective.ObjCount);
                Out.WriteUInt16(0);
                Out.WritePascalString(objective.Objective.Description);
            }
        }

        public void SendQuest(ushort questID)
        {
            Character_quest cQuest = GetQuest(questID);
            SendQuest(cQuest);
        }

        public void SendQuests()
        {
            List<Character_quest> quests = Quests.Values.ToList().FindAll(q => q.Done == false);

            PacketOut Out = new PacketOut((byte)Opcodes.F_QUEST_LIST, 1 + quests.Count * 14);
            Out.WriteByte((byte)quests.Count);
            foreach (Character_quest quest in quests)
            {
                Out.WriteUInt16(quest.QuestID);
                Out.WriteByte(0);
                Out.WritePascalString(quest.Quest.Name);
                Out.WriteByte(0);
            }

            GetPlayer().SendPacket(Out);
        }

        public void SendQuest(Character_quest cQuest)
        {
            if (cQuest == null)
            {
                Log.Error("QuestsInterface", "SendQuest CQuest == null");
                return;
            }

            PacketOut packet = new PacketOut((byte)Opcodes.F_QUEST_INFO);
            packet.WriteUInt16(cQuest.QuestID);
            packet.WriteByte(cQuest.Quest.Type); // Quest Type (database is wrong)
            BuildQuestHeader(packet, GetPlayer(), cQuest.Quest, true);

            Dictionary<Item_Info, uint> rewards = GenerateRewards(cQuest.Quest, GetPlayer());

            packet.WriteByte(cQuest.Quest.ChoiceCount);
            packet.WriteByte(0);
            packet.WriteByte((byte)rewards.Count);

            foreach (KeyValuePair<Item_Info, uint> kp in rewards)
            {
                Item.BuildItem(ref packet, null, kp.Key,null, 0, (ushort)kp.Value);
            }

            packet.WriteByte(0);

            BuildObjectives(packet, cQuest._Objectives);

            List<Quest_Map> SortedMaps = cQuest.Quest.Maps.OrderBy(x => x.Entry).ThenByDescending(x => x.Id).ToList();

            foreach (Quest_Map map in SortedMaps)
            {
                packet.WriteByte(map.Id);
                packet.WritePascalString(map.Name);
                packet.WritePascalString(map.Description);
                packet.WriteUInt16(map.ZoneId);
                packet.WriteUInt16(map.Icon);
                packet.WriteUInt16(map.X);
                packet.WriteUInt16(map.Y);
                packet.WriteUInt16(map.Unk);
                packet.WriteByte(map.When);
            }

            packet.WriteByte(0);

            GetPlayer().SendPacket(packet);
        }

        public void SendQuestDoneInfo(Player plr, ushort questID)
        {
            Character_quest quest = plr.QtsInterface.GetQuest(questID);

            if (quest == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 12 + 13 + quest.Quest.Name.Length + quest.Quest.Description.Length);
            Out.WriteByte(3);
            Out.WriteByte(0);

            BuildQuestInteract(Out, quest.QuestID, _Owner.Oid, plr.Oid); // 10

            BuildQuestComplete(Out, quest.Quest, false);

            BuildQuestRewards(Out, plr, quest.Quest);

            plr.SendPacket(Out);
        }

        public void SendQuestInProgressInfo(Player plr, ushort questID)
        {
            Character_quest quest = plr.QtsInterface.GetQuest(questID);

            if (quest == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE);
            Out.WriteByte(2);
            Out.WriteByte(1);

            BuildQuestInteract(Out, quest.QuestID, _Owner.Oid, plr.Oid);

            BuildQuestInProgress(Out, quest.Quest, false);

            plr.SendPacket(Out);
        }

        public void SendQuestState(Quest quest,QuestCompletion state)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_QUEST_LIST_UPDATE, 32);
            Out.WriteUInt16(quest.Entry);

            if (state == QuestCompletion.QUESTCOMPLETION_ABANDONED || state == QuestCompletion.QUESTCOMPLETION_DONE)
                Out.WriteByte(0);
            else
                Out.WriteByte(1);

            Out.WriteByte((byte)(state == QuestCompletion.QUESTCOMPLETION_DONE ? 1 : 0));

            Out.WriteUInt32(0x0000FFFF);
            Out.WritePascalString(quest.Name);
            Out.WriteByte(0);
            GetPlayer().SendPacket(Out);
        }

        public void SendQuestUpdate(Character_quest quest)
        {
            if (GetPlayer() == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_QUEST_UPDATE, 6 + quest._Objectives.Count);
            Out.WriteUInt16(quest.QuestID);
            Out.WriteByte(Convert.ToByte(quest.IsDone()));
            Out.WriteByte((byte)quest._Objectives.Count);
            foreach (Character_Objectives obj in quest._Objectives)
            {
                Out.WriteByte((byte)obj.Count);
            }
            Out.WriteUInt16(0);
            GetPlayer().SendPacket(Out);
        }

        public static Dictionary<Item_Info,uint> GenerateRewards(Quest q, Player plr)
        {
            Dictionary<Item_Info,uint> rewards = new Dictionary<Item_Info,uint>();

            foreach (KeyValuePair<Item_Info, uint> kp in q.Rewards)
                if (ItemsInterface.CanUse(kp.Key, plr, true, false))
                    rewards.Add(kp.Key, kp.Value);

            return rewards;
        }

        public bool GameObjectNeeded(uint entry)
        {
            foreach (KeyValuePair<ushort, Character_quest> questKp in Quests)
            {
                foreach (Character_Objectives objective in questKp.Value._Objectives)
                {
                    if (objective.Objective == null)
                        return false;

                    if (objective.Objective.ObjType == (uint)Objective_Type.QUEST_USE_GO)
                    {
                        if (objective.IsDone())
                            continue;

                        if (objective.Objective.GameObject != null && entry == objective.Objective.GameObject.Entry)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // For quests which require you to loot GameObjects this will update any objects
        // around you and make them lootable if they have items you need for a quest.
        // Notes: We could minimise the amount of SendMeTo's by checking if object are already
        // flagged and unflag them or unflagged and need flagging. However it isnt possible
        // to see if its already been flagged at the moment.
        public void UpdateObjects()
        {
            GameObject gameObject;

            foreach (Object obj in _Owner.ObjectsInRange)
            {
                if (obj.IsGameObject())
                {
                    gameObject = obj.GetGameObject();
                    //Loot Loots = LootsMgr.GenerateLoot(GameObject, _Owner.GetPlayer());
                    //if (Loots != null && Loots.IsLootable())
                    gameObject.SendMeTo(_Owner.GetPlayer());
                }
            }
        }

        #region Public Quest

        public PublicQuest PublicQuest { get; set; }

        public void NotifyReceivedPQuestMobHit(PQuestCreature mob, uint damageCount)
        {
            PublicQuest?.AddTrackedDamage((Player)_Owner, mob, damageCount);
        }

        public void NotifyHealingReceived(Player healer, uint count)
        {
            PublicQuest?.NotifyPlayerHealed((Player) _Owner, healer, count);
        }

        #endregion
    }
}
