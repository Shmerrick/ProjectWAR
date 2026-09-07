using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using Common;
using FrameWork;
using MySql.Data.MySqlClient;

// Verifies the bestiary kill counter repair (migration 54) and the tome tactic data set
// (migrations 55-57) against the configured Release world database. SELECT only; writes nothing.
//
// Two distinct things are checked:
//   1. Every new DataObject binds to its table through the real ORM binder and every mapped
//      property resolves to a readable column. This is the failure that would otherwise only
//      appear as a LoadingFunction exception at boot.
//   2. The data matches the 1.4.8 client files it was derived from, including the values read
//      off the live client's own Greenskin fragment tooltip.
internal static class TomeTacticChecks
{
    private static MySqlConnection _connection;
    private static int _checks;

    private static int Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };

        try { Run(); return 0; }
        catch (Exception error) { Console.Error.WriteLine(error.Message); return 1; }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run()
    {
        var config = new XmlDocument();
        config.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "World.xml"));
        XmlNode db = config.DocumentElement.SelectSingleNode("WorldDatabase");

        var builder = new MySqlConnectionStringBuilder(db["Custom"].InnerText);
        builder.Server = db["Server"].InnerText;
        builder.Port = uint.Parse(db["Port"].InnerText);
        builder.Database = db["Database"].InnerText.Replace("%name%", "world");
        builder.UserID = db["Username"].InnerText;
        builder.Password = db["Password"].InnerText;

        using (_connection = new MySqlConnection(builder.ConnectionString))
        {
            _connection.Open();

            var orm = (MySQLObjectDatabase)FormatterServices.GetUninitializedObject(typeof(MySQLObjectDatabase));
            Set(orm, "Connection", new MySqlDataConnection(builder.ConnectionString, builder.Database));
            Type bindingType = typeof(ObjectDatabase).GetNestedType("BindingInfo", BindingFlags.NonPublic);
            Set(orm, "_bindingInfos", Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(Type), bindingType.MakeArrayType())));
            Set(orm, "RelationAttributes", new Dictionary<MemberInfo, Relation[]>());

            BindsCleanly<Tok_Bestiary>(orm, bindingType, "tok_bestiary");
            BindsCleanly<Tome_Tactic_Line>(orm, bindingType, "tome_tactic_lines");
            BindsCleanly<Tome_Tactic_Fragment>(orm, bindingType, "tome_tactic_fragments");
            BindsCleanly<Tome_Tactic_Line_Creature_Type>(orm, bindingType, "tome_tactic_line_creature_types");

            CheckBestiaryCounters();
            CheckTacticLines();
            CheckFragments();
            CheckCreatureTypes();
            CheckAbilityAndTomeRows();
        }

        Console.WriteLine("PASS: " + _checks + " checks; bestiary kill counters and tome tactic data match the 1.4.8 client. SELECT only.");
    }

    private static void Set(object target, string name, object value)
    {
        typeof(ObjectDatabase).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }

    /// <summary>
    /// Resolves the type's ORM bindings and selects exactly those columns, so a property with no
    /// column, or a column the reader cannot convert, fails here rather than at server boot.
    /// </summary>
    private static void BindsCleanly<T>(MySQLObjectDatabase orm, Type bindingType, string table) where T : DataObject
    {
        var bindings = (Array)typeof(ObjectDatabase).GetMethod("GetBindingInfo", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(orm, new object[] { typeof(T) });

        PropertyInfo[] members = bindings.Cast<object>()
            .Select(x => (PropertyInfo)bindingType.GetField("Member").GetValue(x))
            .ToArray();

        string[] columns = members.Select(m => m.Name).ToArray();

        // A NULL only matters where the member cannot represent it: a non-nullable value type
        // silently becomes 0, which is exactly how Bestiary_ID broke every bestiary kill counter.
        // Strings and Nullable<T> legitimately hold NULL (e.g. a species with no 100,000 kill tier).
        bool[] mustNotBeNull = members
            .Select(m => m.PropertyType.IsValueType && Nullable.GetUnderlyingType(m.PropertyType) == null)
            .ToArray();

        if (columns.Length == 0)
            throw new Exception(typeof(T).Name + " bound no columns.");

        string sql = "SELECT " + string.Join(",", columns.Select(c => "`" + c + "`")) + " FROM `" + table + "`";

        int rows = 0;
        using (var command = new MySqlCommand(sql, _connection))
        using (MySqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    if (mustNotBeNull[i] && reader.IsDBNull(i))
                        throw new Exception(table + "." + columns[i] + " is NULL; " + typeof(T).Name
                            + " maps it to a non-nullable " + members[i].PropertyType.Name
                            + " that would silently read as a default.");
                }

                rows++;
            }
        }

        if (rows == 0)
            throw new Exception(table + " is empty.");

        Report(typeof(T).Name + " binds " + columns.Length + " columns over " + rows + " rows");
    }

    private static void CheckBestiaryCounters()
    {
        // Migration 54. 137 species, 136 with a client action counter; subtype 68 "Hammerer" has
        // no client bestiary species at all and is the single deliberate zero.
        Equal(137, Scalar("SELECT COUNT(*) FROM tok_bestiary"), "tok_bestiary rows");
        Equal(136, Scalar("SELECT COUNT(*) FROM tok_bestiary WHERE Bestiary_ID > 0"), "species with a counter");
        Equal(136, Scalar("SELECT COUNT(DISTINCT Bestiary_ID) FROM tok_bestiary WHERE Bestiary_ID > 0"), "distinct counters");
        Equal(68, Scalar("SELECT COALESCE(MAX(Creature_Sub_Type), 0) FROM tok_bestiary WHERE Bestiary_ID = 0"), "the only uncounted subtype");

        // The whole bug was a NULL column read back as 0, so the column must no longer be nullable.
        Equal(0, Scalar("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE()"
            + " AND TABLE_NAME = 'tok_bestiary' AND COLUMN_NAME = 'Bestiary_ID' AND IS_NULLABLE = 'YES'"),
            "Bestiary_ID nullable");

        // Spot checks proving the mapping is by species identity, not by subtype id. These three
        // are exactly the pairs a naive Creature_Sub_Type = species id join would have got wrong.
        Equal(3, Scalar("SELECT Bestiary_ID FROM tok_bestiary WHERE Creature_Sub_Type = 2"), "Bear counter");
        Equal(10, Scalar("SELECT Bestiary_ID FROM tok_bestiary WHERE Creature_Sub_Type = 3"), "Boar counter");
        Equal(2, Scalar("SELECT Bestiary_ID FROM tok_bestiary WHERE Creature_Sub_Type = 4"), "Giant Bat counter");
        Equal(107, Scalar("SELECT Bestiary_ID FROM tok_bestiary WHERE Creature_Sub_Type = 85"), "Snotling counter");
    }

    private static void CheckTacticLines()
    {
        Equal(9, Scalar("SELECT COUNT(*) FROM tome_tactic_lines"), "tactic lines");
        Equal(330, Scalar("SELECT MIN(AcId) FROM tome_tactic_lines"), "first line counter");
        Equal(338, Scalar("SELECT MAX(AcId) FROM tome_tactic_lines"), "last line counter");

        // A non-ascending threshold set would let a higher tier unlock before a lower one, and a
        // zero first threshold would grant tier 1 for free. TomeTacticService refuses such rows.
        Equal(0, Scalar("SELECT COUNT(*) FROM tome_tactic_lines WHERE Threshold1 = 0"
            + " OR Threshold2 <= Threshold1 OR Threshold3 <= Threshold2"), "lines with bad thresholds");

        // Tactic entry n maps to ability 15099+n and Tome entry 6199+n, so the two must stay in step.
        Equal(0, Scalar("SELECT COUNT(*) FROM tome_tactic_lines WHERE Tactic1 - TokEntry1 <> 8900"
            + " OR Tactic2 - TokEntry2 <> 8900 OR Tactic3 - TokEntry3 <> 8900"), "tactic/tok pairs out of step");

        // Read directly off the live client's own fragment tooltip.
        Equal(2, Scalar("SELECT Threshold1 FROM tome_tactic_lines WHERE AcId = 333"), "Outmaneuver the Dim threshold");
        Equal(3, Scalar("SELECT Threshold2 FROM tome_tactic_lines WHERE AcId = 333"), "Outmaneuver the Cunning threshold");
        Equal(5, Scalar("SELECT Threshold3 FROM tome_tactic_lines WHERE AcId = 333"), "Outmaneuver the Clever threshold");
        Equal(15109, Scalar("SELECT Tactic1 FROM tome_tactic_lines WHERE AcId = 333"), "Greenskin tier 1 ability");

        // The one other threshold the community record preserved.
        Equal(10, Scalar("SELECT Threshold1 FROM tome_tactic_lines WHERE AcId = 331"), "Beastial tier 1 threshold");
    }

    private static void CheckFragments()
    {
        Equal(138, Scalar("SELECT COUNT(*) FROM tome_tactic_fragments"), "bound fragments");

        // Per-line totals straight from bestiary/species.csv. Man is 6 rather than the client's 8
        // because both missing fragments sit on client species 46 ("Dwarven, Slayer"), which has no
        // tok_bestiary row here (BUG-117).
        var expected = new Dictionary<int, int>
        {
            { 330, 24 }, { 331, 32 }, { 332, 16 }, { 333, 5 }, { 334, 18 },
            { 335, 12 }, { 336, 6 }, { 337, 3 }, { 338, 22 }
        };

        foreach (KeyValuePair<int, int> pair in expected)
            Equal(pair.Value, Scalar("SELECT COUNT(*) FROM tome_tactic_fragments WHERE AcId = " + pair.Key),
                "fragments on line " + pair.Key);

        // A line whose bound fragments cannot reach its own final threshold can never be completed.
        Equal(0, Scalar("SELECT COUNT(*) FROM (SELECT l.AcId FROM tome_tactic_lines l"
            + " LEFT JOIN tome_tactic_fragments f ON f.AcId = l.AcId"
            + " GROUP BY l.AcId, l.Threshold3 HAVING COUNT(f.TokEntry) < l.Threshold3) x"),
            "lines whose final tier is unreachable");

        // Every fragment must name a real Tome entry, or the counter advances on nothing.
        Equal(0, Scalar("SELECT COUNT(*) FROM tome_tactic_fragments f"
            + " LEFT JOIN tok_infos t ON t.Entry = f.TokEntry WHERE t.Entry IS NULL"),
            "fragments with no Tome entry");

        // And must belong to a line that exists.
        Equal(0, Scalar("SELECT COUNT(*) FROM tome_tactic_fragments f"
            + " LEFT JOIN tome_tactic_lines l ON l.AcId = f.AcId WHERE l.AcId IS NULL"),
            "fragments with no line");

        // Snotling slot 7, the entry visible as "Green Lightning" in the client's bestiary.
        Equal(333, Scalar("SELECT AcId FROM tome_tactic_fragments WHERE TokEntry = 4066"), "Green Lightning line");

        CheckFragmentsAreObtainable();
    }

    /// <summary>
    /// A fragment is only real if something in the world can award its Tome entry. A bestiary kill
    /// milestone is awarded by TokInterface.AddKill; every other fragment sits on a one-off task
    /// that must be granted by a creature's TokUnlock or an item's TokUnlock/2/3. Fragments with
    /// neither are unobtainable, and a line whose obtainable fragments fall short of a threshold
    /// has a tier no player can ever reach.
    ///
    /// Counting only *bound* fragments, as the boot-time check in TomeTacticService does, is not
    /// enough: it passes on all nine lines while four of them are in fact uncompletable.
    /// </summary>
    private static void CheckFragmentsAreObtainable()
    {
        // One round trip: the award-path lookups are correlated subqueries over item_infos'
        // 88,727 rows, so this is evaluated once and the thresholds compared in code.
        const string obtainable =
            "SELECT f.AcId, SUM(CASE WHEN EXISTS (SELECT 1 FROM creature_protos p"
            + "   WHERE FIND_IN_SET(f.TokEntry, REPLACE(p.TokUnlock, ';', ',')))"
            + " OR EXISTS (SELECT 1 FROM item_infos i WHERE i.TokUnlock = f.TokEntry"
            + "   OR i.TokUnlock2 = f.TokEntry OR i.TokUnlock3 = f.TokEntry)"
            + " OR f.TokEntry IN (CAST(SUBSTRING_INDEX(tb.Kill1,';',1) AS UNSIGNED),"
            + "   CAST(SUBSTRING_INDEX(tb.Kill25,';',1) AS UNSIGNED), CAST(SUBSTRING_INDEX(tb.Kill100,';',1) AS UNSIGNED),"
            + "   CAST(SUBSTRING_INDEX(tb.Kill1000,';',1) AS UNSIGNED), CAST(SUBSTRING_INDEX(tb.Kill10000,';',1) AS UNSIGNED),"
            + "   CAST(SUBSTRING_INDEX(tb.Kill100000,';',1) AS UNSIGNED))"
            + " THEN 1 ELSE 0 END) n"
            + " FROM tome_tactic_fragments f JOIN tok_bestiary tb ON tb.Creature_Sub_Type = ("
            + "   SELECT tb2.Creature_Sub_Type FROM tok_bestiary tb2"
            + "   WHERE CAST(SUBSTRING_INDEX(tb2.Kill1,';',1) AS UNSIGNED) <= f.TokEntry"
            + "   ORDER BY CAST(SUBSTRING_INDEX(tb2.Kill1,';',1) AS UNSIGNED) DESC LIMIT 1)"
            + " GROUP BY f.AcId";

        // Each threshold unlocks a SEPARATELY NAMED tactic, not a rank of one tactic: the Giant
        // line's 5/10/15 give Sky Titan's Bulwark, Sky Titan's Favor and Sky Titan's Strength,
        // each bought and slotted on its own. So an unreachable threshold costs a distinct
        // ability, and that is what this reports.
        string sql = "SELECT l.Name, l.Threshold1, l.Threshold2, l.Threshold3, COUNT(*) bound, o.n,"
            + " a1.Name, a2.Name, a3.Name"
            + " FROM (" + obtainable + ") o JOIN tome_tactic_lines l ON l.AcId = o.AcId"
            + " JOIN tome_tactic_fragments f2 ON f2.AcId = o.AcId"
            + " JOIN abilities a1 ON a1.Entry = l.Tactic1"
            + " JOIN abilities a2 ON a2.Entry = l.Tactic2"
            + " JOIN abilities a3 ON a3.Entry = l.Tactic3"
            + " GROUP BY l.AcId, l.Name, l.Threshold1, l.Threshold2, l.Threshold3, o.n,"
            + " a1.Name, a2.Name, a3.Name ORDER BY l.AcId";

        int tier1Dead = 0;
        int tier2Dead = 0;
        int unobtainableTactics = 0;
        int tier3Dead = 0;
        int totalBound = 0;
        int totalObtainable = 0;

        using (var command = new MySqlCommand(sql, _connection))
        {
            command.CommandTimeout = 300;

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    int t1 = reader.GetInt32(1);
                    int t2 = reader.GetInt32(2);
                    int t3 = reader.GetInt32(3);
                    int bound = reader.GetInt32(4);
                    int have = reader.GetInt32(5);
                    string[] tacticNames = { reader.GetString(6), reader.GetString(7), reader.GetString(8) };
                    int[] thresholds = { t1, t2, t3 };

                    totalBound += bound;
                    totalObtainable += have;

                    // Every threshold is tested. An earlier revision compared only the first and
                    // third, which hid the Man line stopping after its first tactic.
                    if (have < t1) tier1Dead++;
                    if (have < t2) tier2Dead++;
                    if (have < t3) tier3Dead++;

                    for (int i = 0; i < 3; i++)
                    {
                        if (have >= thresholds[i])
                            continue;

                        unobtainableTactics++;
                        Console.WriteLine("  " + tacticNames[i] + " (" + name + ") is unobtainable: needs "
                            + thresholds[i] + " fragments, only " + have + " of " + bound + " can be earned");
                    }
                }
            }
        }

        // Tier 1 must be reachable on every line, or the line is entirely dead content.
        Equal(0, tier1Dead, "lines whose FIRST tier is unobtainable");

        // BUG-117. Some fragments sit on Tome entries that nothing in the world awards, so four
        // lines cannot reach tier 3 and one of those four -- Man -- cannot even reach tier 2.
        // Pinned at the known counts so this cannot silently worsen, and so fixing BUG-117 trips
        // the check and forces these numbers to be updated.
        // Migration 71 fixed the Metal Construct kill tiers, which restored the fourth Man-line
        // fragment and with it Boon of Tenacity, so every line's SECOND tactic is now reachable.
        Equal(0, tier2Dead, "lines whose SECOND tactic is unobtainable (BUG-117)");
        Equal(4, tier3Dead, "lines whose THIRD tactic is unobtainable (BUG-117)");

        // The figure that actually matters: each threshold is a separate, uniquely named tactic,
        // so four distinct abilities cannot be earned at all -- Harrier's Ken, Sky Titan's
        // Strength, Boon of Persistence and Cunning Stratagem.
        Equal(4, unobtainableTactics, "named tactics that cannot be earned (BUG-117)");
        Equal(20, totalBound - totalObtainable, "fragments with no award path (BUG-117)");
    }

    private static void CheckCreatureTypes()
    {
        Equal(31, Scalar("SELECT COUNT(*) FROM tome_tactic_line_creature_types"), "creature type bindings");

        // A creature type owned by two lines would make tactic matching ambiguous.
        Equal(0, Scalar("SELECT COUNT(*) FROM (SELECT CreatureType FROM tome_tactic_line_creature_types"
            + " GROUP BY CreatureType HAVING COUNT(*) > 1) x"), "creature types claimed twice");

        // Every line must act against something.
        Equal(0, Scalar("SELECT COUNT(*) FROM tome_tactic_lines l"
            + " LEFT JOIN tome_tactic_line_creature_types t ON t.AcId = l.AcId WHERE t.AcId IS NULL"),
            "lines with no creature types");

        Equal(15, Scalar("SELECT CreatureType FROM tome_tactic_line_creature_types WHERE AcId = 333"), "Greenskin creature type");
        Equal(18, Scalar("SELECT CreatureType FROM tome_tactic_line_creature_types WHERE AcId = 337"), "Skaven creature type");
    }

    private static void CheckAbilityAndTomeRows()
    {
        // The 27 tactics, their Section 26 Tome rows, and the buff rows without which
        // TacticsInterface rejects every one of them as "Nonexistent tactic".
        Equal(27, Scalar("SELECT COUNT(*) FROM abilities WHERE Entry BETWEEN 15100 AND 15126 AND Category = 16"), "tactic abilities");
        Equal(27, Scalar("SELECT COUNT(*) FROM tok_infos WHERE Entry BETWEEN 6200 AND 6226 AND Section = 26"), "Section 26 Tome rows");
        Equal(27, Scalar("SELECT COUNT(*) FROM buff_infos WHERE Entry BETWEEN 15100 AND 15126"), "tactic buff rows");

        // Each line's three tactics must be distinct abilities that actually exist.
        Equal(27, Scalar("SELECT COUNT(*) FROM (SELECT Tactic1 t FROM tome_tactic_lines"
            + " UNION SELECT Tactic2 FROM tome_tactic_lines UNION SELECT Tactic3 FROM tome_tactic_lines) x"),
            "distinct tactics referenced");

        Equal(0, Scalar("SELECT COUNT(*) FROM (SELECT Tactic1 t FROM tome_tactic_lines"
            + " UNION SELECT Tactic2 FROM tome_tactic_lines UNION SELECT Tactic3 FROM tome_tactic_lines) x"
            + " LEFT JOIN abilities a ON a.Entry = x.t WHERE a.Entry IS NULL"),
            "tactics with no ability row");

        Equal(0, Scalar("SELECT COUNT(*) FROM (SELECT TokEntry1 t FROM tome_tactic_lines"
            + " UNION SELECT TokEntry2 FROM tome_tactic_lines UNION SELECT TokEntry3 FROM tome_tactic_lines) x"
            + " LEFT JOIN tok_infos i ON i.Entry = x.t WHERE i.Entry IS NULL"),
            "tactic unlocks with no Tome row");

        // The nine aggro-range tactics AIInterface keys on must all be real Category 16 abilities.
        Equal(9, Scalar("SELECT COUNT(*) FROM abilities WHERE Category = 16 AND Entry IN"
            + " (15103,15104,15105,15109,15110,15111,15118,15119,15120)"), "aggro-range tactics");
    }

    private static long Scalar(string sql)
    {
        using (var command = new MySqlCommand(sql, _connection))
        {
            object value = command.ExecuteScalar();
            return value == null || value == DBNull.Value ? 0L : Convert.ToInt64(value);
        }
    }

    private static void Equal(long expected, long actual, string what)
    {
        if (expected != actual)
            throw new Exception(what + ": expected " + expected + ", got " + actual + ".");

        _checks++;
    }

    private static void Report(string message)
    {
        Console.WriteLine("  " + message);
        _checks++;
    }
}
