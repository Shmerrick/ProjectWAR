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

// Loads both ability tables through the server's own ORM, once by reflection and once by the compiled
// expression binder WorldServer uses, and requires every mapped property to agree. That covers the
// columns migrations 03, 09 and 10 added -- AICooldown; TargetType, a nullable column where NULL means
// the client has no record of the ability; ChannelDuration and ChannelInterval -- so a column the ORM
// cannot bind fails here rather than at boot. SELECT only.
internal static class AbilityLoaderChecks
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
        XmlNode db = config.DocumentElement.SelectSingleNode("WorldDatabase");
        var builder = new MySqlConnectionStringBuilder(db["Custom"].InnerText);
        builder.Server = db["Server"].InnerText;
        builder.Port = uint.Parse(db["Port"].InnerText);
        builder.Database = db["Database"].InnerText.Replace("%name%", "world");
        builder.UserID = db["Username"].InnerText;
        builder.Password = db["Password"].InnerText;
        var orm = (MySQLObjectDatabase)FormatterServices.GetUninitializedObject(typeof(MySQLObjectDatabase));
        Set(orm, "Connection", new MySqlDataConnection(builder.ConnectionString, builder.Database));
        Type bindingType = typeof(ObjectDatabase).GetNestedType("BindingInfo", BindingFlags.NonPublic);
        Set(orm, "_bindingInfos", Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(Type), bindingType.MakeArrayType())));
        Set(orm, "RelationAttributes", new Dictionary<MemberInfo, Relation[]>());
        Check<MythicSourceAbilityInfo>(orm, "mythic_src_abilities", bindingType, builder.ConnectionString);
        Check<DBAbilityInfo>(orm, "abilities", bindingType, builder.ConnectionString);
    }

    private static void Set(object target, string name, object value)
    {
        typeof(ObjectDatabase).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }

    private static long Scalar(string connectionString, string sql)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(sql, connection))
                return Convert.ToInt64(command.ExecuteScalar());
        }
    }

    private static void Check<T>(MySQLObjectDatabase orm, string table, Type bindingType, string connectionString) where T : DBAbilityInfo
    {
        var bindings = (Array)typeof(ObjectDatabase).GetMethod("GetBindingInfo", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(orm, new object[] { typeof(T) });
        var properties = bindings.Cast<object>().Select(x => (PropertyInfo)bindingType.GetField("Member").GetValue(x)).ToArray();
        foreach (string column in new[] { "AICooldown", "TargetType", "ChannelDuration", "ChannelInterval", "CooldownMilliseconds" })
        {
            if (!properties.Any(x => x.Name == column))
                throw new Exception(table + ": the ORM does not bind " + column);
        }

        string sql = "SELECT " + string.Join(",", properties.Select(x => "`" + x.Name + "`")) + " FROM `" + table + "`";
        int keyIndex = Array.FindIndex(properties, x => x.Name == "Entry");
        if (keyIndex < 0)
            throw new Exception(table + ": Entry is not a bound column");
        var args = new object[] { keyIndex, 10000, IsolationLevel.DEFAULT, table, sql, bindings, false };
        var expected = (Dictionary<ushort, T>)typeof(MySQLObjectDatabase).GetMethod("ReflectionMap", BindingFlags.Instance | BindingFlags.NonPublic)
            .MakeGenericMethod(typeof(ushort), typeof(T)).Invoke(orm, args);
        var actual = (Dictionary<ushort, T>)typeof(MySQLObjectDatabase).GetMethod("CompiledExpressionMap", BindingFlags.Instance | BindingFlags.NonPublic)
            .MakeGenericMethod(typeof(ushort), typeof(T)).Invoke(orm, args);
        long rows = Scalar(connectionString, "SELECT COUNT(*) FROM `" + table + "`");
        if (expected.Count == 0 || expected.Count != rows || actual.Count != expected.Count)
            throw new Exception(table + ": ability count differs (table " + rows + ", reflection " + expected.Count + ", compiled " + actual.Count + ")");

        foreach (var pair in expected)
        {
            T loaded;
            if (!actual.TryGetValue(pair.Key, out loaded))
                throw new Exception(table + ": missing ability " + pair.Key);
            foreach (var property in properties)
            {
                if (!Equals(property.GetValue(pair.Value, null), property.GetValue(loaded, null)))
                    throw new Exception(table + ": ability " + pair.Key + " differs at " + property.Name);
            }
            if (!loaded.IsValid || loaded.AllowAdd || loaded.Dirty)
                throw new Exception(table + ": ability persistence flags differ");
        }

        // The loaded values must be the database's, NULL included: a binder that read NULL as 0 would
        // turn every ability the client has no record of into one that targets its caster.
        long withTargetType = Scalar(connectionString, "SELECT COUNT(*) FROM `" + table + "` WHERE TargetType IS NOT NULL");
        long withAiCooldown = Scalar(connectionString, "SELECT COUNT(*) FROM `" + table + "` WHERE AICooldown > 0");
        if (actual.Values.Count(x => x.TargetType.HasValue) != withTargetType)
            throw new Exception(table + ": loaded TargetType values do not match the table's non-NULL count " + withTargetType);
        if (actual.Values.Count(x => x.AICooldown > 0) != withAiCooldown)
            throw new Exception(table + ": loaded AICooldown values do not match the table's count " + withAiCooldown);

        Console.WriteLine("PASS: " + table + " loads " + actual.Count + " abilities identically through both binders; "
            + withTargetType + " carry the client's TargetType, " + withAiCooldown + " an AICooldown. SELECT only.");
    }
}
