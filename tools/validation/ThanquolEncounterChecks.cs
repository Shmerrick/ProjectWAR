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

// SELECT-only checks that the Thanquol's Incursion encounter data (public quest 911) still
// matches the official 1.4.8 captures it was decoded from:
//   WAR-RE-Toolkit\libs\protocolservices\Packet Logs\Tanquollincursion.txt.gz
//   WAR-RE-Toolkit\libs\protocolservices\Packet Logs\THANQUOL INCURSION (FULL RUN ...).log.txt.gz
//   WAR-RE-Toolkit\libs\protocolservices\Packet Logs\thanquollfull+RvR.txt.gz
// Every expected value below was read out of F_OBJECTIVE_INFO (0xC1) or F_CREATE_STATIC (0x14)
// in those captures. This does not run AI, networking or an in-client public quest.
internal static class ThanquolEncounterChecks
{
    private const uint PQuestEntry = 911;
    private const ushort ZoneId = 410;

    // stage label, tracker title, objective type, objective text, count, ObjectId,
    // ClientObjectiveId, stage seconds, no-timer
    private static readonly object[][] ExpectedStages =
    {
        new object[] { "Setup", "Setup", (byte)12,
            "Setup", (ushort)1, "0", 2531u, (ushort)300, false },
        new object[] { "Stage I", "Destroy Siphoning Contraptions", (byte)11,
            "Siphoning Contraptions Destroyed", (ushort)2, "100517", 2532u, (ushort)0, true },
        new object[] { "Stage II", "Dispatch Warlock Engineer Skeetk", (byte)2,
            "Warlock Engineer Skeetk", (ushort)1, "99621", 2533u, (ushort)0, true },
        new object[] { "Stage III", "Destroy the Siphoning Contraptions", (byte)11,
            "Siphoning Contraptions Destroyed", (ushort)4, "100517", 2534u, (ushort)0, true },
        new object[] { "Stage IV", "Dispatch Throt the Unclean", (byte)2,
            "Throt the Unclean", (ushort)1, "99623", 2535u, (ushort)0, true },
        new object[] { "Stage V", "Dispatch Thanquol", (byte)2,
            "Thanquol", (ushort)1, "99624", 2536u, (ushort)0, true },
    };

    private static readonly string[] ExpectedDescriptions =
    {
        "Take a moment to collect yourself",
        "Those infernal Skaven machines are diverting resources from the surface! They must be stopped - Destroy them immediately!",
        "Make an example of this foul creature.",
        "Finish what you started, and make sure all four of these warp-fueled monstrosities cease to function!",
        "Make an example of this foul creature.",
        "Make an example of this foul creature.",
    };

    // World positions of the four Siphoning Contraptions, from F_CREATE_STATIC. Client space
    // plus 57344 (OffX/OffY 16 minus the captures' instance shift of 1) plus the constant
    // +53 / -52 the existing zone-410 spawns carry.
    private static readonly int[][] ExpectedContraptions =
    {
        new[] { 84600, 84371, 8698,  512 },
        new[] { 82003, 81925, 9275, 3777 },
        new[] { 80660, 84725, 8506, 3083 },
        new[] { 85411, 80422, 9275, 3879 },
    };

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

            PQuest_Info info = Read<PQuest_Info>(connection,
                "SELECT * FROM pquest_info WHERE Entry=" + PQuestEntry).SingleOrDefault();
            if (info == null) throw new InvalidOperationException("Public quest 911 is missing. Apply Database/68_thanquols_incursion_encounter.sql.");
            Expect(info.Name, "Thanquol's Incursion", "pquest_info.Name");
            Expect(info.ZoneId, ZoneId, "pquest_info.ZoneId");
            // PQDifficult 0 is what serialises the difficulty byte the captures carry (0xFF).
            Expect(info.PQDifficult, (byte)0, "pquest_info.PQDifficult");

            info.Objectives = Read<PQuest_Objective>(connection,
                "SELECT * FROM pquest_objectives WHERE Entry=" + PQuestEntry + " ORDER BY StageId");
            if (info.Objectives.Count != ExpectedStages.Length)
                throw new InvalidOperationException("Expected " + ExpectedStages.Length + " objective rows, found " + info.Objectives.Count);

