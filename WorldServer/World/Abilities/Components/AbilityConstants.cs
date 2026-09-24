using Common;
using GameData;
using System.Collections.Generic;

namespace WorldServer.World.Abilities.Components
{
    public class AbilityConstants
    {
        public AbilityConstants()
        {
        }

        public AbilityConstants(DBAbilityInfo dbObj)
        {
            Entry = dbObj.Entry;
            CareerLine = dbObj.CareerLine;
            Name = dbObj.Name;
            MinimumRank = dbObj.MinimumRank;
            MinimumRenown = dbObj.MinimumRenown;
            MasteryTree = dbObj.MasteryTree;
            PointCost = dbObj.PointCost;
            CashCost = dbObj.CashCost;
            AbilityType = (AbilityType)dbObj.AbilityType;
            if (dbObj.Specline == "Item")
                Origin = AbilityOrigin.AO_ITEM;
            ChannelID = dbObj.ChannelID;
            ChannelDuration = dbObj.ChannelDuration;
            ChannelInterval = dbObj.ChannelInterval;
            CastAngle = dbObj.CastAngle;
            EffectID = dbObj.EffectID;
            WeaponNeeded = (WeaponRequirements)dbObj.WeaponNeeded;
            InvokeDelay = dbObj.InvokeDelay;
            EffectDelay = dbObj.EffectDelay;
            IgnoreGlobalCooldown = dbObj.IgnoreGlobalCooldown;
            AffectsDead = dbObj.AffectsDead;
            StealthInteraction = (AbilityStealthType)dbObj.StealthInteraction;
            Fragile = dbObj.Fragile;
            CooldownEntry = dbObj.CooldownEntry;
            ToggleEntry = dbObj.ToggleEntry;
            IgnoreOwnModifiers = dbObj.IgnoreOwnModifiers;
            BaseCastTime = dbObj.CastTime;
            AIRange = dbObj.AIRange;
            ClientTargetType = dbObj.TargetType;
        }

        public static List<AbilityConstants> Convert(List<DBAbilityInfo> dbObjs)
        {
            List<AbilityConstants> objects = new List<AbilityConstants>();

            foreach (DBAbilityInfo dbObj in dbObjs)
                objects.Add(new AbilityConstants(dbObj));

            return objects;
        }

        public ushort GetDelayFor(ushort range)
        {
            return EffectDelay >= 0 ? (ushort)(EffectDelay * (range * 0.01f)) : (ushort)(-EffectDelay);
        }

        public ushort Entry;
        public uint CareerLine;

        public string Name;

        public byte MinimumRank;
        public byte MinimumRenown;

        public byte MasteryTree;

        public byte PointCost;
        public uint CashCost;

        public AbilityType AbilityType;

        public AbilityOrigin Origin = AbilityOrigin.AO_STANDARD;

        public ushort ChannelID;

        /// <summary>How long a channel lasts, in milliseconds (the client's component duration).</summary>
        public uint ChannelDuration;

        /// <summary>How often a channel spends its AP, in milliseconds; 0 means once a second.</summary>
        public ushort ChannelInterval;

        public ushort CastAngle;

        public ushort EffectID;

        public WeaponRequirements WeaponNeeded;

        public ushort InvokeDelay;

        /// <summary>
        /// <para>The abs value of this is the delay (in ms) required before commands with IsDelayedEffect will execute.</para>
        /// <para>If positive, the value will be multiplied by (Range/100).</para>
        /// <para>If negative, the absolute value is taken with no further adjustment.</para>
        /// </summary>
        public short EffectDelay;

        public bool IgnoreGlobalCooldown;

        public bool AffectsDead;

        public AbilityStealthType StealthInteraction;

        public byte Fragile;

        public ushort CooldownEntry;
        public ushort ToggleEntry;

        public bool IgnoreOwnModifiers;

        public bool IsHealing { get; set; }
        public bool IsDamaging { get; set; }

        public ushort BaseCastTime;

        public ushort AIRange;

        /// <summary>
        /// The client's TargetType (data/bin/abilityexport.bin); null where the client has no record of the
        /// ability. Read through <c>AbilityInfo.TargetsCaster</c>.
        /// </summary>
        public byte? ClientTargetType;
    }
}