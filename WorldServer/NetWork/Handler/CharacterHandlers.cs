using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.World.Objects;

namespace WorldServer.NetWork.Handler
{
    public class CharacterHandlers : IPacketHandler
    {
        public struct CreateInfo
        {
            public byte slot, race, career, sex, model;
            public ushort NameSize;
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_CREATE_CHARACTER, (int)eClientState.CharScreen, "onCreateCharacter")]
        public static void F_CREATE_CHARACTER(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient) client;
            CreateInfo Info;

            Info.slot = packet.GetUint8();
            Info.race = packet.GetUint8();
            Info.career = packet.GetUint8();
            Info.sex = packet.GetUint8();
            Info.model = packet.GetUint8();
            Info.NameSize = packet.GetUint16();
            packet.Skip(2);

            byte[] traits = new byte[8];
            packet.Read(traits, 0, traits.Length);
            packet.Skip(7);

            string name = packet.GetString(Info.NameSize);

            ushort duplicate = 0;
            for (int i = 0; i < name.Length; i++)
            {
                if (i != 0)
                {
                    if (name[i] == name[i - 1])
                    {
                        duplicate++;
                    }
                    else
                        duplicate = 0;

                    if (duplicate > 3)
                        break;

                }
            }

            if (name.Length > 2 && !CharMgr.NameIsUsed(name) && CharMgr.AllowName(name) && !CharMgr.NameIsDeleted(name) && duplicate < 3)
            {
                CharacterInfo CharInfo = CharMgr.GetCharacterInfo(Info.career);
                if (CharInfo == null)
                {
                    Log.Error("ON_CREATE", "Can not find career :" + Info.career);
                }
                else
                {

                    //Log.Success("OnCreate", "New Character : " + Name);

                    Character Char = new Character
                    {
                        AccountId = cclient._Account.AccountId,
                        bTraits = traits,
                        Career = Info.career,
                        CareerLine = CharInfo.CareerLine,
                        ModelId = Info.model,
                        Name = name,
                        Race = Info.race,
                        Realm = CharInfo.Realm,
                        RealmId = Program.Rm.RealmId,
                        Sex = Info.sex,
                        FirstConnect = true
                    };

                    if (!CharMgr.CreateChar(Char))
                    {
                        Log.Error("CreateCharacter", "Hack : can not create more than 10 characters!");
                    }
                    else
                    {
                        List < CharacterInfo_item > Items = CharMgr.GetCharacterInfoItem(Char.CareerLine);

                        foreach (CharacterInfo_item Itm in Items)
                        {
                            if (Itm == null)
                                continue;

                            CharacterItem Citm = new CharacterItem
                            {
                                Counts = Itm.Count,
                                CharacterId = Char.CharacterId,
                                Entry = Itm.Entry,
                                ModelId = Itm.ModelId,
                                SlotId = Itm.SlotId,
                                PrimaryDye = 0,
                                SecondaryDye = 0
                            };
                            CharMgr.CreateItem(Citm);
                        }

                        Character_value CInfo = new Character_value
                        {
                            CharacterId = Char.CharacterId,
                            Level = 1,
                            Money = 2000,
                            Online = false,
                            RallyPoint = CharInfo.RallyPt,
                            RegionId = CharInfo.Region,
                            Renown = 0,
                            RenownRank = 1,
                            RestXp = 0,
                            Skills = CharInfo.Skills,
                            Speed = 100,
                            PlayedTime = 0,
                            WorldO = CharInfo.WorldO,
                            WorldX = CharInfo.WorldX,
                            WorldY = CharInfo.WorldY,
                            WorldZ = CharInfo.WorldZ,
                            Xp = 0,
                            ZoneId = CharInfo.ZoneId
                        };

                        CharMgr.Database.AddObject(CInfo);
                        Program.AcctMgr.UpdateRealmCharacters(Program.Rm.RealmId, (uint)CharMgr.Database.GetObjectCount<Character>(" Realm=1"), (uint)CharMgr.Database.GetObjectCount<Character>(" Realm=2"));

                        CharacterClientData clientData = new CharacterClientData { CharacterId = Char.CharacterId };
                        CharMgr.Database.AddObject(clientData);

                        Char.Value = CInfo;
                        Char.ClientData = clientData;

                        PacketOut Out = new PacketOut((byte)Opcodes.F_SEND_CHARACTER_RESPONSE, 32);
                        Out.WritePascalString(cclient._Account.Username);
                        cclient.SendPacket(Out);
                    }
                }
            }
            else
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_SEND_CHARACTER_ERROR, 64);
                Out.FillString(cclient._Account.Username, 24);
                Out.WriteStringBytes("You have entered a duplicate or invalid name. Please enter a new name.");
                cclient.SendPacket(Out);
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_CRASH_PACKET, (int)eClientState.WorldEnter, "F_CRASH_PACKET")]
        public static void F_CRASH_PACKET(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient) client;

            if (cclient.Plr != null)
                cclient.Plr.DisconnectType = Player.EDisconnectType.Crash;

            string folder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "CrashLogs");

            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            var builder = new StringBuilder();

            try
            {
                while (!cclient.PLogBuf.IsEmpty)
                {
                    var p = cclient.PLogBuf.Dequeue();
                    if(p is PacketIn)
                        builder.AppendLine(Utils.ToLogHexString((byte)((PacketIn)p).Opcode, false, ((PacketIn)p).ToArray()));
                    else if(p is PacketOut)
                            builder.AppendLine(Utils.ToLogHexString((byte)((PacketOut)p).Opcode, true, ((PacketOut)p).ToArray()));
                }
               
                string name = "";
                if(cclient.Plr != null && cclient.Plr.Name != null)
                    name = "_" + cclient.Plr.Name;

                if (builder.Length > 0)
                    File.WriteAllText(Path.Combine(folder, cclient._Account.Username + name) + ".txt", builder.ToString());
            }
            catch (Exception e)
            {
                Log.Error("F_CRASH_PACKET","Error saving crash log. " + e.Message + "\r\n" + builder.ToString() +"\r\n");
            }


        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_PLAYER_EXIT, (int)eClientState.WorldEnter, "F_PLAYER_EXIT")]
        public static void F_PLAYER_EXIT(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient) client;

            if (cclient.Plr != null)
                cclient.Plr.DisconnectType = Player.EDisconnectType.Clean;
        }


        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_DELETE_CHARACTER, (int)eClientState.CharScreen, "onDeleteCharacter")]
        public static void F_DELETE_CHARACTER(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            byte Slot = packet.GetUint8();

            if (cclient._Account == null)
            {
                cclient.Disconnect("Null Account in F_DELETE_CHARACTER");
                return;
            }

            CharMgr.RemoveCharacter(Slot, cclient);

            PacketOut Out = new PacketOut((byte)Opcodes.F_SEND_CHARACTER_RESPONSE, 24);
            Out.FillString(cclient._Account.Username, 21);
            Out.Fill(0, 3);
            cclient.SendPacket(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_DELETE_NAME, (int)eClientState.CharScreen, "onDeleteName")]
        public static void F_DELETE_NAME(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            string CharName = packet.GetString(30);
            string UserName = packet.GetString(20);

            byte Bad = 0;

            if (CharName.Length < 3 || CharMgr.NameIsUsed(CharName) || !System.Text.RegularExpressions.Regex.IsMatch(CharName, @"^[a-zA-Z]+$"))
                Bad = 1;

            Log.Debug("F_DELETE_NAME", "Bad=" + Bad + ",Name=" + CharName);

            PacketOut Out = new PacketOut((byte)Opcodes.F_CHECK_NAME, 54);
            Out.FillString(CharName, 30);
            Out.FillString(UserName, 20);
            Out.WriteByte(Bad);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            cclient.SendPacket(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_DUMP_ARENAS_LARGE, (int)eClientState.CharScreen, "onDumpArenasLarge")]
        public static void F_DUMP_ARENAS_LARGE(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (!cclient.HasAccount())
            {
                cclient.Disconnect("No Account in F_DUMP_ARENAS_LARGE");
                return;
            }

            if (Program.Rm.OnlinePlayers >= Program.Rm.MaxPlayers && cclient._Account.GmLevel == 1)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_LOGINQUEUE);
                client.SendPacket(Out);
                return;
            }

            byte CharacterSlot = packet.GetUint8();
            Character Char = CharMgr.GetAccountChar(cclient._Account.AccountId).GetCharacterBySlot(CharacterSlot);

            if (Char == null)
            {
                Log.Error("F_DUMP_ARENAS_LARGE", "Can not find character on slot : " + CharacterSlot);
                cclient.Disconnect("Character not found in F_DUMP_ARENAS_LARGE");
                return;
            }

            {
                if (cclient.Plr == null)
                    cclient.Plr = Player.CreatePlayer(cclient, Char);

                if (cclient.Plr == null)
                { 
                    cclient.Disconnect("NULL Player from CreatePlayer?");
                    return;
                }

                if (cclient.Plr.Client != cclient)
                {
                    cclient.Plr.Client?.Disconnect("Ghost client");
                    cclient.Plr.Destroy();
                    cclient.Disconnect("Player already exists");

                    return;
                }

                PacketOut Out = new PacketOut((byte)Opcodes.F_WORLD_ENTER, 64);
                Out.WriteUInt16(0x0608); // TODO
                Out.Fill(0, 20);
                Out.WriteString("38699", 5);
                Out.WriteString("38700", 5);
                Out.WriteString("0.0.0.0", 20);
                cclient.SendPacket(Out);
            }
        }


        struct RandomNameInfo
        {
            public byte Race, Unk, Slot;
            
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_RANDOM_NAME_LIST_INFO, (int)eClientState.CharScreen, "onRandomNameListInfo")]
        public static void F_RANDOM_NAME_LIST_INFO(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;
            RandomNameInfo Info = BaseClient.ByteToType<RandomNameInfo>(packet);

            List<Random_name> Names = CharMgr.GetRandomNames();

            PacketOut Out = new PacketOut((byte)Opcodes.F_RANDOM_NAME_LIST_INFO);
            Out.WriteByte(0);
            Out.WriteByte(Info.Unk);
            Out.WriteByte(Info.Slot);
            Out.WriteUInt16(0);
            Out.WriteByte((byte)Names.Count);

            for (int i = Names.Count - 1; i >= 0; --i)
                Out.FillString(Names[i].Name, Names[i].Name.Length + 1);

            cclient.SendPacket(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_REQUEST_CHAR, (int)eClientState.CharScreen, "onRequestChar")]
        public static void F_REQUEST_CHAR(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            cclient.State = (int)eClientState.CharScreen;

            ushort Operation = packet.GetUint16();

            if (Operation == 0x2D58)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_REQUEST_CHAR_ERROR, 1);
                Out.WriteByte((byte)CharMgr.GetAccountRealm(cclient._Account.AccountId));
                cclient.SendPacket(Out);
            }
            else
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_REQUEST_CHAR_RESPONSE, 64);
                Out.FillString(cclient._Account.Username, 20);
                Out.WriteUInt32(0); //RemainingLockoutTime
                Out.WriteByte(0);

                if (cclient._Account.GmLevel == 1 && !Program.Config.CreateBothRealms)
                    Out.WriteByte((byte)CharMgr.GetAccountRealm(cclient._Account.AccountId));
                else
                    Out.WriteByte(0);

                Out.WriteByte(CharMgr.MaxSlot); //Maximum Characters you can have
                Out.WriteByte(0); //GameplayRulesetType
                Out.WriteByte(0); //LastSwitchedToRealm
                Out.WriteByte(0); //NumPaidNameChangesAvailable
                Out.WriteByte(0); // unk

                byte[] Chars = CharMgr.BuildCharacters(cclient._Account.AccountId);
                Out.Write(Chars, 0, Chars.Length);

                cclient.SendPacket(Out);
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_REQUEST_CHAR_TEMPLATES, (int)eClientState.CharScreen, "onRequestCharTemplates")]
        public static void F_REQUEST_CHAR_TEMPLATES(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            PacketOut Out = new PacketOut((byte)Opcodes.F_REQUEST_CHAR_TEMPLATES, 12);
            Out.Write(new byte[0x11], 0, 0x11);
            cclient.SendPacket(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_RENAME_CHARACTER, (int)eClientState.CharScreen, "onRenameCharacter")]
        public static void F_RENAME_CHARACTER(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            packet.Skip(3);

            string OldName = packet.GetString(24);
            string NewName = packet.GetString(24);

            //Log.Success("F_RENAME_CHARACTER", "Renaming '" + OldName + "' to '" + NewName + "'");

            if (NewName.Length > 2 && !CharMgr.NameIsUsed(NewName))
            {
                Character Char = CharMgr.GetCharacter(Player.AsCharacterName(OldName), false);

                if (Char == null || Char.AccountId != cclient._Account.AccountId)
                {
                    Log.Error("CharacterRename", "Hack: Tried to rename character which account doesn't own");
                    cclient.Disconnect("Null or unowned character in F_RENAME_CHARACTER");
                    return;
                }

                Char.Name = NewName;
                CharMgr.Database.SaveObject(Char);

                // Wrong response? Perhaps needs to send F_REQUEST_CHAR_RESPONSE again.
                PacketOut Out = new PacketOut((byte)Opcodes.F_SEND_CHARACTER_RESPONSE);
                Out.WritePascalString(cclient._Account.Username);
                cclient.SendPacket(Out);
            }
            else
            {
                // Wrong response?
                PacketOut Out = new PacketOut((byte)Opcodes.F_SEND_CHARACTER_ERROR);
                Out.WritePascalString(cclient._Account.Username);
                cclient.SendPacket(Out);
            }
        }
    }
}
