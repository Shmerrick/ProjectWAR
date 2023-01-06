using FrameWork;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorldServer.World.Objects;

namespace WorldServer.Managers.Commands
{
    internal class OcclusionCommands
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static bool LOSTest(Player plr, ref List<string> values)
        {
            Unit unit = plr.CbtInterface.GetCurrentTarget();
            int amt = Convert.ToInt32(values[0]);
            if (unit != null)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                for (int i = 0; i < amt; ++i)
                {
                    plr.LOSHit(unit);
                }
                watch.Stop();

                plr.SendClientMessage($"[LOS CHECK]: {amt} LOS checks passed in {watch.ElapsedMilliseconds}. One LOS check taken {((float)watch.ElapsedMilliseconds / (float)amt)} ms average.");
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}