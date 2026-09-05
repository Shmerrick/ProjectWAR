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
using WorldServer.Services.World;
using WorldServer.World.Map;
using WorldServer.World.Objects.PublicQuests;

// SELECT-only checks against Release data. No ORM registration, server threads or character writes.
internal static class PublicQuestDataChecks
{
    private static int Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
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
        var xml = new XmlDocument();
        xml.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "World.xml"));
        XmlNode db = xml.DocumentElement.SelectSingleNode("WorldDatabase");
        if (db["ConnectionType"].InnerText != "DATABASE_MYSQL") throw new InvalidOperationException("MySQL required");
        var settings = new MySqlConnectionStringBuilder(db["Custom"].InnerText);
        settings.Server = db["Server"].InnerText;
        settings.Port = uint.Parse(db["Port"].InnerText);
        settings.Database = db["Database"].InnerText.Replace("%name%", "world");
        settings.UserID = db["Username"].InnerText;
        settings.Password = db["Password"].InnerText;
        using (var connection = new MySqlConnection(settings.ConnectionString))
        {
            connection.Open();
            CreatureService.CreatureProtos = Read<Creature_proto>(connection,
                "SELECT p.* FROM creature_protos p WHERE p.Entry IN (SELECT s.Entry FROM pquest_spawns s JOIN pquest_objectives o ON o.Guid=s.Objective WHERE o.Entry IN (188,264) AND s.Type=1)").ToDictionary(p => p.Entry);
            GameObjectService.GameObjectProtos = Read<GameObject_proto>(connection,
                "SELECT p.* FROM gameobject_protos p WHERE p.Entry IN (SELECT s.Entry FROM pquest_spawns s JOIN pquest_objectives o ON o.Guid=s.Objective WHERE o.Entry IN (188,264) AND s.Type=2)").ToDictionary(p => p.Entry);
            Zone_Info zoneInfo = Read<Zone_Info>(connection, "SELECT * FROM zone_infos WHERE ZoneId=100").Single();
            ZoneService._Zone_Info = new List<Zone_Info> { zoneInfo };
            foreach (PQuest_Info info in Read<PQuest_Info>(connection, "SELECT * FROM pquest_info WHERE Entry IN (188,264)"))
            {
                info.Objectives = Read<PQuest_Objective>(connection, "SELECT * FROM pquest_objectives WHERE Type<>0 AND Entry=" + info.Entry + " ORDER BY Guid");
                foreach (PQuest_Objective objective in info.Objectives)
                    objective.Spawns = Read<PQuest_Spawn>(connection, "SELECT * FROM pquest_spawns WHERE Objective=" + objective.Guid);
                var pq = new PublicQuest(info);
                pq.Start(); // Reproduces the original pre-load call with no Region available.
                if (pq.Stage != null) throw new InvalidOperationException("PQ started before region load");
                var region = (RegionMgr)FormatterServices.GetUninitializedObject(typeof(RegionMgr));
                region.ZonesInfo = ZoneService._Zone_Info;
                var queued = new List<RegionMgr.ObjectAdd>();
                typeof(RegionMgr).GetField("_objectsToAdd", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(region, queued);
                var zone = (ZoneMgr)FormatterServices.GetUninitializedObject(typeof(ZoneMgr));
                zone.ZoneId = 100; zone.Info = zoneInfo; zone.Region = region;
                pq.SetZone(zone);
                pq.Loaded = true;
                pq.Start();
                int expected = info.Entry == 264 ? 36 : 85;
                if (pq.Stage == null || queued.Count != expected)
                    throw new InvalidOperationException(info.Name + ": expected " + expected + " first-stage spawns, queued " + queued.Count);
                pq.Start();
                if (queued.Count != expected) throw new InvalidOperationException("Repeated Start duplicated spawns");
                Console.WriteLine("PASS: " + info.Name + " queues " + queued.Count + " first-stage objects after deferred load, without duplicates.");
            }
        }
        Console.WriteLine("These checks construct real DB-backed spawns; they do not run AI, interaction, networking or an in-client PQ.");
    }

    private static List<T> Read<T>(MySqlConnection connection, string sql) where T : new()
    {
        var result = new List<T>();
        using (var command = new MySqlCommand(sql, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var value = new T();
                for (int i = 0; i < reader.FieldCount; ++i)
                {
                    PropertyInfo property = typeof(T).GetProperty(reader.GetName(i));
                    if (property == null || !property.CanWrite || reader.IsDBNull(i)) continue;
                    object field = reader.GetValue(i);
                    if (property.PropertyType == typeof(ushort[]))
                        field = Utils.ConvertStringToArray<ushort>((string)field).ToArray();
                    else
                        field = Convert.ChangeType(field, property.PropertyType);
                    property.SetValue(value, field, null);
                }
                result.Add(value);
            }
        }
        return result;
    }
}
