using GameData;
using NLog;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class VictoryPointProgress
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Object thisLock = new Object();
        private float _dVP;
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
    }
}