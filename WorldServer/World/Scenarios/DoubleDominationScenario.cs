using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios
{
    public class DoubleDominationScenario : Scenario
    {
        private readonly List<CapturePoint> _capturePoints = new List<CapturePoint>();

        public DoubleDominationScenario(Scenario_Info info, int tier)
            : base(info, tier)
        {
            //flags.Add(new Flag(RKF_1, "The Landing", 360109, 428854, 6433, 1024));

            foreach (Scenario_Object scenarioObject in info.ScenObjects)
            {
                if (scenarioObject.Type == "Capture Point")
                {
                    CapturePoint cPoint = new CapturePoint(scenarioObject, null, OnCapture);
                    _capturePoints.Add(cPoint);
                    Region.AddObject(cPoint, info.MapId);
                }

                else
                    LoadScenarioObject(scenarioObject);
            }
        }

        protected override void UpdateScenario()
        {
            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    if (plr == null || plr.IsDisposed || !plr.IsInWorld())
                        continue;

                    foreach (CapturePoint cPoint in _capturePoints)
                    {
                        Point3D flagLocation = cPoint.WorldPosition;
                        if (!plr.IsDead && plr.Get2DDistanceToWorldPoint(flagLocation) < 50)
                        {
                            if (!cPoint.PlayersInCloseRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Contains(plr))
                                cPoint.AddInCloseRange(plr);
                        }
                        else if (cPoint.PlayersInCloseRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Contains(plr))
                            cPoint.RemoveInCloseRange(plr);
                    }
                }
            }

            base.UpdateScenario();
        }

        private bool _pendingLockdown;

        public void OnCapture(CapturePoint captured)
        {
            if (_capturePoints[0].OwningRealm == _capturePoints[1].OwningRealm && _capturePoints[0].OwningRealm != Realms.REALMS_REALM_NEUTRAL)
            {
                foreach (Object obj in Region.Objects)
                {
                    Player plr = obj as Player;
                    plr?.SendLocalizeString((_capturePoints[0].OwningRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " will lock down both control points in 15 seconds!", ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                }

                _capturePoints[0].CountdownTimerEnd = TCPManager.GetTimeStamp() + 15;
                _capturePoints[1].CountdownTimerEnd = TCPManager.GetTimeStamp() + 15;

                EvtInterface.AddEvent(Lockdown, 15000, 1);
                _pendingLockdown = true;
            }

            else
            {
                if (!_pendingLockdown)
                    return;
                EvtInterface.RemoveEvent(Lockdown);

                _capturePoints[0].CountdownTimerEnd = 0;
                _capturePoints[1].CountdownTimerEnd = 0;

                if (captured == _capturePoints[0])
                    _capturePoints[1].BroadcastObjectiveInfo();
                else
                    _capturePoints[0].BroadcastObjectiveInfo();

                _pendingLockdown = false;
            }

        }

        public void Lockdown()
        {
            GivePoints((byte) _capturePoints[0].OwningRealm, 80);

            _capturePoints[0].Locked = true;
            _capturePoints[1].Locked = true;

            _capturePoints[0].CountdownTimerEnd = TCPManager.GetTimeStamp() + 30;
            _capturePoints[1].CountdownTimerEnd = TCPManager.GetTimeStamp() + 30;

            _pendingLockdown = false;

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_SOUND);
            Out.WriteByte(0);
            Out.WriteUInt16(_capturePoints[0].OwningRealm == Realms.REALMS_REALM_ORDER ? (ushort)0x0C : (ushort)0x332);
            Out.Fill(0, 10);

            foreach (Object obj in Region.Objects)
            {
                Player plr = obj as Player;
                if (plr != null)
                {
                    plr.SendLocalizeString((_capturePoints[0].OwningRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " has locked down " + _capturePoints[0].ObjectiveName + " and " + _capturePoints[1].ObjectiveName+"!", ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                    plr.SendPacket(Out);

                    // 25% damage buff for 30 seconds
                    if (plr.Realm == _capturePoints[0].OwningRealm)
                        plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.Level, AbilityMgr.GetBuffInfo(14052)));
                }
            }

            _capturePoints[0].BroadcastObjectiveInfo();
            _capturePoints[1].BroadcastObjectiveInfo();

            EvtInterface.AddEvent(ReopenObjectives, 30000, 1);
        }

        public void ReopenObjectives()
        {
            _capturePoints[0].Reset();
            _capturePoints[1].Reset();

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_SOUND);
            Out.WriteByte(0);
            Out.WriteUInt16(0x229);
            Out.Fill(0, 10);

            foreach (Object obj in Region.Objects)
            {
                Player plr = obj as Player;
                if (plr != null)
                {
                    plr.SendLocalizeString(_capturePoints[0].ObjectiveName + " and " + _capturePoints[1].ObjectiveName + " are now open for capture!", ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                    plr.SendPacket(Out);
                }
            }
        }

        public override void SendObjectiveStates(Player plr)
        {
            foreach (CapturePoint cPoint in _capturePoints)
                cPoint.SendObjectiveState(plr);
        }
    }
}