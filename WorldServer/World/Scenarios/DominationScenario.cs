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
    public class DominationScenario : Scenario
    {
        public List<ProximityFlag> Flags = new List<ProximityFlag>();

        public DominationScenario(Scenario_Info info, int tier)
            : base(info, tier)
        {
            //flags.Add(new Flag(RKF_1, "The Landing", 360109, 428854, 6433, 1024));

            foreach (Scenario_Object scenarioObject in info.ScenObjects)
            {
                if (scenarioObject.Type == "Flag")
                {
                    ProximityFlag proximityFlag = new ProximityFlag(scenarioObject.Identifier, scenarioObject.ObjectiveName,
                        scenarioObject.WorldPosX, scenarioObject.WorldPosY, scenarioObject.PosZ, scenarioObject.Heading,
                        scenarioObject.PointGain, scenarioObject.PointOverTimeGain);
                    Flags.Add(proximityFlag);
                    Region.AddObject(proximityFlag, info.MapId);
                }

                else
                    LoadScenarioObject(scenarioObject);
            }
        }

        public override void OnStart()
        {
            //PlaySoundToAll(SOUND_BATTLEGROUND_BEGIN);
            EvtInterface.AddEvent(GeneratePoints, 6000, 0);
        }

        protected override void UpdateScenario()
        {
            // Update flag encroachment
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
                            if (!flag.playersInRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Contains(plr))
                            {
                                flag.playersInRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Add(plr);
                                flag.SendFlagInfo(plr);
                            }
                        }
                        else if (flag.playersInRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Contains(plr))
                        {
                            flag.playersInRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Remove(plr);
                            flag.SendFlagLeft(plr);
                        }
                    }
                }
            }

            // Update capture progress of flags
            foreach (ProximityFlag flag in Flags)
            {
                if (flag.UpdateDominationProgress())
                {
                    FlagOwnershipChanged((byte)flag.OwningRealm);

                    if (flag.OwningRealm == 0)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            foreach (Player plr in Players[i])
                                SendObjectiveStates(plr);
                        }
                    }

                    else
                    {
                        GivePoints(flag.OwningRealm, flag.CapturePoints);

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
                if (flag.OwningRealm == 0)
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

        private void FlagOwnershipChanged(byte newOwnership)
        {
            if (MapTrackedObjects.Count > 0)
            {
                foreach (HoldObject obj in MapTrackedObjects)
                {
                    if (obj.name == "Isha's Will")
                    {
                        if (newOwnership == 0)
                            obj.ResetTo(EHeldState.Inactive);
                        else
                        {
                            obj.SetActive(newOwnership == 2 ? Realms.REALMS_REALM_ORDER : Realms.REALMS_REALM_DESTRUCTION);
                            for (int i = 0; i < 2; ++i)
                                foreach (Player plr in Players[i])
                                    plr.SendLocalizeString((newOwnership == 1 ? "Destruction" : "Order") + "'s warball is in play!", ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                        }
                    }
                }

            }

        }
    }
}   