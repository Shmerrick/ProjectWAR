using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Abilities.CareerInterfaces;
using WorldServer.World.Scenarios;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects.PublicQuests
{

    public class PublicQuest : Object
    {
        public PQuest_Info Info;
        public PQuestStage Stage;
        public List<uint> ActivePlayers;
        public Dictionary<uint, ContributionInfo> Players;
        public List<PQuestStage> Stages;

        const ushort TIME_PQ_RESET = 180;
        const ushort TIME_DUNGEON_RESET = 28800;
        const ushort TIME_EACH_STAGE = 540;

        private bool _started;
        private bool _ended;
        private int _stageTimeEnd;
        private ushort _interactiveGO;

        public PublicQuest()
        {

        }

        /// <summary>
        /// Initializes the public quest's objectives and stages.
        /// </summary>
        /// <param name="info"></param>
        public PublicQuest(PQuest_Info info)
            : this()
        {
            Info = info;
            Name = info.Name;
            ActivePlayers = new List<uint>();
            Players = new Dictionary<uint, ContributionInfo>();
            Stages = new List<PQuestStage>();

            foreach (PQuest_Objective obj in info.Objectives)
            {
                // Create a new public quest stage for this objective, if one did not previously exist.
                bool exists = Stages.Any(x => x.StageName == obj.StageName);

                if (!exists)
                {
                    PQuestStage stage = new PQuestStage
                    {
                        StageName = obj.StageName,
                        Number = Stages.Count,
                        Description = obj.Description,
                        Time = obj.Time
                    };
                    Stages.Add(stage);
                }

                // Assign this objective to its public quest stage.
                foreach (PQuestStage stage in Stages)
                {
                    if (stage.StageName == obj.StageName)
                    {
                        PQuestObjective objective = new PQuestObjective
                        {
                            Quest = this,
                            Objective = obj,
                            ObjectiveID = obj.Guid,
                            Count = 0
                        };
                        stage.AddObjective(objective);
                    }
                }
            }
        }

        #region Packet Sending

        /*public override void SendMeTo(Player plr)
        {
            //if ((byte)plr.Realm != Info.Type)
            return;

            //SendCurrentStage(plr);
            //SendStageInfo(plr);
            /*
             if(!started && ended)
                 SendReinitTime(plr, TIME_PQ_RESET);
*/
            // TODO
            // Send Quest Info && Current Stage && Current Players
        /*}*/
    
        public void SendReinitTime(Player player, ushort time)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO);
            Out.WriteUInt32(Info.Entry);
            Out.WriteByte(1);
            Out.WriteByte(2);
            Out.WriteByte(1);
            Out.WriteUInt16(0);
            Out.WritePascalString(Info.Name);
            Out.WriteUInt16(0);
            Out.WriteUInt16(time); // LastUpdatedTime in seconds
            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            player.SendPacket(Out);
        }

        public void SendStageInfo(Player player)
        {
            /*
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE);
            Out.WriteUInt32(Info.Entry);
            Out.WriteByte(0);
            Out.Fill(0, 3);
            Plr.SendPacket(Out);
            */
        }

        public void SendCurrentStage(Player player)
        {
            player.QtsInterface.UpdateObjects();

            if (Stage == null)
            {
                //Log.Error("Public Quest Object", "No Stage!");
                return;
            }
            if (_ended)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO);
                Out.WriteUInt32(Info.Entry);
                Out.WriteByte(1);
                Out.WriteByte(0);
                Out.WriteByte(1);  // 1 1 condition 2 or condition
                Out.WriteUInt16(0);
                Out.WritePascalString(Info.Name);
                Out.WriteUInt32((UInt32)(_stageTimeEnd - TCPManager.GetTimeStamp()));       // time left
                Out.Fill(0, 4);
                player.SendPacket(Out);
            }
            else
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO);
                Out.WriteUInt32(Info.Entry);
                Out.WriteByte(0); // Type
                Out.WriteByte(Info.Type);
                Out.WriteByte(1);  // 1 1 condition 2 or condition
                Out.WriteUInt16(0);
                Out.WritePascalString(Info.Name);
                Out.WriteByte(Info.Type);
                Out.WriteUInt32(UInt32.Parse(Stage.Objectives.First().Objective.ObjectId));
                Out.WriteByte(0);
                Out.WriteByte((byte)Stage.Objectives.Count);
                byte i = 0;
                foreach (PQuestObjective obj in Stage.Objectives)
                {
                    Out.WriteByte(i);
                    Out.WriteUInt16((ushort)obj.Objective.Count);  // kill count
                    Out.WriteUInt16((ushort)obj.Count);
                    Out.WriteByte(0);
                    Out.WritePascalString(obj.Objective.Objective);
                    i++;
                }

                Out.WriteByte((byte)(Info.PQDifficult > 0 ? (Info.PQDifficult - 1) : 0 - 1));    // difficulty 0 - 2
                Out.WriteByte(0);
                Out.WritePascalString(Stage.StageName);
                Out.WriteByte(0);
                Out.WritePascalString(Stage.Objectives.First().Objective.Description);
                Out.WriteUInt16(0);
                if (Stage.Number == 0)
                    Out.WriteUInt16(0);
                else
                    Out.WriteUInt16(1);
                Out.WriteUInt32((UInt32)(_stageTimeEnd - TCPManager.GetTimeStamp()));       // time left
                Out.WriteUInt32(0);
                Out.WriteByte(0x48);
                Out.WriteUInt32(0);
                player.SendPacket(Out);


                if (player.DebugMode)
                    player.SendLocalizeString("PQ STAGE ID: " + Stage.Objectives.First().ObjectiveID, ChatLogFilters.CHATLOGFILTERS_ALLIANCE, GameData.Localized_text.CHAT_TAG_MONSTER_EMOTE);


            }
        }

        #endregion

        public override void OnLoad()
        {
            #if DEBUG
            Log.Success("PQ", "Loading :" + Info.Name);
            #endif

            X = (int)Info.PinX;
            Y = (int)Info.PinY;
            if (Info.GoldChestWorldZ > 0)
                Z = Info.GoldChestWorldZ;
            else
                Z = 16384;
            SetOffset(Info.OffX, Info.OffY);

            UpdateWorldPosition();

            base.OnLoad();
            IsActive = true;
        }

        public void AddPlayer(Player plr)
        {
  
            if (Info.Type !=0  && (byte)plr.Realm != Info.Type)
                return;

            if (plr.QtsInterface.PublicQuest != null)
                plr.QtsInterface.PublicQuest.RemovePlayer(plr, false);

            // RB   5/22/2016   Prevent underleveled players from leeching rewards.
            switch (Info.PQTier)
            {
                case 1:
                    if (!ActivePlayers.Contains(plr.CharacterId))
                    {
                        ActivePlayers.Add(plr.CharacterId);
                    }
                    if (!Players.ContainsKey(plr.CharacterId))
                    {
                        Players.Add(plr.CharacterId, new ContributionInfo(plr));
                    }
                    #if DEBUG
                    Log.Success("PQuest", "Adding " + plr.Name);
                    #endif
                    break;
                case 2:
                    if (plr.Level > 8) // Players level 9 or higher can get T2 PQ rewards now.
                    {
                        if (!ActivePlayers.Contains(plr.CharacterId))
                        {
                            ActivePlayers.Add(plr.CharacterId);
                        }

                        if (!Players.ContainsKey(plr.CharacterId))
                        {
                            Players.Add(plr.CharacterId, new ContributionInfo(plr));
                        }
                        #if DEBUG
                        Log.Success("PQuest", "Adding " + plr.Name);
                        #endif
                    }
                    break;
                case 3:
                    if (plr.Level > 17)
                    {
                        if (!ActivePlayers.Contains(plr.CharacterId))
                        {
                            ActivePlayers.Add(plr.CharacterId);
                        }
                        if (!Players.ContainsKey(plr.CharacterId))
                        {
                            Players.Add(plr.CharacterId, new ContributionInfo(plr));
                        }
                        #if DEBUG
                        Log.Success("PQuest", "Adding " + plr.Name);
                        #endif
                    }
                    break;
                case 4:
                    if (plr.Level > 27)
                    {
                        if (!ActivePlayers.Contains(plr.CharacterId))
                        {
                            ActivePlayers.Add(plr.CharacterId);
                        }
                        if (!Players.ContainsKey(plr.CharacterId))
                        {
                            Players.Add(plr.CharacterId, new ContributionInfo(plr));
                        }
                        #if DEBUG
                        Log.Success("PQuest", "Adding " + plr.Name);
                        #endif
                    }
                    break;
            }

            plr.QtsInterface.PublicQuest = (this);

            if (Info.TokDiscovered > 0)
                plr.TokInterface.AddTok((ushort)Info.TokDiscovered);
            if (Info.TokUnlocked > 0)
                plr.TokInterface.AddTok((ushort)Info.TokUnlocked);

            if (!_started && !_ended)
                Start();

            SendCurrentStage(plr);
        }

        public void RemovePlayer(Player plr, bool ForceRemove = false)
        {
            if(ActivePlayers.Contains(plr.CharacterId))
                ActivePlayers.Remove(plr.CharacterId);
            if (ForceRemove)
            {
                if (Players.ContainsKey(plr.CharacterId))
                    Players.Remove(plr.CharacterId);
            }
            #if DEBUG
            Log.Success("PQuest", "Removing " + plr.Name);
            #endif

            plr.QtsInterface.PublicQuest = null;

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE, 8);
            Out.WriteUInt32(Info.Entry);
            Out.WriteUInt32(0);
            plr.SendPacket(Out);
        }

        #region Progression

        public void Start()
        {
            if (_started || _ended)
                return;

            foreach (PQuestStage sStage in Stages)
            {
                try
                {

                    if (sStage.Number == 0)
                    {
                        Stage = sStage;
                        Stage.Reset();
                        foreach (uint Plr in ActivePlayers)
                        {
                            Player targPlayer = Player.GetPlayer(Plr);
                            if (targPlayer != null)
                            {
                                SendCurrentStage(targPlayer);
                            }
                        }
                        break;
                    }

                }
                catch
                {
                    continue;
                }
            }

            #if DEBUG
            Log.Success("PQuest", "Starting Quest " + Info.Name);
            #endif

            _started = true;
        }

        private long _nextContributionTick;
        private bool _activityTickPending;

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

    public void HandleEvent(Player player, Objective_Type type, uint entry, int count, ushort contributionGain,string pqGoClicked = null)
        {
            if (Stage == null)
                return;

            byte objid = 0;
            string objectID = "0";
            foreach (PQuestObjective obj in Stage.Objectives)
            {
                if (obj.IsDone())
                {
                    objid++;
                    continue;
                }
                if (obj.Objective.Type != (int)type)
                {
                    objid++;
                    continue;
                }
                int oldCount = obj.Count;

                switch (type)
                {
                    case Objective_Type.QUEST_SPEAK_TO:
                    case Objective_Type.QUEST_KILL_MOB:

                        uint outVal = 0;
                        //More than one type of NPC can count to the same PQ objective kill count
                        if (obj.Objective.Creature != null && (UInt32.TryParse(obj.Objective.ObjectId, out outVal) && Convert.ToUInt32(obj.Objective.ObjectId) == entry))
                        {
                            objectID = obj.Objective.ObjectId;
                            obj.Count += count;
                            break;
                        }
                        if (obj.Objective.Creature != null && (UInt32.TryParse(obj.Objective.ObjectId2, out outVal) && Convert.ToUInt32(obj.Objective.ObjectId2) == entry))
                        {
                            objectID = obj.Objective.ObjectId2;
                            obj.Count += count;
                            break;
                        }
                        if (obj.Objective.Creature != null && (UInt32.TryParse(obj.Objective.ObjectId3, out outVal) && Convert.ToUInt32(obj.Objective.ObjectId3) == entry))
                        {
                            objectID = obj.Objective.ObjectId3;
                            obj.Count += count;
                            break;
                        }
                        if (obj.Objective.Creature != null && (UInt32.TryParse(obj.Objective.ObjectId4, out outVal) && Convert.ToUInt32(obj.Objective.ObjectId4) == entry))
                        {
                            objectID = obj.Objective.ObjectId4;
                            obj.Count += count;
                            break;
                        }
                        if (obj.Objective.Creature != null && (UInt32.TryParse(obj.Objective.ObjectId4, out outVal) && Convert.ToUInt32(obj.Objective.ObjectId5) == entry))
                        {
                            objectID = obj.Objective.ObjectId4;
                            obj.Count += count;
                            break;
                        }
                        if (obj.Objective.Creature != null && (UInt32.TryParse(obj.Objective.ObjectId4, out outVal) && Convert.ToUInt32(obj.Objective.ObjectId6) == entry))
                        {
                            objectID = obj.Objective.ObjectId4;
                            obj.Count += count;
                            break;
                        }

                        break;

                    case Objective_Type.QUEST_PROTECT_UNIT:
                        if (obj.Objective.Creature != null && entry == obj.Objective.Creature.Entry)
                            obj.Count += count;
                        break;

                    case Objective_Type.QUEST_USE_GO:
                        if (obj.Objective.GameObject != null && entry == obj.Objective.GameObject.Entry)
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

                            obj.Count += count;
                        }
                        break;

                    case Objective_Type.QUEST_KILL_GO:
                        if (obj.Objective.GameObject != null && entry == obj.Objective.GameObject.Entry)
                        {
                            obj.Count += count;
                        }
                        break;

                    case Objective_Type.QUEST_UNKNOWN:
                        if (obj.Objective.Guid == entry)
                            obj.Count += count;
                        break;

                }

                if (obj.Count != oldCount)
                {
                    if (player != null)
                    {
                        uint influenceid;

                        if (Info.Type == 0 && player.CurrentArea != null)
                        {
                            if (player.Realm == GameData.Realms.REALMS_REALM_ORDER)
                                influenceid = player.CurrentArea.OrderInfluenceId;
                            else
                                influenceid = player.CurrentArea.DestroInfluenceId;
                        }
                        else // normaly we should only use the area inf but to not break every pq we keep it like this for now
                        {
                            influenceid = Info.ChapterId;
                        }



                        if (player.WorldGroup == null)
                            player.AddInfluence((ushort)influenceid, (ushort)(100));
                        else
                            player.WorldGroup.AddInfluenceCount(player, (ushort)influenceid, (ushort)(100 * player.WorldGroup.GetPlayersCloseTo(player, 300).Count));

                        if (!Players.ContainsKey(player.CharacterId))
                            Players.Add(player.CharacterId, new ContributionInfo(player));

                        if (Players.ContainsKey(player.CharacterId))
                        {
                            Players[player.CharacterId].BaseContribution += contributionGain;
                            Players[player.CharacterId].ActiveTimeEnd = TCPManager.GetTimeStamp() + 5;
                        }
                    }
                    else
                    {
                        foreach (uint plr in ActivePlayers)
                        {
                            if (Players.ContainsKey(plr))
                                Players[plr].BaseContribution += contributionGain;
                        }
                    }

                    foreach (uint plr in ActivePlayers)
                    {
                        Player targPlayer = Player.GetPlayer(plr);
                        if (targPlayer != null)
                        {
                            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE);
                            Out.WriteUInt32(Info.Entry);
                            Out.WriteByte(1);
                            Out.WriteByte(Info.Type); //realm
                            Out.WriteUInt32(UInt32.Parse(Stage.Objectives.First().Objective.ObjectId)); //ephermal id, sent in main packet for PQ_INFO
                            Out.WriteByte(objid); //index of objective to update
                            Out.WriteUInt16((ushort)obj.Count); // new total
                            targPlayer.SendPacket(Out);
                        }
                    }
                }
                objid++;
            }

            if (Stage.IsDone())
            {
                NextStage();
            }
        }

        public override void Update(long msTick)
        {
            base.Update(msTick);

            long curTimeSeconds = TCPManager.GetTimeStamp();

            TickContribution(curTimeSeconds);

            if (curTimeSeconds > _nextContributionTick)
            {
                _nextContributionTick = TCPManager.GetTimeStamp() + 3;
            }

                if (curTimeSeconds > _nextContributionTick)
            {
                _nextContributionTick = TCPManager.GetTimeStamp() + 3;

                List<uint> PendingRemovals = new List<uint>();
                List<uint> PendingActiveRemovals = new List<uint>();


                foreach(uint plrInfo in ActivePlayers)
                {
                    Player targPlayer = Player.GetPlayer(plrInfo);
                    if (targPlayer != null)
                    {
                        if (targPlayer.ZoneId != Info.ZoneId)
                            PendingRemovals.Add(plrInfo);
                        if (targPlayer.CurrentPQArea != Info.PQAreaId)
                            PendingActiveRemovals.Add(plrInfo);
                    }
                    else
                    {
                            PendingActiveRemovals.Add(plrInfo);
                    }
                }

                foreach (ContributionInfo plrInfo in Players.Values)
                {
                    Player targPlayer = Player.GetPlayer(plrInfo.PlayerCharId);
                    if (targPlayer != null)
                    {

                        if (plrInfo.PendingContribution > 0)
                        {
                            if (targPlayer != null)
                            {
                                SplitContribution(targPlayer, (ushort)plrInfo.PendingContribution);
                            }
                            plrInfo.PendingContribution = 0;
                        }

                        if (plrInfo.ActiveTimeEnd > curTimeSeconds)
                        {
                            plrInfo.BaseContribution += 50;
                            //plrInfo.Player.SendClientMessage("Received 50 contribution points from the activity ticker.", ChatLogFilters.CHATLOGFILTERS_QUEST, true);
                        }
                    }
                }

                foreach (uint plrId in PendingRemovals)
                {
                    Player targPlayer = Player.GetPlayer(plrId);
                    if (targPlayer != null)
                    {
                        RemovePlayer(targPlayer, true);
                    }
                }

                foreach (uint plrId in PendingActiveRemovals)
                {
                    Player targPlayer = Player.GetPlayer(plrId);
                    if (targPlayer != null)
                    {
                        RemovePlayer(targPlayer, false);
                    }
                    else
                    {
                        if (ActivePlayers.Contains(plrId))
                            ActivePlayers.Remove(plrId);
                    }
                }
            }
        }

        public void NextStage()
        {
            ushort tokunlocked = (ushort)Stage.Objectives.First().Objective.TokCompleted;
            foreach (uint Plr in ActivePlayers)
            {
                Player targPlayer = Player.GetPlayer(Plr);
                if (targPlayer != null)
                {
                    if (tokunlocked > 0)
                       targPlayer.TokInterface.AddTok(tokunlocked);
                }
            }

            Stage.Cleanup();
            int nextStageId = Stage.Number + 1;
            EvtInterface.RemoveEvent(Failed);

            foreach (PQuestStage sStage in Stages)
            {
                if (sStage.Number == nextStageId)
                {
                    Stage = sStage;
                    Stage.Reset();
                    _stageTimeEnd = TCPManager.GetTimeStamp() + ((Stage.Time > 0 ? Stage.Time : TIME_EACH_STAGE));
                    if (sStage.Objectives.First().Objective.Type != (byte)Objective_Type.QUEST_PROTECT_UNIT)
                        EvtInterface.AddEvent(Failed, (Stage.Time > 0 ? Stage.Time : TIME_EACH_STAGE) * 1000, 1);

                    foreach (uint Plr in ActivePlayers)
                    {
                        Player targPlayer = Player.GetPlayer(Plr);
                        if (targPlayer != null)
                        {
                            SendCurrentStage(targPlayer);
                            if (Stage.Number > 1)
                                targPlayer.AddInfluence((ushort)Info.ChapterId, 250);
                        }
                    }
                    return;
                }
            }

            End();
        }

        public bool IsDungeon()
        {
            switch (ZoneId)
            {
                case 60:

                    return true;
            }

            return false;
        }

        public void End()
        {
            if (_ended)
                return;

            if (IsDungeon())
            {
                _stageTimeEnd = TCPManager.GetTimeStamp() + TIME_DUNGEON_RESET;
                EvtInterface.AddEvent(Reset, TIME_DUNGEON_RESET * 1000, 1);
            }
            else
            {
                _stageTimeEnd = TCPManager.GetTimeStamp() + TIME_PQ_RESET;
                EvtInterface.AddEvent(Reset, TIME_PQ_RESET * 1000, 1);
            }

            _started = false;
            _ended = true;

            foreach (uint plr in ActivePlayers)
            {
                Player targPlayer = Player.GetPlayer(plr);
                if (targPlayer != null)
                {
                    targPlayer.SendLocalizeString(Info.Name + " Complete", ChatLogFilters.CHATLOGFILTERS_SAY, GameData.Localized_text.CHAT_TAG_MONSTER_EMOTE);
                    //SendReinitTime(Plr, TIME_PQ_RESET);
                    targPlayer.AddInfluence((ushort)Info.ChapterId, 500);
                }
            }
            //sanity check - remove players in the PQ who won without doing anything
            foreach (KeyValuePair<uint, ContributionInfo> kV in Players)
            {
                if (kV.Value.ActiveTimeEnd == 0) // no contribution yet
                    _toRemove.Add(kV);
            }

            if (_toRemove.Count > 0)
            {
                foreach (var kVr in _toRemove)
                {
                    Players.Remove(kVr.Key);
                }
                _toRemove.Clear();
            }

            GoldChest.Create(Region, Info, ref Players, 1);

            // We are playing some sounds when we finish the whole PQ
            if (Info.SoundPQEnd != 0)
            { 
                GameObject_proto Proto = GameObjectService.GetGameObjectProto(2000489);

                GameObject_spawn S = new GameObject_spawn();
                S.Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID();
                S.BuildFromProto(Proto);
                S.WorldX = Info.GoldChestWorldX;
                S.WorldY = Info.GoldChestWorldY;
                S.WorldZ = Info.GoldChestWorldZ;
                S.WorldO = 0;
                S.ZoneId = Info.ZoneId;

                GameObject GOSoundPlayer = Region.CreateGameObject(S);

                // PQ Sound player - this will play sounds on PQ Stage, need to be setup in DB
                var prms = new List<object>() { GOSoundPlayer, (ushort)Info.SoundPQEnd };

                GOSoundPlayer.EvtInterface.AddEvent(PlayFinalPQSound, 1 * 1000, 1, prms);

                //GOSoundPlayer.EvtInterface.AddEvent(Destroy, 5 * 1000, 1);
            }
        }

        // This plays final sound in PQ
        public void PlayFinalPQSound(object go)
        {
            var Params = (List<object>)go;
            GameObject GO = Params[0] as GameObject;

            if (go != null && GO != null)
            {
                GO.PlaySound((ushort)Params[1]);
                GO.Destroy();
            }
        }

        public void Reset()
        {
            Players.Clear();
            Players = new Dictionary<uint, ContributionInfo>();
            _playerHealing = new Dictionary<ushort, Dictionary<uint, uint>>();
            

            _started = false;
            _ended = false;
            EvtInterface.RemoveEvent(Failed);

            Start();
        }

        public void Failed()
        {
            EvtInterface.RemoveEvent(Failed);
            EvtInterface.AddEvent(Reset, TIME_PQ_RESET * 1000, 1);
            _started = false;
            _ended = true;
            Stage.Cleanup();

            foreach (uint Plr in ActivePlayers)
            {
                Player targPlayer = Player.GetPlayer(Plr);
                if (targPlayer != null)
                {
                    targPlayer.SendLocalizeString(Info.Name + " Failed", ChatLogFilters.CHATLOGFILTERS_SAY, GameData.Localized_text.CHAT_TAG_MONSTER_EMOTE);
                    SendCurrentStage(targPlayer);
                    // SendReinitTime(Plr, TIME_PQ_RESET);
                }
            }
        }

