using System;

using FrameWork;

namespace Common
{
    /// <summary>
    /// A creature whose death advances a ward fragment task counter.
    ///
    /// Many-to-one: a task may name more than one creature ("Kill Warlock Peenk and/or Korthuk
    /// the Raging 12 Times"), and any of them counts. Seeded from the task names in tok_infos
    /// matched against creature_protos. The names that could not be resolved are
    /// deliberately absent from this table.
    /// </summary>
    [DataTable(PreCache = true, TableName = "ward_task_creatures", DatabaseName = "World")]
    [Serializable]
    public class Ward_Task_Creature : DataObject
    {
        [PrimaryKey]
        public ushort AcId { get; set; }

        [PrimaryKey]
        public uint CreatureEntry { get; set; }
    }
}
