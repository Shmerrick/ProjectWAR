using System;
using System.Collections.Generic;
using Common;

namespace WorldServer.World.Abilities.Components
{
    public class BuffCommandInfo
    {
        public BuffCommandInfo()
        {
        }

        public BuffCommandInfo(DBBuffCommandInfo dbObj)
        {
            Entry = dbObj.Entry;
            Name = dbObj.Name;
            CommandID = dbObj.CommandID;
            CommandSequence = dbObj.CommandSequence;
            CommandName = dbObj.CommandName;
            if (!string.IsNullOrEmpty(dbObj.BuffClassString))
                BuffClass = (BuffClass)Enum.Parse(typeof(BuffClass), dbObj.BuffClassString);
            PrimaryValue = dbObj.PrimaryValue;
            SecondaryValue = dbObj.SecondaryValue;
            TertiaryValue = dbObj.TertiaryValue;
            InvokeOn = dbObj.InvokeOn;
            EffectRadius = dbObj.EffectRadius;
            EffectAngle = dbObj.EffectAngle;
            if (!string.IsNullOrEmpty(dbObj.Target))
                TargetType = (CommandTargetTypes)Enum.Parse(typeof(CommandTargetTypes), dbObj.Target);
            if (!string.IsNullOrEmpty(dbObj.EffectSource))
                AoESource = (CommandTargetTypes)Enum.Parse(typeof(CommandTargetTypes), dbObj.EffectSource);
            MaxTargets = dbObj.MaxTargets;
            if (!string.IsNullOrEmpty(dbObj.EventIDString))
                EventID = (byte)Enum.Parse(typeof(BuffCombatEvents), dbObj.EventIDString);
            EventCheck = dbObj.EventCheck;
            EventCheckParam = dbObj.EventCheckParam;
            EventChance = dbObj.EventChance;
            RetriggerInterval = dbObj.RetriggerInterval;
            ConsumesStack = dbObj.ConsumesStack;
            BuffLine = dbObj.BuffLine;
            NoAutoUse = dbObj.NoAutoUse;
        }

        public static List<BuffCommandInfo> Convert(List<DBBuffCommandInfo> dbObjs)
        {
            List<BuffCommandInfo> objects = new List<BuffCommandInfo>();

            foreach (DBBuffCommandInfo dbObj in dbObjs)
                objects.Add(new BuffCommandInfo(dbObj));

            return objects;
        }

        #region Vars

        public short CommandResult { get; set; }

        public AbilityDamageInfo DamageInfo;

        public long NextTriggerTime;

        public ushort Entry;

        public string Name;

        public byte CommandID;

        public byte CommandSequence;

        public string CommandName;

        /// <summary>Extra property overriding default class from BuffInfo</summary>
        public BuffClass BuffClass;

        public int PrimaryValue;

        public int SecondaryValue;

        public int TertiaryValue;

        public byte InvokeOn;

        public byte EffectRadius;

        public short EffectAngle;

        public CommandTargetTypes TargetType;

        public CommandTargetTypes AoESource;

        public byte MaxTargets;

        public byte EventID { get; private set; }

        public string EventCheck;

        public uint EventCheckParam;

        public byte EventChance;

        public ushort RetriggerInterval;

        public bool ConsumesStack;

        public byte BuffLine;

        public bool NoAutoUse;

        #endregion

        #region Command Access

        public BuffCommandInfo LastCommand, NextCommand;

        public void AddCommandToChain(BuffCommandInfo slaveCommand)
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
                if (slaveCommand.CommandSequence <= CommandSequence)
                {
                    LastCommand.NextCommand = slaveCommand;
                    slaveCommand.LastCommand = LastCommand;

                    // Handle chains of commands
                    while (slaveCommand.NextCommand != null)
                        slaveCommand = slaveCommand.NextCommand;

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

        public BuffCommandInfo GetSubcommand(byte commandSeq)
        {
            if (CommandSequence == commandSeq)
                return this;
            return NextCommand?.GetSubcommand(commandSeq);
        }

        #endregion

        public BuffCommandInfo CloneChain()
        {
            BuffCommandInfo cCommandInfo = (BuffCommandInfo)MemberwiseClone();
            cCommandInfo.CommandName = (string)CommandName.Clone();

            if (DamageInfo != null)
                cCommandInfo.DamageInfo = DamageInfo.Clone();
            if (NextCommand != null)
                cCommandInfo.NextCommand = NextCommand.CloneChain();
            if (cCommandInfo.NextCommand != null)
                cCommandInfo.NextCommand.LastCommand = cCommandInfo;

            return cCommandInfo;
        }
    }
}