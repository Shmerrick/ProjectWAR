﻿using FrameWork;
using Google.ProtocolBuffers;
using System.Text;

namespace LobbyServer.NetWork.Handler
{
    public class AuthentificationHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.CMSG_VerifyProtocolReq, 0, "onVerifyProtocolReq")]
        public static void CMSG_VerifyProtocolReq(BaseClient client, PacketIn packet)
        {
            Log.Debug("LServ", "CMSG_VerifyProtocolReq");
            Client cclient = (Client)client;

            PacketOut Out = new PacketOut((byte)Opcodes.SMSG_VerifyProtocolReply);

            byte[] IV_HASH1 = { 0x01, 0x53, 0x21, 0x4d, 0x4a, 0x04, 0x27, 0xb7, 0xb4, 0x59, 0x0f, 0x3e, 0xa7, 0x9d, 0x29, 0xe9 };
            byte[] IV_HASH2 = { 0x49, 0x18, 0xa1, 0x2a, 0x64, 0xe1, 0xda, 0xbd, 0x84, 0xd9, 0xf4, 0x8a, 0x8b, 0x3c, 0x27, 0x20 };

            ByteString iv1 = ByteString.CopyFrom(IV_HASH1);
            ByteString iv2 = ByteString.CopyFrom(IV_HASH2);
            VerifyProtocolReply.Builder verify = VerifyProtocolReply.CreateBuilder();
            verify.SetResultCode(VerifyProtocolReply.Types.ResultCode.RES_SUCCESS);

            verify.SetIv1(ByteString.CopyFrom(IV_HASH1));
            verify.SetIv2(ByteString.CopyFrom(IV_HASH2));

            Out.Write(verify.Build().ToByteArray());

            cclient.SendTCPCuted(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.CMSG_AuthSessionTokenReq, 0, "onAuthSessionTokenReq")]
        public static void CMSG_AuthSessionTokenReq(BaseClient client, PacketIn packet)
        {
            Log.Debug("LServ", "CMSG_AuthSessionTokenReq");
            Client cclient = (Client)client;

            PacketOut Out = new PacketOut((byte)Opcodes.SMSG_AuthSessionTokenReply);

            AuthSessionTokenReq.Builder authReq = AuthSessionTokenReq.CreateBuilder();
            authReq.MergeFrom(packet.ToArray());

            string session = Encoding.ASCII.GetString(authReq.SessionToken.ToByteArray());
            Log.Debug("AuthSession", "session " + session);
            cclient.Username = "";                                  //username is not important anymore in 1.4.8
            cclient.Token = session;

            AuthSessionTokenReply.Builder authReply = AuthSessionTokenReply.CreateBuilder();
            authReply.SetResultCode(AuthSessionTokenReply.Types.ResultCode.RES_SUCCESS);

            Out.Write(authReply.Build().ToByteArray());

            cclient.SendTCPCuted(Out);

            /*   //TODO: need auth check

                if (Result != AuthResult.AUTH_SUCCESS)
                    cclient.Disconnect();
                else
                {
                    cclient.Username = Username;
                    cclient.Token = Token;
                }*/
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.CMSG_GetAcctPropListReq, 0, "onAcctPropListReq")]
        public static void CMSG_GetAcctPropListReq(BaseClient client, PacketIn packet)
        {
            Log.Debug("LServ", "GetAcctPropListReq");

            Client cclient = (Client)client;

            PacketOut Out = new PacketOut((byte)Opcodes.SMSG_GetAcctPropListReply);
            byte[] val = { 0x08, 0x00 };
            Out.Write(val);
            cclient.SendTCPCuted(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.CMSG_MetricEventNotify, 0, "onMetricEventNotify")]
        public static void CMSG_MetricEventNotify(BaseClient client, PacketIn packet)
        {
            //do nothing
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.CMSG_GetClusterListReq, 0, "onGetServerListReq")]
        public static void CMSG_GetClusterListReq(BaseClient client, PacketIn packet)
        {
            Log.Debug("LServ", "GetClusterListReq");
            Client cclient = (Client)client;
            PacketOut Out = new PacketOut((byte)Opcodes.SMSG_GetClusterListReply);
            byte[] ClustersList = Core.AcctMgr.BuildClusterList();

            Log.Debug("LServ", "Received " + ClustersList.Length + " clusters");

            Out.Write(ClustersList);
            cclient.SendTCPCuted(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.CMSG_GetCharSummaryListReq, 1, "onGetCharacterSummaries")]
        public static void CMSG_GetCharSummaryListReq(BaseClient client, PacketIn packet)
        {
            Log.Debug("LServ", "GetCharSummaryListReq");

            Client cclient = (Client)client;

            PacketOut Out = new PacketOut((byte)Opcodes.SMSG_GetCharSummaryListReply);

            Out.Write(new byte[] { 0x08, 00 });
            cclient.SendTCPCuted(Out);

            if (Core.Config.SeverOnFinish)
                cclient.Disconnect("Transaction complete");
        }
    }
}