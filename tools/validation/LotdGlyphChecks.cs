using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using MySql.Data.MySqlClient;

// SELECT-only checks for Land of the Dead data: the glyph sources, the tomb entry costs from
// interface/interfacecore/maps/zone191/mappoints.xml, and the in-zone respawn points that the
// death rule depends on.
//
// Does not verify that a PQ can actually be completed -- 42 of the 46 have no creatures (BUG-134),
// which is a separate problem this cannot see.
internal static class LotdGlyphChecks
{
    private static MySqlConnection _connection;

    private const int DestructionBase = 7960;
    private const int OrderBase = 7970;

    // Tomb zone -> the glyph indices the client's zone map assigns it.
    // Stars 1,2,3 / Sky 4,5,6 / Moon 7,8 / Sun 9,10 in the map's own numbering, translated through
    // the public quests that appear in both the map and pquest_objectives. The Vulture Lord (179)
    // carries no <glyphs> element and is deliberately absent.
    private static readonly int[][] ExpectedCosts =
    {
        new[] { 241, 0, 1, 3 },
        new[] { 243, 2, 4, 8 },
        new[] { 242, 5, 6 },
        new[] { 244, 7, 9 },
    };

    private static int Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
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

            // Every one of the twenty glyph entries must have at least one public quest awarding it.
            // Six Order glyphs had none until migration 78 (BUG-135).
            Equal(20, Scalar(
                "SELECT COUNT(DISTINCT t.Entry) FROM tok_infos t"
                + " JOIN pquest_objectives o ON o.TokCompleted = t.Entry"
                + " WHERE t.Entry BETWEEN 7960 AND 7979"),
                "glyph entries with an awarding public quest");

            // The realm blocks are the same ten glyphs in the same order, so an Order PQ must never
            // award a Destruction glyph or the reverse. pquest_info.Type is 1 Order, 2 Destruction;
            // Type 0 rows are left out because Pit of Asaph carries one.
            Equal(0, Scalar(
                "SELECT COUNT(*) FROM pquest_objectives o JOIN pquest_info i ON i.Entry = o.Entry"
                + " WHERE i.ZoneId = 191 AND i.Type = 1 AND o.TokCompleted BETWEEN 7960 AND 7969"),
                "Order public quests awarding a Destruction glyph");

            Equal(0, Scalar(
                "SELECT COUNT(*) FROM pquest_objectives o JOIN pquest_info i ON i.Entry = o.Entry"
                + " WHERE i.ZoneId = 191 AND i.Type = 2 AND o.TokCompleted BETWEEN 7970 AND 7979"),
                "Destruction public quests awarding an Order glyph");

            // The tomb costs, exactly as the client's zone map has them.
            int expectedRows = 0;
            foreach (int[] tomb in ExpectedCosts)
            {
                int zoneId = tomb[0];
                for (int i = 1; i < tomb.Length; ++i)
                {
                    Equal(1, Scalar("SELECT COUNT(*) FROM lotd_tomb_glyph_costs"
                        + " WHERE TombZoneId = " + zoneId + " AND GlyphIndex = " + tomb[i]),
                        "tomb " + zoneId + " requiring glyph index " + tomb[i]);
                    ++expectedRows;
                }

                Equal(tomb.Length - 1, Scalar(
                    "SELECT COUNT(*) FROM lotd_tomb_glyph_costs WHERE TombZoneId = " + zoneId),
                    "cost rows on tomb " + zoneId);
            }

            Equal(expectedRows, Scalar("SELECT COUNT(*) FROM lotd_tomb_glyph_costs"),
                "total tomb cost rows");

            // All ten glyphs are spent across the four gated tombs, none twice. If a glyph were
            // required by two tombs, or by none, the translation from the map's numbering is wrong.
            Equal(10, Scalar("SELECT COUNT(DISTINCT GlyphIndex) FROM lotd_tomb_glyph_costs"),
                "distinct glyphs spent across the tombs");

            // Nothing may reference a glyph outside 0-9; the service ignores such a row and the
            // tomb would silently cost less than it should.
            Equal(0, Scalar("SELECT COUNT(*) FROM lotd_tomb_glyph_costs WHERE GlyphIndex > 9"),
                "cost rows with an out-of-range glyph index");

            // The Tomb of the Vulture Lord has no glyph cost in the client and must stay ungated.
            Equal(0, Scalar("SELECT COUNT(*) FROM lotd_tomb_glyph_costs WHERE TombZoneId = 179"),
                "cost rows on the Tomb of the Vulture Lord");

            // "After dying, you will respawn inside the Land of the Dead if your realm currently
            // controls the dungeon." The holding realm therefore needs a respawn point of its own
            // inside zone 191; without one WorldMgr falls through to the capital-city fallback and
            // the winning realm gets sent home too, which looks exactly like the rule misfiring.
            Equal(1, Scalar("SELECT COUNT(*) FROM zone_respawns WHERE ZoneID = 191 AND Realm = 1"),
                "Order respawn points inside the Land of the Dead");
            Equal(1, Scalar("SELECT COUNT(*) FROM zone_respawns WHERE ZoneID = 191 AND Realm = 2"),
                "Destruction respawn points inside the Land of the Dead");

            Console.WriteLine("PASS: all 20 glyph entries have an awarding public quest, and neither realm awards the other's.");
            Console.WriteLine("PASS: 4 tombs spend all 10 glyphs between them, matching the client zone map; the Vulture Lord is ungated.");
            Console.WriteLine("PASS: both realms have a respawn point inside zone 191, so the expedition holder respawns there.");
            Console.WriteLine("These are data checks. 42 of the 46 Land of the Dead public quests still have no creatures (BUG-134),");
            Console.WriteLine("so most glyphs cannot be earned in play regardless of what this reports.");
        }
    }

    private static long Scalar(string sql)
    {
        using (var command = new MySqlCommand(sql, _connection))
            return Convert.ToInt64(command.ExecuteScalar());
    }

    private static void Equal(long expected, long actual, string what)
    {
        if (expected != actual)
            throw new Exception(what + ": expected " + expected + ", found " + actual + ".");
    }
}
