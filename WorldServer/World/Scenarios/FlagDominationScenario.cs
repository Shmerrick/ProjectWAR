using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios
{
    public class FlagDominationScenario : Scenario
    {
        private readonly List<CapturePoint> _capturePoints = new List<CapturePoint>();
        private readonly HoldObject _flag;

        public FlagDominationScenario(Scenario_Info info, int tier)
            : base(info, tier)
        {
            foreach (Scenario_Object obj in info.ScenObjects)
            {
                if (obj.Type == "Capture Point")
                {
                    CapturePoint cPoint = new CapturePoint(obj, CaptureValid, OnCapture);
                    _capturePoints.Add(cPoint);
                    Region.AddObject(cPoint, info.MapId);
                }

                else if (obj.Type == "Flag")
                {
                    _flag = new HoldObject(obj.Identifier, obj.ObjectiveName, new Point3D(obj.WorldPosX, obj.WorldPosY, obj.PosZ), 60001, 13000, FlagPickedUp, ObjectDropped, ObjectReset, null, 299, 299);
                    Region.AddObject(_flag, info.MapId);
                    AddTrackedObject(_flag);
                }

                else
                    LoadScenarioObject(obj);
            }

            ZoneMgr curZone = Region.GetZoneMgr(Info.MapId);

            Region.LoadCells((ushort)(curZone.Info.OffX + 8), (ushort)(curZone.Info.OffY + 8), 8);
        }

        #region Troll Pacifier

        public override void OnStart()
        {
            EvtInterface.AddEvent(ActivateFlag, 30000, 1);

            foreach (Object obj in Region.Objects)
            {
                Creature crea = obj as Creature;

                if (crea == null)
                    continue;

                if (crea.Name == "Stone Troll")
                    crea.IsInvulnerable = true;
            }
        }

        public void ActivateFlag()
        {
            _flag.SetActive(0);

            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                    SendFlagObjectState(plr, _flag);
        }

        public void FlagPickedUp(HoldObject flag, Player pickedBy)
        {
            ObjectPickedUp(flag, pickedBy);

            if (!EvtInterface.HasEvent(TimerUp))
                SetTimerState(true);
        }

        #endregion

        #region Troll Stones

        public bool CaptureValid(Player player)
        {
            return player == _flag.Holder;
        }

        public void OnCapture(CapturePoint captured)
        {
            Realms targetRealm = captured.OwningRealm;

            if (targetRealm == Realms.REALMS_REALM_NEUTRAL)
                return;

            GivePoints((int)captured.OwningRealm, 35);

            for (int i = 0; i < _capturePoints.Count; ++i)
            {
                if (_capturePoints[i].OwningRealm != targetRealm)
                {
                    SetTimerState(true);
                    return;
                }
            }

            Lockdown();
        }

        private void Lockdown()
        {
            GivePoints((int)_capturePoints[0].OwningRealm, 65);

            foreach (CapturePoint c in _capturePoints)
            {
                c.Locked = true;
                c.CountdownTimerEnd = TCPManager.GetTimeStamp() + 8;
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_SOUND);
            Out.WriteByte(0);
            Out.WriteUInt16(_capturePoints[0].OwningRealm == Realms.REALMS_REALM_ORDER ? (ushort)0x0C : (ushort)0x332);
            Out.Fill(0, 10);

            foreach (Object obj in Region.Objects)
            {
                Player plr = obj as Player;
                if (plr == null)
                    continue;

                plr.SendLocalizeString((_capturePoints[0].OwningRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " has pacified all of the Trolls!", ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                plr.SendPacket(Out);
            }

            foreach (CapturePoint c in _capturePoints)
                c.BroadcastObjectiveInfo();

            _flag.ResetTo(EHeldState.Home);

            SetTimerState(false);

            EvtInterface.AddEvent(ReopenObjectives, 8000, 1);
        }

        private void ReopenObjectives()
        {
            foreach (CapturePoint c in _capturePoints)
                c.Reset();
        }

        public void TimerUp()
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                {
                    plr.SendClientMessage("LastUpdatedTime up! All Control Points reset.", ChatLogFilters.CHATLOGFILTERS_C_WHITE);
                    if (_flag.Holder?.Realm == plr.Realm)
                        plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_C_RED_LARGE, Localized_text.TEXT_PQ_FAILED);
                }

            _flag.ResetTo(EHeldState.Home);

            foreach (CapturePoint c in _capturePoints)
                c.Reset();

            // Disable the timer on UI
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE);
            Out.WriteUInt32(0x1BA0);
            Out.WriteHexStringBytes("0300000016560100000000");

            for (int i = 0; i < 2; ++i)
            {
                foreach (Player plr in Players[i])
                    plr.SendPacket(Out);
            }
        }

        #endregion

        #region Progression

        protected override void UpdateScenario()
        {
            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    if (plr == null)
                        continue;

                    foreach (CapturePoint cPoint in _capturePoints)
                    {
                        Point3D flagLocation = cPoint.WorldPosition;
                        if (!plr.IsDead && !plr.IsDisposed && plr.IsInWorld() && plr.PointWithinRadiusFeet(flagLocation, 50))
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

        #endregion

        #region Senders

        private void SetTimerState(bool active)
        {
            PacketOut Out;

            EvtInterface.RemoveEvent(TimerUp);

            if (!active)
            {
                Out = new PacketOut((byte) Opcodes.F_OBJECTIVE_UPDATE);
                Out.WriteUInt32(0x1BA0);
                Out.WriteHexStringBytes("0300000016560100000000");
            }

            else
            {
                Out = new PacketOut((byte) Opcodes.F_OBJECTIVE_INFO);
                Out.WriteUInt32(0x1BA0);
                Out.WriteHexStringBytes("000001000000020000167C00");

                Out.WriteByte((byte)_capturePoints.Count);

                foreach (CapturePoint c in _capturePoints)
                {
                    Out.WriteHexStringBytes("000001000000");
                    Out.WritePascalString(c.ObjectiveName);
                }

                Out.WriteHexStringBytes("FF00");
                Out.WritePascalString("Pacify all of the Trolls!");

                Out.WriteByte(0);
                Out.WritePascalString("These are the areas that Destruction controls. They can only be claimed while holding the Troll Pacifier located at the top of Stone Troll Hill.");

                // Timer
                Out.WriteUInt32(60);
                // Timer
                Out.WriteUInt32(60);
                Out.WriteUInt32(0);
                Out.WriteByte(0);
                Out.WriteUInt32(0);

                EvtInterface.AddEvent(TimerUp, 60000, 1);
            }

            for (int i = 0; i < 2; ++i)
            {
                foreach (Player plr in Players[i])
                    plr.SendPacket(Out);
            }

        }

        public override void SendObjectiveStates(Player plr)
        {
            foreach (CapturePoint cPoint in _capturePoints)
                cPoint.SendObjectiveState(plr);
        }

        #endregion
    }
}