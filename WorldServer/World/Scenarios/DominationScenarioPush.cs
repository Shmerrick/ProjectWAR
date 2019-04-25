using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios
{
    public class DominationScenarioPush : Scenario
    {
        public List<ProximityFlag> Flags = new List<ProximityFlag>();
        public Dictionary<int, List<ProximityFlag>> FlagLevel = new Dictionary<int, List<ProximityFlag>>();
        private int _orderLevel = 0;
        private int _destroLevel = 2;

        public DominationScenarioPush(Scenario_Info info, int tier)
            : base(info, tier)
        {
            //flags.Add(new Flag(RKF_1, "The Landing", 360109, 428854, 6433, 1024));

            FlagLevel[0] = new List<ProximityFlag>();
            FlagLevel[1] = new List<ProximityFlag>();
            FlagLevel[2] = new List<ProximityFlag>();

            foreach (Scenario_Object scenarioObject in info.ScenObjects)
            {
                Log.Info("DominationScenarioPush", "Adding flag " + scenarioObject.ObjectiveName + " realm:" + scenarioObject.Realm);
                if (scenarioObject.Type == "Flag")
                {
                    ProximityFlag proximityFlag = new ProximityFlag(scenarioObject.Identifier, scenarioObject.ObjectiveName,
                        scenarioObject.WorldPosX, scenarioObject.WorldPosY, scenarioObject.PosZ, scenarioObject.Heading,
                        scenarioObject.PointGain, scenarioObject.PointOverTimeGain, scenarioObject);
                    Flags.Add(proximityFlag);
                    Region.AddObject(proximityFlag, info.MapId);

                    if(scenarioObject.Realm == 0)

                        FlagLevel[1].Add(proximityFlag);
                    else if (scenarioObject.Realm == 1)
                        FlagLevel[0].Add(proximityFlag);
                    else if (scenarioObject.Realm == 2)
                        FlagLevel[2].Add(proximityFlag);

                    
                }

                else
                    LoadScenarioObject(scenarioObject);
            }
        }

        public override void OnStart()
        {
            //PlaySoundToAll(SOUND_BATTLEGROUND_BEGIN);
            EvtInterface.AddEvent(GeneratePoints, 6000, 0);

            foreach (var orderFlag in FlagLevel[0])
                BroadcastFlagUnlock(orderFlag, Realms.REALMS_REALM_ORDER);

            foreach (var destroFlag in FlagLevel[2])
                BroadcastFlagUnlock(destroFlag, Realms.REALMS_REALM_DESTRUCTION);
        }

        private void BroadcastFlagUnlock(ProximityFlag flag, Realms? realm = null)
        {
            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    if (!realm.HasValue || plr.Realm == realm.Value)
                        plr.SendLocalizeString(flag.ObjectiveName + " is open for capture!", ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                }
            }
        }

        private bool IsLevelCaptured(int level, Realms realm)
        {
            if(!FlagLevel.ContainsKey(level))
                return false;

            foreach (var flag in FlagLevel[level])
                if(flag.OwningRealm != (int)realm)
                    return false;

            return true;
                
        }

        private int GetFlagLevel(ProximityFlag flag)
        {
            if(FlagLevel[0].Contains(flag))
                return 0;
            else if (FlagLevel[1].Contains(flag))
                return 1;
            else if (FlagLevel[2].Contains(flag))
                return 2;

            return -1;
        }

        protected override void UpdateScenario()
        {
            // Update flag 
            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    if (plr == null || plr.IsDisposed || !plr.IsInWorld())
                        continue;

                    Point2D playerLocation2D = new Point2D(plr._Value.WorldX, plr._Value.WorldY);

                    foreach (ProximityFlag flag in Flags)
                    {
                        Point3D flagLocation = flag.WorldPosition;
                        if (!plr.IsDead && playerLocation2D.GetDistance(flagLocation) < 20 && Math.Abs(plr.Z - flag.WorldPosition.Z) < 30)
                        {
                            var level = GetFlagLevel(flag);
                            //check if player is allowed to capture this flag (must capture flags closer to base first)
                            if ( plr.Realm == Realms.REALMS_REALM_ORDER && level > _orderLevel && flag.OwningRealm == 0)
                                continue;
                            if(plr.Realm == Realms.REALMS_REALM_DESTRUCTION && level < _destroLevel && flag.OwningRealm == 0)
                                continue;
                            

                   
                            if (!flag.playersInRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Contains(plr))
                            {
                                flag.playersInRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Add(plr);
                                flag.SendFlagInfo(plr);
                            }
                        }
                        else if (flag.playersInRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Contains(plr))
                        {
                            //cannot capture center flag until base is captured

                            flag.playersInRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Remove(plr);
                            flag.SendFlagLeft(plr);
                        }
                    }
                }
            }

            int currentOrderLevel = _orderLevel;
            int currentDestroLevel = _destroLevel;

            // Update capture progress of flags
            foreach (ProximityFlag flag in Flags)
            {
                var prevOwner = flag.OwningRealm;
                var flagLevel = GetFlagLevel(flag);
                if (flag.UpdateDominationProgress())
                {
                    if (flag.OwningRealm == 0)
                    {
                        //flag has been unlocked, move owner level to level of unlocked flag unlock all flags on levels after it
                        if (prevOwner == 2) //destro's flag was hijacked, reset all their gains up to that flag
                        {
                            for (int i = 0; i <= flagLevel; i++)
                            {
                                foreach (var f in FlagLevel[i])
                                {
                                    f.OwningRealm = 0;
                                    f.DeltaOwnership = 0;
                                }
                            }
                            _destroLevel = flagLevel;
                        }
                        else if (prevOwner == 1)
                        {
                            for (int i = 2; i >= flagLevel; i--)
                            {
                                foreach (var f in FlagLevel[i])
                                {
                                    f.OwningRealm = 0;
                                    f.DeltaOwnership = 0;
                                }
                            }
                            _orderLevel = flagLevel;
                        }
                        flag.playersInRange[0].Clear();
                        flag.playersInRange[1].Clear();
                    }

                    else
                    {

                        //update levels

                        if (IsLevelCaptured(0, Realms.REALMS_REALM_ORDER))
                            _orderLevel = 1;
                        if (IsLevelCaptured(1, Realms.REALMS_REALM_ORDER))
                            _orderLevel = 2;

                        if (IsLevelCaptured(2, Realms.REALMS_REALM_DESTRUCTION))
                            _destroLevel = 1;
                        if (IsLevelCaptured(1, Realms.REALMS_REALM_DESTRUCTION))
                            _destroLevel = 0;

                        string packetString =
                            $"{flag.ObjectiveName} is now {(flag.OwningRealm == 1 ? "Order" : "Destruction")} controlled!";

                        for (int i = 0; i < 2; i++)
                        {
                            foreach (Player plr in Players[i])
                            {
                                SendObjectiveStates(plr);
                                plr.SendLocalizeString(packetString, ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                            }
                        }

                    }

                    for (int i = 0; i < 2; i++)
                    {
                        foreach (Player plr in Players[i])
                        {
                            SendObjectiveStates(plr);

                            if (_orderLevel != currentOrderLevel && plr.Realm == Realms.REALMS_REALM_ORDER)
                            {
                                foreach(var f in FlagLevel[_orderLevel])
                                    plr.SendLocalizeString(f.ObjectiveName + " is open for capture!", ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                            }

                            if (_destroLevel != currentDestroLevel && plr.Realm == Realms.REALMS_REALM_DESTRUCTION)
                            {
                                foreach (var f in FlagLevel[_destroLevel])
                                    plr.SendLocalizeString(f.ObjectiveName + " is open for capture!", ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                            }
                        }
                    }


                }
            }

            base.UpdateScenario();
        }

        public override void SendObjectiveStates(Player plr)
        {
            foreach (ProximityFlag flag in Flags)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE, 16);
                Out.WriteUInt32((uint)flag.ObjectiveID);
                Out.Fill(0xFF, 6);
                Out.WriteUInt16(0);
                Out.WriteByte((byte)flag.OwningRealm);
                Out.Fill(0, 3);
                plr.SendPacket(Out);
            }
        }

        public virtual void GeneratePoints()
        {
            byte[] points = new byte[2];
            byte[] ownedFlags = new byte[2];

            foreach (ProximityFlag flag in Flags)
            {
                //no points for neutral flag or flag near your own base
                if (flag.OwningRealm == 0 || flag.OwningRealm == flag.ScenarioObject.Realm)
                    continue;

                ++ownedFlags[flag.OwningRealm - 1];
                points[flag.OwningRealm - 1] += flag.TickPoints;
            }

            if (ownedFlags[0] == Flags.Count)
                points[0] *= 3;
            else if (ownedFlags[1] == Flags.Count)
                points[1] *= 3;

            GivePoints(1, points[0]);
            GivePoints(2, points[1]);
        }

        public override void OnClose()
        {
            EvtInterface.RemoveEvent(UpdateScenario);
            EvtInterface.RemoveEvent(GeneratePoints);
        }


    }
}