#endregion

        public static uint GetBag(int bagWon)
        {
            switch (bagWon)
            {
                case 1:
                    return 9940;
                case 2:
                    return 9941;
                case 3:
                    return 9942;
                case 4:
                    return 9943;
                case 5:
                    return 9980;
            }
            return 0;
        }

        public void ProcessOptOut(Player player, byte optOutType)
        {
            if (!Players.ContainsKey(player.CharacterId))
            {
                Players.Add(player.CharacterId, new ContributionInfo(player));
            }

            ContributionInfo contrib = Players[player.CharacterId];

            if (contrib.OptOutType == optOutType)
                return;

            contrib.OptOutType = optOutType;

            if (optOutType == 0)
                player.SendLocalizeString(Info.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_PUBLIC_QUEST_OPT_OUT_DISABLE);
            else
                player.SendLocalizeString(Info.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_PUBLIC_QUEST_OPT_OUT_ENABLE);

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE);
            Out.WriteUInt32(Info.Entry);
            Out.WriteByte(8);
            Out.WriteByte(optOutType);
            Out.WriteUInt16(0);
            player.SendPacket(Out);
        }

#region ContributionManagerInstance

        /// <summary>
        /// Contains information about the players who healed damage dealt by mobs in this PQ. Indices: mob OID, CharacterID, player healing
        /// </summary>
        Dictionary<ushort, Dictionary<uint, uint>> _playerHealing = new Dictionary<ushort, Dictionary<uint, uint>>();

        /// <summary>
        /// When a player receives a hit from a Public Quest mob, tracks the damage dealt.
        /// </summary>
        public void AddTrackedDamage(Player target, PQuestCreature mob, uint damage)
        {
            if (!Players.ContainsKey(target.CharacterId))
            {
                Players.Add(target.CharacterId, new ContributionInfo(target));
            }

            if (!Players[target.CharacterId].DamageTakenFrom.ContainsKey(mob.Oid))
                Players[target.CharacterId].DamageTakenFrom.Add(mob.Oid, new ContributionInfo.HitInfo(damage));
            else
            {
                if (mob.Rank > 0 || Players[target.CharacterId].DamageTakenFrom[mob.Oid].NumHits < 10)
                    Players[target.CharacterId].DamageTakenFrom[mob.Oid].DamageTaken += damage;
                Players[target.CharacterId].DamageTakenFrom[mob.Oid].NumHits++;
            }

        }

        /// <summary>
        /// When a player is healed, grants delayed contribution to the healing player if there is damage from a PQ mob yet to be healed through.
        /// </summary>
        /// <param name="target">The healed player.</param>
        /// <param name="healer">The healing player.</param>
        /// <param name="healCount">The amount healed.</param>
        public void NotifyPlayerHealed(Player target, Player healer, uint healCount)
        {
            if (!Players.ContainsKey(target.CharacterId))
            {
                Players.Add(target.CharacterId, new ContributionInfo(target));
            }

            if (!Players.ContainsKey(healer.CharacterId))
            {
                Players.Add(healer.CharacterId, new ContributionInfo(healer));
            }

            ContributionInfo targetInfo = Players[target.CharacterId];

            if (targetInfo.HealingDamagePool > 0)
            {
                if (healCount > targetInfo.HealingDamagePool)
                {
                    healCount -= targetInfo.HealingDamagePool;
                    targetInfo.HealingDamagePool = 0;
                    SplitContribution(healer, (ushort)targetInfo.HealingContribPool);
                    //healer.SendClientMessage("Received " + targetInfo.HealingContribPool + " contribution from emptying the target's healing contribution pool.", ChatLogFilters.CHATLOGFILTERS_QUEST, true);
                    targetInfo.HealingContribPool = 0;
                }

                else
                {
                    float contribFactor = (float)healCount / targetInfo.HealingDamagePool;
                    SplitContribution(healer, (ushort)(targetInfo.HealingContribPool * contribFactor));
                    //healer.SendClientMessage("Received " + (uint)(targetInfo.HealingContribPool * contribFactor) + " contribution from the target's healing contribution pool.", ChatLogFilters.CHATLOGFILTERS_QUEST, true);
                    targetInfo.HealingContribPool = (uint)(targetInfo.HealingContribPool * (1f - contribFactor));
                    return;
                }
            }

            foreach (ushort objectId in targetInfo.DamageTakenFrom.Keys)
            {
                ContributionInfo.HitInfo hitInfo = targetInfo.DamageTakenFrom[objectId];

                if (hitInfo.DamageTaken == 0)
                    continue;

                uint deltaHeal;

                if (healCount > hitInfo.DamageTaken)
                {
                    deltaHeal = hitInfo.DamageTaken;
                    hitInfo.DamageTaken = 0;
                    healCount -= deltaHeal;
                }

                else
                {
                    deltaHeal = healCount;
                    hitInfo.DamageTaken -= healCount;
                }

                AddTrackedHeal(objectId, healer.CharacterId, deltaHeal);

                if (healCount == 0)
                    break;
            }
        }

        /// <summary>
        /// Dishes out contribution earned from this mob.
        /// </summary>
        /// <param name="mob"></param>
        public void NotifyKilled(PQuestCreature mob)
        {
            float totalDamageDealt = 1.0f;
            float totalHits = 0;
            int damagedCount = 0;

            // Handle contribution to the PQ.
            int rankMod;

            switch (mob.Rank)
            {
                case 1:
                    rankMod = 4;
                    break;
                case 2:
                    rankMod = 20; break;
                default:
                    rankMod = 1; break;
            }

#region For having taken damage
            foreach (ContributionInfo plrInfo in Players.Values)
            {
                if (plrInfo.DamageTakenFrom.ContainsKey(mob.Oid))
                {
                    damagedCount++;
                    totalDamageDealt += plrInfo.DamageTakenFrom[mob.Oid].DamageTaken;
                    totalHits += plrInfo.DamageTakenFrom[mob.Oid].NumHits;
                }
            }

            foreach (ContributionInfo tankerInfo in Players.Values)
            {
                if (tankerInfo.DamageTakenFrom.ContainsKey(mob.Oid))
                {
                    Player targPlayer = Player.GetPlayer(tankerInfo.PlayerCharId);
                    float scaler = tankerInfo.DamageTakenFrom[mob.Oid].NumHits / totalHits;
                    if (targPlayer != null && targPlayer.CrrInterface.GetArchetype() != EArchetype.ARCHETYPE_Tank && totalHits < 6)
                        scaler *= totalHits / 6;

                    if (targPlayer != null)
                    {
                        SplitContribution(targPlayer, (ushort)(100 * rankMod * scaler));
                        //targPlayer.SendClientMessage("Received " + (ushort)(100 * rankMod * scaler) + " contribution from tanking damage.", ChatLogFilters.CHATLOGFILTERS_QUEST, true);
                    }

                    tankerInfo.ActiveTimeEnd = TCPManager.GetTimeStamp() + 5;
                }
            }
#endregion

#region For healers of damage

            float totalHealing = 0;

            if (_playerHealing.ContainsKey(mob.Oid))
            {
                Dictionary<uint, uint> mobHealing = _playerHealing[mob.Oid];

                foreach (uint healing in mobHealing.Values)
                    totalHealing += healing;

                foreach (uint Plr in ActivePlayers)
                {
                    Player targPlayer = Player.GetPlayer(Plr);
                    if (targPlayer != null)
                    {
                        if (mobHealing.ContainsKey(targPlayer.CharacterId))
                        {
                            if (!Players.ContainsKey(targPlayer.CharacterId))
                            {
                                Players.Add(targPlayer.CharacterId, new ContributionInfo(targPlayer));
                            }

                            if (Players.ContainsKey(targPlayer.CharacterId))
                            {
                                SplitContribution(targPlayer, (ushort)(mobHealing[targPlayer.CharacterId] / totalHealing * rankMod * 100));
                                //targPlayer.SendClientMessage("Received " + (ushort)(mobHealing[targPlayer.CharacterId] / totalHealing * rankMod * 100) + " contribution from healing other players.", ChatLogFilters.CHATLOGFILTERS_QUEST, true);
                                Players[targPlayer.CharacterId].ActiveTimeEnd = TCPManager.GetTimeStamp() + 5;
                            }
                        }
                    }
                }
            }

#endregion

            float leftoverFactor = 1f - totalHealing / totalDamageDealt;

            if (leftoverFactor > 0)
            {
                foreach (ContributionInfo plrInfo in Players.Values)
                {
                    if (plrInfo.DamageTakenFrom.ContainsKey(mob.Oid))
                    {
                        plrInfo.HealingDamagePool += (uint)(totalDamageDealt * (leftoverFactor / damagedCount));
                        plrInfo.HealingContribPool += (uint)(100 * rankMod * (leftoverFactor / damagedCount));
                        plrInfo.DamageTakenFrom.Remove(mob.Oid);
                    }
                }
            }

            _playerHealing.Remove(mob.Oid);
        }

        private void AddTrackedHeal(ushort objectId, uint healCharId, uint healCount)
        {
            if (!_playerHealing.ContainsKey(objectId))
                _playerHealing.Add(objectId, new Dictionary<uint, uint>());

            if (!_playerHealing[objectId].ContainsKey(healCharId))
                _playerHealing[objectId].Add(healCharId, healCount);
            else _playerHealing[objectId][healCharId] += healCount;
        }

        public bool MobLeavingCombat(Object mob, object args)
        {
            if (((Unit)mob).IsDead)
                return false;

            _playerHealing.Remove(mob.Oid);

            foreach (ContributionInfo playerInfo in Players.Values)
            {
                if (playerInfo.DamageTakenFrom.ContainsKey(mob.Oid))
                {
                    playerInfo.DamageTakenFrom.Remove(mob.Oid);
                }
            }

            return false;
        }

        private readonly List<Player> _inGroup = new List<Player>(5);
        private readonly Vector2 _balance = new Vector2();

        public void SplitContribution(Player earnedBy, uint contrib)
        {
            if (earnedBy.WorldGroup == null)
            {
                if (Players.ContainsKey(earnedBy.CharacterId))
                {
                    Players[earnedBy.CharacterId].BaseContribution += contrib;
                }
            }

            else
            {

                earnedBy.WorldGroup.GetPlayerList(_inGroup);

                for (int i = 0; i < _inGroup.Count; ++i)
                {
                    if (!Players.ContainsKey(_inGroup[i].CharacterId))
                    {
                        _inGroup.RemoveAt(i);
                        --i;
                    }

                    else
                        ScenarioMgr.AddToBalanceVector(_balance, _inGroup[i]);
                }

                _balance.Multiply(1f / _inGroup.Count);

                // Divide the contribution by the number of members, 
                // and add a scaling bonus depending on how
                // balanced the current group is, up to 4x.
                float balanceBonusMult = 1f - _balance.Magnitude;
                contrib = (uint)(contrib * (1f / _inGroup.Count) * (1 + balanceBonusMult));

                foreach (Player player in _inGroup)
                {
                    if (player != null && Players.ContainsKey(player.CharacterId))
                    {
                        Players[player.CharacterId].BaseContribution += Math.Max(contrib, 1);
                    }
                }

                _inGroup.Clear();
                _balance.X = 0;
                _balance.Y = 0;
            }
        }
#if DEBUG
        private const int CONTRIB_ELAPSE_INTERVAL = 60 * 1; // 1 mins of no contribution forfeits.
#else
        private const int CONTRIB_ELAPSE_INTERVAL = 60 * 15; // 15 mins of no contribution forfeits.
#endif
        private readonly List<KeyValuePair<uint, ContributionInfo>> _toRemove = new List<KeyValuePair<uint, ContributionInfo>>(8);
        private void TickContribution(long curTimeSeconds)
        {
            foreach (KeyValuePair<uint, ContributionInfo> kV in Players)
            {
                if (kV.Value.ActiveTimeEnd == 0) // no contribution yet
                    continue;
              if (curTimeSeconds - kV.Value.ActiveTimeEnd > CONTRIB_ELAPSE_INTERVAL && !ActivePlayers.Contains(kV.Key)) //make sure they aren't in the pq area before removing them
                    _toRemove.Add(kV);
            }

            if (_toRemove.Count > 0)
            {
            
                foreach (var kVr in _toRemove)
                {
                    Players.Remove(kVr.Key);
                }
                _toRemove.Clear();
            }
        }

        #endregion
    }
}
