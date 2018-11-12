using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;

namespace WorldServer.Services.World
{
    [Service]
    public class BountyService : ServiceBase
    {
        public static List<ContributionDefinition> _ContributionDefinitions;

        [LoadingFunction(true)]
        public static void LoadContributionDefinition()
        {
            Log.Debug("WorldMgr", "Loading ContributionManagerInstance Definitions...");
            _ContributionDefinitions = Database.SelectAllObjects<ContributionDefinition>() as List<ContributionDefinition>;
            Log.Success("ContributionDefinition", "Loaded " + _ContributionDefinitions.Count + " ContributionDefinitions");

            if (_ContributionDefinitions.Count == 0)
                Log.Error("Error Loading DB", "No Bounty Contributions Loaded");
        }

        public ContributionDefinition GetDefinition(byte value)
        {
            foreach (var contributionDefinition in _ContributionDefinitions)
            {
                if (contributionDefinition.ContributionId == value)
                    return contributionDefinition;
            }
            return null;
        }
    }

}
