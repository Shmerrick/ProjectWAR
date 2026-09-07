using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Common.Database.World.Battlefront;
using FrameWork;
using MySql.Data.MySqlClient;

// Executes the built ORM's INSERT/UPDATE SQL against a session-local copy of the
// configured Release table. No services, registration, or persistent writes.
internal static class DatabaseNullChecks
{
    private static int Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        try { Run(); return 0; }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run()
    {
        var config = new XmlDocument();
        config.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "World.xml"));
        XmlNode settings = config.DocumentElement.SelectSingleNode("WorldDatabase");
        var builder = new MySqlConnectionStringBuilder(settings["Custom"].InnerText);
        builder.Server = settings["Server"].InnerText;
        builder.Port = uint.Parse(settings["Port"].InnerText);
        builder.Database = settings["Database"].InnerText.Replace("%name%", "world");
        builder.UserID = settings["Username"].InnerText;
        builder.Password = settings["Password"].InnerText;

        // Bypass the ORM constructor's background save thread. Initialize only
        // the formatter's reflection caches; never register or alter a table.
        var orm = (MySQLObjectDatabase)FormatterServices.GetUninitializedObject(typeof(MySQLObjectDatabase));
        Set(orm, typeof(ObjectDatabase), "Connection", new MySqlDataConnection(builder.ConnectionString, builder.Database));
        Type bindingType = typeof(ObjectDatabase).GetNestedType("BindingInfo", BindingFlags.NonPublic);
        Type cacheType = typeof(Dictionary<,>).MakeGenericType(typeof(Type), bindingType.MakeArrayType());
        Set(orm, typeof(ObjectDatabase), "_bindingInfos", Activator.CreateInstance(cacheType));
        Set(orm, typeof(ObjectDatabase), "RelationAttributes", new Dictionary<MemberInfo, Relation[]>());
        Set(orm, typeof(ObjectDatabase), "TableDatasets", new Dictionary<string, DataTableHandler>
        {
            { "lotd_resource_tracker", new DataTableHandler(new DataSet()) },
            { "null_scalar", new DataTableHandler(new DataSet()) }
        });
        Set(orm, typeof(MySQLObjectDatabase), "_opBuilder", new StringBuilder());
        Set(orm, typeof(MySQLObjectDatabase), "_whereBuilder", new StringBuilder());

        using (var connection = new MySqlConnection(builder.ConnectionString))
        {
            connection.Open();
            Execute(connection, "CREATE TEMPORARY TABLE null_roundtrip LIKE lotd_resource_tracker");
            Execute(connection, "SET SESSION sql_mode = 'STRICT_ALL_TABLES,NO_ZERO_DATE,NO_ZERO_IN_DATE'");
            var tracker = new LotdResourceTracker
            {
                TrackerId = 1, Threshold = 100, LastUpdatedOnUtc = new DateTime(2026, 9, 6),
                UnlockEndsOnUtc = null
            };
            Execute(connection, Format(orm, "FormulateInsert", tracker));
            Check(connection, null);
            DateTime expires = new DateTime(2026, 9, 7, 12, 34, 56);
            tracker.UnlockEndsOnUtc = expires;
            Execute(connection, Format(orm, "FormulateUpdate", tracker));
            Check(connection, expires);
            tracker.UnlockEndsOnUtc = null;
            Execute(connection, Format(orm, "FormulateUpdate", tracker));
            Check(connection, null);
            // Also exercise a populated nullable value on INSERT.
            Execute(connection, "DELETE FROM null_roundtrip");
            tracker.UnlockEndsOnUtc = expires;
            Execute(connection, Format(orm, "FormulateInsert", tracker));
            Check(connection, expires);
            object converted = typeof(MySQLObjectDatabase).GetMethod("ConvertFromDatabaseFormat", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(orm, new object[] { typeof(DateTime?), new MySql.Data.Types.MySqlDateTime(expires) });
            if (!expires.Equals(converted)) throw new Exception("Nullable date read conversion failed.");
            Execute(connection, "CREATE TEMPORARY TABLE null_scalar (Id INT PRIMARY KEY, OptionalText TEXT NULL, RequiredText TEXT NOT NULL, OptionalNumber INT NULL, OptionalFlag BOOLEAN NULL)");
            var scalar = new NullScalarFixture { Id = 1 };
            Execute(connection, Format(orm, "FormulateInsert", scalar));
            // Only Nullable<T> members become SQL NULL. A null string keeps the legacy empty
        // string, because DataElement defaults AllowDbNull to true and many base-dump
        // columns behind those members are declared NOT NULL.
        CheckScalar(connection, "OptionalText='' AND RequiredText='' AND OptionalNumber IS NULL AND OptionalFlag IS NULL");
            scalar.OptionalText = "";
            scalar.RequiredText = "O'Brien\\test";
            scalar.OptionalNumber = 0;
            scalar.OptionalFlag = false;
            Execute(connection, Format(orm, "FormulateUpdate", scalar));
            CheckScalar(connection, "OptionalText='' AND OptionalText IS NOT NULL AND OptionalNumber=0 AND OptionalFlag=0");
            using (var command = new MySqlCommand("SELECT RequiredText FROM null_scalar WHERE Id=1", connection))
                if (!scalar.RequiredText.Equals(command.ExecuteScalar())) throw new Exception("String escaping changed.");
        }
        Console.WriteLine("PASS: Release schema, strict SQL NULL/date INSERT and UPDATE round trips, nullable date reader, optional scalars and string escaping; no persistent rows changed.");
    }

    private static void Set(object target, Type owner, string name, object value)
    {
        owner.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }

    private static string Format(MySQLObjectDatabase orm, string method, DataObject tracker)
    {
        return ((string)typeof(MySQLObjectDatabase).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(orm, new object[] { tracker, false })).Replace("`lotd_resource_tracker`", "`null_roundtrip`");
    }

    private static void Execute(MySqlConnection connection, string sql)
    {
        using (var command = new MySqlCommand(sql, connection)) command.ExecuteNonQuery();
    }

    private static void CheckScalar(MySqlConnection connection, string predicate)
    {
        using (var command = new MySqlCommand("SELECT COUNT(*) FROM null_scalar WHERE Id=1 AND " + predicate, connection))
            if (Convert.ToInt32(command.ExecuteScalar()) != 1) throw new Exception("Optional scalar/empty string round trip failed.");
    }

    private static void Check(MySqlConnection connection, DateTime? expected)
    {
        using (var command = new MySqlCommand("SELECT UnlockEndsOnUtc FROM null_roundtrip WHERE TrackerId=1", connection))
        {
            object actual = command.ExecuteScalar();
            if (expected.HasValue ? !expected.Value.Equals(actual) : actual != DBNull.Value)
                throw new Exception("Nullable date did not round trip through the built ORM SQL.");
        }
    }
}

[FrameWork.DataTable(TableName = "null_scalar")]
public class NullScalarFixture : DataObject
{
    [PrimaryKey] public int Id { get; set; }
    [DataElement] public string OptionalText { get; set; }
    [DataElement(AllowDbNull = false)] public string RequiredText { get; set; }
    [DataElement] public int? OptionalNumber { get; set; }
    [DataElement] public bool? OptionalFlag { get; set; }
}