            for (int i = 0; i < ExpectedStages.Length; ++i)
            {
                PQuest_Objective row = info.Objectives[i];
                object[] want = ExpectedStages[i];
                string where = "stage " + i + " (" + row.StageName + ")";
                Expect(row.StageName, (string)want[0], where + " StageName");
                Expect(row.StageTitle, (string)want[1], where + " StageTitle");
                Expect(row.Type, (byte)want[2], where + " Type");
                Expect(row.Objective, (string)want[3], where + " Objective");
                Expect(row.Count, (ushort)want[4], where + " Count");
                Expect(row.ObjectId, (string)want[5], where + " ObjectId");
                Expect(row.ClientObjectiveId, (uint)want[6], where + " ClientObjectiveId");
                Expect(row.Time, (ushort)want[7], where + " Time");
                Expect(row.NoStageTimer != 0, (bool)want[8], where + " NoStageTimer");
                Expect(row.Description, ExpectedDescriptions[i], where + " Description");

                row.Spawns = Read<PQuest_Spawn>(connection, "SELECT * FROM pquest_spawns WHERE Objective=" + row.Guid);
            }

            // The two contraption stages each stage all four objects: the capture creates four
            // F_CREATE_STATIC contraptions at once, Stage I needs two of them destroyed and
            // Stage III needs all four after the stage reset respawns them.
            foreach (PQuest_Objective row in info.Objectives.Where(o => o.Type == 11))
            {
                if (row.Spawns.Count != 4)
                    throw new InvalidOperationException(row.StageName + ": expected 4 contraption spawns, found " + row.Spawns.Count);

                foreach (int[] want in ExpectedContraptions)
                {
                    PQuest_Spawn spawn = row.Spawns.FirstOrDefault(s => s.WorldX == want[0] && s.WorldY == want[1]);
                    if (spawn == null)
                        throw new InvalidOperationException(row.StageName + ": no contraption at " + want[0] + "," + want[1]);
                    Expect(spawn.Entry, 100517u, row.StageName + " contraption Entry");
                    Expect(spawn.ZoneId, ZoneId, row.StageName + " contraption ZoneId");
                    Expect(spawn.WorldZ, want[2], row.StageName + " contraption WorldZ");
                    Expect(spawn.WorldO, want[3], row.StageName + " contraption WorldO");
                    Expect(spawn.Type, (byte)2, row.StageName + " contraption spawn Type");
                }
            }

            GameObject_proto contraption = Read<GameObject_proto>(connection,
                "SELECT * FROM gameobject_protos WHERE Entry=100517").SingleOrDefault();
            if (contraption == null) throw new InvalidOperationException("Gameobject prototype 100517 is missing.");
            Expect(contraption.Name, "Siphoning Contraption", "gameobject_protos.Name");
            Expect(contraption.DisplayID, 7454, "gameobject_protos.DisplayID");

            // The three bosses the numbered stages target must exist as prototypes; the stage
            // credits off any death of that entry inside the zone (Creature.SetDeath).
            foreach (var boss in new[] { new { Entry = 99621u, Name = "Skeetk" },
                                         new { Entry = 99623u, Name = "Throt" },
                                         new { Entry = 99624u, Name = "Thanquol" } })
            {
                Creature_proto proto = Read<Creature_proto>(connection,
                    "SELECT * FROM creature_protos WHERE Entry=" + boss.Entry).SingleOrDefault();
                if (proto == null) throw new InvalidOperationException("Creature prototype " + boss.Entry + " (" + boss.Name + ") is missing.");
                if (!proto.Name.StartsWith(boss.Name, StringComparison.Ordinal))
                    throw new InvalidOperationException("Creature " + boss.Entry + " is '" + proto.Name + "', expected " + boss.Name);

                long spawns = (long)new MySqlCommand(
                    "SELECT COUNT(*) FROM creature_spawns WHERE ZoneId=" + ZoneId + " AND Entry=" + boss.Entry, connection).ExecuteScalar();
                if (spawns == 0)
                    throw new InvalidOperationException(boss.Name + " has no spawn in zone " + ZoneId + "; stages II/IV/V would be uncompletable.");
            }

            // Gold-bag rewards (migration 70). PQType 2 is required for a bag to roll at all, and
            // is the type that does not read PQDifficult -- which must stay 0 so the difficulty
            // byte serialises as the captured 0xFF.
            Expect(info.PQType, (byte)2, "pquest_info.PQType (gold bag requires a bag-rolling type)");

