using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using Common;
using FrameWork;
using MySql.Data.MySqlClient;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

// Writes only a reserved, unowned fixture character key and removes it in finally.
internal static class ItemCooldownPersistenceChecks
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
        // The normal ORM starts a foreground persistence pump; all fixture work is synchronous.
        Environment.Exit(result);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static AbilityInterface Login()
    {
        var player = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        player.Info = new Character { CharacterId = Fixture };
        player.ItmInterface = new ItemsInterface { Items = new WorldServer.World.Objects.Item[0] };
        var abilities = new AbilityInterface();
        abilities.SetOwner(player);
        player.AbtInterface = abilities;
        return abilities;
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
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT GET_LOCK('ProjectWAR_ItemCooldownPersistenceChecks',0)";
                Assert(Convert.ToInt32(command.ExecuteScalar()) == 1, "Another persistence check is running");
                command.CommandText = "SELECT (SELECT COUNT(*) FROM characters WHERE CharacterId=4294967295) + " +
                    "(SELECT COUNT(*) FROM character_item_cooldowns WHERE CharacterId=4294967295)";
                Assert(Convert.ToInt64(command.ExecuteScalar()) == 0, "Reserved fixture key must be unused");
            }
            try
            {
                var orm = new MySQLObjectDatabase(new MySqlDataConnection(builder.ConnectionString, builder.Database));
                orm.RegisterDataObject(typeof(CharacterItemCooldown));
                CharMgr.Database = orm;
                var first = Login();
                first.LoadItemCooldowns();
                first.SetItemCooldown(1353, 60123, true);
                first.SetItemGroupCooldown(7, 60789);
                first.SetItemCooldown(7, 65001, true);
                long spellEnd = first.Cooldowns[1353];
                long groupEnd = first.ItemGroupCooldowns[7];
                var second = Login();
                second.LoadItemCooldowns();
                Assert(second.Cooldowns[1353] == spellEnd && second.ItemGroupCooldowns[7] == groupEnd,
                    "Fresh login restores exact deadlines even with empty inventory");
                Assert(!second.CanCastCooldown(1353) && !second.CanCastItemGroupCooldown(7), "Restored timers enforce cooldowns");
                Assert(second.Cooldowns[7] == first.Cooldowns[7], "Spell and group keys cannot collide");
                ItemService._Item_Info = new Dictionary<uint, Item_Info>
                {
                    { 1, new Item_Info { Entry = 1, SpellId = 1353 } }
                };
                var record = new CharacterItem { Entry = 1 };
                var item = new WorldServer.World.Objects.Item(second._Owner);
                Assert(item.Load(record) && record.NextAllowedUseTime == (spellEnd + 999) / 1000,
                    "Inventory load reconstructs display deadline from exact persisted timer");
                second.SetCooldown(1353, -1, true);
                second.SetCooldown(7, -1, true);
                second.SetItemGroupCooldown(7, 0);
                var third = Login();
                third.LoadItemCooldowns();
                Assert(third.CanCastCooldown(1353) && third.CanCastItemGroupCooldown(7), "Resets remain reset after relog");
                item.Owner = third._Owner;
                Assert(item.Load(record) && record.NextAllowedUseTime == 0, "Cached inventory cannot resurrect a reset timer");
                Assert(!third.RestoreItemCooldown(65535, groupEnd) && !third.RestoreItemCooldown(65536, groupEnd)
                    && !third.RestoreItemCooldown(uint.MaxValue, groupEnd)
                    && !third.RestoreItemCooldown(1353, long.MaxValue), "Invalid keys and corrupt deadlines rejected");
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM character_item_cooldowns WHERE CharacterId=4294967295";
                    Assert(Convert.ToInt64(command.ExecuteScalar()) == 0, "Expired/reset rows removed during login");
                }
                Console.WriteLine("PASS: Release DB item cooldown writes, ORM reload, exact deadlines, empty inventory, reset and stale cleanup.");
            }
            finally
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM character_item_cooldowns WHERE CharacterId=4294967295";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
