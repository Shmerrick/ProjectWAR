using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using Common;
using FrameWork;
using MySql.Data.MySqlClient;
using WorldServer.Managers;
using WorldServer.World.Objects;

// Verifies that a stack-count change and a money change reach the database on the next ORM pass,
// without the owner's 60s autosave. Writes only rows under a reserved, unowned fixture character
// key and removes them in finally.
internal static class CrashWindowPersistenceChecks
{
    private const uint Fixture = uint.MaxValue;

    private static void Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        int result = 0;
        try { Run(); }
        catch (Exception error) { Console.Error.WriteLine(error); result = 1; }
        // The ORM's persistence pump is a foreground thread.
        Environment.Exit(result);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static long Scalar(MySqlConnection connection, string sql)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = sql;
            object value = command.ExecuteScalar();
            return value == null || value is DBNull ? -1 : Convert.ToInt64(value);
        }
    }

    // The pump wakes every 5s; ForceSave makes that wake-up a save pass. Poll rather than sleep a fixed time.
    private static bool WaitFor(MySqlConnection connection, string sql, long expected)
    {
        for (int i = 0; i < 30; i++)
        {
            if (Scalar(connection, sql) == expected)
                return true;
            Thread.Sleep(500);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run()
    {
        var config = new XmlDocument();
        config.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "World.xml"));
        XmlNode db = config.DocumentElement.SelectSingleNode("CharacterDatabase");
        var builder = new MySqlConnectionStringBuilder(db["Custom"].InnerText);
        builder.Server = db["Server"].InnerText;
        builder.Port = uint.Parse(db["Port"].InnerText);
        builder.Database = db["Database"].InnerText.Replace("%name%", "characters");
        builder.UserID = db["Username"].InnerText;
        builder.Password = db["Password"].InnerText;

        using (var connection = new MySqlConnection(builder.ConnectionString))
        {
            connection.Open();
            Assert(Scalar(connection, "SELECT GET_LOCK('ProjectWAR_CrashWindowPersistenceChecks',0)") == 1, "Another persistence check is running");
            Assert(Scalar(connection, "SELECT (SELECT COUNT(*) FROM characters WHERE CharacterId=4294967295) + " +
                "(SELECT COUNT(*) FROM characters_items WHERE CharacterId=4294967295) + " +
                "(SELECT COUNT(*) FROM characters_value WHERE CharacterId=4294967295)") == 0, "Reserved fixture key must be unused");
            try
            {
                var orm = new MySQLObjectDatabase(new MySqlDataConnection(builder.ConnectionString, builder.Database));
                orm.RegisterDataObject(typeof(CharacterItem));
                orm.RegisterDataObject(typeof(Character_value));
                CharMgr.Database = orm;

                // Fixture rows, inserted the way the server inserts them.
                var row = new CharacterItem { Guid = long.MaxValue - 1, CharacterId = Fixture, Entry = 208470, SlotId = 40, Counts = 20 };
                var value = new Character_value { CharacterId = Fixture, Money = 1000 };
                orm.AddObject(row);
                orm.AddObject(value);
                orm.ForceSave();
                Assert(WaitFor(connection, "SELECT Counts FROM characters_items WHERE CharacterId=4294967295", 20), "Fixture item inserted");
                Assert(WaitFor(connection, "SELECT Money FROM characters_value WHERE CharacterId=4294967295", 1000), "Fixture value inserted");
                Assert(row.IsValid && value.IsValid, "Inserted rows are valid for update");

                var player = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
                player.Info = new Character { CharacterId = Fixture };
                player._Value = value;

                // A stack drop, as when part of it is traded, mailed or crafted. No player autosave runs.
                var item = new Item(player) { CharSaveInfo = row };
                item.Count = 12;
                // Money, as when gold is mailed, deposited or paid for an auction.
                Assert(player.RemoveMoney(300), "Fixture has the money to remove");

                // The receiving side of such a transfer calls ForceSave (CharMgr.CreateItem) or is a mail
                // flushed on the same pass; the changes above must ride that pass, not the 60s autosave.
                orm.ForceSave();
                Assert(WaitFor(connection, "SELECT Counts FROM characters_items WHERE CharacterId=4294967295", 12),
                    "A stack count change is written on the next pass, not the owner's autosave");
                Assert(WaitFor(connection, "SELECT Money FROM characters_value WHERE CharacterId=4294967295", 700),
                    "A money change is written on the next pass, not the owner's autosave");

                // Writing the same count again queues nothing new and changes nothing.
                item.Count = 12;
                Assert(Scalar(connection, "SELECT Counts FROM characters_items WHERE CharacterId=4294967295") == 12, "Unchanged count is stable");

                Console.WriteLine("PASS: stack counts and money reach the database on the next ORM pass, closing the crash window.");
            }
            finally
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM characters_items WHERE CharacterId=4294967295; " +
                        "DELETE FROM characters_value WHERE CharacterId=4294967295; " +
                        "SELECT RELEASE_LOCK('ProjectWAR_CrashWindowPersistenceChecks')";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
