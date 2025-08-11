using System;
using System.Collections.Generic;
using System.Linq;
using FrameWork;
using WorldServer.Configs;
using WorldServer.World.Objects;
using WorldServer.Services.World;

namespace WorldServer.World.Scripting.Dungeons
{
    public class AltdorfSiege : InstanceScript
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private int _stage = 0;
        private long _stageTimer;
        private GameObject _palaceDoor;
        private List<BattlefieldObjective> _stage1Objectives = new List<BattlefieldObjective>();
        private List<Creature> _warlords = new List<Creature>();
        private Creature _kingKarlFranz;
        private List<Player> _champions = new List<Player>();

        #region Stage 1: The Assault

        private void StartStage1()
        {
            _stage = 1;
            Logger.Info("AltdorfSiege", "Instance script starting Stage 1.");
            Instance.SendInstanceMessageToAll("Stage 1 has begun! Destroy the Palace Door!");

            // Set the timer for Stage 1
            _stageTimer = TCPManager.GetTimeStamp() + (Core.CitySiegeConfig.Stage1DurationMinutes * 60);

            // Spawn the main palace door
            // TODO: Get correct GameObject entry and spawn coordinates for the door.
            uint doorEntry = 12345;
            var doorProto = GameObjectService.GetGameObjectProto(doorEntry);
            if (doorProto != null)
            {
                GameObject_spawn spawn = new GameObject_spawn { Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(), ZoneId = Instance.ZoneID };
                spawn.BuildFromProto(doorProto);
                // TODO: Set correct coordinates
                _palaceDoor = Instance.Region.CreateGameObject(spawn);
                Logger.Info("AltdorfSiege", "Palace door has been spawned.");
            }

            // Spawn Stage 1 Objectives
            // TODO: Get correct BattlefieldObjective entries from database
            // Example:
            // var objectiveProto = BattlefieldObjectiveService.GetObjective(1);
            // var bo = new BattlefieldObjective(objectiveProto);
            // Instance.Region.AddObject(bo, Instance.ZoneID);
            // _stage1Objectives.Add(bo);
        }

        private void UpdateStage1()
        {
            if (_stageTimer < TCPManager.GetTimeStamp())
            {
                EndStage1(Realms.REALMS_REALM_ORDER); // Defenders win by timeout
                return;
            }

            if (_palaceDoor != null && _palaceDoor.IsDead)
            {
                EndStage1(Realms.REALMS_REALM_DESTRUCTION); // Attackers win by destroying door
                return;
            }

            // TODO: Check for objective captures and trigger special events
            // (e.g., spawn Rock Lobbas, Lord of Change)
        }

        private void EndStage1(Realms winningRealm)
        {
            Logger.Info("AltdorfSiege", $"Stage 1 has ended. Winner: {winningRealm}");
            DistributeRewards(1, winningRealm);
            StartTransition(2, winningRealm);
        }

        #endregion

        #region Stage 2: Warlords

        private void StartStage2(Realms stage1Winner)
        {
            _stage = 2;
            Logger.Info("AltdorfSiege", "Instance script starting Stage 2.");
            Instance.SendInstanceMessageToAll("Stage 2 has begun! Escort your Warlord to victory!");
            _stageTimer = TCPManager.GetTimeStamp() + (Core.CitySiegeConfig.Stage2DurationMinutes * 60);

            // TODO: Spawn Warlords for each realm based on stage1Winner
        }

        private void UpdateStage2()
        {
            if (_stageTimer < TCPManager.GetTimeStamp())
            {
                // TODO: Determine winner by Warlord positions
                EndStage2(Realms.REALMS_REALM_NEUTRAL);
                return;
            }

            // TODO: Check if a Warlord has reached its destination or if both are dead
        }

        private void EndStage2(Realms winningRealm)
        {
            Logger.Info("AltdorfSiege", $"Stage 2 has ended. Winner: {winningRealm}");
            DistributeRewards(2, winningRealm);
            StartTransition(3, winningRealm);
        }

        #endregion

        #region Stage 3: The King

