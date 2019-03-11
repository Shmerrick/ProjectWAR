using SystemData;
using FrameWork;
using WorldServer.World.Objects;

namespace WorldServer.NetWork.Handler
{
    public class SocialHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_SOCIAL_NETWORK, (int)eClientState.Playing, "onSocialNetWork")]
        public static void F_SOCIAL_NETWORK(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (!cclient.IsPlaying() || !cclient.Plr.IsInWorld())
                return;

            Player Plr = cclient.Plr;

            byte Type = packet.GetUint8();

            switch (Type)
            {
                case 7: // World Groups
                    {

                    } break;
                case 8:
                    {
                        packet.Skip(1);
                        byte NameSize = packet.GetUint8();
                        packet.Skip(1);
                        string Name = packet.GetString(NameSize);
                        byte GuildSize = packet.GetUint8();
                        packet.Skip(1);
                        string GuildName = packet.GetString(GuildSize);
                        packet.Skip(1);
                        ushort Career = packet.GetUint16();
                        packet.Skip(4);
                        ushort ZoneId = packet.GetUint16();

                        while (ZoneId > 256)
                            ZoneId -= 256;

                        while (packet.GetUint8() != 0xFF) ;

                        packet.Skip(2 + (ZoneId == 255 ? 0 : 1));

                        byte MinLevel = packet.GetUint8();
                        byte MaxLevel = packet.GetUint8();

                        Plr.SocInterface.SendPlayers(Player.GetPlayers(Name, GuildName, Career, ZoneId, MinLevel, MaxLevel, cclient.Plr), Plr.GmLevel != 0); //cant hide location/details from gm

                    } break;
                case 11: // Inspection
                    {
                        Player Target = Plr.CbtInterface.GetTarget(GameData.TargetTypes.TARGETTYPES_TARGET_ALLY) as Player;
                        if (Target == null)
                            Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_SN_LISTS_ERR_PLAYER_NOT_FOUND);
                        else if(!Target.Info.Anonymous || Plr.GmLevel > 1) //do not allow inspect of anonymous players, unless by gm
                            Target.ItmInterface.SendInspect(Plr);
                        else
                            Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_UNABLE_TO_INSPECT_PLAYER_EQUIP);
                    } break;
                case 13: // Nearby Groups
                    {
                        Group.SendWorldGroups(Plr);
                    } break;
                case 18: // AFK
                    {
                        packet.Skip(1);
                        bool AFKState = packet.GetUint8() == 1; // Went afk manually.
                        bool AutoAFK = packet.GetUint8() == 1; // Kick them from SC if this happens?

                        //Use
                        Plr.IsAFK = AFKState;
                        Plr.IsAutoAFK = AutoAFK;
                        //remove a player from an SC if they are in one and afk is toggled.
                        if ((Plr.IsAFK || Plr.IsAutoAFK) && Plr.ScnInterface.Scenario != null)
                        {
                            Plr.SendClientMessage("You have been removed from the scenario due to afk", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                            Plr.ScnInterface.Scenario.RemovePlayer(Plr, true);
                        }

                        byte length = packet.GetUint8();
                        packet.Skip(1);
                        string message = packet.GetString(length);
                        if (message != null)
                        {
                            Plr.AFKMessage = message;
                        }
                        else
                        {
                            Plr.AFKMessage = "";
                        }
                    }
                    break;
            }
        }
    }
}
