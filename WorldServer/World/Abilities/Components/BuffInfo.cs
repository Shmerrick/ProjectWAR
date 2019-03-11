using System;
using System.Collections.Generic;
using Common;

namespace WorldServer.World.Abilities.Components
{
    /// <summary>
    /// Contains information about an effect which may be cast upon players.
    /// </summary>
    public class BuffInfo
    {
        public List<BuffCommandInfo> CommandInfo;

        public BuffInfo()
        {
        }

        public BuffInfo(DBBuffInfo dbObj)
        {
            Entry = dbObj.Entry;
            Name = dbObj.Name;
            if (!string.IsNullOrEmpty(dbObj.BuffClassString))
                BuffClass = (BuffClass)Enum.Parse(typeof(BuffClass), dbObj.BuffClassString);
            if (!string.IsNullOrEmpty(dbObj.TypeString))
                Type = (BuffTypes)Enum.Parse(typeof(BuffTypes), dbObj.TypeString);
            Group = (BuffGroups)dbObj.Group;
            AuraPropagation = dbObj.AuraPropagation;
            MaxCopies = dbObj.MaxCopies;
            MaxStack = dbObj.MaxStack;
            InitialStacks = dbObj.UseMaxStackAsInitial ? MaxStack : 1;
            StackLine = dbObj.StackLine;
            StacksFromCaster = dbObj.StacksFromCaster;
            Duration = dbObj.Duration;
            LeadInDelay = dbObj.LeadInDelay;
            Interval = dbObj.Interval;
            if (Interval > 0)
                BuffIntervals = (byte)((Duration * 1000) / Interval);
            PersistsOnDeath = dbObj.PersistsOnDeath;
            CanRefresh = dbObj.CanRefresh;
            FriendlyEffectID = dbObj.FriendlyEffectID;
            EnemyEffectID = dbObj.EnemyEffectID;
            EffectType = dbObj.Silent;
        }

        public static List<BuffInfo> Convert(List<DBBuffInfo> dbObjs)
        {
            List<BuffInfo> objects = new List<BuffInfo>();

            foreach (DBBuffInfo dbObj in dbObjs)
                objects.Add(new BuffInfo(dbObj));

            return objects;
        }

        public BuffInfo Clone()
        {
            BuffInfo cBuffInfo = (BuffInfo)MemberwiseClone();

            if (CommandInfo != null)
            {
                cBuffInfo.CommandInfo = new List<BuffCommandInfo>();

                foreach (BuffCommandInfo cmd in CommandInfo)
                {
                    if (!cmd.NoAutoUse)
                        cBuffInfo.CommandInfo.Add(cmd.CloneChain());
                }
            }

            return cBuffInfo;
        }

        public void AddBuffCommand(BuffCommandInfo cmd)
        {
            CommandInfo.Add(cmd);
        }

        public void AppendBuffCommand(BuffCommandInfo cmd, byte Slot)
        {
            CommandInfo[Slot].AddCommandToChain(cmd);
        }

        public void DeleteCommand(byte slot, byte seq)
        {
            if (seq == 0)
                CommandInfo.RemoveAt(slot);
            else
            {
                BuffCommandInfo toDelete = CommandInfo[slot].GetSubcommand(seq);
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

        /// <summary>
        /// The number of intervals over which this buff will tick.
        /// </summary>
        public byte BuffIntervals;

        /// <summary>
        /// Indicates that this effect was cast from an AoE.
        /// </summary>
        public bool IsAoE { get; set; }
        /// <summary>
        /// Indicates that this effect should be removed if the target is not the caster and the target leaves the group.
        /// </summary>
        public bool IsGroupBuff { get; set; }

        /// <summary>
        /// The ID of this effect.
        /// </summary>
        public ushort Entry;

        public string Name;

        /// <summary>
        /// The classification of an effect (standard, morale, career-linked or tactic).
        /// </summary>
        public BuffClass BuffClass;

        /// <summary>
        /// The cleanse type of an effect.
        /// </summary>
        public BuffTypes Type;

        /// <summary>
        /// The effect group, used when applying an effect to determine whether or not it is valid.
        /// </summary>
        public BuffGroups Group;

        /// <summary>
        /// Indicates how an aura effect should propagate, in terms of target type affected and radius.
        /// </summary>
        public string AuraPropagation;

        /// <summary>
        /// The maximum number of copies of this effect which may be present upon a single entity at any one time.
        /// </summary>
        public byte MaxCopies;

        /// <summary>
        /// The number of stacks to apply.
        /// </summary>
        public int InitialStacks;

        /// <summary>
        /// The maximum number of stacks of this effect which can be applied by the same caster.
        /// </summary>
        public byte MaxStack;

        /// <summary>
        /// The index of a parameter to use for indicating, on the client, how many stacks of this effect have been applied.
        /// </summary>
        public byte StackLine;

        /// <summary>
        /// If true, multiple copies of the effect will be created when the effect is reapplied by a caster to the target.
        /// </summary>
        public bool StacksFromCaster;

        /// <summary>
        /// The duration, in seconds, of this effect.
        /// </summary>
        public uint Duration;

        /// <summary>
        /// Additional delay before the effect's first tick. Will also extend the duration of the buff.
        /// </summary>
        public int LeadInDelay;

        /// <summary>
        /// The interval, in milliseconds, between ticks of this effect.
        /// </summary>
        public ushort Interval;

        /// <summary>
        /// Determines whether a buff persists through death (1) or can only be cast on dead players (2).
        /// </summary>
        public byte PersistsOnDeath;

        public bool RequiresTargetAlive => PersistsOnDeath == 0;
        public bool AlwaysOn => PersistsOnDeath == 1;
        public bool RequiresTargetDead => PersistsOnDeath == 2;

        /// <summary> 
        /// The mastery path in which this buff resides. 
        /// </summary>
        public byte MasteryTree;

        /// <summary>
        /// Indicates that this effect will refresh if it is reapplied.
        /// </summary>
        public bool CanRefresh;

        /// <summary>
        /// The index of an visual effect linked to this ID to use when this effect is created on a friendly target.
        /// </summary>
        public byte FriendlyEffectID;

        /// <summary>
        /// The index of a visual effect linked to this ID to use when this effect is created on an enemy target.
        /// </summary>
        public byte EnemyEffectID;

        /// <summary>
        /// 0 - no FX, 1 - start/end effect, 5 - start effect only
        /// </summary>
        public byte EffectType;
    }
}
