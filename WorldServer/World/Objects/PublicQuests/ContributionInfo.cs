using Common;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer.World.Objects.PublicQuests
{
    /// <summary>
    /// Entity regrouping a player's contributon to a public quest or a Campaign.
    /// </summary>
    public class ContributionInfo
    {
        public uint PlayerCharId;
        public string PlayerName;
        public Realms PlayerRealm;
        public byte PlayerCareerLine;
        public byte PlayerLevel;

        /// <summary>
        /// The numerical contribution a player made to this PQ.
        /// </summary>
        public uint BaseContribution;

        /// <summary>
        /// A random factor for the roll.
        /// </summary>
        public uint RandomBonus;
        /// <summary>
        /// A bonus to the roll based on consecutive PQs completed without success.
        /// </summary>
        public uint PersistenceBonus;
        /// <summary>
        /// A bonus to the roll derived from the player's relative ContributionManagerInstance value.
        /// </summary>
        public uint ContributionBonus;

        public uint HealingDamagePool { get; set; }
        public uint HealingContribPool { get; set; }

        /// <summary>
        /// ContributionManagerInstance that will be split on the next PQ tick.
        /// </summary>
        public uint PendingContribution { get; set; }

        public byte OptOutType { get; set; }

        /// <summary>
        /// Indicates continued activity, granting contribution from the PQ ticker.
        /// </summary>
        public long ActiveTimeEnd;

        /// <summary>
        /// The type of bag won.
        /// </summary>
        public byte BagWon;

        public class HitInfo
        {
            public uint NumHits;
            public uint DamageTaken;

            public HitInfo(uint damage)
            {
                NumHits = 1;
                DamageTaken = damage;
            }
        }

        /// <summary>
        /// Information regarding the number of hits, and damage, that this player took from the NPC with given OID.
        /// </summary>
        public readonly Dictionary<ushort, HitInfo> DamageTakenFrom = new Dictionary<ushort, HitInfo>();

        public ContributionInfo(Player player)
        {
            PlayerCharId = player.CharacterId;
            PlayerRealm = player.Realm;
            PlayerCareerLine = player.Info.CareerLine;
            PlayerLevel = player.Level;
            PlayerName = player.Name;
        }

        public void Reset(bool pqCompleted)
        {
            RandomBonus = 0;
            if (pqCompleted && BaseContribution > 0 && BagWon == 0)
                PersistenceBonus += 100;
            else
                PersistenceBonus = 0;
            BaseContribution = 0;
            BagWon = 0;
            ContributionBonus = 0;
            HealingDamagePool = 0;
            HealingContribPool = 0;
            DamageTakenFrom.Clear();
        }
    }
}
