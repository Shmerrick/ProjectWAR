using ClientDataMatrix.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClientDataMatrix.Parsers
{
    internal static class AbilityBinaryParser
    {
        private const uint ExtDataSentinel = 0xAAAAAAAA;

        public static List<BinaryAbilityRecord> ParseAbilityExport(string path)
        {
            using (FileStream stream = OpenShared(path))
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                uint header = reader.ReadUInt32();
                uint count = reader.ReadUInt32();
                List<BinaryAbilityRecord> records = new List<BinaryAbilityRecord>((int)Math.Min(count, int.MaxValue));

                for (int index = 0; index < count; ++index)
                {
                    long recordOffset = stream.Position;
                    BinaryAbilityRecord record = new BinaryAbilityRecord
                    {
                        SourceFamily = "client_bin",
                        TableName = "abilityexport.bin",
                        SourcePath = path,
                        Header = header,
                        RecordIndex = index + 1,
                        ByteOffset = recordOffset,
                        CastTime = reader.ReadUInt32(),
                        Cooldown = reader.ReadUInt32(),
                        TacticType = reader.ReadUInt32(),
                        TargetType = reader.ReadUInt32(),
                        AbilityType = reader.ReadUInt32(),
                        AttackType = reader.ReadUInt32()
                    };

                    uint opaque24 = reader.ReadUInt32();
                    uint careerLine = reader.ReadUInt32();
                    uint counterAmount = reader.ReadUInt32();
                    uint flagsRaw = reader.ReadUInt32();
                    ushort abilityId = reader.ReadUInt16();
                    ushort effectId = reader.ReadUInt16();

                    record.Opaque24 = opaque24;
                    record.CareerLine = careerLine;
                    record.CounterAmount = counterAmount;
                    record.FlagsRaw = flagsRaw;
                    record.AbilityId = abilityId;
                    record.EffectId = effectId;
                    record.RowKey = "record=" + record.RecordIndex + ";AbilityId=" + abilityId + ";offset=" + recordOffset;
                    record.RawRow = null;
                    record.IsGranted = (flagsRaw & 0x1) != 0;
                    record.IsPassive = (flagsRaw & 0x2) != 0;
                    record.IsBuff = (flagsRaw & 0x4) != 0;
                    record.IsDebuff = (flagsRaw & 0x8) != 0;
                    record.IsDamaging = (flagsRaw & 0x10) != 0;
                    record.IsHealing = (flagsRaw & 0x20) != 0;
                    record.IsDefensive = (flagsRaw & 0x40) != 0;
                    record.IsOffensive = (flagsRaw & 0x80) != 0;
                    record.RequiresPet = (flagsRaw & 0x100) != 0;
                    record.IsStatsBuff = (flagsRaw & 0x40000) != 0;
                    record.IsBuffDebuff = ((flagsRaw >> 21) & 0x1) == 1;
                    record.IsChanneled = ((flagsRaw >> 22) & 0x1) == 1;

                    record.Value44 = reader.ReadUInt16();
                    record.Range = reader.ReadUInt16();
                    record.Angle = reader.ReadUInt16();
                    record.MoraleCost = reader.ReadUInt16();
                    record.ChannelInterval = reader.ReadUInt16();
                    record.Value54 = reader.ReadUInt16();
                    record.ScaleStatMultiplier = reader.ReadUInt16();
                    record.NumTacticSlots = reader.ReadByte();
                    record.MoraleLevel = reader.ReadByte();
                    record.ApCost = reader.ReadByte();
                    record.Byte61 = reader.ReadByte();
                    record.Byte62 = reader.ReadByte();
                    record.Faction = reader.ReadByte();
                    record.ImprovementThreshold = reader.ReadByte();
                    record.ImprovementCap = reader.ReadByte();
                    record.Specialization = reader.ReadByte();
                    record.StanceOrder = reader.ReadByte();
                    record.Byte68 = reader.ReadByte();
                    record.MinLevel = reader.ReadByte();
                    record.ComponentIds = ReadUInt16List(reader, 10);
                    record.Triggers = ReadUInt32List(reader, 10);
                    record.UsableWithBuff = ReadUInt16List(reader, 4);
                    record.Value136 = reader.ReadUInt32();
                    record.Value140 = reader.ReadUInt16();
                    record.Groups = ReadUInt16List(reader, 4);
                    record.Labels = ReadUInt16List(reader, 5);
                    record.ComponentVfx = reader.ReadBytes(10).ToList();
                    record.Value170 = reader.ReadUInt16();
                    record.ExtData = ReadExtDataList(reader, 5);

                    records.Add(record);
                }

                if (stream.Position != stream.Length)
                    throw new InvalidDataException("abilityexport.bin parse did not consume the full file. Remaining bytes: " + (stream.Length - stream.Position));

                return records;
            }
        }

        public static List<BinaryComponentRecord> ParseAbilityComponentExport(string path)
        {
            List<BinaryComponentRecord> records;
            if (TryParseAbilityComponentExport(path, true, out records))
                return records;

            if (TryParseAbilityComponentExport(path, false, out records))
                return records;

            throw new InvalidDataException("Unable to parse abilitycomponentexport.bin with either known value layout.");
        }

        public static List<BinaryRequirementRecord> ParseAbilityRequirementExport(string path)
        {
            using (FileStream stream = OpenShared(path))
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                uint header = reader.ReadUInt32();
                uint count = reader.ReadUInt32();
                List<BinaryRequirementRecord> records = new List<BinaryRequirementRecord>((int)Math.Min(count, int.MaxValue));

                for (int index = 0; index < count; ++index)
                {
                    long recordOffset = stream.Position;
                    ushort requirementId = reader.ReadUInt16();
                    BinaryRequirementRecord record = new BinaryRequirementRecord
                    {
                        SourceFamily = "client_bin",
                        TableName = "abilityrequirementexport.bin",
                        SourcePath = path,
                        Header = header,
                        RecordIndex = index + 1,
                        ByteOffset = recordOffset,
                        RequirementId = requirementId,
                        RowKey = "record=" + (index + 1) + ";RequirementId=" + requirementId + ";offset=" + recordOffset,
                        ExtData = ReadExtDataList(reader, 6)
                    };

                    records.Add(record);
                }

                if (stream.Position != stream.Length)
                    throw new InvalidDataException("abilityrequirementexport.bin parse did not consume the full file. Remaining bytes: " + (stream.Length - stream.Position));

                return records;
            }
        }

        private const int UpgradeItemsPerRecord = 20;

        /// <summary>
        /// `upgradetableexport.bin`: a uint32 header and a uint32 count, then per record a uint16 id
        /// and 20 items of eight uint16s. See <see cref="BinaryUpgradeTableRecord"/>.
        /// </summary>
        public static List<BinaryUpgradeTableRecord> ParseUpgradeTableExport(string path)
        {
            using (FileStream stream = OpenShared(path))
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                uint header = reader.ReadUInt32();
                uint count = reader.ReadUInt32();
                const int recordBytes = sizeof(ushort) + UpgradeItemsPerRecord * 8 * sizeof(ushort);
                if (count > int.MaxValue || (long)count * recordBytes != stream.Length - stream.Position)
                    throw new InvalidDataException("upgradetableexport.bin record count does not match its file length.");
                List<BinaryUpgradeTableRecord> records = new List<BinaryUpgradeTableRecord>((int)count);

                for (int index = 0; index < count; ++index)
                {
                    long recordOffset = stream.Position;
                    ushort upgradeId = reader.ReadUInt16();
                    BinaryUpgradeTableRecord record = new BinaryUpgradeTableRecord
                    {
                        SourceFamily = "client_bin",
                        TableName = "upgradetableexport.bin",
                        SourcePath = path,
                        Header = header,
                        RecordIndex = index + 1,
                        ByteOffset = recordOffset,
                        UpgradeId = upgradeId,
                        RowKey = "record=" + (index + 1) + ";UpgradeId=" + upgradeId + ";offset=" + recordOffset,
                        Items = new List<BinaryUpgradeItemRecord>(UpgradeItemsPerRecord)
                    };

                    for (int item = 0; item < UpgradeItemsPerRecord; ++item)
                    {
                        BinaryUpgradeItemRecord entry = new BinaryUpgradeItemRecord
                        {
                            Index = item,
                            V1 = reader.ReadUInt16(),
                            V2 = reader.ReadUInt16(),
                            V3 = reader.ReadUInt16(),
                            V4 = reader.ReadUInt16(),
                            V5 = reader.ReadUInt16(),
                            V6 = reader.ReadUInt16(),
                            V7 = reader.ReadUInt16(),
                            V8 = reader.ReadUInt16()
                        };
                        entry.Scalar = BitConverter.ToSingle(BitConverter.GetBytes((uint)entry.V1 | ((uint)entry.V2 << 16)), 0);
                        record.Items.Add(entry);
                    }

                    records.Add(record);
                }

                if (stream.Position != stream.Length)
                    throw new InvalidDataException("upgradetableexport.bin parse did not consume the full file. Remaining bytes: " + (stream.Length - stream.Position));

                return records;
            }
        }

        private static bool TryParseAbilityComponentExport(string path, bool useUInt32Values, out List<BinaryComponentRecord> records)
        {
            try
            {
                using (FileStream stream = OpenShared(path))
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    uint header = reader.ReadUInt32();
                    uint count = reader.ReadUInt32();
                    records = new List<BinaryComponentRecord>((int)Math.Min(count, int.MaxValue));

                    for (int index = 0; index < count; ++index)
                    {
                        long recordOffset = stream.Position;
                        BinaryComponentRecord record = new BinaryComponentRecord
                        {
                            SourceFamily = "client_bin",
                            TableName = "abilitycomponentexport.bin",
                            SourcePath = path,
                            Header = header,
                            RecordIndex = index + 1,
                            ByteOffset = recordOffset,
                            LayoutVariant = useUInt32Values ? "u32-values" : "u16-values",
                            Value00 = reader.ReadUInt16(),
                            RowKey = "record=" + (index + 1) + ";offset=" + recordOffset
                        };

                        record.ExtData = ReadExtDataList(reader, 8);
                        record.Values = useUInt32Values ? ReadInt32List(reader, 8) : ReadInt16List(reader, 8).Select(value => (int)value).ToList();
                        record.Multipliers = ReadInt32List(reader, 8);
                        record.ActivationDelay = reader.ReadUInt32();
                        record.Duration = reader.ReadUInt32();
                        record.FlagsRaw = reader.ReadUInt32();
                        record.Value08 = reader.ReadUInt32();
                        record.Operation = reader.ReadUInt32();
                        record.Interval = reader.ReadUInt32();
                        record.ComponentId = reader.ReadUInt16();
                        record.Radius = reader.ReadUInt16();
                        record.ConeAngle = reader.ReadUInt16();
                        record.FlightSpeed = reader.ReadUInt16();
                        record.Value15 = reader.ReadUInt16();
                        record.MaxTargets = reader.ReadByte();
                        record.RowKey = "record=" + (index + 1) + ";ComponentId=" + record.ComponentId + ";offset=" + recordOffset;

                        records.Add(record);
                    }

                    if (stream.Position != stream.Length)
                    {
                        records = null;
                        return false;
                    }

                    return true;
                }
            }
            catch
            {
                records = null;
                return false;
            }
        }

        private static List<BinaryExtDataRecord> ReadExtDataList(BinaryReader reader, int slotCount)
        {
            List<BinaryExtDataRecord> records = new List<BinaryExtDataRecord>();
            for (int slotIndex = 0; slotIndex < slotCount; ++slotIndex)
            {
                uint marker = reader.ReadUInt32();
                if (marker != ExtDataSentinel)
                    continue;

                records.Add(new BinaryExtDataRecord
                {
                    SlotIndex = slotIndex,
                    Val1 = reader.ReadInt32(),
                    Val2 = reader.ReadInt32(),
                    Val3 = reader.ReadInt32(),
                    Val4 = reader.ReadInt32(),
                    Val5 = reader.ReadInt32(),
                    Val6 = reader.ReadInt32(),
                    Val7 = reader.ReadInt32(),
                    Val8 = reader.ReadInt32(),
                    Val9 = reader.ReadByte()
                });
            }

            return records;
        }

        private static List<ushort> ReadUInt16List(BinaryReader reader, int count)
        {
            List<ushort> values = new List<ushort>(count);
            for (int index = 0; index < count; ++index)
                values.Add(reader.ReadUInt16());
            return values;
        }

        private static List<uint> ReadUInt32List(BinaryReader reader, int count)
        {
            List<uint> values = new List<uint>(count);
            for (int index = 0; index < count; ++index)
                values.Add(reader.ReadUInt32());
            return values;
        }

        private static List<int> ReadInt32List(BinaryReader reader, int count)
        {
            List<int> values = new List<int>(count);
            for (int index = 0; index < count; ++index)
                values.Add(reader.ReadInt32());
            return values;
        }

        private static List<short> ReadInt16List(BinaryReader reader, int count)
        {
            List<short> values = new List<short>(count);
            for (int index = 0; index < count; ++index)
                values.Add(reader.ReadInt16());
            return values;
        }

        private static FileStream OpenShared(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        }
    }
}
