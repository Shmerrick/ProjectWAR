using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios.Objects
{
    public class CapturePoint : Object
    {
        public uint ObjectiveID;
        public string ObjectiveName;

        private GameObject _glowObject;
        private readonly GameObject_proto _proto;

        public List<Player>[] PlayersInCloseRange = { new List<Player>(), new List<Player>(), };

        private int _x, _y, _z, _o;

        public Realms OwningRealm;

        public bool Locked;

        private readonly Func<Player, bool> _captureCheck; 
        private readonly Action<CapturePoint> _onCapture;

        private string _captureText, _captureDesc, _captureAnnouncement;
        private string _holdText, _holdDesc;

        public CapturePoint(uint id, string name, int x, int y, int z, int o, GameObject_proto proto, Func<Player, bool> captureCheck, Action<CapturePoint> onCapture)
        {
            ObjectiveID = id;
            ObjectiveName = name;
            _x = x;
            _y = y;
            _z = z;
            _o = o;
            _proto = proto;
            _captureCheck = captureCheck;
            _onCapture = onCapture;

            CaptureDuration = 3;
        }

        public CapturePoint(Scenario_Object scenarioObject, Func<Player, bool> captureCheck, Action<CapturePoint> onCapture)
        {
            ObjectiveID = scenarioObject.Identifier;
            ObjectiveName = scenarioObject.ObjectiveName;
            _x = scenarioObject.WorldPosX;
            _y = scenarioObject.WorldPosY;
            _z = scenarioObject.PosZ;
            _o = scenarioObject.Heading;
            _proto = GameObjectService.GetGameObjectProto(scenarioObject.ProtoEntry);
            _captureCheck = captureCheck;
            _onCapture = onCapture;

            _captureText = scenarioObject.CaptureObjectiveText.Replace("%n", ObjectiveName);
            _captureDesc = scenarioObject.CaptureObjectiveDescription.Replace("%n", ObjectiveName);
            _holdText = scenarioObject.HoldObjectiveText.Replace("%n", ObjectiveName);
            _holdDesc = scenarioObject.HoldObjectiveDescription.Replace("%n", ObjectiveName);
            _captureAnnouncement = scenarioObject.CaptureAnnouncement;

            CaptureDuration = 3;
        }

        public override void OnLoad()
        {
            Z = _z;
            X = Zone.CalculPin((uint)_x, true);
            Y = Zone.CalculPin((uint)_y, false);
            base.OnLoad();

            Heading = (ushort)_o;
            WorldPosition.X = _x;
            WorldPosition.Y = _y;
            WorldPosition.Z = _z;

            SetOffset((ushort)(_x >> 12), (ushort)(_y >> 12));

            IsActive = true;

            // Spawn objective glow (Big Poofy Glowy Nuet)
            GameObject_proto glowProto = GameObjectService.GetGameObjectProto(99858);
            if (glowProto == null)
                return;

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldO = Heading,
                WorldY = WorldPosition.Y,
                WorldZ = WorldPosition.Z - 256,
                WorldX = WorldPosition.X,
                ZoneId = Zone.ZoneId,
            };
            spawn.BuildFromProto(glowProto);

            _glowObject = new GameObject(spawn);
            Region.AddObject(_glowObject, spawn.ZoneId);
        }

        public override void SendMeTo(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16((ushort)_o);
            Out.WriteUInt16((ushort)_z);
            Out.WriteUInt32((uint)_x);
            Out.WriteUInt32((uint)_y);

            Out.WriteUInt16(_proto.DisplayID);

            Out.WriteUInt16(0x1E00);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0x1E1A);
            Out.WriteByte(0);

            // Flags
            Out.WriteUInt16(0x20 + 0x4);

            Out.WriteByte(0);

            Out.WriteUInt16(0);
            Out.WriteByte(0);
            Out.WriteByte(100);

            Out.WriteUInt16(0x76D5);
            Out.WriteUInt16(0x771F);
            Out.WriteUInt32(0);

            Out.WritePascalString(_proto.Name);
            Out.WriteByte(0);

            plr.SendPacket(Out);

            base.SendMeTo(plr);
        }

        #region Range

        public void AddInCloseRange(Player plr)
        {
            PlayersInCloseRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Add(plr);
            SendObjectiveInfo(plr);
        }

        public void RemoveInCloseRange(Player plr)
        {
            PlayersInCloseRange[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 1 : 0].Remove(plr);
            SendObjectiveLeft(plr);
        }

        #endregion

        #region Interaction

        public override void SendInteract(Player player, InteractMenu menu)
        {
            if (_captureCheck != null && !_captureCheck.Invoke(player))
                return;

            if (Locked)
            {
                player.SendClientMessage("This point is locked down.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }
            if (OwningRealm == player.Realm)
            {
                player.SendClientMessage("Your realm already owns this point.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (player.StealthLevel > 0)
            {
                player.SendClientMessage("You can't interact with objects while in stealth.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (CaptureDuration == 0)
            {
                player.SendClientMessage("Capture duration is 0!");
                return;
            }

            BeginInteraction(player);
        }

        #endregion

        #region Capturing

        public override void NotifyInteractionComplete(NewBuff b)
        {
            if (Locked || CapturingPlayer == null)
            {
                CapturingPlayer = null;
                return;
            }

            OwningRealm = CapturingPlayer.Realm;
            CapturingPlayer = null;

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_SOUND);
            Out.WriteByte(0);
            Out.WriteUInt16(OwningRealm == Realms.REALMS_REALM_ORDER ? (ushort)0x0C : (ushort)0x332);
            Out.Fill(0, 10);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append(OwningRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction");
            sb.Append(" ");
            sb.Append(_captureAnnouncement);
            sb.Append(" ");
            sb.Append(ObjectiveName + "!");

            foreach (Object obj in Region.Objects)
            {
                Player plr = obj as Player;
                if (plr != null)
                {
                    plr.SendLocalizeString(sb.ToString(), ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                    SendObjectiveState(plr);
                }
            }

            _onCapture(this);

            foreach (Player plr in PlayersInRange)
            {
                UpdateGlow(plr);
            }

            foreach (Player plr in PlayersInCloseRange[0])
            {
                SendObjectiveInfo(plr);
                SendRealmBonus(plr);
                plr.SendPacket(Out);
            }

            foreach (Player plr in PlayersInCloseRange[1])
            {
                SendObjectiveInfo(plr);
                SendRealmBonus(plr);
                plr.SendPacket(Out);
            }
        }

        public void BroadcastObjectiveInfo()
        {
            foreach (Player plr in PlayersInCloseRange[0])
                SendObjectiveInfo(plr);

            foreach (Player plr in PlayersInCloseRange[1])
                SendObjectiveInfo(plr);
        }

        public void Reset()
        {
            CountdownTimerEnd = 0;

            OwningRealm = Realms.REALMS_REALM_NEUTRAL;

            foreach (Player plr in PlayersInRange)
                UpdateGlow(plr);

            BroadcastObjectiveInfo();

            foreach (Object obj in Region.Objects)
            {
                Player plr = obj as Player;

                if (plr != null)
                {
                    SendObjectiveState(plr);
                    SendRealmBonus(plr);
                }
            }

            Locked = false;
        }

        #endregion

        #region Senders

        public void SendObjectiveState(Player plr, bool announce = false)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE);
            Out.WriteUInt32(ObjectiveID);
            Out.Fill(0xFF, 6);
            Out.WriteUInt16(0);
            Out.WriteByte((byte)OwningRealm);
            Out.Fill(0, 2);
            Out.WriteByte(0);

            plr.SendPacket(Out);
        }

        public void SendRealmBonus(Player plr)
        {
            PacketOut Out = new PacketOut((byte) Opcodes.F_REALM_BONUS);
            Out.WriteByte((byte)OwningRealm);
            Out.WriteUInt16(0);
            plr.SendPacket(Out);
        }

        public void SendObjectiveInfo(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO, 64);
                Out.WriteUInt32(ObjectiveID);
                Out.WriteByte(0);
                Out.WriteByte((byte)OwningRealm);
                Out.WriteByte(1);
                Out.WriteUInt16(0);
                Out.WritePascalString(ObjectiveName);
                Out.WriteByte(2);
                Out.WriteUInt32(0x0000348F);
                Out.WriteUInt32(0x0000FF00);

                if (plr.Realm == OwningRealm)
                    Out.WritePascalString(_holdText);
                else
                    Out.WritePascalString(_captureText);

                Out.WriteByte(0);

                if (plr.Realm == OwningRealm)
                    Out.WritePascalString(_holdDesc);
                else
                    Out.WritePascalString(_captureDesc);

                uint timerend = (uint)(CountdownTimerEnd > TCPManager.GetTimeStamp() ? CountdownTimerEnd - TCPManager.GetTimeStamp() : 0);
                Out.WriteUInt32(timerend);
                Out.WriteUInt32(timerend);

                Out.Fill(0, 9);
            plr.SendPacket(Out);
        }

        public void UpdateGlow(Player plr)
        {
            _glowObject.VfxState = (byte) OwningRealm;

            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 20);

            Out.WriteUInt16(_glowObject.Oid);
            Out.WriteByte(6); //state
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(8);
            Out.WriteByte(0);
            Out.WriteByte(_glowObject.VfxState);
            Out.Fill(0, 10);

            plr.SendPacket(Out);
        }

        public void SendObjectiveLeft(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE);
            Out.WriteUInt32(ObjectiveID);
            Out.WriteUInt32(0);
            plr.SendPacket(Out);
        }

        #endregion
    }
}