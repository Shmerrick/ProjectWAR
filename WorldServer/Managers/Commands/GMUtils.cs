using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using SystemData;
using WorldServer.Services.World;
using WorldServer.World.Objects;

namespace WorldServer.Managers.Commands
{
    /// <summary>
    /// Regroups several utils for GM commands.
    /// </summary>
    internal class GMUtils
    {

        private GMUtils() { }
        public static void PrintCommands(Player plr, List<GmCommandHandler> handler)
        {
            byte count = 0;

            plr.SendClientMessage("===================================\n");

            foreach (GmCommandHandler com in handler)
            {
                if ((com.AccessRequired == 0 || Utils.HasFlag((int)com.AccessRequired, plr.GmLevel)) && com.Handler == null)
                {
                    plr.SendClientMessage("[Subgroup] " + com.Name.ToUpper() + ": " + com.Description);
                    ++count;
                }
            }

            if (count > 0)
                plr.SendClientMessage("\n");

            foreach (GmCommandHandler com in handler)
            {
                if ((com.AccessRequired == 0 || Utils.HasFlag((int)com.AccessRequired, plr.GmLevel)) && com.Handler != null)
                    plr.SendClientMessage(com.Name.ToUpper() + ": " + com.Description + "\n");
            }
            plr.SendClientMessage("===================================");
        }
        public static string GetTotalString(ref List<string> values)
        {
            string Str = "";
            bool first = true;

            foreach (string str in values)
            {
                if (!first)
                    Str += " " + str;
                else
                {
                    Str += str;
                    first = false;
                }
            }

            return Str;
        }
        /// <summary>
        /// Retrieves a zone id from a gm command parameter.
        /// </summary>
        /// <param name="player">GM</param>
        /// <param name="values">Command arguments</param>
        /// <returns>Existing zone ID or -1 if not found (an error has been sent)</returns>
        public static int GetZoneId(Player player, ref List<string> values)
        {
            string zoneName = GetString(ref values);
            int zoneID;
            if (int.TryParse(zoneName, out zoneID))
            {
                Zone_Info info = ZoneService.GetZone_Info((ushort)zoneID);
                if (info != null)
                    return zoneID;
                player.SendMessage("Could not find zone for id " + zoneID, ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return -1;
            }

            List<Zone_Info> results = new List<Zone_Info>();
            foreach (Zone_Info zone in ZoneService._Zone_Info)
            {
                if (zone.Name.ToLower().Contains(zoneName.ToLower()))
                    results.Add(zone);
            }
            if (results.Count == 0)
            {
                player.SendMessage("Could not find zone for name " + zoneName, ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
            }
            else if (results.Count > 1)
            {
                string[] names = new string[results.Count];
                for (int i = 0; i < results.Count; i++)
                    names[i] = results[i].Name;
                player.SendMessage($"Several zones match '{zoneName}' : {string.Join(",", names)}" + zoneID, ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
            }
            else
            {
                return results[0].ZoneId;
            }
            return -1;
        }
        public static string GetString(ref List<string> values)
        {
            if (values.Count <= 0)
                return "0";

            string str = values[0];
            values.RemoveAt(0);

            return str;
        }
        public static int GetInt(ref List<string> values)
        {
            return int.Parse(GetString(ref values));
        }

        public static uint GetUInt(ref List<string> values)
        {
            return uint.Parse(GetString(ref values));
        }

        /// <summary>
        /// Tries to parse given raw value.
        /// </summary>
        /// <param name="expected">Parameter reflexion into</param>
        /// <param name="rawValue">Raw value from user input</param>
        /// <returns>Parsed value or null if could not be parsed</returns>
        public static object TryParse(Type expected, string rawValue)
        {
            if (expected == typeof(string))
            {
                return rawValue;
            }
            else if (expected == typeof(short))
            {
                short parsed;
                if (short.TryParse(rawValue, out parsed))
                    return parsed;
            }
            else if (expected == typeof(int))
            {
                int parsed;
                if (int.TryParse(rawValue, out parsed))
                    return parsed;
            }
            else if (expected == typeof(long))
            {
                long parsed;
                if (long.TryParse(rawValue, out parsed))
                    return parsed;
            }
            else if (expected == typeof(ushort))
            {
                ushort parsed;
                if (ushort.TryParse(rawValue, out parsed))
                    return parsed;
            }
            else if (expected == typeof(uint))
            {
                uint parsed;
                if (uint.TryParse(rawValue, out parsed))
                    return parsed;
            }
            else if (expected == typeof(ulong))
            {
                ulong parsed;
                if (ulong.TryParse(rawValue, out parsed))
                    return parsed;
            }
            else if (expected == typeof(float))
            {
                float parsed;
                if (float.TryParse(rawValue, out parsed))
                    return parsed;
            }
            else if (expected == typeof(double))
            {
                double parsed;
                if (double.TryParse(rawValue, out parsed))
                    return parsed;
            }
            else if (expected == typeof(bool))
            {
                if (rawValue == null)
                    return false;
                rawValue = rawValue.ToLowerInvariant();
                return rawValue == "1" || rawValue == "true" || rawValue == "on";
            }
            else if (expected.IsEnum)
            {
                object index = TryParse(typeof(int), rawValue);
                Array values = Enum.GetValues(expected);
                if (index != null && (int)index > 0)
                    return values.GetValue((int)index);

                rawValue = rawValue.ToLower();
                object match = null;
                foreach (object value in values)
                {
                    if (Enum.GetName(expected, value).ToLower().Contains(rawValue.ToLower()))
                    {
                        if (match != null)
                            return null;
                        match = value;
                    }
                }
                return match;
            }
            return null;
        }

        public static Unit GetTargetOrMe(Player plr)
        {
            return plr.CbtInterface.GetCurrentTarget() ?? plr;
        }

        public static void LogSanction(int accountId, Player issuedBy, string actionType, string actionDuration, string actionReason)
        {
            Program.AcctMgr.AddSanction(
                new AccountSanctionInfo
                {
                    AccountId = accountId,
                    IssuedBy = issuedBy.Client._Account.Username,
                    ActionType = actionType,
                    IssuerGmLevel = issuedBy.Client._Account.GmLevel,
                    ActionDuration = actionDuration,
                    ActionLog = actionReason,
                    ActionTime = TCPManager.GetTimeStamp()
                });
        }

        public static Unit GetObjectTarget(Player obj)
        {
            Unit target = obj.CbtInterface.GetCurrentTarget() ?? obj;

            return target;
        }

        /// <summary>Utility method sending an a CSR tell to user.</summary>
        /// <param name="player">Player to send the error to</param>
        /// <param name="text">Parts of text to send</param>
        public static void SendCsr(Player player, params object[] text)
        {
            player.SendClientMessage(string.Concat(text), ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        }
    }
}
