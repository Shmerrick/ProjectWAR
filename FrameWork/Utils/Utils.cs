

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace FrameWork
{
    static public class Utils
    {
        public static bool getBit(Int32 value, byte pos)
        {
            return (value & (1 << pos)) > 0;
        }
        public static Int32 setBit(Int32 value,byte pos,bool ON)
        {
            if (ON)
                return (Int32)(value | (Int32)(0x01 << pos));
            else
                return (value &= ~(1 << pos));
        }


        public static string SQLEscape(string s)
        {
            s = s.Replace("\\", "\\\\");
            s = s.Replace("\"", "\\\"");
            s = s.Replace("'", "\\'");
            s = s.Replace("’", "\\’");

            return s;
        }

        static public List<T> ConvertStringToArray<T>(string Value)
        {
            string[] Values = Value.Split(' ');
            List<T> L = new List<T>();
            foreach (string Val in Values)
                if (Val.Length > 0)
                {
                    try
                    {
                        // For now, anticipate this exception in advance
                        if (typeof(T) == typeof(ushort) && Val[0] == char.Parse("-"))
                        {
                            //Log.Error("FrameWork.Utils.ConvertStringToArray", "Value isn't castable to unsigned short");
                            L.Add((T)Convert.ChangeType("0", typeof(T)));
                        }

                        else
                        {
                            if (typeof(T) == typeof(byte))
                            {
                                byte result = 0;
                                if (!byte.TryParse(Val, out result))
                                {
                                    Log.Error("FrameWork.Utils.ConvertStringToArray", "Unable to convert to byte " + Val);
                                }
                                L.Add((T)(object)result);
                            }
                            else if (typeof(T) == typeof(ushort))
                            {
                                ushort result = 0;
                                if (!ushort.TryParse(Val, out result))
                                {
                                    Log.Error("FrameWork.Utils.ConvertStringToArray", "Unable to convert to ushort " + Val);
                                }
                                L.Add((T)(object)result);
                            }
                            else
                                L.Add((T)Convert.ChangeType(Val, typeof(T)));
                        }
                    }
                    catch
                    {
                        Log.Error("FrameWork.Utils.ConvertStringToArray", "Overflow exception when attempting to cast to type " + typeof(T));
                        L.Add((T)Convert.ChangeType("0", typeof(T)));
                    }
                }

            return L;
        }
        static public string ConvertArrayToString<T>(T[] Value)
        {
            string Result = "";
            foreach (T val in Value)
                Result += (string)Convert.ChangeType(val, typeof(string)) + " ";

            return Result;
        }

        static public string VectorToString(Vector3 Vec)
        {
            return PositionToString(Vec.X, Vec.Y, Vec.Z);
        }
        static public Vector3 StringToVector(string Str)
        {
            float[] Value = StringToPosition(Str);
            return new Vector3(Value[0], Value[1], Value[2]);
        }

        static public string PositionToString(float X, float Y, float Z)
        {
            return "" + X + ":" + Y + ":" + Z;
        }

        public static uint Adler32(uint adler, byte[] bytes, UInt64 length)
        {
            const uint a32mod = 65521;
            uint s1 = (uint)(adler & 0xFFFF), s2 = (uint)(adler >> 16);
            for (UInt64 i = 0; i < length; i++)
            {
                byte b = bytes[i];
                s1 = (s1 + b) % a32mod;
                s2 = (s2 + s1) % a32mod;
            }
            return unchecked((uint)((s2 << 16) + s1));
        }

        public static uint Adler32(Stream stream, long size, int blockSize = 0xFFFFF, uint adler = 0)
        {
            var pos = stream.Position;
            long remain = size;
            long readSize = blockSize;
            byte[] block = new byte[blockSize];
            var a = new FrameWork.Adler32();
            while (remain > 0)
            {
                if (stream.Position + blockSize > stream.Length)
                    readSize = stream.Length - stream.Position;

                stream.Read(block, 0, (int)readSize);
                adler = (uint)a.adler32(adler, block, 0, (int)readSize);
                //   adler = Adler32(adler, block, (int)readSize);
                remain -= readSize;
            }
            stream.Position = pos;
            return adler;
        }

        public static string ToLogHexString(byte op, bool server, byte[] dump)
        {
            var hexDump = new StringBuilder();

            if (!server)
            {
                var withHeader = new byte[dump.Length + 10];
                Buffer.BlockCopy(dump, 0, withHeader, 10, dump.Length);
                withHeader[9] = op;
                dump = withHeader;
            }

            hexDump.AppendLine("[" + (server ? "Server" : "Client") + "] packet : (0x" + op.ToString("X").PadLeft(2, '0').ToUpper() + ") " + ((Opcodes)op).ToString()
                + " Size = " + dump.Length + " " + DateTime.Now.ToString("hh:mm:ss.ffff"));
            hexDump.AppendLine("|------------------------------------------------|----------------|");
            hexDump.AppendLine("|00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F |0123456789ABCDEF|");
            hexDump.AppendLine("|------------------------------------------------|----------------|");
            try
            {
                int end = dump.Length;
                for (int i = 0; i < end; i += 16)
                {
                    StringBuilder text = new StringBuilder();
                    StringBuilder hex = new StringBuilder();


                    for (int j = 0; j < 16; j++)
                    {
                        if (j + i < end)
                        {
                            byte val = dump[j + i];

                            hex.Append(dump[j + i].ToString("X2"));

                            hex.Append(" ");
                            if (val >= 32 && val < 127)
                            {

                                text.Append((char)val);
                            }
                            else
                            {
                                text.Append(".");
                            }
                        }
                    }

                    hexDump.AppendLine("|" + hex.ToString().PadRight(48) + "|" + text.ToString().PadRight(16) + "|");
                }
            }
            catch (Exception)
            {
                // Log.Error("HexDump", e.ToString());
            }

            hexDump.Append("-------------------------------------------------------------------");
            return hexDump.ToString();
        }
    
        static public float[] StringToPosition(string Str)
        {
            float[] Result = new float[3] { 0, 0, 0 };
            string[] Value = Str.Split(':');
            for (int i = 0; i < Result.Length; ++i)
                if (Value.Length >= i)
                    Result[i] = float.Parse(Value[i]);
            return Result;
        }

        static public string ChannelsToString(Dictionary<int, Color> Chans)
        {
            string Result = "";

            if (Chans.Count <= 0)
                return Result;

            foreach (KeyValuePair<int, Color> Kp in Chans)
                Result += Kp.Key + "," + ColorToString(Kp.Value) + "|";

            // 0 - 1 - 2    
            return Result.Remove(Result.Length - 1, 1);
        }
        static public Dictionary<int, Color> StringToChannels(string Str)
        {
            // 1,18:20:25:45|2,457:154:651:0
            Dictionary<int, Color> Chans = new Dictionary<int, Color>();
            if (Str.Length <= 0)
                return Chans;

            foreach (string Kp in Str.Split('|'))
            {
                if (Kp.Length <= 0)
                    continue;

                int ChannelId = int.Parse(Kp.Split(',')[0]);
                Color Col = StringToColor(Kp.Split(',')[1]);

                if (!Chans.ContainsKey(ChannelId))
                    Chans.Add(ChannelId, Col);
            }
            return Chans;
        }

        static public string IntArrayToString(int[] Values)
        {
            string Result = "";

            if (Values.Length <= 0)
                return Result;

            foreach (int Val in Values)
                Result += Val + ":";

            return Result.Remove(Result.Length - 1, 1);
        }
        static public int[] StringToIntArray(string Str)
        {
            if (Str.Length <= 0)
                return new int[0];

            string[] Values = Str.Split(':');
            int[] Result = new int[Values.Length];

            for (int i = 0; i < Values.Length; ++i)
                if (Values[i].Length > 0)
                    Result[i] = int.Parse(Values[i]);

            return Result;
        }

        static public string EquipementToString(Dictionary<int, int> Chans)
        {
            string Result = "";

            if (Chans.Count <= 0)
                return Result;

            foreach (KeyValuePair<int, int> Kp in Chans)
                Result += Kp.Key + "," + Kp.Value + "|";

            return Result.Remove(Result.Length - 1, 1);
        }
        static public Dictionary<int, int> StringToEquipement(string Str)
        {
            // 1,134|2,18
            Dictionary<int, int> Chans = new Dictionary<int, int>();
            if (Str.Length <= 0)
                return Chans;

            foreach (string Kp in Str.Split('|'))
            {
                if (Kp.Length <= 0)
                    continue;

                int ChannelId = int.Parse(Kp.Split(',')[0]);
                int ItemId = int.Parse(Kp.Split(',')[1]);

                if (!Chans.ContainsKey(ChannelId))
                    Chans.Add(ChannelId, ItemId);
            }
            return Chans;
        }

        static public string ColorToString(Color Col)
        {
            return Col.R + ":" + Col.G + ":" + Col.B + ":" + Col.A;
        }
        static public Color StringToColor(string Str)
        {
            string[] Bytes = Str.Split(':');
            Color Col = new Color();
            Col.R = float.Parse(Bytes[0]);
            Col.G = float.Parse(Bytes[1]);
            Col.B = float.Parse(Bytes[2]);
            Col.A = float.Parse(Bytes[3]);
            return Col;

        }
        static public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        static public long Encode2Values(int P1, int P2)
        {
            return P1 << 32 + P2;
        }
        static public void Decode2Values(long Value, out int P1, out int P2)
        {
            P1 = (int)(Value >> 32);
            P2 = (int)(Value & 0xFFFFFFFF);
        }

        static public int Encode2Values(ushort P1, ushort P2)
        {
            return P1 << 16 + P2;
        }
        static public void Decode2Values(int Value, out ushort P1, out ushort P2)
        {
            P1 = (ushort)(Value >> 16);
            P2 = (ushort)(Value & 0xFFFF);
        }

        static public ushort Encode2Values(byte P1, byte P2)
        {
            return (ushort)((P1 << 8) + P2);
        }
        static public void Decode2Values(ushort Value, out byte P1, out byte P2)
        {
            P1 = (byte)(Value >> 8);
            P2 = (byte)(Value & 0xFF);
        }

        static public int Encode4Values(byte P1, byte P2, byte P3, byte P4)
        {
            return (P1 << 24) + (P2 << 16) + (P3 << 8) + P4;
        }
        static public void Decode4Values(int Value, out byte P1, out byte P2, out byte P3, out byte P4)
        {
            P1 = (byte)(Value >> 24);
            P2 = (byte)(Value >> 16);
            P3 = (byte)(Value >> 8);
            P4 = (byte)(Value & 0xFF);
        }

        public static bool HasFlag(int field, int flag)
        {
           return (field & flag) > 0;
        }
    }

    public enum Opcodes
{
    F_UNK1 = 0x01,
    F_QUEST = 0x02,
    F_UPDATE_SIEGE_LOOK_AT = 0x03,
    F_PLAYER_EXIT = 0x04,
    F_PLAYER_HEALTH = 0x05,
    F_CHAT = 0x06,
    F_TEXT = 0x07,
    F_OBJECT_STATE = 0x09,
    F_OBJECT_DEATH = 0x0A,
    F_PING = 0x0B,
    F_PLAYER_QUIT = 0x0C,
    F_DUMP_STATICS = 0x0D,
    F_WAR_REPORT = 0x0E,
    F_CONNECT = 0x0F,
    F_DISCONNECT = 0x10,
    F_HEARTBEAT = 0x11,
    F_REQUEST_CHAR_TEMPLATES = 0x13,
    F_HIT_PLAYER = 0x14,
    F_DEATHSPAM = 0x15,
    F_REQUEST_INIT_OBJECT = 0x16,
    F_OPEN_GAME = 0x17,
    F_PLAYER_INFO = 0x18,
    F_WORLD_ENTER = 0x19,
    F_CAMPAIGN_STATUS = 0x1A,
    F_REQ_CAMPAIGN_STATUS = 0x1B,
    F_GUILD_DATA = 0x1D,
    F_MAX_VELOCITY = 0x1E,
    F_SWITCH_REGION = 0x1F,
    F_PET_INFO = 0x20,
    F_PLAYER_CLEAR_DEATH = 0x21,
    F_COMMAND_CONTROLLED = 0x22,
    F_GUILD_COMMAND = 0x25,
    F_RENAME_CHARACTER = 0x26,
    F_REQUEST_TOK_REWARD = 0x27,
    F_SURVEY_BEGIN = 0x28,
    F_SHOW_DIALOG = 0x29,
    F_QUEST_INFO = 0x2B,
    F_RANDOM_NAME_LIST_INFO = 0x2C,
    F_INVITE_GROUP = 0x2F,
    F_PLAYERORG_APPROVAL = 0x2A,
    F_JOIN_GROUP = 0x30,
    F_PLAYER_DEATH = 0x31,
    F_DUMP_ARENAS_LARGE = 0x35,
    F_GROUP_COMMAND = 0x37,
    F_ZONEJUMP = 0x38,
    F_PLAYER_EXPERIENCE = 0x39,
    F_XENON_VOICE = 0x3A,
    F_REQUEST_WORLD_LARGE = 0x40,
    F_ACTION_COUNTER_INFO = 0x41,
    F_ACTION_COUNTER_UPDATE = 0x44,
    F_PLAYER_STATS = 0x46,
    F_MONSTER_STATS = 0x47,
    F_PLAY_EFFECT = 0x48,
    F_REMOVE_PLAYER = 0x49,
    F_PLAYER_RENOWN = 0x4E,
    F_MOUNT_UPDATE = 0x4F,
    F_ZONEJUMP_FAILED = 0x4A,
    F_TRADE_STATUS = 0x4B,
    F_PLAYER_LEVEL_UP = 0x50,
    F_ANIMATION = 0x51,
    F_PLAYER_WEALTH = 0x52,
    F_TROPHY_SETLOCATION = 0x53,
    F_REQUEST_CHAR = 0x54,
    F_REQUEST_CHAR_RESPONSE = 0x55,
    F_REQUEST_CHAR_ERROR = 0x56,
    F_CHARACTER_PREFS = 0x57,
    F_SEND_CHARACTER_RESPONSE = 0x58,
    F_SEND_CHARACTER_ERROR = 0x59,
    F_PING_DATAGRAM = 0x5A,
    F_ENCRYPTKEY = 0x5C,
    F_SET_TARGET = 0x5E,
    F_PQLOOT_TRIGGER = 0x5D,
    F_MYSTERY_BAG = 0x60,
    F_PLAY_SOUND = 0x61,
    F_PLAYER_STATE2 = 0x62,
    F_QUERY_NAME = 0x63,
    F_QUERY_NAME_RESPONSE = 0x64,
    F_ADD_NAME = 0x65,
    F_DELETE_NAME = 0x68,
    F_CHECK_NAME = 0x6A,
    F_CHECK_NAME_RESPONSE = 0x6B,
    F_LOCALIZED_STRING = 0x6F,
    F_KILLING_SPREE = 0x70,
    F_CREATE_STATIC = 0x71,
    F_CREATE_MONSTER = 0x72,
    F_PLAYER_IMAGENUM = 0x73,
    F_TRANSFER_ITEM = 0x75,
    F_CRAFTING_STATUS = 0x79,
    F_INIT_PLAYER = 0x7C,
    F_REQUEST_INIT_PLAYER = 0x7D,
    F_SET_ABILITY_TIMER = 0x7E,
    F_REQUEST_LASTNAME = 0x7A,
    S_PID_ASSIGN = 0x80,
    S_PONG = 0x81,
    S_CONNECTED = 0x82,
    S_WORLD_SENT = 0x83,
    S_NOT_CONNECTED = 0x84,
    S_GAME_OPENED = 0x85,
    F_MAIL = 0x86,
    S_DATAGRAM_ESTABLISHED = 0x87,
    S_PLAYER_INITTED = 0x88,
    S_PLAYER_LOADED = 0x89,
    F_RECEIVE_ENCRYPTKEY = 0x8A,
    F_MORALE_LIST = 0x8C,
    F_SURVEY_ADDQUESTION = 0x8D,
    F_SURVEY_END = 0x8E,
    F_SURVEY_RESULT = 0x8F,
    F_EMOTE = 0x90,
    F_CREATE_CHARACTER = 0x91,
    F_DELETE_CHARACTER = 0x92,
    F_GFX_MOD = 0x93,
    F_INSTANCE_INFO = 0x94,
    F_BAG_INFO = 0x95,
    F_KEEP_STATUS = 0x96,
    F_PLAY_TIME_STATS = 0x97,
    F_CATAPULT = 0x98,
    F_GRAVITY_UPDATE = 0x99,
    F_UPDATE_LASTNAME = 0x9B,
    F_CRASH_PACKET = 0x9F,
    F_HELP_DATA = 0x9A,
    F_GET_CULTIVATION_INFO = 0x9E,
    F_LOGINQUEUE = 0xA0,
    F_INTERRUPT = 0xA1,
    F_INSTANCE_SELECTED = 0xA2,
    F_ACTIVE_EFFECTS = 0xA3,
    F_SERVER_INFO = 0xA5,
    F_START_SIEGE_MULTIUSER = 0xA6,
    F_SIEGE_WEAPON_RESULTS = 0xA7,
    F_INTERACT_QUEUE = 0xA8,
    F_UPDATE_HOT_SPOT = 0xA9,
    F_GET_ITEM = 0xAA,
    F_DUEL = 0xAB,
    F_PLAYER_JUMP = 0xAC,
    F_INTRO_CINEMA = 0xAD,
    F_MAGUS_DISC_UPDATE = 0xAE,
    F_FIRE_SIEGE_WEAPON = 0xAF,
    F_GRAPHICAL_REVISION = 0xB0,
    F_AUCTION_POST_ITEM = 0xB2,
    F_CAST_PLAYER_EFFECT = 0xB3,
    F_AUCTION_SEARCH_QUERY = 0xB4,
    F_FLIGHT = 0xB5,
    F_SOCIAL_NETWORK = 0xB6,
    F_AUCTION_SEARCH_RESULT = 0xB7,
    F_PLAYER_ENTER_FULL = 0xB8,
    F_UPDATE_ITEM_COOLDOWN = 0xB9,
    F_AUCTION_BID_ITEM = 0xBB,
    F_ESTABLISH_DATAGRAM = 0xBC,
    F_PLAYER_INVENTORY = 0xBD,
    F_CHARACTER_INFO = 0xBE,
    F_INIT_STORE = 0xBF,
    F_STORE_BUY_BACK = 0xC0,
    F_OBJECTIVE_INFO = 0xC1,
    F_OBJECTIVE_UPDATE = 0xC2,
    F_SCENARIO_INFO = 0xC3,
    F_SCENARIO_POINT_UPDATE = 0xC4,
    F_OBJECTIVE_STATE = 0xC5,
    F_REALM_BONUS = 0xC6,
    F_OBJECTIVE_CONTROL = 0xC7,
    F_INTERFACE_COMMAND = 0xC8,
    F_SCENARIO_PLAYER_INFO = 0xC9,
    F_FLAG_OBJECT_STATE = 0xCA,
    F_FLAG_OBJECT_LOCATION = 0xCB,
    F_CITY_CAPTURE = 0xCC,
    F_ZONE_CAPTURE = 0xCD,
    F_SALVAGE_ITEM = 0xCE,
    F_AUCTION_BID_STATUS = 0xCF,
    F_PUNKBUSTER = 0xD0,
    F_ITEM_SET_DATA = 0xD1,
    F_INTERACT = 0xD2,
    F_DO_ABILITY = 0xD5,
    F_SET_TIME = 0xD6,
    F_INIT_EFFECTS = 0xD7,
    F_GROUP_STATUS = 0xD8,
    F_USE_ITEM = 0xD9,
    F_USE_ABILITY = 0xDA,
    F_INFLUENCE_DETAILS = 0xDB,
    F_SWITCH_ATTACK_MODE = 0xDC,
    F_BUG_REPORT = 0xDD,
    F_OBJECT_EFFECT_STATE = 0xDE,
    F_EXPERIENCE_TABLE = 0xE2,
    F_CREATE_PLAYER = 0xE3,
    F_UPDATE_STATE = 0xE4,
    F_UI_MOD = 0xE5,
    F_RVR_STATS = 0xE7,
    F_CLIENT_DATA = 0xE8,
    F_INTERACT_RESPONSE = 0xE9,
    F_QUEST_LIST = 0xEA,
    F_QUEST_UPDATE = 0xEB,
    F_REQUEST_QUEST = 0xEC,
    F_QUEST_LIST_UPDATE = 0xED,
    F_CAREER_CATEGORY = 0xEE,
    F_PLAYER_INIT_COMPLETE = 0xEF,
    F_CAREER_PACKAGE_UPDATE = 0xF1,
    F_BUY_CAREER_PACKAGE = 0xF2,
    F_CAREER_PACKAGE_INFO = 0xF3,
    F_PLAYER_RANK_UPDATE = 0xF4,
    F_DO_ABILITY_AT_POS = 0xF5,
    F_CHANNEL_LIST = 0xF6,
    F_TACTICS = 0xF7,
    F_TOK_ENTRY_UPDATE = 0xF8,
    F_TRADE_SKILL_UPDATE = 0xF9,
    F_RENDER_PRIMITIVE = 0xFA,
    F_INFLUENCE_UPDATE = 0xFB,
    F_INFLUENCE_INFO = 0xFC,
    F_KNOCKBACK = 0xFD,
    F_PLAY_VOICE_OVER = 0xFE,

    //these opcodes were missing need checks to confirm the right value
    F_CURRENT_EVENTS = 0x95, //works but prob sub id?
    //F_VIEW_LOOT_BAG = 0x60
    //F_MARKETING_REWARD_LIST = 0x065
    F_ADVANCED_WAR_REPORT = 0x95, //works but prob sub id?
    //F_MONSTER_POSITION =???
    //F_ACTION_COUNTER_TIMESTAMP_INFO = ???
    F_RRQ = 0x00,

    //part of old ones but needs to be bottom
    MAX_GAME_OPCODE = 0xFF
    };
}
