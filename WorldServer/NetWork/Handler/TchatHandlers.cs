using FrameWork;
using WorldServer.Managers;

namespace WorldServer.NetWork.Handler
{
    public class TchatHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_TEXT, (int)eClientState.Playing, "onText")]
        public static void F_TEXT(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null)
                return;

            byte Unk = packet.GetUint8();
            string Text = packet.GetString((int)(packet.Length - packet.Position));

            CommandMgr.HandleText(cclient.Plr, Text);
        }

        [PacketHandler(PacketHandlerType.TCP, (int) Opcodes.F_EMOTE, (int) eClientState.Playing, "onEmote")]
        public static void F_EMOTE(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null)
                return;

            if (TCPManager.GetTimeStampMS() < cclient.Plr.LastEmoteTime + 2000)
                return;

            if (cclient.Plr.IsBanned)
            { 
                cclient.Plr.SendClientMessage("You feel that this isn't the time or the place for any emotional outbursts.", SystemData.ChatLogFilters.CHATLOGFILTERS_EMOTE);
                return;
            }

            uint emote = packet.GetUint32();

            PacketOut Out = new PacketOut((byte)Opcodes.F_EMOTE, 8);
            Out.WriteUInt16(cclient.Plr.Oid);
            Out.WriteUInt16((ushort)emote);
            if (cclient.Plr.CbtInterface.HasTarget(GameData.TargetTypes.TARGETTYPES_TARGET_ENEMY))
            {
                Out.WriteUInt16(cclient.Plr.CbtInterface.Targets[(int)GameData.TargetTypes.TARGETTYPES_TARGET_ENEMY]);
                if (cclient.Plr.CbtInterface.HasTarget(GameData.TargetTypes.TARGETTYPES_TARGET_ALLY))
                {
                    if (cclient.Plr.CbtInterface.Targets[(int)GameData.TargetTypes.TARGETTYPES_TARGET_ALLY] == cclient.Plr.Oid)
                        Out.WriteUInt16(0);
                    else Out.WriteUInt16(cclient.Plr.CbtInterface.Targets[(int)GameData.TargetTypes.TARGETTYPES_TARGET_ALLY]);
                }
                else Out.WriteUInt16(0);
            }
            else if (cclient.Plr.CbtInterface.HasTarget(GameData.TargetTypes.TARGETTYPES_TARGET_ALLY))
            {
                if (cclient.Plr.CbtInterface.Targets[(int)GameData.TargetTypes.TARGETTYPES_TARGET_ALLY] == cclient.Plr.Oid)
                    Out.WriteUInt16(0);
                else Out.WriteUInt16(cclient.Plr.CbtInterface.Targets[(int)GameData.TargetTypes.TARGETTYPES_TARGET_ALLY]);

                Out.WriteUInt16(0);
            }

            else
            {
                Out.WriteUInt16(0);
                Out.WriteUInt16(0);
            }
                

            cclient.Plr.DispatchPacket(Out, false);
            cclient.Plr.SendPacket(Out);

            cclient.Plr.LastEmoteTime = TCPManager.GetTimeStampMS();
        }
    }
}