            long goldRows = (long)new MySqlCommand(
                "SELECT COUNT(*) FROM pquest_loot WHERE PQEntry=911 AND Bag=5 AND PQType=2", connection).ExecuteScalar();
            if (goldRows == 0)
                throw new InvalidOperationException("No gold-bag loot for public quest 911. Apply Database/70_thanquol_gold_bag_rewards.sql.");

            long wrongBag = (long)new MySqlCommand(
                "SELECT COUNT(*) FROM pquest_loot WHERE PQEntry=911 AND (Bag<>5 OR PQType<>2 OR PQTier<>4)", connection).ExecuteScalar();
            Expect(wrongBag, 0L, "public quest 911 loot rows outside the gold bag");

            // Currency items (crests, insignias, exchange bags) carry Career 0 and must not be
            // chest rewards; a Career-0 row would also never match the career bitmask filter.
            long currency = (long)new MySqlCommand(
                "SELECT COUNT(*) FROM pquest_loot WHERE PQEntry=911 AND Career=0", connection).ExecuteScalar();
            Expect(currency, 0L, "currency items wrongly added to the reward chest");

            // Every career must resolve to at least one candidate, or that career wins an empty bag.
            for (int careerLine = 1; careerLine <= 24; ++careerLine)
            {
                long forCareer = (long)new MySqlCommand(
                    "SELECT COUNT(*) FROM pquest_loot WHERE PQEntry=911 AND Bag=5 AND (Career & "
                    + (1 << (careerLine - 1)) + ")<>0", connection).ExecuteScalar();
                if (forCareer == 0)
                    throw new InvalidOperationException("Career line " + careerLine + " has no gold-bag reward and would win an empty bag.");
            }

            // Stage ordering and timer plumbing, through the real PublicQuest constructor.
            GameObjectService.GameObjectProtos = new Dictionary<uint, GameObject_proto> { { contraption.Entry, contraption } };
            Zone_Info zoneInfo = Read<Zone_Info>(connection, "SELECT * FROM zone_infos WHERE ZoneId=" + ZoneId).Single();
            ZoneService._Zone_Info = new List<Zone_Info> { zoneInfo };

            var pq = new PublicQuest(info);
            if (pq.Stages.Count != ExpectedStages.Length)
                throw new InvalidOperationException("PublicQuest built " + pq.Stages.Count + " stages, expected " + ExpectedStages.Length);
            for (int i = 0; i < ExpectedStages.Length; ++i)
            {
                PQuestStage stage = pq.Stages[i];
                Expect(stage.Number, i, "stage order at index " + i);
                Expect(stage.StageName, (string)ExpectedStages[i][0], "stage " + i + " label");
                Expect(stage.StageTitle, (string)ExpectedStages[i][1], "stage " + i + " title");
                Expect(stage.NoTimer, (bool)ExpectedStages[i][8], "stage " + i + " NoTimer");
            }

            Console.WriteLine("PASS: public quest 911 carries the captured Setup + five-stage sequence.");
            Console.WriteLine("PASS: both contraption stages stage all four captured Siphoning Contraption positions.");
            Console.WriteLine("PASS: Skeetk, Throt and Thanquol are present in zone " + ZoneId + ".");
            Console.WriteLine("PASS: stages I-V run with no fail timer, Setup runs on its captured 300s timer.");
            Console.WriteLine("PASS: " + goldRows + " gold-bag rewards bound to the quest, every career line covered, no currency items.");
            Console.WriteLine("These are data checks against the official captures; they do not run AI, networking or an in-client public quest.");
        }
    }

    private static void Expect<T>(T actual, T expected, string what)
    {
        if (!EqualityComparer<T>.Default.Equals(actual, expected))
            throw new InvalidOperationException(what + ": expected '" + expected + "', found '" + actual + "'");
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
                    // Creature_proto stores States as a packed string; PQuest_Spawn stores Unks
                    // the same way. Both bind through the same helper the ORM uses.
                    if (property.PropertyType == typeof(ushort[]))
                        field = Utils.ConvertStringToArray<ushort>((string)field).ToArray();
                    else if (property.PropertyType == typeof(byte[]))
                        field = Utils.ConvertStringToArray<byte>((string)field).ToArray();
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
