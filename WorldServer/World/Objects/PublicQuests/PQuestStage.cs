using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer.World.Objects.PublicQuests
{
    public class PQuestStage
    {
        public int Number;
        public string StageName;
        public string Description;
        public ushort Time;
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
