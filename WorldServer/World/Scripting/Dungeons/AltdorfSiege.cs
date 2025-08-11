using System;
using System.Collections.Generic;
using FrameWork;
using WorldServer.World.Objects;
using WorldServer.Services.World;

namespace WorldServer.World.Scripting.Dungeons
{
    public class AltdorfSiege : InstanceScript
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private long _siegeTimer;
        private GameObject _palaceDoor;
        private const int SIEGE_DURATION_SECONDS = 45 * 60; // 45 minutes

        public override void OnInstanceLoad()
        {
            Logger.Info("AltdorfSiege", "Instance script loaded. Starting Stage 1.");

            // Set the timer for Stage 1
            _siegeTimer = TCPManager.GetTimeStamp() + SIEGE_DURATION_SECONDS;

            // Spawn the main palace door
            // Note: We need the correct GameObject entry and spawn coordinates for the door.
            // These values are placeholders.
            uint doorEntry = 12345; // Placeholder entry for the palace door
            var doorProto = GameObjectService.GetGameObjectProto(doorEntry);
            if (doorProto != null)
            {
                GameObject_spawn spawn = new GameObject_spawn
                {
                    Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                    WorldX = 0, // Placeholder coordinates
                    WorldY = 0,
                    WorldZ = 0,
                    WorldO = 0,
                    ZoneId = Instance.ZoneID
                };
                spawn.BuildFromProto(doorProto);
                _palaceDoor = Instance.Region.CreateGameObject(spawn);
                Logger.Info("AltdorfSiege", "Palace door has been spawned.");
            }
            else
            {
                Logger.Error("AltdorfSiege", $"Could not find GameObject prototype for entry {doorEntry}");
            }

            // TODO: Spawn the Battlefield Objectives for Stage 1
        }

        public override void Update(long tick)
        {
            // Check if the siege has ended (either by timer or door destruction)
            if (_siegeTimer < TCPManager.GetTimeStamp())
            {
                // Defenders win
                Logger.Info("AltdorfSiege", "Stage 1 timer has expired. Defenders win!");
                EndStage1(Realms.REALMS_REALM_ORDER);
            }

            if (_palaceDoor != null && _palaceDoor.IsDead)
            {
                // Attackers win
                Logger.Info("AltdorfSiege", "Palace door has been destroyed. Attackers win!");
                EndStage1(Realms.REALMS_REALM_DESTRUCTION);
            }
        }

        private void EndStage1(Realms winningRealm)
        {
            Logger.Info("AltdorfSiege", $"Stage 1 has ended. Winner: {winningRealm}");

            var rewardManager = new World.Battlefronts.Bounty.RewardManager(null, null, null, null);

            foreach (var player in Instance.Players)
            {
                if (player != null && !player.IsDisposed)
                {
                    bool playerWon = player.Realm == winningRealm;
                    rewardManager.DistributeCitySiegeStageReward(player, 1, playerWon, CitySiegeService.CityRating);
                }
            }

            // For now, we will end the siege entirely after stage 1.
            // Later, we will implement the transition to Stage 2.
            CitySiegeService.EndSiege();

            // Teleport all players out
            foreach (var player in Instance.Players)
            {
                if (player != null && !player.IsDisposed)
                {
                    // Teleport player to their capital city
                    player.TeleportToCapital();
                }
            }

            // End the script
            Instance.Script = null;
        }

        public override void OnPlayerEnter(Player player)
        {
            player.SendClientMessage("Welcome to the Siege of Altdorf! Stage 1: Destroy the Palace Door!", ChatLogFilters.CHATLOGFILTERS_RVR);
        }
    }
}
