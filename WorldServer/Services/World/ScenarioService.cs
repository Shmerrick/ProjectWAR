using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer.Services.World
{
    [Service]
    class ScenarioService : ServiceBase
    {
        public static List<Scenario_Info> Scenarios;
        public static List<Scenario_Info> ActiveScenarios;

        [LoadingFunction(true)]
        public static void LoadScenarioInfo()
        {
            Log.Debug("WorldMgr", "Loading Scenario_Info...");

            Scenarios = new List<Scenario_Info>();
            IList<Scenario_Info> infos = Database.SelectAllObjects<Scenario_Info>();
            if (infos != null)
                Scenarios.AddRange(infos);

            ActiveScenarios = new List<Scenario_Info>();

            foreach (var info in Scenarios)
            {
                IList<Scenario_Object> scenObjects =
                    Database.SelectObjects<Scenario_Object>("ScenarioId = " + info.ScenarioId);

                foreach (var obj in scenObjects)
                    info.ScenObjects.Add(obj);

                if (info.Enabled > 0)
                    ActiveScenarios.Add(info);
            }

            Log.Success("Scenario_Info", "Loaded " + Scenarios.Count + " Scenario_Info");
        }

        public static Scenario_Info GetScenario_Info(ushort ScenarioId)
        {
            foreach (Scenario_Info scenario in Scenarios)
                if (scenario != null && scenario.ScenarioId == ScenarioId)
                    return scenario;
            return null;
        }

    }
}
