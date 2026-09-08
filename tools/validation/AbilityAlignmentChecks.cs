using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using MySql.Data.MySqlClient;

// SELECT-only checks that the ability table the server actually loads agrees with the client.
//
// World.xml ships UseMythicActionCoverageTables = true, so AbilityMgr loads mythic_src_abilities
// rather than abilities. mythic_src_abilities carried a misaligned identity block -- Name, EffectID
// and IconId belonging to a different ability on 1007 of the 4221 shared entries -- while every
// mechanical column matched. EffectID is written into the cast packets, so those rows told the
// client to play the wrong visual; migration 76 realigned them. These checks keep that closed.
//
// The client (mythic_bin_ability) is the arbiter, per CLAUDE.md hard rule 3. Where abilities
// deliberately annotates a name ("Gift of Brutality Proc", "Vehement Blades Self AP") the two
// server tables must still agree with EACH OTHER; only the client comparison is allowed to differ,
// and only by a bounded amount.
internal static class AbilityAlignmentChecks
{
    private static MySqlConnection _connection;

    // mythic_src_abilities rows whose name matched the client, measured after migration 76.
    // A drop means something rewrote the table from a misaligned source again.
    private const long MinimumClientNameAgreement = 6012;

    // Rows whose EffectID matches the client, measured after migration 77. EffectID goes into the
    // cast packets, so this is the number that decides whether an ability plays its own visual.
    private const long MinimumSrcEffectAgreement = 8349;
    private const long MinimumAbilitiesEffectAgreement = 4164;
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

            // The two server tables must not disagree about what an entry IS.
            Equal(0, Scalar(
                "SELECT COUNT(*) FROM abilities a JOIN mythic_src_abilities m ON m.Entry = a.Entry"
                + " WHERE NOT (a.Name <=> m.Name) OR NOT (a.EffectID <=> m.EffectID) OR NOT (a.IconId <=> m.IconId)"),
                "shared entries whose identity differs between abilities and mythic_src_abilities");

            // Mechanics agreed before the repair and must still agree after it; the migration
            // touched only the identity columns.
            long shared = Scalar("SELECT COUNT(*) FROM abilities a JOIN mythic_src_abilities m ON m.Entry = a.Entry");
            Equal(shared, Scalar(
                "SELECT COUNT(*) FROM abilities a JOIN mythic_src_abilities m ON m.Entry = a.Entry"
                + " WHERE a.CareerLine <=> m.CareerLine AND a.MinRange <=> m.MinRange AND a.Range <=> m.Range"
                + " AND a.CastTime <=> m.CastTime AND a.Cooldown <=> m.Cooldown AND a.ApCost <=> m.ApCost"
                + " AND a.AbilityType <=> m.AbilityType AND a.MasteryTree <=> m.MasteryTree"
                + " AND a.Specline <=> m.Specline AND a.MinimumRank <=> m.MinimumRank"),
                "shared entries whose mechanics agree");

            // Entries that exist only in mythic_src_abilities are creature and world abilities
            // (all carry CareerLine 0). Each has a real client row, so where the client names one,
            // the server must use that name rather than a mislabelled one.
            Equal(0, Scalar(
                "SELECT COUNT(*) FROM mythic_src_abilities m"
                + " LEFT JOIN abilities a ON a.Entry = m.Entry"
                + " JOIN mythic_bin_ability b ON b.ID = m.Entry"
                + " WHERE a.Entry IS NULL AND b.Name <> '' AND NOT (m.Name <=> b.Name)"),
                "mythic_src-only entries mislabelled against the client");

            // No entry the server loads may be an id the client does not know; if one were, the
            // client would silently ignore everything the server said about it.
            Equal(0, Scalar(
                "SELECT COUNT(*) FROM mythic_src_abilities m"
                + " LEFT JOIN mythic_bin_ability b ON b.ID = m.Entry WHERE b.ID IS NULL"),
                "loaded abilities with no client row");

            long agreement = Scalar(
                "SELECT COUNT(*) FROM mythic_src_abilities m JOIN mythic_bin_ability b ON b.ID = m.Entry"
                + " WHERE b.Name <> '' AND m.Name = b.Name");
            AtLeast(MinimumClientNameAgreement, agreement,
                "mythic_src_abilities names agreeing with the client");

            // EffectID is the visual the client plays for the ability. These were taken from the
            // client by migration 77; the shortfall is the 67/57 rows where the server carries an
            // effect the client record does not, which are deliberately left alone.
            long srcEffects = Scalar(
                "SELECT COUNT(*) FROM mythic_src_abilities m JOIN mythic_bin_ability b ON b.ID = m.Entry"
                + " WHERE m.EffectID <=> b.EffectID");
            AtLeast(MinimumSrcEffectAgreement, srcEffects,
                "mythic_src_abilities EffectIDs agreeing with the client");

            AtLeast(MinimumAbilitiesEffectAgreement, Scalar(
                "SELECT COUNT(*) FROM abilities a JOIN mythic_bin_ability b ON b.ID = a.Entry"
                + " WHERE a.EffectID <=> b.EffectID"),
                "abilities EffectIDs agreeing with the client");

            // The signature of the original corruption: values copied from mythic_csv_abilities
            // (data/gamedata/abilities.csv), whose ID column is an authoring id space that agrees
            // with the client's own name table on 13 ids out of 3,115. If these climb, something
            // has joined on that key again.
            AtMost(40, Scalar(
                "SELECT COUNT(*) FROM mythic_src_abilities m JOIN mythic_csv_abilities c ON c.AbilityId = m.Entry"
                + " JOIN mythic_bin_ability b ON b.ID = m.Entry"
                + " WHERE COALESCE(b.EffectID, 0) = 0 AND m.EffectID <> 0 AND m.EffectID = m.Entry"
                + " AND m.EffectID = c.EffectAbilityId"),
                "rows carrying a CSV-derived EffectID the client does not have");

            Console.WriteLine("PASS: " + shared + " shared entries, identity and mechanics identical across both ability tables.");
            Console.WriteLine("PASS: every loaded ability has a client row; " + agreement + " names and "
                + srcEffects + " EffectIDs match the client exactly.");
            Console.WriteLine("These are data checks. They do not verify that a cast plays the right visual in the client.");
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

    private static void AtLeast(long floor, long actual, string what)
    {
        if (actual < floor)
            throw new Exception(what + ": " + actual + ", down from the " + floor
                + " migrations 76 and 77 left. The table has been rewritten from a misaligned source.");
    }

    private static void AtMost(long ceiling, long actual, string what)
    {
        if (actual > ceiling)
            throw new Exception(what + ": " + actual + ", above the " + ceiling
                + " that remain as coincidence. Something has joined on mythic_csv_abilities.AbilityId again.");
    }
}
