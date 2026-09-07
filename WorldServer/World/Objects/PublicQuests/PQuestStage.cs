using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer.World.Objects.PublicQuests
{
    public class PQuestStage
    {
        public int Number;
        public ushort StageId;
        public string StageName;

        /// <summary>
        /// Long tracker-header title. F_OBJECTIVE_INFO carries it alongside the short
        /// <see cref="StageName"/> label. Falls back to StageName when the row has none.
        /// </summary>
        public string StageTitle;

        public string Description;
        public ushort Time;

        /// <summary>
        /// True when the stage runs with no countdown and no fail timer. See
        /// <see cref="Common.PQuest_Objective.NoStageTimer"/>.
        /// </summary>
        public bool NoTimer;
        public List<PQuestObjective> Objectives = new List<PQuestObjective>();

        public void AddObjective(PQuestObjective Objective)
        {
            Objectives.Add(Objective);
        }

        public void Reset()
        {
            foreach (PQuestObjective Obj in Objectives)
            {
                    Obj.Count = 0;
                    Obj.Reset();
            }
        }

        public bool IsDone()
        {
            bool done = true;
            foreach (PQuestObjective Obj in Objectives)
            {
                if (!Obj.IsDone())
                    done = false;
            }

            return done;
        }

        public void Cleanup()
        {
            foreach (PQuestObjective Obj in Objectives)
            {
                Obj.Cleanup();
            }
        }
    }
}
