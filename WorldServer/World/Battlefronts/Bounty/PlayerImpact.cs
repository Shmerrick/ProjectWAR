using System;

namespace WorldServer.World.Battlefronts.Bounty
{
    public class PlayerImpact :ICloneable
    {
        public const int MAX_IMPACT_VALUE = 10000;
        public const int MAX_MODIFICATION_VALUE = 5;
        

        // The amount of damage or heal upon a target
        public int ImpactValue { get; set; }
        // The timestamp in secs since Epoch that this impact will expire
        public int ExpiryTimestamp { get; set; }
        // The modification value (Source to Target) to apply if the Impact rewards
        public float ModificationValue { get; set; }
        // The player that performed this impact action
        public uint CharacterId { get; set; }

        public override string ToString()
        {
            return $" Character Id {CharacterId} " +
                   $"ImpactValue {ImpactValue}, " +
                   $"Expiry {ExpiryTimestamp} " +
                   $"Modification {ModificationValue} ";
        }

        /// <summary>
        /// Set the impact values, but ensure they make sense.
        /// </summary>
        /// <param name="impactValue"></param>
        /// <param name="expiryTimestamp"></param>
        /// <param name="modificationValue"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public PlayerImpact SetImpact(int impactValue, int expiryTimestamp, float modificationValue, uint impactingCharacterId)
        {
            
            if (impactValue > MAX_IMPACT_VALUE)
                impactValue = MAX_IMPACT_VALUE;
            ImpactValue = impactValue;

            if (expiryTimestamp < FrameWork.TCPManager.GetTimeStamp())
                expiryTimestamp = FrameWork.TCPManager.GetTimeStamp();
            ExpiryTimestamp = expiryTimestamp;

            if (modificationValue > MAX_MODIFICATION_VALUE)
                ModificationValue = MAX_MODIFICATION_VALUE;
            ModificationValue = modificationValue;

            this.CharacterId = impactingCharacterId;

            return this;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}