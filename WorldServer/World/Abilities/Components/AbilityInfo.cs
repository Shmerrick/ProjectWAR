using System;
using System.Collections.Generic;
using Common;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.World.Abilities.Components
{
    public class AbilityInfo
    {
        public AbilityConstants ConstantInfo;
        public List<AbilityCommandInfo> CommandInfo;

        public AbilityInfo()
        {
            
        }

        public AbilityInfo(DBAbilityInfo dbObj)
        {
            Entry = dbObj.Entry;
            CareerLine = dbObj.CareerLine;
            Name = dbObj.Name;
            MinRange = dbObj.MinRange;
            Range = dbObj.Range;
            CastTime = dbObj.CastTime;
            Cooldown = dbObj.Cooldown;
            ApCost = dbObj.ApCost;
            SpecialCost = dbObj.SpecialCost;
            CanCastWhileMoving = dbObj.MoveCast;
            IgnoreCooldownReduction = dbObj.IgnoreCooldownReduction;
            CDcap = dbObj.CDcap;
            VFXTarget = dbObj.VFXTarget;
            abilityID = dbObj.abilityID;
            effectID2 = dbObj.effectID2;
            Time = dbObj.Time;
        }

        public static List<AbilityInfo> Convert(List<DBAbilityInfo> dbObjs)
        {
            List<AbilityInfo> objects = new List<AbilityInfo>();

            foreach (DBAbilityInfo dbObj in dbObjs)
            {
                if (dbObj.AbilityType != (int)AbilityType.Effect)
                    objects.Add(new AbilityInfo(dbObj));
            }

            return objects;
        }

        public AbilityInfo Clone()
        {
            AbilityInfo cAbInfo = (AbilityInfo)MemberwiseClone();
            cAbInfo.CommandInfo = new List<AbilityCommandInfo>();

            return cAbInfo;
        }

        public void AddAbilityCommand(AbilityCommandInfo cmd)
        {
            CommandInfo.Add(cmd);
        }

        public void AppendAbilityCommand(AbilityCommandInfo cmd, byte slot)
        {
            try
            {
                CommandInfo[slot].AddCommandToChain(cmd);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }
            
        }

        public void AppendAbilityCommandWithDamage(AbilityCommandInfo cmd, byte slot)
        {
            CommandInfo[slot].AddCommandToChainWithDamage(cmd);
        }

        public AbilityCommandInfo GetSubcommand(byte slot, byte sequence)
        {
            return CommandInfo[slot].GetSubcommand(sequence);
        }

        public void DeleteCommand(byte slot, byte seq)
        {
            if (seq == 0)
                CommandInfo.RemoveAt(slot);
            else
            {
                AbilityCommandInfo toDelete = CommandInfo[slot].GetSubcommand(seq);
                if (toDelete != null)
                {
                    toDelete.LastCommand.NextCommand = toDelete.NextCommand;
                    if (toDelete.NextCommand != null)
                    {
                        toDelete.NextCommand.LastCommand = toDelete.LastCommand;
                        toDelete.NextCommand = null;
                    }
                    toDelete.LastCommand = null;

                }
            }
        }

        public ushort Entry;

        public uint CareerLine;

        /// <summary>
        /// Represents the basic power of the ability.
        /// </summary>
        public byte Level;

        /// <summary>
        /// Shifter mechanic. If the target is not the caster, the spell will cast with this level instead of Level.
        /// </summary>
        public byte BoostLevel;

        public string Name;

        public byte MinRange;

        public ushort Range;

        public ushort CastTime;

        public ushort Cooldown;

        public byte ApCost;

        /// <summary>
        /// Used for morale and career resource costs
        /// </summary>
        public short SpecialCost;

        public bool CanCastWhileMoving;

        public CommandTargetTypes TargetType { get; set; }
        public float FlightTimeMod = 1f;

        public Unit Instigator;
        public Unit Target;
        public long InvocationTimestamp;
        public Point3D TargetPosition;
        
        /// <summary>
        /// Used for ignoring cd reductions
        /// </summary>
        public ushort IgnoreCooldownReduction;
        /// <summary>
        /// A cap for how much an ability can be reduced to
        /// </summary>
        public ushort CDcap;

        public string VFXTarget;
        public ushort abilityID;
        public ushort effectID2;
        public ushort Time;
    }
}