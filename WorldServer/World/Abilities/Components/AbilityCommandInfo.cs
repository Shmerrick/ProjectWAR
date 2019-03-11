using System;
using System.Collections.Generic;
using Common;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.Components
{
    public class AbilityCommandInfo
    {
        /// <summary>
        /// A storage field for the result of this command's execution.
        /// </summary>
        public short CommandResult;

        public AbilityCommandInfo LastCommand, NextCommand;

        /// <summary>
        /// Optional information about the damage or healing that this command can cause.
        /// </summary>
        public AbilityDamageInfo DamageInfo;

        public AbilityCommandInfo()
        {
        }

        public AbilityCommandInfo(DBAbilityCommandInfo dbObj)
        {
            Entry = dbObj.Entry;
            AbilityName = dbObj.AbilityName;
            CommandID = dbObj.CommandID;
            CommandSequence = dbObj.CommandSequence;
            CommandName = dbObj.CommandName;
            PrimaryValue = dbObj.PrimaryValue;
            SecondaryValue = dbObj.SecondaryValue;
            EffectRadius = dbObj.EffectRadius;
            EffectAngle = dbObj.EffectAngle;
            if (!string.IsNullOrEmpty(dbObj.EffectSource))
                AoESource = (CommandTargetTypes)Enum.Parse(typeof(CommandTargetTypes), dbObj.EffectSource);
            if (!string.IsNullOrEmpty(dbObj.Target))
                TargetType = (CommandTargetTypes)Enum.Parse(typeof(CommandTargetTypes), dbObj.Target);
            MaxTargets = dbObj.MaxTargets;
            AttackingStat = dbObj.AttackingStat;
            IsDelayedEffect = dbObj.IsDelayedEffect;
            FromAllTargets = dbObj.FromAllTargets;
            NoAutoUse = dbObj.NoAutoUse;
        }

        /// <summary>
        /// Converts a list containing the database representations of this class to the server versions.
        /// </summary>
        public static List<AbilityCommandInfo> Convert(List<DBAbilityCommandInfo> dbObjs)
        {
            List<AbilityCommandInfo> objects = new List<AbilityCommandInfo>();

            foreach (DBAbilityCommandInfo dbObj in dbObjs)
                objects.Add(new AbilityCommandInfo(dbObj));

            return objects;
        }

        public void AddCommandToChain(AbilityCommandInfo slaveCommand)
        {
            if (CommandSequence == 0)
            {
                if (NextCommand == null)
                {
                    NextCommand = slaveCommand;
                    slaveCommand.LastCommand = this;
                }
                else NextCommand.AddCommandToChain(slaveCommand);
            }
            else
            {
                if (slaveCommand.CommandSequence < CommandSequence)
                {
                    LastCommand.NextCommand = slaveCommand;
                    slaveCommand.LastCommand = LastCommand;
                    LastCommand = slaveCommand;
                    slaveCommand.NextCommand = this;
                }

                else if (NextCommand == null)
                {
                    NextCommand = slaveCommand;
                    slaveCommand.LastCommand = this;
                }
                else NextCommand.AddCommandToChain(slaveCommand);
            }
        }

        /// <summary>
        /// Adds a command to the command sequence, linking the previous command's damage information.
        /// </summary>
        public void AddCommandToChainWithDamage(AbilityCommandInfo slaveCommand)
        {
            if (CommandSequence == 0)
            {
                if (NextCommand == null || slaveCommand.CommandSequence == CommandSequence)
                {
                    NextCommand = slaveCommand;
                    slaveCommand.LastCommand = this;
                    if (DamageInfo != null)
                        slaveCommand.DamageInfo = DamageInfo.Clone();
                    else
                        slaveCommand.DamageInfo = LastCommand.DamageInfo.Clone();
                }
                else NextCommand.AddCommandToChainWithDamage(slaveCommand);
            }
            else
            {
                if (slaveCommand.CommandSequence < CommandSequence)
                {
                    LastCommand.NextCommand = slaveCommand;
                    slaveCommand.LastCommand = LastCommand;
                    LastCommand = slaveCommand;
                    slaveCommand.NextCommand = this;

                    if (slaveCommand.LastCommand.DamageInfo != null)
                        slaveCommand.DamageInfo = slaveCommand.LastCommand.DamageInfo.Clone();
                }

                else if (NextCommand == null)
                {
                    NextCommand = slaveCommand;
                    slaveCommand.LastCommand = this;

                    if (DamageInfo != null)
                        slaveCommand.DamageInfo = DamageInfo.Clone();
                    else
                        slaveCommand.DamageInfo = LastCommand.DamageInfo.Clone();
                }
                else NextCommand.AddCommandToChainWithDamage(slaveCommand);
            }
        }

        /// <summary>
        /// Returns the command in this chain with CommandSequence corresponding to the input value.
        /// </summary>
        public AbilityCommandInfo GetSubcommand(byte commandSeq)
        {
            if (CommandSequence == commandSeq)
                return this;
            if (NextCommand == null)
                return null;
            return NextCommand.GetSubcommand(commandSeq);
        }

        public AbilityCommandInfo Clone(Unit caster)
        {
            AbilityCommandInfo cCommandInfo = (AbilityCommandInfo)MemberwiseClone();
            cCommandInfo.CommandName = (string)CommandName.Clone();

            cCommandInfo.LastCommand = null;
            cCommandInfo.NextCommand = null;

            if (DamageInfo != null)
                cCommandInfo.DamageInfo = DamageInfo.Clone(caster);

            return cCommandInfo;
        }

        /// <summary>
        /// The ID of the ability of which this command is a part.
        /// </summary>
        public ushort Entry;

        /// <summary>
        /// The name of the ability of which this command is a part.
        /// </summary>
        public string AbilityName;

        /// <summary>
        /// The command execution chain to which this command belongs.
        /// </summary>
        public byte CommandID;

        /// <summary>
        /// The position of this command within the command execution chain.
        /// </summary>
        public byte CommandSequence;

        /// <summary>
        /// The name of the delegate to invoke when this command is processed.
        /// </summary>
        public string CommandName;

        /// <summary>
        /// A parameter for the invoked delegate.
        /// </summary>
        public int PrimaryValue;

        /// <summary>
        /// A parameter for the invoked delegate.
        /// </summary>
        public int SecondaryValue;

        /// <summary>
        /// The radius, in feet, for the AoE target selection of this command.
        /// </summary>
        public byte EffectRadius;

        /// <summary>
        /// The angle, in degrees, for the AoE target selection of this command. This is only meaningful for point-blank AoE commands.
        /// </summary>
        public byte EffectAngle;

        /// <summary>
        /// If EffectRadius is set, modifies the target used as the source for the AoE.
        /// </summary>
        public CommandTargetTypes AoESource;

        /// <summary>
        /// The type of target affected by this command.
        /// </summary>
        public CommandTargetTypes TargetType;

        /// <summary>
        /// If EffectRadius is set and this value is nonzero, modifies the maximum number of targets which can be hit by this ability. The default is 9 targets.
        /// </summary>
        public byte MaxTargets;

        /// <summary>
        /// If nonzero, causes the command to run a defense check. The value corresponds to the index of the stat to attack with.
        /// </summary>
        public byte AttackingStat;

        /// <summary>
        /// Indicates that this effect has delayed execution.
        /// </summary>
        public bool IsDelayedEffect;

        /// <summary>
        /// If TargetType is 0, indicates that the last target, or target list in the case of an AoE, should be used.
        /// </summary>
        public bool FromAllTargets;

        /// <summary>
        /// If set, this command will not automatically be added to the ability's command set when it is constructed. It must be added using the modifier system.
        /// </summary>
        public bool NoAutoUse;
    }
}
