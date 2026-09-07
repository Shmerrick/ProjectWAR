using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using MySql.Data.MySqlClient;

// SELECT-only checks for Play as Skaven: the control-ability buffs that swap the action bar,
// the Excavated Skaven Device placements, and the form kits read off the official captures.
// Does not run AI, networking or an in-client test.
internal static class SkavenFormChecks
{
    private static MySqlConnection _connection;

    // form -> (Order control ability, Destruction control ability)
    private static readonly Dictionary<string, int[]> ControlAbilities = new Dictionary<string, int[]>
    {
        { "Warlock Engineer", new[] { 24857, 24861 } },
        { "Gutter Runner",    new[] { 24858, 24862 } },
        { "Rat Ogre",         new[] { 24859, 24863 } },
        { "Pack Master",      new[] { 24860, 24864 } },
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

            // The action-bar swap is done by the client from the control ability's op-51 component,
            // so the buff must exist or the form is only an ability list with no stance.
            // Buffs live in two parallel tables and the server reads mythic_src_buff_infos; writing
            // only one is invisible at runtime with no error (BUG-120), so both are required.
            foreach (KeyValuePair<string, int[]> form in ControlAbilities)
            {
                foreach (int entry in form.Value)
                {
                    Equal(1, Scalar("SELECT COUNT(*) FROM buff_infos WHERE Entry = " + entry),
                        form.Key + " control buff " + entry + " in buff_infos");
                    Equal(1, Scalar("SELECT COUNT(*) FROM mythic_src_buff_infos WHERE Entry = " + entry),
                        form.Key + " control buff " + entry + " in mythic_src_buff_infos");
                }
            }

            // Ending a form must actually end it, so the buff must not survive death.
            Equal(0, Scalar("SELECT COUNT(*) FROM mythic_src_buff_infos"
                + " WHERE Entry BETWEEN 24857 AND 24864 AND PersistsOnDeath <> 0"),
                "control buffs surviving death");

            // The device the forms are taken at.
            Equal(1, Scalar("SELECT COUNT(*) FROM gameobject_protos WHERE Entry = 98811"),
                "Excavated Skaven Device prototype");

            long devices = Scalar("SELECT COUNT(*) FROM gameobject_spawns WHERE Entry = 98811");
            if (devices == 0)
                throw new Exception("No Excavated Skaven Device is spawned; there is nowhere to take a form.");

            // A spawn outside its zone's 0-65535 span is never sent to a client. Both original
            // devices carried ZoneId 100 with coordinates that resolve far outside Norsca, which is
            // why neither was visible until migration 74 corrected them.
            Equal(0, Scalar(
                "SELECT COUNT(*) FROM gameobject_spawns s JOIN zone_infos z ON z.ZoneId = s.ZoneId"
                + " WHERE s.Entry = 98811 AND (s.WorldX - (z.OffX << 12) NOT BETWEEN 0 AND 65535"
                + " OR s.WorldY - (z.OffY << 12) NOT BETWEEN 0 AND 65535)"),
                "devices placed outside their own zone");

            Console.WriteLine("PASS: 8 control-ability buffs present in both buff tables, none persisting through death.");
            Console.WriteLine("PASS: " + devices + " Excavated Skaven Device placement(s), all within their zone bounds.");
            Console.WriteLine("These are data checks; they do not verify the client's action-bar swap, which needs an in-client test.");
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
