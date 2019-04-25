using GameData;
using NLog;
using WorldServer.World.Battlefronts.Objectives;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class VictoryPointProgress
    {

        public const int MAX_NUMBER_PLAYER_KILLS = 25;
        public const int MAX_NUMBER_SCENARIO_WINS = 6;

        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Object thisLock = new Object();
        private float _dVP;
        public int NumberDestructionPlayerKills { get; set; }
        public int NumberOrderPlayerKills { get; set; }
        public int NumberDestructionScenarioWins { get; set; }
        public int NumberOrderScenarioWins { get; set; }
        public int DestructionVictoryDominationCount { get; set; }
        public int OrderVictoryDominationCount { get; set; }

        public float DestructionVictoryPoints
        {
            get
            {
                lock (thisLock)
                {
                    return _dVP;
                }
            }
            set
            {
                lock (thisLock)
                {
                    _dVP = value;
                }
            }
        }

        private float _oVP;
        public float OrderVictoryPoints
        {
            get
            {
                lock (thisLock)
                {
                    return _oVP;
                }
            }
            set
            {
                lock (thisLock)
                {
                    _oVP = value;
                }
            }
        }

        public VictoryPointProgress()
        {
            OrderVictoryPoints = 0;
            DestructionVictoryPoints = 0;
            NumberDestructionPlayerKills = 0;
            NumberOrderPlayerKills = 0;
            NumberDestructionScenarioWins = 0;
            NumberOrderScenarioWins = 0;
            DestructionVictoryDominationCount = 0;
            OrderVictoryDominationCount = 0;
        }

        public VictoryPointProgress(float orderVP, float destroVP)
        {
            OrderVictoryPoints = orderVP;
            DestructionVictoryPoints = destroVP;
        }

        public float DestructionVictoryPointPercentage
        {
            get { return DestructionVictoryPoints * 100 / BattleFrontConstants.LOCK_VICTORY_POINTS; }
        }

        public float OrderVictoryPointPercentage
        {
            get { return OrderVictoryPoints * 100 / BattleFrontConstants.LOCK_VICTORY_POINTS; }
        }

        public void AddPlayerKill(Realms killerRealm)
        {
            if (killerRealm == Realms.REALMS_REALM_ORDER)
            {
                if (NumberOrderPlayerKills <= MAX_NUMBER_PLAYER_KILLS)
                {
                    NumberOrderPlayerKills++;
                }
            }

            if (killerRealm == Realms.REALMS_REALM_DESTRUCTION)
            {
                if (NumberDestructionPlayerKills <= MAX_NUMBER_PLAYER_KILLS)
                {
                    NumberDestructionPlayerKills++;
                }
            }
        }

        public void AddScenarioWin(Realms winningRealm)
        {
            if (winningRealm == Realms.REALMS_REALM_ORDER)
            {
                if (NumberOrderScenarioWins <= MAX_NUMBER_SCENARIO_WINS)
                {
                    NumberOrderScenarioWins++;
                }
            }

            if (winningRealm == Realms.REALMS_REALM_DESTRUCTION)
            {
                if (NumberDestructionScenarioWins <= MAX_NUMBER_SCENARIO_WINS)
                {
                    NumberDestructionScenarioWins++;
                }
            }
        }

        public override string ToString()
        {
            return
                $"Order VP:{OrderVictoryPoints} ({OrderVictoryPointPercentage}%) Destruction VP:{DestructionVictoryPoints} ({DestructionVictoryPointPercentage}%)";
        }

        public void Lock(Realms lockingRealm)
        {
            _logger.Debug($"Locking Realm : {lockingRealm}");
            if (lockingRealm == Realms.REALMS_REALM_DESTRUCTION)
            {
                DestructionVictoryPoints = BattleFrontConstants.LOCK_VICTORY_POINTS;
                OrderVictoryPoints = 0;
            }
            if (lockingRealm == Realms.REALMS_REALM_ORDER)
            {
                OrderVictoryPoints = BattleFrontConstants.LOCK_VICTORY_POINTS;
                DestructionVictoryPoints = 0;
            }
            if (lockingRealm == Realms.REALMS_REALM_NEUTRAL)
            {
                OrderVictoryPoints = 0;
                DestructionVictoryPoints = 0;
            }

        }

        /// <summary>
        /// Reset the realm to be owned by Neutral. 
        /// </summary>
        public void Reset(Campaign BattleFront)
        {
            _logger.Debug($"Resetting Campaign VP {BattleFront.ActiveCampaignName} to Neutral");
            OrderVictoryPoints = 0;
            DestructionVictoryPoints = 0;
        }

        //public void AddKeepTake(Realms attackingRealm)
        //{
        //    _logger.Debug($"AddKeepTake {attackingRealm} ");
        //    if (attackingRealm == Realms.REALMS_REALM_ORDER)
        //    {
        //        OrderVictoryPoints += 300;
        //    }
        //    else
        //    {
        //        DestructionVictoryPoints += 300;
        //    }
        //}

        //public void KeepLost(Realms losingRealm)
        //{
        //    _logger.Debug($"KeepLost {losingRealm} ");
        //    if (losingRealm == Realms.REALMS_REALM_ORDER)
        //    {
        //        OrderVictoryPoints -= 300;
        //    }
        //    else
        //    {
        //        DestructionVictoryPoints -= 300;
        //    }
        //}

        //public void BOWon(Realms attackingRealm, string name)
        //{
        //    _logger.Debug($"BOWon {attackingRealm} {name}");
        //    if (attackingRealm == Realms.REALMS_REALM_ORDER)
        //    {
        //        OrderVictoryPoints += 50;
        //    }
        //    else
        //    {
        //        DestructionVictoryPoints += 50;
        //    }
        //}

        //public void BOLost(Realms losingRealm, string name)
        //{
        //    _logger.Debug($"BOLost {losingRealm} {name}");
        //    if (losingRealm == Realms.REALMS_REALM_ORDER)
        //    {
        //        OrderVictoryPoints -= 50;
        //    }
        //    else
        //    {
        //        DestructionVictoryPoints -= 50;
        //    }
        //}


        public void UpdateStatus(Campaign campaign)
        {
            DestructionVictoryPoints = 0;
            OrderVictoryPoints = 0;
            DestructionVictoryDominationCount = 0;
            OrderVictoryDominationCount = 0;

            foreach (var battleFrontKeep in campaign.Keeps)
            {
                foreach (var keep in campaign.ActiveBattleFrontStatus.KeepList)
                {
                    if (keep.KeepId == battleFrontKeep.Info.KeepId)
                    {
                        if (battleFrontKeep.KeepStatus == KeepStatus.KEEPSTATUS_SAFE || battleFrontKeep.KeepStatus == KeepStatus.KEEPSTATUS_LOCKED)
                        {
                            // Add Keep VPP
                            if (battleFrontKeep.Realm == Realms.REALMS_REALM_DESTRUCTION)
                            {
                                DestructionVictoryPoints += 300;
                                DestructionVictoryDominationCount++;
                            }
                            if (battleFrontKeep.Realm == Realms.REALMS_REALM_ORDER)
                            {
                                OrderVictoryPoints += 300;
                                OrderVictoryDominationCount++;
                            }
                        }

                    }
                }
            }

            foreach (var campaignObjective in campaign.Objectives)
            {
                foreach (var bo in campaign.ActiveBattleFrontStatus.BattlefieldObjectives)
                {
                    if (campaignObjective.State == StateFlags.Secure || campaignObjective.State == StateFlags.Locked)
                    {
                        if (bo.Entry == campaignObjective.Id)
                        {
                            if (campaignObjective.OwningRealm == Realms.REALMS_REALM_DESTRUCTION)
                            {
                                DestructionVictoryPoints += 50;
                                DestructionVictoryDominationCount++;
                            }
                            if (campaignObjective.OwningRealm == Realms.REALMS_REALM_ORDER)
                            {
                                OrderVictoryPoints += 50;
                                OrderVictoryDominationCount++;
                            }
                        }
                    }
                }
            }

            OrderVictoryPoints += NumberOrderScenarioWins * 25;
            DestructionVictoryPoints += NumberDestructionScenarioWins * 25;

            OrderVictoryPoints += NumberOrderPlayerKills * 2;
            DestructionVictoryPoints += NumberDestructionPlayerKills * 2;

            if ((OrderVictoryPoints == 0) && (DestructionVictoryPoints == 0))
            {
                _logger.Trace($"VPP {OrderVictoryPoints}/{DestructionVictoryPoints}");
            }
            else
            {
                _logger.Info($"VPP {OrderVictoryPoints}/{DestructionVictoryPoints}");
            }
        }


        public int GetDominationCount(Realms realm)
        {
            if (realm == Realms.REALMS_REALM_DESTRUCTION)
            {
                return DestructionVictoryDominationCount;
            }
            if (realm == Realms.REALMS_REALM_ORDER)
            {
                return OrderVictoryDominationCount;
            }
            return 0;
        }
    }
}