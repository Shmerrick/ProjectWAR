using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using Color = System.Drawing.Color;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Common;
using FrameWork;
using GameData;
using WorldServer.Configs;
using WorldServer.Managers;
using WorldServer.NetWork;
using WorldServer.Services.World;
using WorldServer.World.Map;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.World.Objects.Instances;

// Standalone net48 checks against the built server; no services or databases are started.
internal static class RuntimeRegressionChecks
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
        string root = Path.Combine(Path.GetTempPath(), "ProjectWAR-checks-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            typeof(ClientFileMgr).Assembly.GetType("WorldServer.Program").GetField("Config", BindingFlags.Static | BindingFlags.Public)
                .SetValue(null, new WorldConfigs { ZoneFolder = root });
            ZoneService._Zone_Area = new Dictionary<int, List<Zone_Area>>();
            CheckHeights(root);
            CheckOverlays(root);
            CheckRegionMembership();
            CheckInfluence();
            CheckDungeonPackets();
            CheckDeferredPublicQuestStart();
            CheckDungeonRecoveryAndLockouts();
            CheckEmptyBossBonusCleanup();
            Console.WriteLine("PASS: terrain/overlays, region snapshots, influence, PQ state, dungeon recovery/lockouts, instance packets and bonus cleanup.");
        }
        finally
        {
            // Delete only this invocation's uniquely named fixture directory.
            Directory.Delete(root, true);
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void SaveRaster(string root, int zone, string name, int width, int height, Color color)
    {
        string folder = Path.Combine(root, "zone" + zone.ToString("000"));
        Directory.CreateDirectory(folder);
        using (var bitmap = new Bitmap(width, height))
        {
            using (var graphics = Graphics.FromImage(bitmap)) graphics.Clear(color);
            bitmap.Save(Path.Combine(folder, name), ImageFormat.Png);
        }
    }

    private static void CheckHeights(string root)
    {
        SaveRaster(root, 1, "offset.png", 2, 2, Color.FromArgb(10, 0, 0));
        SaveRaster(root, 1, "terrain.png", 2, 2, Color.FromArgb(20, 0, 0));
        // Independent known sample: ((10*31 + 20)*16 - 30)/2 = 2625.
        Parallel.For(0, 2000, i => Assert(ClientFileMgr.GetHeight(1, 127, 127) == 2625, "Concurrent first height load"));
        Assert(ClientFileMgr.GetHeight(1, 0, 0) == 2625, "Height origin");
        Assert(ClientFileMgr.GetHeight(1, -1, 0) == -1, "Negative pin must not truncate to origin");
        Assert(ClientFileMgr.GetHeight(1, 128, 0) == -1, "Exclusive width bound");
        Assert(ClientFileMgr.GetHeight(1, 0, 128) == -1, "Exclusive height bound");
        Assert(ClientFileMgr.GetHeight(1, int.MaxValue, int.MaxValue) == -1, "Oversized pins");
        Assert(ClientFileMgr.GetHeight(2, 0, 0) == -1, "Missing height must remain unavailable, not zero");
        SaveRaster(root, 3, "offset.png", 2, 2, Color.Red);
        SaveRaster(root, 3, "terrain.png", 1, 1, Color.Red);
        Assert(ClientFileMgr.GetHeight(3, 0, 0) == -1, "Mismatched rasters must not partially load");
        // A sampled image must no longer be held open by GDI+.
        using (File.Open(Path.Combine(root, "zone001", "offset.png"), FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
        using (File.Open(Path.Combine(root, "zone003", "offset.png"), FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
    }

    private static void CheckOverlays(string root)
    {
        ZoneService._Zone_Area[4] = new List<Zone_Area> { new Zone_Area { ZoneId = 4, PieceId = 0 } };
        var missing = new ClientZoneInfo(4);
        Assert(missing.GetZoneAreaFor(0, 0, 4) == null, "Missing area raster must not fabricate PieceId 0");
        Assert(missing.AreaPixels == null && missing.PQAreaPixels == null, "Missing maps must not allocate empty megabyte grids");

        SaveRaster(root, 5, "areas005.png", 1, 1, Color.Black);
        SaveRaster(root, 5, "pqarea005.png", 1024, 1024, Color.FromArgb(32, 16, 0));
        var partial = new ClientZoneInfo(5);
        Assert(partial.AreaPixels == null, "Wrong-sized area raster must be rejected atomically");
        Assert(partial.GetPQAreaFor(65535, 65535, 5) == 4, "Bad area map must not suppress good PQ map");
        Assert(partial.GetPQAreaFor(0, 0, 6) == 0, "Mismatched zone must not reuse PQ map");

        SaveRaster(root, 6, "areas006.png", 1024, 1024, Color.Black);
        ZoneService._Zone_Area[6] = new List<Zone_Area> { new Zone_Area { ZoneId = 6, PieceId = 1 } };
        var valid = new ClientZoneInfo(6);
        Assert(valid.GetZoneAreaFor(65535, 65535, 6).PieceId == 1, "Valid map edge lookup");
        Assert(valid.GetZoneAreaFor(0, 0, 5) == null, "Mismatched zone must not reuse area map");
        var gunbadArea = new Zone_Area { ZoneId = 60, AreaId = 31, PieceId = 1, OrderInfluenceId = 64, DestroInfluenceId = 65 };
        ZoneService._Zone_Area[60] = new List<Zone_Area> { gunbadArea };
        var gunbad = new ClientZoneInfo(60);
        gunbad.AreaPixels = new byte[1024, 1024];
        for (byte piece = 1; piece <= 8; ++piece)
        {
            gunbad.AreaPixels[piece, piece] = piece;
            Assert(gunbad.GetZoneAreaFor((ushort)(piece * 64), (ushort)(piece * 64), 60) == gunbadArea,
                "Every Gunbad piece retains the enclosing influence area");
        }
        Assert(gunbad.GetZoneAreaFor(0, 0, 160) == null, "Gunbad enclosing area must not leak into other zones");
    }

    private static void CheckRegionMembership()
    {
        // Bypass world initialization; exercise the production membership methods with inert players.
        var region = (RegionMgr)FormatterServices.GetUninitializedObject(typeof(RegionMgr));
        typeof(RegionMgr).GetField("_players", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(region, new List<Player>());
        typeof(RegionMgr).GetField("_playerSnapshot", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(region, Array.AsReadOnly(new Player[0]));
        var add = (Action<Player>)Delegate.CreateDelegate(typeof(Action<Player>), region, "AddPlayer");
        var remove = (Action<Player>)Delegate.CreateDelegate(typeof(Action<Player>), region, "RemovePlayer");
        var order = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var destro = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        order.Realm = Realms.REALMS_REALM_ORDER;
        destro.Realm = Realms.REALMS_REALM_DESTRUCTION;
        add(order);
        ReadOnlyCollection<Player> held = region.Players;
        add(order);
        add(destro);
        Assert(held.Count == 1 && held[0] == order, "Held snapshot must remain unchanged after an add");
        Assert(region.OrderPlayers == 1 && region.DestPlayers == 1, "Duplicate joins must not inflate counts");
        remove(order);
        remove(order);
        remove(destro);
        Assert(held.Count == 1 && region.Players.Count == 0, "Held snapshot must survive removals");
        Assert(region.OrderPlayers == 0 && region.DestPlayers == 0, "Departures must decrement counts exactly once");

        Parallel.Invoke(
            () => { for (int i = 0; i < 5000; i++) { add(order); add(destro); remove(order); remove(destro); } },
            () =>
            {
                for (int i = 0; i < 20000; i++)
                {
                    var snapshot = region.Players;
                    int count = 0;
                    foreach (Player player in snapshot) { Assert(player != null, "Torn membership snapshot"); count++; }
                    Assert(count == snapshot.Count, "Membership changed during enumeration");
                }
            });
        Assert(region.Players.Count == 0 && region.OrderPlayers == 0 && region.DestPlayers == 0, "Final concurrent membership counts");
    }

    private sealed class CaptureClient : GameClient
    {
        // Never constructed: an inert client is allocated below without opening sockets.
        public CaptureClient() : base(null) { }
        public byte[] LastPacket;
        public List<byte[]> Packets;
        public override void SendPacket(PacketOut packet)
        {
            LastPacket = packet.ToArray();
            if (Packets != null) Packets.Add(LastPacket);
        }
    }

    private static uint ReadUInt32(byte[] bytes, int offset)
    {
        return ((uint)bytes[offset] << 24) | ((uint)bytes[offset + 1] << 16) |
            ((uint)bytes[offset + 2] << 8) | bytes[offset + 3];
    }

    private static void CheckInfluence()
    {
        var chapter = new Chapter_Info { Entry = 2, InfluenceEntry = 128, CreatureEntry = 1,
            Tier1InfluenceCount = 8120, Tier2InfluenceCount = 32940, Tier3InfluenceCount = 75150 };
        typeof(ChapterService).GetField("_chaptersByInfluence", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, new Dictionary<uint, Chapter_Info> { { 128, chapter } });
        Assert(ChapterService.GetChapterEntry(128) == chapter && ChapterService.GetChapterEntry(2) == null,
            "Influence lookup must use the client track, not the chapter row key");

        var player = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var client = (CaptureClient)FormatterServices.GetUninitializedObject(typeof(CaptureClient));
        var influence = new Characters_influence(1, 128, 65530);
        player.Info = new Character { Influences = new List<Characters_influence> { influence } };
        player.TokInterface = new TokInterface();
        player.ItmInterface = new ItemsInterface();
        player.Client = client;
        PacketOut.SizeLen = 2;
        player.AddInfluence(128, 15);
        Assert(influence.InfluenceCount == 65545, "Crossing ushort boundary must retain progress");
        Assert(client.LastPacket.Length == 15 && ReadUInt32(client.LastPacket, 7) == 65545, "Update packet uint32 at payload +4");
        player.AddInfluence(128, 10000);
        Assert(influence.InfluenceCount == 75150 && ReadUInt32(client.LastPacket, 7) == 75150, "Cap must not wrap to 9614");
        player.SetInfluence(128, uint.MaxValue);
        Assert(influence.InfluenceCount == 75150 && ReadUInt32(client.LastPacket, 7) == 75150, "SetInfluence clamps without narrowing");
        influence.InfluenceCount = uint.MaxValue;
        player.AddInfluence(128, 15);
        Assert(influence.InfluenceCount == 75150, "Corrupt over-cap totals must not overflow before clamping");
        player.SendInfluenceItems(128);
        // No item fixtures: each tier is a 16-byte block after the two-byte chapter header.
        Assert(ReadUInt32(client.LastPacket, 5) == 8120, "First reward cost");
        Assert(ReadUInt32(client.LastPacket, 21) == 32940, "Second reward cost");
        Assert(ReadUInt32(client.LastPacket, 37) == 75150, "Third reward cost must not narrow to 16 bits");
    }

    private static void CheckDungeonPackets()
    {
        var player = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var client = (CaptureClient)FormatterServices.GetUninitializedObject(typeof(CaptureClient));
        client.Packets = new List<byte[]>();
        player.Client = client;
        player.Realm = Realms.REALMS_REALM_DESTRUCTION;
        player.ObjectsInRange = new List<WorldServer.World.Objects.Object>();
        player.QtsInterface = new QuestsInterface();
        player.QtsInterface.SetOwner(player);
        var info = new Zone_Info { ZoneId = 60, OffX = 200, OffY = 200, Type = 4 };
        ZoneService._Zone_Info = new List<Zone_Info> { info };
        var zone = (ZoneMgr)FormatterServices.GetUninitializedObject(typeof(ZoneMgr));
        zone.ZoneId = 60;
        zone.Info = info;
        typeof(WorldServer.World.Objects.Object).GetProperty("Zone").SetValue(player, zone, null);
        player.CurrentArea = new Zone_Area { ZoneId = 60, AreaId = 31, Realm = 0,
            OrderInfluenceId = 64, DestroInfluenceId = 65 };
        var outgoing = new List<PacketOut>();
        typeof(Player).GetField("_packetOut", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, outgoing);
        player.SendChapterBar();
        Assert(!player.IsInRvRLake, "Gunbad tracker activation must not enable PvP lake rules");
        byte[] zonePacket = outgoing[0].ToArray();
        byte[] areaPacket = outgoing[1].ToArray();
        player.ClearZone();
        Assert(BitConverter.ToString(zonePacket, 3) == "00-3C-11-01-01-01-00-00-00-00",
            "Gunbad zone activation: official INSTANCE_GUNBAD_PART1 packet 335");
        Assert(BitConverter.ToString(areaPacket, 3) == "00-1F-11-02-01-01-00-00-00-00",
            "Gunbad area activation: official INSTANCE_GUNBAD_PART1 packet 381");

        var pqInfo = new PQuest_Info { Entry = 513, Name = "A Taint from Below", ZoneId = 60,
            Type = 0, ChapterId = 65, PQDifficult = 3, Objectives = new List<PQuest_Objective>() };
        pqInfo.Objectives.Add(new PQuest_Objective { Guid = 2298, StageName = "Stage I", Type = 2,
            ObjectId = "36554", Objective = "Oozespawn Nurgling", Description = "test", Count = 24 });
        var pq = new PublicQuest(pqInfo);
        pq.Stage = pq.Stages[0];
        client.Packets.Clear();
        pq.SendCurrentStage(player);
        byte[] pqPacket = client.Packets[0];
        Assert(pqPacket[8] == 0 && pqPacket[13 + pqInfo.Name.Length] == 2,
            "Dungeon PQ realm remains 0, independent post-name field is 2");
        Assert(pqPacket[pqPacket.Length - 5] == 65, "PQ trailer uses Destruction Gunbad influence, not hardcoded 72");
        Assert(ReadUInt32(pqPacket, pqPacket.Length - 17) == 0 && ReadUInt32(pqPacket, pqPacket.Length - 13) == 0,
            "Untimed first stage must not serialize a wrapped negative timestamp");
        int difficultyOffset = 13 + pqInfo.Name.Length + 7 + 7 + "Oozespawn Nurgling".Length;
        Assert(pqPacket[difficultyOffset] == 255, "Official Gunbad PQ difficulty sentinel");
        player.Realm = Realms.REALMS_REALM_ORDER;
        client.Packets.Clear();
        pq.SendCurrentStage(player);
        Assert(client.Packets[0][pqPacket.Length - 5] == 64, "Order Gunbad PQ uses Order influence");

        // Exercise a real counter update, with no DB award/contribution for a synthetic event.
        pq.Stage.Objectives[0].Objective.Type = (byte)Objective_Type.QUEST_SCRIPTED_EVENT;
        typeof(PublicQuest).GetField("_started", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pq, true);
        pq.ActivePlayers.Add(0);
        Player.PlayersByCharId[0] = player;
        try
        {
            client.Packets.Clear();
            pq.HandleEvent(null, Objective_Type.QUEST_SCRIPTED_EVENT, 2298, 1, 0);
            Assert(client.Packets[0][7] == 1 && client.Packets[0][8] == 2,
                "Dungeon progress uses objective-list kind 2, not neutral realm 0");
            Assert(pq.Stage.Objectives[0].Count == 1, "Counter advances once");
            client.Packets.Clear();
            pq.End();
            Assert(client.Packets.Exists(bytes => bytes[2] == (byte)WorldServer.NetWork.Opcodes.F_OBJECTIVE_INFO && bytes[7] == 1),
                "Completion must replace the active stage tracker with reset state");
            pq.HandleEvent(null, Objective_Type.QUEST_SCRIPTED_EVENT, 2298, 1, 0);
            Assert(pq.Stage.Objectives[0].Count == 1, "Completed PQ must reject late events");
        }
        finally { Player.PlayersByCharId.Remove(0); }

        Assert(typeof(GoldChest).GetMethod("SendMeTo").DeclaringType == typeof(GameObject),
            "Reward chest must reuse the single instance-aware static-object writer");

        typeof(Player).GetField("_clientInstanceZoneId", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, (ushort)60);
        typeof(Player).GetField("_clientInstanceShiftX", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, (ushort)1);
        typeof(Player).GetField("_clientInstanceShiftY", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, (ushort)9);
        var jump = typeof(Player).GetMethod("BuildPlayerJumpPacket", BindingFlags.Instance | BindingFlags.NonPublic);
        var packet = (PacketOut)jump.Invoke(player, new object[] { (ushort)60, (uint)851702, (uint)849059, (ushort)28906, (ushort)3956 });
        Assert(ReadUInt32(packet.ToArray(), 3) == 40694 && ReadUInt32(packet.ToArray(), 7) == 103587,
            "Respawn jump must translate world coordinates into the initialized instance atlas");
        packet = (PacketOut)jump.Invoke(player, new object[] { (ushort)100, (uint)851702, (uint)849059, (ushort)28906, (ushort)3956 });
        Assert(ReadUInt32(packet.ToArray(), 3) == 851702 && ReadUInt32(packet.ToArray(), 7) == 849059,
            "Ordinary world jump coordinates must remain unchanged");
        var respawn = new SpawnPoint(new Zone_Respawn { ZoneID = 60, InZoneID = 0,
            PinX = 32502, PinY = 29859, PinZ = 28906 });
        Assert(respawn.ZoneId == 60 && respawn.X == 851702 && respawn.Y == 849059,
            "Zero optional respawn zone means own zone, not zone zero");
        foreach (string input in new[] { "on", "off", "99999999999999999999999999999", "-1", "2" })
        {
            var arguments = new List<string> { input };
            var fly = typeof(Player).Assembly.GetType("WorldServer.Managers.Commands.BaseCommands").GetMethod("SetFlightState");
            Assert((bool)fly.Invoke(null, new object[] { player, arguments }), "Invalid flight input must be handled");
            Assert(player.FlightEnabled == 0, "Invalid flight input must not mutate flight state");
        }
    }

    private static void CheckDeferredPublicQuestStart()
    {
        var info = new PQuest_Info { Entry = 264, Name = "Deferred PQ fixture", Objectives = new List<PQuest_Objective>() };
        info.Objectives.Add(new PQuest_Objective { Guid = 1170, StageName = "Stage I", Type = 2,
            ObjectId = "344", Objective = "Refugees", Description = "fixture", Spawns = new List<PQuest_Spawn>() });
        var pq = new PublicQuest(info);
        pq.Start();
        Assert(pq.Stage == null, "Area attachment before PQ OnLoad must defer startup");
        pq.Loaded = true;
        pq.Start();
        Assert(pq.Stage == pq.Stages[0], "Deferred startup must remain possible after load");
    }

    private static void CheckDungeonRecoveryAndLockouts()
    {
        ZoneService._Zone_Info = new List<Zone_Info>
        {
            new Zone_Info { ZoneId = 60, Type = 4 },
            new Zone_Info { ZoneId = 161, Type = 0, OffX = 10, OffY = 10 },
            new Zone_Info { ZoneId = 162, Type = 0, OffX = 20, OffY = 20 },
            new Zone_Info { ZoneId = 100, Type = 0, OffX = 30, OffY = 30 }
        };
        var instance = new Instance_Info { Entry = 60, ZoneID = 60 };
        InstanceService._InstanceInfo = new Dictionary<uint, Instance_Info> { { 60, instance } };
        var exit = new Zone_jump { Entry = 123, ZoneID = 100, WorldX = 123000, WorldY = 123000, WorldZ = 1000 };
        ZoneService.Zone_Jumps = new Dictionary<uint, Zone_jump> { { 0, exit }, { 123, exit } };
        var respawns = (List<Zone_Respawn>)typeof(ZoneService).GetField("_zoneRespawns", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        respawns.Clear();
        respawns.Add(new Zone_Respawn { ZoneID = 161, Realm = 2, PinX = 100, PinY = 100, PinZ = 1000 });
        respawns.Add(new Zone_Respawn { ZoneID = 162, Realm = 1, PinX = 100, PinY = 100, PinZ = 1000 });
        Assert(ZoneService.GetZoneJump(0) == null, "Unconfigured exit must never resolve the legacy jump-zero row");
        Assert(WorldMgr.GetDungeonLoginDestination(60, 2).ZoneID == 161, "Destruction fallback is its capital");
        Assert(WorldMgr.GetDungeonLoginDestination(60, 1).ZoneID == 162, "Order fallback is its capital");
        instance.DestrExitZoneJumpID = 123;
        Assert(WorldMgr.GetDungeonLoginDestination(60, 2) == exit, "Configured valid exit takes precedence");
        exit.WorldX = 1;
        Assert(WorldMgr.GetDungeonLoginDestination(60, 2).ZoneID == 161, "Out-of-zone exit falls back safely");
        exit.WorldX = 123000;
        exit.ZoneID = 60;
        Assert(WorldMgr.GetDungeonLoginDestination(60, 2).ZoneID == 161, "Recovery cannot loop into an instance");
        respawns.Clear();
        Assert(WorldMgr.GetDungeonLoginDestination(60, 2) == null, "Missing capital data must not invent coordinates");

        int future = TCPManager.GetTimeStamp() + 3600;
        var value = new Character_value { Lockouts = "~164:" + future + ":161:164" };
        var resolve = typeof(InstanceMgr).GetMethod("ResolveCharacterLockout", BindingFlags.Static | BindingFlags.NonPublic);
        var restored = (Instance_Lockouts)resolve.Invoke(null, new object[] { value, (ushort)164 });
        Assert(restored.InstanceID == "~164:" + future && restored.Bosseskilled == "161:164",
            "Character lockout must separate its key and killed-boss list");
        var copy = (Instance)FormatterServices.GetUninitializedObject(typeof(Instance));
        copy.Lockout = restored;
        Assert(copy.IsBossKilled(161) && !copy.IsBossKilled(162), "Restored boss suppression uses the saved IDs");
        value.Lockouts = "~164:1:161";
        Assert(resolve.Invoke(null, new object[] { value, (ushort)164 }) == null, "Expired lockout cannot suppress a fresh boss");
        value.Lockouts = "~164:invalid:161";
        Assert(resolve.Invoke(null, new object[] { value, (ushort)164 }) == null, "Malformed lockout must not throw");
    }

    private static void CheckEmptyBossBonusCleanup()
    {
        Type statType = typeof(StatsInterface).GetNestedType("UnitStat", BindingFlags.NonPublic);
        object stat = Activator.CreateInstance(statType, new object[] { new StatsInterface() });
        MethodInfo remove = statType.GetMethod("RemoveBonusMultiplier");
        MethodInfo add = statType.GetMethod("AddBonusMultiplier");
        remove.Invoke(stat, new object[] { 1f, (byte)0 });
        Assert((float)statType.GetProperty("TotalBonusMult").GetValue(stat, null) == 1f, "Absent bonus cleanup is a no-op");
        add.Invoke(stat, new object[] { 1f, (byte)0 });
        remove.Invoke(stat, new object[] { 1f, (byte)1 });
        Assert((float)statType.GetProperty("TotalBonusMult").GetValue(stat, null) == 2f, "Missing bucket must not remove another bucket's bonus");
        remove.Invoke(stat, new object[] { 1f, (byte)0 });
        remove.Invoke(stat, new object[] { 1f, (byte)0 });
        Assert((float)statType.GetProperty("TotalBonusMult").GetValue(stat, null) == 1f, "Repeated standard cleanup must not change the multiplier");
    }
}
