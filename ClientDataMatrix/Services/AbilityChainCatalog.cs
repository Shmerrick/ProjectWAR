using ClientDataMatrix.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ClientDataMatrix.Services
{
    /// <summary>
    /// Follows ability-to-ability references through component values.
    ///
    /// An ability is rarely self-contained: a component can apply, grant or act on ANOTHER
    /// ability, and that ability has components of its own. Reading one ability's row therefore
    /// shows only the first link. The Skaven monster forms are a plain example -- "Order
    /// Controlled Warlock Engineer" (24857) does nothing by itself; its components apply ability
    /// 27950 and issue a server command naming it, and the visible behaviour lives there.
    ///
    /// Every other view in this tool is one ability deep. This one resolves the chain, so an ability
    /// can be read as the tree of behaviour it actually is.
    ///
    /// WHICH VALUES ARE ABILITY REFERENCES. Only slots whose meaning is established are followed.
    /// A component value is just an integer; treating an arbitrary one as an ability id would
    /// invent links that do not exist, and the numeric ranges overlap heavily -- operation 51's
    /// Value[0] looks exactly like an ability id and provably is not. The whitelist below is
    /// sourced from ComponentSchemaCatalog's per-operation findings, and anything not listed is
    /// left alone rather than guessed at.
    /// </summary>
    public sealed class AbilityChainCatalog
    {
        /// <summary>How a referenced ability is reached from its parent.</summary>
        public sealed class AbilityReference
        {
            public ushort FromAbility;
            public ushort ViaComponent;
            public uint Operation;
            public string OperationName;

            /// <summary>Which value slot carried the reference, e.g. "Value[0]".</summary>
            public string Field;

            public ushort ToAbility;

            /// <summary>Why this slot is read as an ability id.</summary>
            public string Basis;
        }

        public sealed class ChainNode
        {
            public ushort AbilityId;
            public string Name;
            public int Depth;
            public AbilityReference ArrivedBy;
            public List<ushort> ComponentIds = new List<ushort>();
            public bool Revisited;
        }

        private readonly AbilityDataset _dataset;
        private readonly Dictionary<ushort, BinaryAbilityRecord> _abilitiesById;
        private readonly Dictionary<ushort, BinaryComponentRecord> _componentsById;
        private readonly Dictionary<int, string> _namesById;

        public AbilityChainCatalog(AbilityDataset dataset)
        {
            _dataset = dataset;

            _abilitiesById = dataset == null
                ? new Dictionary<ushort, BinaryAbilityRecord>()
                : dataset.BinaryAbilities
                    .GroupBy(row => row.AbilityId)
                    .ToDictionary(group => group.Key, group => group.First());

            _componentsById = dataset == null
                ? new Dictionary<ushort, BinaryComponentRecord>()
                : dataset.BinaryComponents
                    .GroupBy(row => row.ComponentId)
                    .ToDictionary(group => group.Key, group => group.OrderBy(r => r.RecordIndex).First());

            _namesById = dataset == null
                ? new Dictionary<int, string>()
                : dataset.AbilityNames
                    .GroupBy(row => (int)row.EntryId)
                    .ToDictionary(group => group.Key, group => group.First().NormalizedValue ?? string.Empty);
        }

        /// <summary>
        /// Value slots that hold an ability id, keyed by component operation.
        ///
        /// Deliberately conservative. Each entry names the evidence for it; an operation absent
        /// from this table has its values left unread rather than assumed.
        /// </summary>
        private static readonly Dictionary<uint, Tuple<int, string, string>[]> AbilityReferenceSlots =
            new Dictionary<uint, Tuple<int, string, string>[]>
            {
                // APPLY_ABILITY applies the ability named in Value[0]. The Skaven controls apply
                // 27950 "Play-As-Monster Master Client Controller" through it.
                { 23, new[] { Tuple.Create(0, "Value[0]",
                    "APPLY_ABILITY applies the ability in Value[0]") } },

                // GRANTED_ABILITY grants the ability in Value[0].
                { 28, new[] { Tuple.Create(0, "Value[0]",
                    "GRANTED_ABILITY grants the ability in Value[0]") } },

                // SERVER_COMMAND is polymorphic on its command code in Value[0]; only command 304
                // is established as taking an ability id, and it names the ability whose persistent
                // state it acts on. "Detonate" targets "Sabotage"; "Tear Down" targets the four
                // deployables. Other command codes are not followed.
                { 36, new[] { Tuple.Create(1, "Value[1]",
                    "SERVER_COMMAND 304 names an ability in Value[1]; only that command code is followed") } },
            };

        /// <summary>Command code that makes SERVER_COMMAND's Value[1] an ability reference.</summary>
        private const int ServerCommandAbilityCode = 304;

        public List<AbilityReference> GetDirectReferences(ushort abilityId)
        {
            var found = new List<AbilityReference>();

            BinaryAbilityRecord ability;
            if (!_abilitiesById.TryGetValue(abilityId, out ability) || ability.ComponentIds == null)
                return found;

            foreach (ushort componentId in ability.ComponentIds.Distinct())
            {
                BinaryComponentRecord component;
                if (!_componentsById.TryGetValue(componentId, out component) || component.Values == null)
                    continue;

                Tuple<int, string, string>[] slots;
                if (!AbilityReferenceSlots.TryGetValue(component.Operation, out slots))
                    continue;

                // SERVER_COMMAND only carries an ability id under one command code.
                if (component.Operation == 36)
                {
                    if (component.Values.Count == 0 || component.Values[0] != ServerCommandAbilityCode)
                        continue;
                }

                foreach (Tuple<int, string, string> slot in slots)
                {
                    if (slot.Item1 >= component.Values.Count)
                        continue;

                    int raw = component.Values[slot.Item1];
                    if (raw <= 0 || raw > ushort.MaxValue)
                        continue;

                    ushort target = (ushort)raw;

                    // Only follow a reference that resolves to a real ability, so a numeric
                    // coincidence cannot manufacture a link.
                    if (!_abilitiesById.ContainsKey(target) && !_namesById.ContainsKey(target))
                        continue;

                    if (target == abilityId)
                        continue;

                    found.Add(new AbilityReference
                    {
                        FromAbility = abilityId,
                        ViaComponent = componentId,
                        Operation = component.Operation,
                        OperationName = DefinitionCatalog.DescribeComponentOperationValue(component.Operation),
                        Field = slot.Item2,
                        ToAbility = target,
                        Basis = slot.Item3
                    });
                }
            }

            return found;
        }

        /// <summary>
        /// Walks the chain breadth-first to <paramref name="maxDepth"/>. A revisited ability is
        /// reported once and not expanded again, so a cycle terminates rather than looping.
        /// </summary>
        public List<ChainNode> BuildChain(ushort rootAbilityId, int maxDepth)
        {
            var ordered = new List<ChainNode>();
            var seen = new HashSet<ushort>();
            var queue = new Queue<ChainNode>();

            queue.Enqueue(new ChainNode
            {
                AbilityId = rootAbilityId,
                Name = DescribeAbility(rootAbilityId),
                Depth = 0,
                ArrivedBy = null
            });

            while (queue.Count > 0)
            {
                ChainNode node = queue.Dequeue();

                if (!seen.Add(node.AbilityId))
                {
                    node.Revisited = true;
                    ordered.Add(node);
                    continue;
                }

                BinaryAbilityRecord ability;
                if (_abilitiesById.TryGetValue(node.AbilityId, out ability) && ability.ComponentIds != null)
                    node.ComponentIds = ability.ComponentIds.Distinct().ToList();

                ordered.Add(node);

                if (node.Depth >= maxDepth)
                    continue;

                foreach (AbilityReference reference in GetDirectReferences(node.AbilityId))
                {
                    queue.Enqueue(new ChainNode
                    {
                        AbilityId = reference.ToAbility,
                        Name = DescribeAbility(reference.ToAbility),
                        Depth = node.Depth + 1,
                        ArrivedBy = reference
                    });
                }
            }

            return ordered;
        }

        public string DescribeAbility(ushort abilityId)
        {
            string name;
            if (_namesById.TryGetValue(abilityId, out name) && !string.IsNullOrWhiteSpace(name))
                return name;

            return _abilitiesById.ContainsKey(abilityId)
                ? "(unnamed ability " + abilityId.ToString(CultureInfo.InvariantCulture) + ")"
                : "(no client row)";
        }

        /// <summary>
        /// Every ability-to-ability reference in the client, for a corpus-wide view of how far
        /// behaviour is displaced from the ability that names it.
        /// </summary>
        public List<AbilityReference> GetAllReferences()
        {
            var all = new List<AbilityReference>();
            foreach (ushort abilityId in _abilitiesById.Keys)
                all.AddRange(GetDirectReferences(abilityId));
            return all;
        }
    }
}
