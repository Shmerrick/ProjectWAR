using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using Common;
using FrameWork;
using MySql.Data.MySqlClient;

internal static class ItemLoaderChecks
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
        Check<MythicSourceItemInfo>(orm, "mythic_src_item_infos", bindingType);
        Check<Item_Info>(orm, "item_infos", bindingType);
    }

    private static void Set(object target, string name, object value)
    {
        typeof(ObjectDatabase).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }

    private static void Check<T>(MySQLObjectDatabase orm, string table, Type bindingType) where T : Item_Info
    {
        var bindings = (Array)typeof(ObjectDatabase).GetMethod("GetBindingInfo", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(orm, new object[] { typeof(T) });
        var properties = bindings.Cast<object>().Select(x => (PropertyInfo)bindingType.GetField("Member").GetValue(x)).ToArray();
        string sql = "SELECT " + string.Join(",", properties.Select(x => "`" + x.Name + "`")) + " FROM `" + table + "` WHERE Name != ''";
        int keyIndex = Array.FindIndex(properties, x => x.Name == "Entry");
        var args = new object[] { keyIndex, 100000, IsolationLevel.DEFAULT, table, sql, bindings, false };
        var watch = Stopwatch.StartNew();
        var expected = (Dictionary<uint,T>)typeof(MySQLObjectDatabase).GetMethod("ReflectionMap", BindingFlags.Instance | BindingFlags.NonPublic)
            .MakeGenericMethod(typeof(uint), typeof(T)).Invoke(orm, args);
        Console.WriteLine(table + " reflection: " + watch.ElapsedMilliseconds + "ms / " + expected.Count + " items");
        watch.Restart();
        var actual = (Dictionary<uint,T>)typeof(MySQLObjectDatabase).GetMethod("CompiledExpressionMap", BindingFlags.Instance | BindingFlags.NonPublic)
            .MakeGenericMethod(typeof(uint), typeof(T)).Invoke(orm, args);
        Console.WriteLine(table + " compiled: " + watch.ElapsedMilliseconds + "ms / " + actual.Count + " items");
        if (expected.Count == 0 || actual.Count != expected.Count) throw new Exception("Item count differs");
        foreach (var pair in expected)
        {
            T loaded;
            if (!actual.TryGetValue(pair.Key, out loaded)) throw new Exception("Missing item " + pair.Key);
            foreach (var property in properties)
            {
                object left = property.GetValue(pair.Value, null), right = property.GetValue(loaded, null);
                bool equal = left is byte[] ? ((byte[])left).SequenceEqual((byte[])right) : Equals(left, right);
                if (!equal) throw new Exception("Item " + pair.Key + " differs at " + property.Name);
            }
            if (!loaded.IsValid || loaded.AllowAdd || loaded.Dirty) throw new Exception("Item persistence flags differ");
        }
        Console.WriteLine("PASS: all mapped properties and item counts match for " + table + "; SELECT only.");
    }
}
