using System;
using System.Collections.Generic;

using Common;
using FrameWork;

namespace WorldServer.Services.World
{
    /// <summary>
    /// Loads the help tip trigger table.
    ///
    /// A help tip is an ordinary Tome entry in section 101 (client entries 11800-11999). The
    /// client raises its HELP_TIP_UPDATED event when a ToK unlock arrives carrying a non-zero
    /// category byte, then resolves the tip title and body from its HelpTipNames and
    /// HelpTipDescriptions string tables with (entry - 11799). Rows whose entry is not a named
    /// section 101 record are therefore rejected here: they would pop an empty tip window.
    /// </summary>
    [Service(typeof(TokService))]
    public class HelpTipService : ServiceBase
    {
        /// <summary>Tome section holding the help tips.</summary>
        public const uint HELP_TIP_SECTION = 101;

        /// <summary>Category sent when an unlock is not a help tip; suppresses the client event.</summary>
        public const byte HELP_TIP_TYPE_NONE = 0;

        private const byte HELP_TIP_TYPE_MAX = 4;

        private static readonly List<Help_Tip> _empty = new List<Help_Tip>();

        private static Dictionary<ushort, Help_Tip> _tipsByEntry = new Dictionary<ushort, Help_Tip>();
        private static Dictionary<HelpTipTrigger, List<Help_Tip>> _tipsByTrigger = new Dictionary<HelpTipTrigger, List<Help_Tip>>();

        [LoadingFunction(true)]
        public static void LoadHelpTips()
        {
            Log.Debug("WorldMgr", "Loading Help_Tips...");

            Dictionary<ushort, Help_Tip> byEntry = new Dictionary<ushort, Help_Tip>();
            Dictionary<HelpTipTrigger, List<Help_Tip>> byTrigger = new Dictionary<HelpTipTrigger, List<Help_Tip>>();

            IList<Help_Tip> tips = Database.SelectAllObjects<Help_Tip>();
            int rejected = 0;

            if (tips != null)
            {
                foreach (Help_Tip tip in tips)
                {
                    if (tip.Enabled == 0)
                        continue;

                    if (!IsHelpTipEntry(tip.TokEntry))
                    {
                        Log.Error("HelpTipService", "help_tips entry " + tip.TokEntry + " is not a named section " + HELP_TIP_SECTION + " Tome record; the client has no text for it.");
                        ++rejected;
                        continue;
                    }

                    if (tip.TipType == HELP_TIP_TYPE_NONE || tip.TipType > HELP_TIP_TYPE_MAX)
                    {
                        Log.Error("HelpTipService", "help_tips entry " + tip.TokEntry + " has category " + tip.TipType + "; expected 1-4.");
                        ++rejected;
                        continue;
                    }

                    HelpTipTrigger trigger;
                    if (!TryParseTrigger(tip.TriggerName, out trigger))
                    {
                        Log.Error("HelpTipService", "help_tips entry " + tip.TokEntry + " has unknown trigger '" + tip.TriggerName + "'.");
                        ++rejected;
                        continue;
                    }

                    if (byEntry.ContainsKey(tip.TokEntry))
                    {
                        Log.Error("HelpTipService", "help_tips entry " + tip.TokEntry + " is declared more than once.");
                        ++rejected;
                        continue;
                    }

                    byEntry.Add(tip.TokEntry, tip);

                    List<Help_Tip> bucket;
                    if (!byTrigger.TryGetValue(trigger, out bucket))
                    {
                        bucket = new List<Help_Tip>();
                        byTrigger.Add(trigger, bucket);
                    }

                    bucket.Add(tip);
                }
            }

            _tipsByEntry = byEntry;
            _tipsByTrigger = byTrigger;

            Log.Success("LoadHelpTips", "Loaded " + byEntry.Count + " Help_Tips" + (rejected > 0 ? " (" + rejected + " rejected)" : string.Empty));
        }

        /// <summary>
        /// Category byte to send in F_TOK_ENTRY_UPDATE for this unlock.
        /// Returns <see cref="HELP_TIP_TYPE_NONE"/> for every entry that is not a configured help
        /// tip, which is what stops an ordinary Tome unlock from popping an empty tip window.
        /// </summary>
        public static byte GetTipType(ushort tokEntry)
        {
            Help_Tip tip;
            if (!_tipsByEntry.TryGetValue(tokEntry, out tip))
                return HELP_TIP_TYPE_NONE;

            return tip.TipType;
        }

        /// <summary>Tips configured for a trigger, in load order. Never null.</summary>
        public static List<Help_Tip> GetTips(HelpTipTrigger trigger)
        {
            List<Help_Tip> tips;
            if (!_tipsByTrigger.TryGetValue(trigger, out tips))
                return _empty;

            return tips;
        }

        private static bool IsHelpTipEntry(ushort tokEntry)
        {
            Tok_Info info = TokService.GetTok(tokEntry);

            return info != null && info.Section == HELP_TIP_SECTION && !string.IsNullOrEmpty(info.Name);
        }

        private static bool TryParseTrigger(string value, out HelpTipTrigger trigger)
        {
            trigger = HelpTipTrigger.None;

            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                trigger = (HelpTipTrigger)Enum.Parse(typeof(HelpTipTrigger), value, true);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return trigger != HelpTipTrigger.None && Enum.IsDefined(typeof(HelpTipTrigger), trigger);
        }
    }
}