        private void StartStage3(Realms stage2Winner)
        {
            _stage = 3;
            Logger.Info("AltdorfSiege", "Instance script starting Stage 3.");
            Instance.SendInstanceMessageToAll("Stage 3 has begun! Defeat the enemy King!");
            _stageTimer = TCPManager.GetTimeStamp() + (Core.CitySiegeConfig.Stage3DurationMinutes * 60);

            // TODO: Spawn King Karl Franz
            // _kingKarlFranz = ...
            // _kingKarlFranz.IsInvulnerable = true;

            // Select Champions
            var enemyPlayers = (CitySiegeService.AttackingRealm == Realms.REALMS_REALM_ORDER) ? Instance.DestructionPlayers : Instance.OrderPlayers;
            _champions = enemyPlayers.OrderBy(x => Guid.NewGuid()).Take(4).ToList();

            Instance.SendInstanceMessageToAll($"King Karl Franz has chosen his Champions: {string.Join(", ", _champions.Select(c => c.Name))}");
            // TODO: Apply a special buff to champions
        }

        private void UpdateStage3()
        {
            if (_stageTimer < TCPManager.GetTimeStamp())
            {
                EndStage3(Realms.REALMS_REALM_ORDER); // Defenders win
                return;
            }

            // Check if champions are dead
            if (_kingKarlFranz.IsInvulnerable)
            {
                _champions.RemoveAll(c => c.IsDead || !c.IsInWorld());
                if (_champions.Count == 0)
                {
                    _kingKarlFranz.IsInvulnerable = false;
                    Instance.SendInstanceMessageToAll("The Champions have fallen! The King is vulnerable!");
                }
            }

            if (_kingKarlFranz.IsDead)
            {
                EndStage3(CitySiegeService.AttackingRealm); // Attackers win
            }
        }

        private void EndStage3(Realms winningRealm)
        {
            Logger.Info("AltdorfSiege", $"Stage 3 has ended. Winner: {winningRealm}");
            DistributeRewards(3, winningRealm);
            EndSiege();
        }

        #endregion

        #region Core Loop

        public override void OnInstanceLoad()
        {
            StartStage1();
        }

        public override void Update(long tick)
        {
            switch (_stage)
            {
                case 1:
                    UpdateStage1();
                    break;
                case 2:
                    UpdateStage2();
                    break;
                case 3:
                    UpdateStage3();
                    break;
            }
        }

        private void StartTransition(int nextStage, Realms lastStageWinner)
        {
            _stage = -1; // Transitioning stage
            Instance.SendInstanceMessageToAll($"Stage ended. Next stage will begin in {Core.CitySiegeConfig.TransitionDurationSeconds} seconds.");
            EvtInterface.AddEvent(() =>
            {
                if (nextStage == 2) StartStage2(lastStageWinner);
                else if (nextStage == 3) StartStage3(lastStageWinner);
            }, Core.CitySiegeConfig.TransitionDurationSeconds * 1000, 1);
        }

        private void DistributeRewards(int stage, Realms winningRealm)
        {
            // This needs a proper RewardManager instance, but we create it here for now.
            var rewardManager = new World.Battlefronts.Bounty.RewardManager(null, null, null, null);

            foreach (var player in Instance.Players)
            {
                if (player != null && !player.IsDisposed)
                {
                    bool playerWon = player.Realm == winningRealm;
                    rewardManager.DistributeCitySiegeStageReward(player, stage, playerWon, CitySiegeService.CityRating);
                }
            }
        }

        private void EndSiege()
        {
            Instance.SendInstanceMessageToAll("The Siege of Altdorf is over!");
            CitySiegeService.EndSiege();

            foreach (var player in Instance.Players)
            {
                if (player != null && !player.IsDisposed)
                {
                    player.TeleportToCapital();
                }
            }
            Instance.Script = null;
        }

        public override void OnPlayerEnter(Player player)
        {
            player.SendClientMessage("Welcome to the Siege of Altdorf!", ChatLogFilters.CHATLOGFILTERS_RVR);
        }

        #endregion
    }
}
