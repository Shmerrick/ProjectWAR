using GameData;
using NLog;

namespace WorldServer.World.Battlefronts.NewDawn
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

        public override string ToString()
        {
            return
                $"Order VP:{OrderVictoryPoints} ({OrderVictoryPoints * 100 / WorldServer.World.Battlefronts.BattlefrontConstants.LOCK_VICTORY_POINTS}%) Destruction VP:{DestructionVictoryPoints} ({DestructionVictoryPoints * 100 / WorldServer.World.Battlefronts.BattlefrontConstants.LOCK_VICTORY_POINTS}%)";
        }

        public void Lock(Realms lockingRealm)
        {
            _logger.Debug($"Locking Realm : {lockingRealm}");
            if (lockingRealm == Realms.REALMS_REALM_DESTRUCTION)
            {
                DestructionVictoryPoints = BattlefrontConstants.LOCK_VICTORY_POINTS;
                OrderVictoryPoints = 0;
            }
            if (lockingRealm == Realms.REALMS_REALM_ORDER)
            {
                OrderVictoryPoints = BattlefrontConstants.LOCK_VICTORY_POINTS;
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
        public void Reset(NewDawnBattlefront battlefront)
        {
            _logger.Debug($"Resetting battlefront {battlefront.ActiveZoneName} to Neutral");
            OrderVictoryPoints = 0;
            DestructionVictoryPoints = 0;
        }
    }
}