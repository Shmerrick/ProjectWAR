using System;

using FrameWork;

namespace Common
{
    /// <summary>
    /// Server-side event that unlocks a help tip.
    /// Stored as text in <c>help_tips.TriggerName</c> so the table stays readable; parsed once at
    /// load time and rejected with a log entry when the value is not a member of this enum.
    /// </summary>
    public enum HelpTipTrigger
    {
        None = 0,

        /// <summary>Player finished entering the world.</summary>
        Login,

        /// <summary>Career rank gained. TriggerValue is the rank reached.</summary>
        RankUp,

        /// <summary>Renown rank gained. TriggerValue is the renown rank reached.</summary>
        RenownRankUp,

        /// <summary>Experience awarded.</summary>
        XpGained,

        /// <summary>Renown awarded.</summary>
        RenownGained,

        /// <summary>Money awarded.</summary>
        MoneyGained,

        /// <summary>Player died.</summary>
        Death,

        /// <summary>Player joined a party.</summary>
        GroupJoined,

        /// <summary>Player became party leader.</summary>
        GroupLeader,

        /// <summary>Player's party was promoted to a warband.</summary>
        WarbandFormed,

        /// <summary>Player became flagged for RvR.</summary>
        RvrFlagged,

        /// <summary>Player entered an RvR area.</summary>
        RvrAreaEntered,

        /// <summary>Player interacted with an NPC. TriggerValue is the GameData.CreatureTitle.</summary>
        NpcInteract,

        /// <summary>Player looted a corpse or chest.</summary>
        Loot,

        /// <summary>Player completed every objective of a quest.</summary>
        QuestCompleted,

        /// <summary>Player entered a public quest area.</summary>
        PublicQuestEntered,

        /// <summary>Player won a public quest loot bag.</summary>
        PublicQuestBag,

        /// <summary>Player used a mailbox.</summary>
        MailboxUsed,

        /// <summary>Player joined a scenario queue.</summary>
        ScenarioJoined,

        /// <summary>Player learned a crafting or gathering skill. TriggerValue is the skill id.</summary>
        TradeSkillLearned,

        /// <summary>Player entered a chapter area.</summary>
        ChapterEntered,

        /// <summary>Player earned a chapter influence reward tier.</summary>
        InfluenceReward
    }

    /// <summary>
    /// Binds a Tome help tip entry (tok_infos section 101) to the server event that unlocks it.
    /// </summary>
    [DataTable(PreCache = false, TableName = "help_tips", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Help_Tip : DataObject
    {
        /// <summary>Tome entry granted when the trigger fires. Must exist in tok_infos.</summary>
        [PrimaryKey]
        public ushort TokEntry { get; set; }

        /// <summary>
        /// Client tip category: 1 Beginner, 2 Gameplay, 3 UI, 4 Advanced. The client hides tips
        /// whose category the player has switched off.
        /// </summary>
        [DataElement]
        public byte TipType { get; set; }

        /// <summary>Name of a <see cref="HelpTipTrigger"/> member.</summary>
        [DataElement(Varchar = 32)]
        public string TriggerName { get; set; }

        /// <summary>Trigger parameter. Zero matches any value the trigger reports.</summary>
        [DataElement]
        public uint TriggerValue { get; set; }

        /// <summary>Highest career rank this tip is still offered at. Zero means no limit.</summary>
        [DataElement]
        public byte MaxRank { get; set; }

        /// <summary>Set to zero to retire a tip without deleting its row.</summary>
        [DataElement]
        public byte Enabled { get; set; }
    }
}
