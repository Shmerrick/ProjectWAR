using Common;
using System.Collections.Generic;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Chapter modification commands under .chapter</summary>
    internal class ChapterCommands
    {

        /// <summary>
        /// Save chapter position
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ChapterSave(Player plr, ref List<string> values)
        {
            int entry = GetInt(ref values);

            Chapter_Info info = ChapterService.GetChapter((ushort)entry);
            if (info == null)
                return false;

            info.PinX = (ushort)plr.X;
            info.PinY = (ushort)plr.Y;

            plr.SendClientMessage("Saved [" + info.Name + "] to '" + plr.X + "','" + plr.Y + "'");

            info.Dirty = true;
            WorldMgr.Database.SaveObject(info);

            return true;
        }

        /// <summary>
        /// Set tok explore
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ChapterTokExplore(Player plr, ref List<string> values)
        {
            int entry = GetInt(ref values);
            int tokExplore = GetInt(ref values);

            Chapter_Info chapter = ChapterService.GetChapter((ushort)entry);
            Tok_Info tok = TokService.GetTok((ushort)entry);

            if (tok == null || chapter == null)
                return false;

            chapter.TokExploreEntry = (uint)tokExplore;
            chapter.Dirty = true;
            WorldMgr.Database.SaveObject(chapter);

            return true;
        }

        /// <summary>
        /// Set tok entry
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ChapterTok(Player plr, ref List<string> values)
        {
            int entry = GetInt(ref values);
            int tokEntry = GetInt(ref values);

            Chapter_Info chapter = ChapterService.GetChapter((ushort)entry);
            Tok_Info tok = TokService.GetTok((ushort)entry);

            if (tok == null || chapter == null)
                return false;

            chapter.TokEntry = (ushort)tokEntry;
            chapter.Dirty = true;
            WorldMgr.Database.SaveObject(chapter);

            return true;
        }
    }
}
