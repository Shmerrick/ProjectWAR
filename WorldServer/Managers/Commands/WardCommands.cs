using System.Collections.Generic;
using System.Text;

using Common;
using FrameWork;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Ward fragment task counter commands under .ward</summary>
    internal class WardCommands
    {
        [CommandAttribute(EGmLevel.GM, "Lists this character's ward fragment task counters. Usage: .ward counters")]
        public static void Counters(Player plr, string unused = null)
        {
            if (plr == null)
                return;

            List<Ward_Fragment_Task> tasks = new List<Ward_Fragment_Task>(WardTaskService.GetWardTasks());

            if (tasks.Count == 0)
            {
                SendCsr(plr, "No ward task counters are loaded. Apply Database/25_ward_fragment_task_counters.sql and restart.");
                return;
            }

            tasks.Sort((a, b) =>
            {
                int bySigil = a.SigilEntry.CompareTo(b.SigilEntry);
                if (bySigil != 0)
                    return bySigil;

                int byFragment = a.FragmentIndex.CompareTo(b.FragmentIndex);
                return byFragment != 0 ? byFragment : a.TaskNum.CompareTo(b.TaskNum);
            });

            SendCsr(plr, "Ward task counters (" + tasks.Count + "):");

            for (int i = 0; i < tasks.Count; ++i)
            {
                Ward_Fragment_Task task = tasks[i];

                Tok_Info info = task.TokEntry == 0 ? null : TokService.GetTok(task.TokEntry);

                StringBuilder line = new StringBuilder();
                line.Append("  ac ").Append(task.AcId)
                    .Append(" sigil ").Append(task.SigilEntry)
                    .Append(" frag ").Append(task.FragmentIndex)
                    .Append(" task ").Append(task.TaskNum)
                    .Append("  ").Append(plr.TokInterface.GetActionCounter(task.AcId))
                    .Append('/').Append(task.Threshold)
                    .Append("  ").Append(info != null ? info.Name : "(no Tome entry)");

                SendCsr(plr, line.ToString());
            }
        }

        [CommandAttribute(EGmLevel.Developer, "Advances a ward task counter. Usage: .ward add <acId> <amount>")]
        public static void Add(Player plr, int acId, int amount)
        {
            if (plr == null)
                return;

            if (acId <= 0 || acId > ushort.MaxValue || amount <= 0)
            {
                SendCsr(plr, "Usage: .ward add <acId> <amount>. List ids with .ward counters");
                return;
            }

            Ward_Fragment_Task task;
            if (!WardTaskService.TryGetWardTask((ushort)acId, out task))
            {
                SendCsr(plr, "Counter " + acId + " is not a ward fragment task counter.");
                return;
            }

            plr.TokInterface.IncrementWardTaskCounter((ushort)acId, (uint)amount);

            SendCsr(plr, "Counter " + acId + " is now "
                         + plr.TokInterface.GetActionCounter((ushort)acId) + "/" + task.Threshold + ".");
        }

        [CommandAttribute(EGmLevel.Developer, "Fills a ward task counter to its threshold. Usage: .ward complete <acId>")]
        public static void Complete(Player plr, int acId)
        {
            if (plr == null)
                return;

            Ward_Fragment_Task task;
            if (acId <= 0 || acId > ushort.MaxValue || !WardTaskService.TryGetWardTask((ushort)acId, out task))
            {
                SendCsr(plr, "Counter " + acId + " is not a ward fragment task counter.");
                return;
            }

            uint current = plr.TokInterface.GetActionCounter((ushort)acId);

            if (current >= task.Threshold)
            {
                SendCsr(plr, "Counter " + acId + " is already at its threshold.");
                return;
            }

            plr.TokInterface.IncrementWardTaskCounter((ushort)acId, task.Threshold - current);

            SendCsr(plr, "Counter " + acId + " filled to " + task.Threshold
                         + (task.TokEntry == 0 ? " (no Tome entry to award)." : "."));
        }
    }
}
