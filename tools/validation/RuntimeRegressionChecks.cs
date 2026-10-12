using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using Color = System.Drawing.Color;
using Item = WorldServer.World.Objects.Item;
using System.Text;
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
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Positions;

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
            CheckCooldowns();
            CheckItemCooldownNotifications();
            CheckEconomyGuards();
            CheckInventoryTransferGuards();
            CheckChannelRange();
            CheckStaleChannelCallback();
            CheckDungeonPackets();
            CheckDeferredPublicQuestStart();
            CheckOrdinaryCreatureQuestCredit();
            CheckDungeonRecoveryAndLockouts();
            CheckGroupInstanceSelection();
            CheckEmptyBossBonusCleanup();
            Console.WriteLine("PASS: terrain/overlays, region snapshots, influence, PQ state, dungeon recovery/lockouts, instance packets and bonus cleanup.");
        }
        finally
        {
            // Delete only this invocation's uniquely named fixture directory.
            Directory.Delete(root, true);
        }
    }

    private static void CheckGroupInstanceSelection()
    {
        var select = typeof(InstanceMgr).GetMethod("IsGroupInstanceCandidate", BindingFlags.Static | BindingFlags.NonPublic);
        Assert(select != null, "Group instance destination filter exists");
        var cave = (Instance)FormatterServices.GetUninitializedObject(typeof(Instance));
        cave.Info = new Instance_Info { Entry = 60, ZoneID = 60 };
        cave.ZoneID = 60;
        typeof(Instance).GetField("Realm").SetValue(cave, (byte)1);
        foreach (ushort zone in new ushort[] { 63, 64, 65, 66 })
        {
            var destination = new Instance_Info { Entry = 60, ZoneID = zone };
            Assert(!(bool)select.Invoke(null, new object[] { cave, destination }), "Boss portal rejects realm cave");
            var boss = (Instance)FormatterServices.GetUninitializedObject(typeof(Instance));
            boss.Info = destination;
            boss.ZoneID = zone;
            Assert((bool)select.Invoke(null, new object[] { boss, destination }), "Boss portal accepts matching group map");
            foreach (ushort other in new ushort[] { 63, 64, 65, 66 })
                if (other != zone)
                    Assert(!(bool)select.Invoke(null, new object[] { boss, new Instance_Info { Entry = 60, ZoneID = other } }),
                        "Boss portal rejects another wing sharing the dungeon entry");
            typeof(Instance).GetField("Realm").SetValue(boss, (byte)2);
            Assert(!(bool)select.Invoke(null, new object[] { boss, destination }), "Group portal rejects a realm copy even on the same map");
        }
        Assert(!(bool)select.Invoke(null, new object[] { null, cave.Info }), "Null instance rejected");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void CheckStaleChannelCallback()
    {
        var host = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var handler = new NewChannelHandler(null, host);
        var cancelled = new AbilityInfo();
        var current = new AbilityInfo();
        var staleBuff = new WorldServer.World.Abilities.Buffs.NewBuff();
        var currentBuff = new WorldServer.World.Abilities.Buffs.NewBuff();
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(NewChannelHandler).GetField("_channelInfo", fields).SetValue(handler, current);
        var buffField = typeof(NewChannelHandler).GetField("_channelBuff", fields);
        buffField.SetValue(handler, currentBuff);
        var callback = typeof(NewChannelHandler).GetMethod("ChannelInitialization", fields);
        callback.Invoke(handler, new object[] { staleBuff, cancelled });
        Assert(staleBuff.BuffHasExpired && staleBuff.ChannelHandler == null,
            "A cancelled cast's delayed buff must expire without joining the current channel");
        Assert(ReferenceEquals(buffField.GetValue(handler), currentBuff) && handler.HasInfo(),
            "A stale callback must preserve the current channel and its buff");
        callback.Invoke(handler, new object[] { null, cancelled });
        Assert(handler.HasInfo(), "A stale failed callback must not cancel the current channel");
        handler.NotifyBuffStarted(staleBuff);
        Assert(handler.HasInfo(), "A stale buff start must not emit a packet for the current channel");
        // A late callback after cancellation with no replacement is rejected too.
        typeof(NewChannelHandler).GetField("_channelInfo", fields).SetValue(handler, null);
        var lateBuff = new WorldServer.World.Abilities.Buffs.NewBuff();
        callback.Invoke(handler, new object[] { lateBuff, current });
        Assert(lateBuff.BuffHasExpired && !handler.HasInfo(), "Late callback must not resurrect a channel");
        Console.WriteLine("PASS: cancelled/replaced channel callbacks cannot affect a newer cast.");
    }

    private static void CheckChannelRange()
    {
        // Exercise a real channel tick with inert units, without starting a buff or a server.
        var host = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var target = (Unit)FormatterServices.GetUninitializedObject(typeof(Unit));
        target.Health = 1;
        var position = typeof(WorldServer.World.Objects.Object).GetField("WorldPosition");
        position.SetValue(host, new Point3D());
        position.SetValue(target, new Point3D { X = 1200 });
        Assert(!host.IsInCastRange(target, 25), "Channel fixture must exceed the old 25-foot limit");

        var channel = new NewChannelHandler(null, host);
        var info = new AbilityInfo
        {
            ConstantInfo = new AbilityConstants { ChannelDuration = 60000, ChannelInterval = 2000 },
            Range = 0
        };
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(NewChannelHandler).GetField("_channelInfo", fields).SetValue(channel, info);
        typeof(NewChannelHandler).GetField("_target", fields).SetValue(channel, target);
        typeof(NewChannelHandler).GetField("_channelStartTime", fields).SetValue(channel, TCPManager.GetTimeStampMS());
        var nextTick = typeof(NewChannelHandler).GetField("_nextTickTime", fields);
        nextTick.SetValue(channel, 0L);
        channel.Update(TCPManager.GetTimeStampMS());
        Assert(channel.HasInfo() && (long)nextTick.GetValue(channel) == 2000,
            "A targeted zero-range channel must continue beyond 25 feet and advance its tick");

        info.Range = 150;
        nextTick.SetValue(channel, 0L);
        channel.Update(TCPManager.GetTimeStampMS());
        Assert(channel.HasInfo() && (long)nextTick.GetValue(channel) == 2000,
            "A nonzero-range channel must continue when its target is within range");
        Console.WriteLine("PASS: zero-range and in-range channel ticks.");
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
        public bool FailSend;
        public override void SendPacket(PacketOut packet)
        {
            if (FailSend) throw new InvalidOperationException("Injected send failure");
            LastPacket = packet.ToArray();
            if (Packets != null) Packets.Add(LastPacket);
        }
    }

    /// <summary>Records each offered stack's count, as HandleTrade does when an offer is committed.</summary>
    private static void SnapshotTradeOffer(ItemsInterface trader)
    {
        ushort[] offered = new ushort[ItemsInterface.MAX_TRADE_SLOT];
        for (int i = 0; i < offered.Length; i++)
            offered[i] = trader.TradeItems[i].Value == null ? (ushort)0 : trader.TradeItems[i].Value.Count;
        typeof(ItemsInterface).GetField("_tradeCounts", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(trader, offered);
    }

    private static void SetPlayerOwner(ItemsInterface items, Player owner)
    {
        items.SetOwner(owner);
        typeof(ItemsInterface).GetField("_playerOwner", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(items, owner);
    }

    private static uint ReadUInt32(byte[] bytes, int offset)
    {
        return ((uint)bytes[offset] << 24) | ((uint)bytes[offset + 1] << 16) |
            ((uint)bytes[offset + 2] << 8) | bytes[offset + 3];
    }

    private static void CheckEconomyGuards()
    {
        var player = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var client = (CaptureClient)FormatterServices.GetUninitializedObject(typeof(CaptureClient));
        client.Packets = new List<byte[]>();
        player.Client = client;
        player._Value = new Character_value();
        PacketOut.SizeLen = 2;

        // BUG-179: a refused creation must not have merged anything into existing stacks.
        var bags = new ItemsInterface { Items = new Item[ItemsInterface.OVERFLOW_END_SLOT] };
        var potion = new Item_Info { Entry = 5000, MaxStack = 10 };
        var filler = new Item_Info { Entry = 5001, MaxStack = 1 };
        for (ushort slot = ItemsInterface.INVENTORY_START_SLOT; slot < ItemsInterface.INVENTORY_START_SLOT + 32; slot++)
            bags.Items[slot] = new Item(player) { Info = filler, CharSaveInfo = new CharacterItem { Counts = 1 } };
        bags.Items[ItemsInterface.INVENTORY_START_SLOT] = new Item(player) { Info = potion, CharSaveInfo = new CharacterItem { Counts = 5 } };
        Assert(bags.CreateItem(potion, 20) == ItemResult.RESULT_MAX_BAG, "Full bags refuse an overflowing stack");
        Assert(bags.Items[ItemsInterface.INVENTORY_START_SLOT].Count == 5, "Refused creation leaves the partial stack untouched");
        // BUG-183: N non-stackables need N slots; this used to report success and create nothing.
        Assert(bags.CreateItem(filler, 3) == ItemResult.RESULT_MAX_BAG, "Three non-stackables need three slots");
        Assert(bags.CreateItem(new Item_Info { Entry = 5003, MaxStack = 0 }, 2) == ItemResult.RESULT_MAX_BAG,
            "MaxStack 0 items are counted as one per slot");

        // BUG-178: the same stack offered in two trade slots is refused at execution.
        var trader = new ItemsInterface { Items = new Item[ItemsInterface.OVERFLOW_END_SLOT] };
        trader.SetOwner(player);
        var sword = new Item(player) { Info = new Item_Info { Entry = 5002, MaxStack = 1 }, CharSaveInfo = new CharacterItem { Counts = 1 } };
        trader.Items[ItemsInterface.INVENTORY_START_SLOT] = sword;
        MethodInfo validate = typeof(ItemsInterface).GetMethod("ValidateTradeOffer", BindingFlags.Instance | BindingFlags.NonPublic);
        object[] counts = new object[2];
        // HandleTrade records each offered stack's count beside the offer; tests that set the offer
        // directly record it the same way, so each refusal below is for the reason it names.
        Action<ItemsInterface> recordOffer = SnapshotTradeOffer;
        trader.TradeItems = new KeyValuePair<ushort, Item>[ItemsInterface.MAX_TRADE_SLOT];
        trader.TradeItems[0] = new KeyValuePair<ushort, Item>(ItemsInterface.INVENTORY_START_SLOT, sword);
        recordOffer(trader);
        Assert((bool)validate.Invoke(trader, counts) && (ushort)counts[0] == 1 && (ushort)counts[1] == 1,
            "A single offered stack validates, needing and freeing one main-bag slot");
        trader.TradeItems[1] = new KeyValuePair<ushort, Item>(ItemsInterface.INVENTORY_START_SLOT, sword);
        recordOffer(trader);
        Assert(!(bool)validate.Invoke(trader, counts), "A stack offered twice is refused");
        trader.TradeItems[1] = new KeyValuePair<ushort, Item>(0, null);
        recordOffer(trader);
        trader.Items[ItemsInterface.INVENTORY_START_SLOT] = null;
        trader.Items[ItemsInterface.INVENTORY_START_SLOT + 1] = sword;
        Assert(!(bool)validate.Invoke(trader, counts), "A stack moved after the offer is refused");
        trader.Items[ItemsInterface.INVENTORY_START_SLOT + 1] = null;
        trader.Items[10] = sword;
        trader.TradeItems[0] = new KeyValuePair<ushort, Item>(10, sword);
        recordOffer(trader);
        Assert(!(bool)validate.Invoke(trader, counts), "Equipped items cannot be traded");
        // BUG-185: selling an equipped item cleared the slot without unequipping it.
        sword.Info.SellPrice = 100;
        trader.SellItem(new WorldServer.NetWork.Handler.InteractMenu { Num = 10, Count = 1 });
        Assert(trader.Items[10] == sword && player._Value.Money == 0, "Equipped items cannot be sold");

        // BUG-195: only broken items (2500000-2599999) are repairable; any Craftresult item used to convert.
        var pinion = new Item(player) { Info = new Item_Info { Entry = 82150, MaxStack = 20, Craftresult = "188917;188917;188917;0" },
            CharSaveInfo = new CharacterItem { Counts = 5 } };
        trader.Items[ItemsInterface.INVENTORY_START_SLOT + 3] = pinion;
        trader.RepairItem(new WorldServer.NetWork.Handler.InteractMenu { Num = (byte)(ItemsInterface.INVENTORY_START_SLOT + 3) });
        Assert(trader.Items[ItemsInterface.INVENTORY_START_SLOT + 3] == pinion && pinion.Count == 5,
            "A non-broken item with a Craftresult is not repaired");
        Assert(ItemsInterface.IsRepairable(new Item_Info { Entry = 2500001 }) && !ItemsInterface.IsRepairable(pinion.Info),
            "Repairable range matches the one Item.BuildItem advertises");

        // BUG-196: crafting lock leaks, swapped ingredients and phantom bonus removal.
        {
            player.ItmInterface = trader;
            var craft = new CraftingApoInterface();
            craft.SetOwner(player);
            FieldInfo stageField = typeof(CraftingApoInterface).GetField("_stage", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo stabilityField = typeof(CraftingApoInterface).GetField("_stability", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo lockField = typeof(CraftingApoInterface).GetField("locked", BindingFlags.Instance | BindingFlags.NonPublic);
            ushort potSlot = (ushort)(ItemsInterface.INVENTORY_START_SLOT + 10), mainSlot = (ushort)(potSlot + 1),
                addSlot = (ushort)(potSlot + 2), otherSlot = (ushort)(potSlot + 3);
            craft.AddContainer((ushort)(ItemsInterface.INVENTORY_START_SLOT + 20)); // empty slot
            Assert(!(bool)lockField.GetValue(craft), "An empty slot does not leave crafting locked");
            trader.Items[potSlot] = new Item(player) { Info = new Item_Info { Entry = 6000, Crafts = "8:5" }, CharSaveInfo = new CharacterItem { Counts = 1 } };
            trader.Items[mainSlot] = new Item(player) { Info = new Item_Info { Entry = 6001, Crafts = "8:2", Craftresult = "100;101;102" }, CharSaveInfo = new CharacterItem { Counts = 1 } };
            trader.Items[addSlot] = new Item(player) { Info = new Item_Info { Entry = 6002, Crafts = "8:3;1:5" }, CharSaveInfo = new CharacterItem { Counts = 1 } };
            trader.Items[otherSlot] = new Item(player) { Info = new Item_Info { Entry = 6003, Crafts = "8:3;1:65531" }, CharSaveInfo = new CharacterItem { Counts = 1 } };
            craft.AddContainer(potSlot);
            craft.AddContainer(mainSlot);
            craft.AddContainer(mainSlot);
            Assert((byte)stageField.GetValue(craft) == 3, "A single item cannot be both main ingredient and additive");
            craft.AddContainer(addSlot);
            Assert((short)stabilityField.GetValue(craft) == 5, "Additive bonuses apply once");
            craft.RemoveContainer(otherSlot);
            Assert((short)stabilityField.GetValue(craft) == 5, "Removing a slot that was never added changes nothing");
            Item rare = trader.Items[addSlot];
            trader.Items[addSlot] = trader.Items[otherSlot];
            trader.Items[otherSlot] = rare;
            craft.Craft();
            Assert((byte)stageField.GetValue(craft) == 0 && trader.Items[addSlot].Count == 1 && !(bool)lockField.GetValue(craft),
                "A slot swapped after adding aborts the craft without consuming, and the lock is released");
        }

        // BUG-196 (talisman side): same lock and swapped-ingredient guards.
        {
            var tal = new CraftingTalInterface();
            tal.SetOwner(player);
            player._Value.CraftingSkill = 5;
            FieldInfo talStage = typeof(CraftingTalInterface).GetField("_stage", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo talLock = typeof(CraftingTalInterface).GetField("locked", BindingFlags.Instance | BindingFlags.NonPublic);
            ushort first = (ushort)(ItemsInterface.INVENTORY_START_SLOT + 20);
            tal.AddContainer((ushort)(first + 9)); // empty slot
            Assert(!(bool)talLock.GetValue(tal), "An empty slot does not leave talisman crafting locked");
            string[] roles = { "8:13", "8:14", "8:15", "8:16", "8:17" };
            for (int r = 0; r < roles.Length; r++)
                trader.Items[first + r] = new Item(player) { Info = new Item_Info { Entry = (uint)(6100 + r), Crafts = roles[r], Craftresult = "100;101", Unk27 = new byte[27] },
                    CharSaveInfo = new CharacterItem { Counts = 1 } };
            trader.Items[first + 5] = new Item(player) { Info = new Item_Info { Entry = 6199, Crafts = "8:17", Unk27 = new byte[27] }, CharSaveInfo = new CharacterItem { Counts = 1 } };
            for (int r = 0; r < roles.Length; r++)
                tal.AddContainer((ushort)(first + r));
            Assert((byte)talStage.GetValue(tal) == 3, "All five talisman roles filled");
            Item kept = trader.Items[first + 4];
            trader.Items[first + 4] = trader.Items[first + 5];
            trader.Items[first + 5] = kept;
            tal.Craft();
            Assert((byte)talStage.GetValue(tal) == 0 && trader.Items[first + 4].Count == 1 && !(bool)talLock.GetValue(tal),
                "A swapped talisman ingredient aborts the craft without consuming");
        }

        // System guilds: GM accounts hold rank 9 (every permission); everyone else keeps their rank.
        {
            var systemGuild = (WorldServer.World.Guild.Guild)FormatterServices.GetUninitializedObject(typeof(WorldServer.World.Guild.Guild));
            systemGuild.Info = new Guild_info { Name = "Forces of Order", LeaderId = 0 };
            var playerGuild = (WorldServer.World.Guild.Guild)FormatterServices.GetUninitializedObject(typeof(WorldServer.World.Guild.Guild));
            playerGuild.Info = new Guild_info { Name = "Some Player Guild", LeaderId = 77 };
            MethodInfo rankFor = typeof(WorldServer.World.Guild.Guild).GetMethod("SystemGuildRankFor", BindingFlags.Instance | BindingFlags.NonPublic);
            var gmClient = (CaptureClient)FormatterServices.GetUninitializedObject(typeof(CaptureClient));
            gmClient._Account = new Account { GmLevel = 3 };
            var gm = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
            gm.Client = gmClient;
            var normalClient = (CaptureClient)FormatterServices.GetUninitializedObject(typeof(CaptureClient));
            normalClient._Account = new Account { GmLevel = 1 };
            var normal = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
            normal.Client = normalClient;
            Func<WorldServer.World.Guild.Guild, Player, byte, byte> rank = (g, p, current) => (byte)rankFor.Invoke(g, new object[] { p, current });
            Assert(rank(systemGuild, gm, 1) == 9, "A GM joining a system guild gets rank 9");
            Assert(rank(systemGuild, normal, 1) == 1, "A player joining a system guild stays a Member");
            Assert(rank(systemGuild, normal, 9) == 1, "A former GM loses rank 9 in a leaderless system guild");
            Assert(rank(systemGuild, normal, 8) == 8, "Officers promoted by a GM keep their rank");
            Assert(rank(playerGuild, gm, 1) == 1, "GMs get no special rank in player guilds");
            systemGuild.Info.LeaderId = 55;
            Assert(rank(systemGuild, normal, 9) == 9, "A system guild given a real leader keeps that leader's rank 9");
        }

        // Item audit line format (checked without writing to the real ITEMS log).
        {
            MethodInfo format = typeof(ItemAudit).GetMethod("Format", BindingFlags.Static | BindingFlags.NonPublic);
            string line = (string)format.Invoke(null, new object[] { "Trade", "Give", 12u, "Alice", 34u, "Bob", 208470u, (ushort)10, 0u, 0ul });
            Assert(line == "Trade|Give|from=12:Alice|to=34:Bob|item=208470x10", "Audit item line: " + line);
            line = (string)format.Invoke(null, new object[] { "Vault", "Deposit", 12u, "Alice", 5u, "guild:Order", 0u, (ushort)0, 2500u, 1003ul });
            Assert(line == "Vault|Deposit|from=12:Alice|to=5:guild:Order|money=2500|ref=1003", "Audit money line: " + line);

            // The deployed NLog.config routes ItemAudit to its own file and stops it there (final),
            // so audit lines neither go missing nor flood the console and main log.
            bool routed = false;
            foreach (NLog.Config.LoggingRule rule in NLog.LogManager.Configuration.LoggingRules)
                if (rule.LoggerNamePattern == "ItemAudit" && rule.Final)
                    foreach (NLog.Targets.Target target in rule.Targets)
                        routed |= target.Name == "itemAuditLogToFile";
            Assert(routed, "NLog.config routes ItemAudit to itemAuditLogToFile as a final rule");
        }

        // BUG-189: a requested slot must be an empty bag/bank/overflow slot, never equipment or past the array.
        Assert(!trader.CanCreateInSlot(10) && !trader.CanCreateInSlot(ushort.MaxValue) && !trader.CanCreateInSlot(ItemsInterface.AUTO_BANK_SLOT),
            "Equipment, auto and out-of-range slots cannot receive a created item");
        Assert(trader.CanCreateInSlot((ushort)(ItemsInterface.INVENTORY_START_SLOT + 2)) && trader.CanCreateInSlot(ItemsInterface.BANK_START_SLOT),
            "Empty bag and bank slots can");
        Assert(trader.CreateItem(sword.Info, 1, new List<Talisman>(), 0, 0, false, 10) == ItemResult.RESULT_ITEMID_INVALID,
            "CreateItem refuses an equipment slot");

        // BUG-188: the vault price is the server's; a full vault costs nothing more because it cannot grow.
        var guild = (WorldServer.World.Guild.Guild)FormatterServices.GetUninitializedObject(typeof(WorldServer.World.Guild.Guild));
        guild.Info = new Guild_info { Level = 40 };
        guild.Info.guildvaultpurchased[0] = 10;
        guild.Info.guildvaultpurchased[1] = 60;
        MethodInfo vaultPrice = typeof(WorldServer.World.Guild.Guild).GetMethod("GetVaultSlotPrice", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert((uint)vaultPrice.Invoke(guild, new object[] { 0 }) == 20000 && (uint)vaultPrice.Invoke(guild, new object[] { 1 }) == 0
            && (uint)vaultPrice.Invoke(guild, new object[] { 4 }) == 20000000, "Vault slot prices follow the displayed table");

        // BUG-180: a COD byte other than 0/1 is refused before any money moves.
        var mail = new MailInterface();
        mail.SetOwner(player);
        // Skip(1), empty name, empty subject and body, money 1000, cr 2, no items.
        byte[] body = { 0, 0, 0, 0, 0, 0, 0xE8, 0x03, 0, 0, 2, 0 };
        client.Packets.Clear();
        mail.SendPacketMail(new PacketIn(body, 0, body.Length));
        Assert(client.Packets.Count == 1 && client.LastPacket[client.LastPacket.Length - 1] == (byte)MailResult.TEXT_MAIL_RESULT5,
            "Out-of-range COD byte is refused");
        Console.WriteLine("PASS: atomic and non-stackable creation, slot-targeted creation, trade offers, equipped sales, vault prices, repair, crafting and mail COD.");
    }

    private static void CheckInventoryTransferGuards()
    {
        var player = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var client = (CaptureClient)FormatterServices.GetUninitializedObject(typeof(CaptureClient));
        client.Packets = new List<byte[]>();
        player.Client = client;
        player.Info = new Character { CharacterId = 77 };
        player._Value = new Character_value();
        PacketOut.SizeLen = 2;
        ushort bag = ItemsInterface.INVENTORY_START_SLOT;

        // F_TRANSFER_ITEM: the client never sends a count of 0 or a move onto its own slot (all 1,847
        // client frames in the official captures). Both used to duplicate.
        {
            var bags = new ItemsInterface { Items = new Item[ItemsInterface.OVERFLOW_END_SLOT] };
            SetPlayerOwner(bags, player);
            var gem = new Item_Info { Entry = 7000, MaxStack = 20, Name = "gem" };
            var gear = new Item_Info { Entry = 7004, MaxStack = 1, Name = "gear" };
            bags.Items[bag] = new Item(player) { Info = gem, CharSaveInfo = new CharacterItem { Counts = 15, SlotId = bag } };
            bags.Items[bag + 2] = new Item(player) { Info = gear, CharSaveInfo = new CharacterItem { Counts = 1, SlotId = (ushort)(bag + 2) } };
            Assert(!bags.MoveSlot(bag, (ushort)(bag + 1), 0) && bags.Items[bag + 1] == null && bags.Items[bag].Count == 15,
                "A zero-count move into an empty slot creates nothing");
            Assert(!bags.MoveSlot((ushort)(bag + 2), (ushort)(bag + 3), 0) && bags.Items[bag + 3] == null,
                "A zero-count move cannot copy gear either");
            Assert(!bags.MoveSlot(bag, bag, 15) && bags.Items[bag].Count == 15,
                "A partial stack moved onto its own slot does not grow to MaxStack");

            // Unbought bank slots: 80 bank slots plus 8 per purchase, so 880 is the first unbought one.
            Assert(!bags.CanMoveToSlot(bags.Items[bag], 900) && bags.CanMoveToSlot(bags.Items[bag], 850),
                "Only bought bank slots accept items");
            Assert(bags.IsStorageSlot(bag) && bags.IsStorageSlot(850) && !bags.IsStorageSlot(900) && !bags.IsStorageSlot(10)
                && !bags.IsStorageSlot(ItemsInterface.TALISMAN_SLOT_WINDOW) && !bags.IsStorageSlot(ItemsInterface.QUEST_START_SLOT),
                "Mail and vault deposits take bags and bought bank slots only");
        }

        // Requirements are paid from the crafting bag too, and a removal is all or nothing. The
        // crafting bag was skipped, so cultivation's rollback took nothing back and its harvest repeated.
        {
            var bags = new ItemsInterface { Items = new Item[ItemsInterface.OVERFLOW_END_SLOT] };
            bags.SetOwner(player); // no _playerOwner: DeleteItem then sends nothing
            var herb = new Item_Info { Entry = 7001, MaxStack = 20, Type = 34, Name = "herb" };
            bags.Items[ItemsInterface.CRAFTING_START_SLOT] = new Item(player) { Info = herb, CharSaveInfo = new CharacterItem { Counts = 5 } };
            bags.Items[20] = new Item(player) { Info = herb, CharSaveInfo = new CharacterItem { Counts = 10 } }; // worn, never consumed
            Assert(bags.HasItemCountInInventory(7001, 5) && !bags.HasItemCountInInventory(7001, 6),
                "The crafting bag counts toward a requirement; worn items do not");
            Assert(!bags.RemoveItems(7001, 6) && bags.Items[ItemsInterface.CRAFTING_START_SLOT].Count == 5,
                "A removal that cannot be completed removes nothing and reports failure");
            MethodInfo removeImpl = typeof(ItemsInterface).GetMethod("RemoveItemsImpl", BindingFlags.Instance | BindingFlags.NonPublic);
            object[] args = { 7001u, (ushort)3, new List<ushort>(), true };
            Assert((int)removeImpl.Invoke(bags, args) == 3 && bags.Items[ItemsInterface.CRAFTING_START_SLOT].Count == 2,
                "Three are taken from the crafting bag");
        }

        // Trade: the partner accepts the count they were shown, and the frames keep the retail layout.
        {
            var trader = new ItemsInterface { Items = new Item[ItemsInterface.OVERFLOW_END_SLOT] };
            trader.SetOwner(player);
            var ore = new Item(player) { Info = new Item_Info { Entry = 7002, MaxStack = 20, Name = "ore" }, CharSaveInfo = new CharacterItem { Counts = 20 } };
            ushort oreSlot = (ushort)(bag + 25); // 0x41, as in the official frames
            trader.Items[oreSlot] = ore;
            trader.TradeItems = new KeyValuePair<ushort, Item>[ItemsInterface.MAX_TRADE_SLOT];
            trader.TradeItems[0] = new KeyValuePair<ushort, Item>(oreSlot, ore);
            SnapshotTradeOffer(trader);
            MethodInfo validate = typeof(ItemsInterface).GetMethod("ValidateTradeOffer", BindingFlags.Instance | BindingFlags.NonPublic);
            object[] counts = new object[2];
            Assert((bool)validate.Invoke(trader, counts), "An unchanged offer validates");
            ore.CharSaveInfo.Counts = 1;
            Assert(!(bool)validate.Invoke(trader, counts), "A stack split after it was offered is refused");
            ore.CharSaveInfo.Counts = 20;

            SetPlayerOwner(trader, player);
            trader.TradingMoney = 100000;
            trader.TradingAccepted = 2;
            client.Packets.Clear();
            trader.SendTradeInfo(trader);
            byte[] frame = client.LastPacket;
            // status | 0 | oid | money | 3 | count | 9 x slot, from e.g. "4B 02 00 00 00 00 01 86 A0 03 01 00 41".
            Assert(frame.Length == 31 && frame[2] == 0x4B && frame[3] == 2 && frame[5] == 0 && frame[6] == 0
                && ReadUInt32(frame, 7) == 100000 && frame[11] == 3 && frame[12] == 1 && frame[13] == 0 && frame[14] == 0x41
                && frame[15] == 0 && frame[16] == 0, "An accepted trade frame keeps the money, timeout byte and offered slot");
            trader.SendTradeClose(0);
            frame = client.LastPacket;
            Assert(frame.Length == 31 && frame[3] == 3 && frame[11] == 3 && trader.TradingMoney == 0 && trader.TradeItems[0].Value == null,
                "A close frame carries the timeout byte and clears the offer");
        }

        // Money: transfers that already took gold from one side must not saturate on the other.
        {
            player._Value.Money = uint.MaxValue - 5;
            Assert(player.CanAddMoney(5) && !player.CanAddMoney(6), "CanAddMoney stops at the uint ceiling");
            var seller = new ItemsInterface { Items = new Item[ItemsInterface.OVERFLOW_END_SLOT] };
            SetPlayerOwner(seller, player);
            var gold = new Item(player) { Info = new Item_Info { Entry = 7005, MaxStack = 1, SellPrice = 10, Name = "gold" }, CharSaveInfo = new CharacterItem { Counts = 1 } };
            seller.Items[bag] = gold;
            seller.SellItem(new WorldServer.NetWork.Handler.InteractMenu { Num = (byte)bag, Count = 1 });
            Assert(seller.Items[bag] == gold && player._Value.Money == uint.MaxValue - 5, "A sale whose proceeds cannot be carried is refused");
            player._Value.Money = 0;
        }

        // A bolstered ranged weapon adds its bolster term once (MainAndRanged added it twice).
        {
            var weapons = new ItemsInterface { Items = new Item[ItemsInterface.OVERFLOW_END_SLOT] };
            weapons.SetOwner(player);
            weapons.Items[(int)EquipSlot.RANGED_WEAPON] = new Item(player) { Info = new Item_Info { Entry = 7003, Dps = 100 }, CharSaveInfo = new CharacterItem { Counts = 1 } };
            weapons.BolsterFactor = 1f;
            float ranged = weapons.GetWeaponDamage(WeaponDamageContribution.Ranged);
            float both = weapons.GetWeaponDamage(WeaponDamageContribution.MainAndRanged);
            Assert(Math.Abs(ranged - 11f) < 0.001f && Math.Abs(both - 11f) < 0.001f,
                "Ranged and main-and-ranged agree with no main hand: " + ranged + " / " + both);
        }

        // A currency box is consumed only once its contents are delivered.
        {
            Dictionary<uint, Item_Info> savedInfos = ItemService._Item_Info;
            ItemService._Item_Info = new Dictionary<uint, Item_Info>(); // the contents resolve to nothing
            try
            {
                var bags = new ItemsInterface { Items = new Item[ItemsInterface.OVERFLOW_END_SLOT] };
                SetPlayerOwner(bags, player);
                var box = new Item(player) { Info = new Item_Info { Entry = 208424, MaxStack = 20, Name = "box" }, CharSaveInfo = new CharacterItem { Counts = 1 } };
                bags.Items[bag] = box;
                bags.OpenBox(bag, box);
                Assert(bags.Items[bag] == box && box.Count == 1, "An undeliverable box is not consumed");
            }
            finally
            {
                ItemService._Item_Info = savedInfos;
            }
        }

        // Guild banners and tactics need the permissions the client gates them on.
        {
            var guild = (WorldServer.World.Guild.Guild)FormatterServices.GetUninitializedObject(typeof(WorldServer.World.Guild.Guild));
            guild.Info = new Guild_info { Name = "Some Player Guild", LeaderId = 1 };
            guild.Info.Members = new Dictionary<uint, Guild_member> { { 77u, new Guild_member { CharacterId = 77, RankId = 1 } } };
            guild.Info.Ranks = new Dictionary<byte, Guild_rank>
            {
                { 1, new Guild_rank { RankId = 1, Name = "Member", Permissions = "00 00 00 00 00 00 00 00" } },
                { 9, new Guild_rank { RankId = 9, Name = "Leader", Permissions = "FF FF FF FF FF FF FF 3F" } }
            };
            guild.Info.GuildTacticsPurchased = new ushort[40];
            FieldInfo bannersField = typeof(WorldServer.World.Guild.Guild).GetField("_banners", BindingFlags.Instance | BindingFlags.NonPublic);
            var banners = new ushort[3, 4] { { 1, 0, 0, 0 }, { 1, 0, 0, 0 }, { 1, 0, 0, 0 } };
            bannersField.SetValue(guild, banners);

            guild.SaveBanner(player, 0, 2, 999, 999, 999);
            Assert(banners[0, 1] == 0, "A member without BANNER_MANAGEMENT cannot edit the standard");
            guild.TrainGuildTactics(player, 0, 999);
            Assert(guild.Info.GuildTacticsPurchased[0] == 0, "A member without TACTICS_PURCHASE cannot buy a tactic");

            guild.Info.Members[77].RankId = 9;
            guild.SaveBanner(player, 255, 2, 999, 999, 999); // banner 0 minus one, wrapped
            guild.TrainGuildTactics(player, 40, 999);
            Assert(banners[0, 1] == 0 && Array.IndexOf(guild.Info.GuildTacticsPurchased, (ushort)999) < 0,
                "Out-of-range banner and tactic slots are refused, not thrown on");
        }

        // The auction expiry timer contains its failures: an exception escaping a Timer callback ends
        // the process. With no database here, resolving the deleted seller throws inside the pass.
        {
            List<Common.Auction> savedAuctions = WorldServer.World.Auction.AuctionHouse.Auctions;
            WorldServer.World.Auction.AuctionHouse.Auctions = new List<Common.Auction>
            {
                new Common.Auction { AuctionId = 1, SellerId = 4000000, ItemId = 7000, StartTime = 1 }
            };
            try
            {
                WorldServer.World.Auction.AuctionHouse.CheckAuctionExpiry(null);
            }
            finally
            {
                WorldServer.World.Auction.AuctionHouse.Auctions = savedAuctions;
            }
        }

        Console.WriteLine("PASS: transfer counts, same-slot moves, bank bounds, storage slots, crafting-bag removal, trade counts and frames, money ceiling, ranged bolster, box delivery, guild permissions and auction-timer containment.");
    }

    private static void CheckItemCooldownNotifications()
    {
        var player = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var client = (CaptureClient)FormatterServices.GetUninitializedObject(typeof(CaptureClient));
        client.Packets = new List<byte[]>();
        player.Client = client;
        var inventory = new ItemsInterface { Items = new Item[ItemsInterface.OVERFLOW_END_SLOT] };
        inventory.SetOwner(player);
        PacketOut.SizeLen = 2;
        for (int i = 0; i < 260; i++)
        {
            var info = new Item_Info { Entry = (uint)(1000 + i), SpellId = 1353 };
            info.Unk27 = new byte[27];
            info.Unk27[19] = 7;
            inventory.Items[i] = new Item(player) { Info = info, CharSaveInfo = new CharacterItem() };
        }
        inventory.Items[260] = new Item(player) { Info = inventory.Items[0].Info, CharSaveInfo = new CharacterItem() };
        inventory.Items[261] = new Item(player) { Info = new Item_Info { Entry = 9999, SpellId = 999 }, CharSaveInfo = new CharacterItem() };
        inventory.SendItemCooldown(1353, 1500);
        Assert(client.Packets.Count == 2, "Large cooldown notifications split into packets");
        var seen = new HashSet<uint>();
        foreach (byte[] packet in client.Packets)
        {
            int count = packet[3];
            Assert(count > 0 && packet.Length == 4 + count * 12, "Item packet count matches payload length");
            for (int i = 0; i < count; i++)
            {
                int offset = 4 + i * 12;
                Assert(seen.Add(ReadUInt32(packet, offset)), "Notification entries deduplicate across batches");
                Assert(packet[offset + 10] == 0 && packet[offset + 11] == 2, "Fractional duration rounds upward at wire boundary");
            }
        }
        Assert(seen.Count == 260 && client.Packets[0][3] == 255 && client.Packets[1][3] == 5, "Every distinct item is sent once");
        long deadline = inventory.Items[0].CharSaveInfo.NextAllowedUseTime;
        Assert(deadline > TCPManager.GetTimeStamp(), "Cooldown assigned");
        for (int i = 0; i <= 260; i++)
            Assert(inventory.Items[i].CharSaveInfo.NextAllowedUseTime == deadline, "Every copy receives the same deadline");
        Assert(inventory.Items[261].CharSaveInfo.NextAllowedUseTime == 0, "Unrelated items are unchanged");
        inventory.SendItemGroupCooldown(7, 0);
        for (int i = 0; i <= 260; i++)
            Assert(inventory.Items[i].CharSaveInfo.NextAllowedUseTime == 0, "Group reset clears every copy immediately");

        client.FailSend = true;
        bool failed = false;
        try { inventory.SendItemCooldown(1353, 1500); }
        catch (InvalidOperationException) { failed = true; }
        Assert(failed, "Transport failure exercised");
        client.FailSend = false;
        client.Packets.Clear();
        inventory.SendItemCooldown(999, 1000);
        Assert(client.Packets.Count == 1 && client.LastPacket[3] == 1 && ReadUInt32(client.LastPacket, 4) == 9999,
            "Failed send cannot contaminate subsequent notification");
        var record = new CharacterItem { NextAllowedUseTime = long.MaxValue };
        Assert(record.RemainingCooldown == ushort.MaxValue, "Long remaining deadlines saturate instead of wrapping");
        record.NextAllowedUseTime = 0;
        Assert(record.RemainingCooldown == 0, "Reset has no remaining cooldown");
        Console.WriteLine("PASS: duplicate item cooldowns, bounded packet counts, reset, overflow and send-failure cleanup.");
    }

    private static void CheckCooldowns()
    {
        Assert(new AbilityInfo(new DBAbilityInfo { Cooldown = 7 }).CooldownMilliseconds == 7000, "Legacy seconds fallback");
        Assert(new AbilityInfo(new DBAbilityInfo { Cooldown = 7, CooldownMilliseconds = 0 }).CooldownMilliseconds == 0, "Explicit zero overrides legacy");
        Assert(AbilityInfo.ClampCooldownMilliseconds(double.NaN) == 0 && AbilityInfo.ClampCooldownMilliseconds(-1) == 0,
            "Invalid cooldowns clamp to zero");
        Assert(AbilityInfo.ClampCooldownMilliseconds(double.MaxValue) == int.MaxValue, "Cooldown overflow saturates");
        Assert(AbilityInfo.GetItemCooldownSeconds(1500) == 2 && AbilityInfo.GetItemCooldownSeconds(int.MaxValue) == ushort.MaxValue,
            "Item seconds boundary rounds up and saturates");
        var info = new AbilityInfo { Entry = 1353, CooldownMilliseconds = 1500, AICooldown = 300 };
        Assert(info.GetAICooldownMilliseconds() == 300000, "AI pacing does not narrow to byte");
        var npc = new NPCAbility(1353, 10, 0, true, "", cooldownMilliseconds: 1500);
        npc.CooldownEnd = 9999;
        npc.AbilityUsed = 1;
        var clone = npc.CreateInstance();
        Assert(clone.CooldownMilliseconds == 1500 && clone.CooldownEnd == 0 && clone.AbilityUsed == 0, "NPC clone retains precision and isolates state");
        Assert(new NPCAbility(1, 10, 300, true, "").CooldownMilliseconds == 300000, "Authored NPC constructor remains seconds");

        var modifier = typeof(AbilityModifierInvoker);
        modifier.GetMethod("AddCooldownMS", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null,
            new object[] { null, info, new AbilityModifierEffect { PrimaryValue = -250 } });
        Assert(info.CooldownMilliseconds == 1250, "Add modifier retains fractional seconds");
        modifier.GetMethod("MultiplyCooldown", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null,
            new object[] { null, info, new AbilityModifierEffect { PrimaryValue = -50 } });
        Assert(info.CooldownMilliseconds == 625, "Percentage modifier retains milliseconds");
        modifier.GetMethod("SetCooldown", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null,
            new object[] { null, info, new AbilityModifierEffect { PrimaryValue = 3 } });
        Assert(info.CooldownMilliseconds == 3000, "Set modifier preserves authored seconds contract");

        var player = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var client = (CaptureClient)FormatterServices.GetUninitializedObject(typeof(CaptureClient));
        player.Client = client;
        var abilities = new AbilityInterface();
        abilities.SetOwner(player);
        PacketOut.SizeLen = 2;
        AbilityMgr.NewAbilityVolatiles[1353] = info;
        try
        {
            info.CDcap = 2;
            abilities.SetCooldown(1353, 1500);
            Assert(ReadUInt32(client.LastPacket, 7) == 2000, "Timer advertises enforced cap");
            info.IgnoreCooldownReduction = 1;
            info.CooldownMilliseconds = 4500;
            abilities.SetCooldown(1353, 250);
            Assert(ReadUInt32(client.LastPacket, 7) == 4500, "No-reduction floor and packet agree");
            info.IgnoreCooldownReduction = 0;
            info.CDcap = 0;
            abilities.SetCooldown(1353, 1999);
            Assert(ReadUInt32(client.LastPacket, 7) == 1999, "Fractional timer payload at offset four");
            abilities.Cooldowns[1353] = TCPManager.GetTimeStampMS() + 350;
            Assert(!abilities.CanCastCooldown(1353), "Individual cooldown cannot bypass last 400ms");
            abilities.Cooldowns[0] = TCPManager.GetTimeStampMS() + 350;
            Assert(abilities.CanCastCooldown(0), "Existing global cooldown grace retained");
            abilities.Cooldowns[1353] = TCPManager.GetTimeStampMS() - 1;
            abilities.ResendCooldown(1353);
            Assert(ReadUInt32(client.LastPacket, 7) == 0, "Expired resend cannot underflow");
            abilities.SetCooldown(1353, -1);
            Assert(abilities.CanCastCooldown(1353) && ReadUInt32(client.LastPacket, 7) == 0, "Reset clears ordinary timer");
            abilities.SetItemCooldown(1353, 1500, true);
            long remaining = abilities.Cooldowns[1353] - TCPManager.GetTimeStampMS();
            Assert(remaining > 1400 && remaining <= 1500, "Silent item timer stores exact milliseconds");
        }
        finally { AbilityMgr.NewAbilityVolatiles.Remove(1353); }
        Console.WriteLine("PASS: millisecond cooldown loading, arithmetic, AI isolation, timer packets, expiry and item boundary.");
    }

    private static void CheckInfluence()
    {
        var buildIndex = typeof(ChapterService).GetMethod("BuildInfluenceIndex", BindingFlags.Static | BindingFlags.NonPublic);
        var mapOnly = new Chapter_Info { Entry = 1, InfluenceEntry = 47 };
        var rewards = new Chapter_Info { Entry = 141, InfluenceEntry = 47, CreatureEntry = 66173,
            Tier1InfluenceCount = 3520, Tier2InfluenceCount = 7040, Tier3InfluenceCount = 10560 };
        var sharedArea = new Chapter_Info { Entry = 161, InfluenceEntry = 47, CreatureEntry = 66173,
            Tier1InfluenceCount = 3520, Tier2InfluenceCount = 7040, Tier3InfluenceCount = 10560 };
        foreach (var rows in new[] { new[] { mapOnly, rewards, sharedArea }, new[] { sharedArea, rewards, mapOnly } })
        {
            var index = (Dictionary<uint, Chapter_Info>)buildIndex.Invoke(null, new object[] { rows });
            Assert(index.Count == 1 && ReferenceEquals(index[47], rewards), "Shared tracks select existing complete rewards independent of database row order");
            Assert(mapOnly.Tier3InfluenceCount == 0, "Index must not invent or fill placeholder thresholds");
        }
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

        // PQType 1 matches the real pquest_info row; the post-name field below is driven from
        // it, because the Thanquol's Incursion captures send 0 there where Gunbad sends 2 and
        // PQType is what separates the two sets.
        var pqInfo = new PQuest_Info { Entry = 513, Name = "A Taint from Below", ZoneId = 60,
            Type = 0, ChapterId = 65, PQType = 1, PQDifficult = 3, Objectives = new List<PQuest_Objective>() };
        pqInfo.Objectives.Add(new PQuest_Objective { Guid = 2298, StageName = "Stage I", Type = 2,
            ObjectId = "36554", Objective = "Oozespawn Nurgling", Description = "test", Count = 24 });
        var pq = new PublicQuest(pqInfo);
        pq.Stage = pq.Stages[0];
        client.Packets.Clear();
        pq.SendCurrentStage(player);
        byte[] pqPacket = client.Packets[0];
        Assert(pqPacket[8] == 0 && pqPacket[13 + pqInfo.Name.Length] == 2,
            "Dungeon PQ realm remains 0, independent post-name field is 2");
        // Trailer, from INSTANCE_GUNBAD_PART1 F_OBJECTIVE_INFO: uint16 0, the short stage
        // label as a pascal string, then the influence id as a uint32 and two zero bytes.
        // The influence used to be written as a lone byte two positions earlier, with the
        // stage label missing entirely.
        Assert(pqPacket[pqPacket.Length - 14] == "Stage I".Length &&
               Encoding.ASCII.GetString(pqPacket, pqPacket.Length - 13, 7) == "Stage I",
            "PQ trailer carries the short stage label the client shows above the objectives");
        Assert(ReadUInt32(pqPacket, pqPacket.Length - 6) == 65, "PQ trailer uses Destruction Gunbad influence, not hardcoded 72");
        Assert(ReadUInt32(pqPacket, pqPacket.Length - 24) == 0 && ReadUInt32(pqPacket, pqPacket.Length - 20) == 0,
            "Untimed first stage must not serialize a wrapped negative timestamp");
        int difficultyOffset = 13 + pqInfo.Name.Length + 7 + 7 + "Oozespawn Nurgling".Length;
        Assert(pqPacket[difficultyOffset] == 255, "Official Gunbad PQ difficulty sentinel");
        player.Realm = Realms.REALMS_REALM_ORDER;
        client.Packets.Clear();
        pq.SendCurrentStage(player);
        Assert(ReadUInt32(client.Packets[0], pqPacket.Length - 6) == 64, "Order Gunbad PQ uses Order influence");

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
        var destination = new Zone_jump { ZoneID = 164, InstanceID = 164 };
        var restored = (Instance_Lockouts)resolve.Invoke(null, new object[] { value, destination });
        Assert(restored.InstanceID == "~164:" + future && restored.Bosseskilled == "161:164",
            "Character lockout must separate its key and killed-boss list");
        var copy = (Instance)FormatterServices.GetUninitializedObject(typeof(Instance));
        copy.Lockout = restored;
        Assert(copy.IsBossKilled(161) && !copy.IsBossKilled(162), "Restored boss suppression uses the saved IDs");
        value.Lockouts = "~164:1:161";
        Assert(resolve.Invoke(null, new object[] { value, destination }) == null, "Expired lockout cannot suppress a fresh boss");
        value.Lockouts = "~164:invalid:161";
        Assert(resolve.Invoke(null, new object[] { value, destination }) == null, "Malformed lockout must not throw");
        foreach (ushort bossZone in new ushort[] { 63, 64, 65, 66 })
        {
            value.Lockouts = "~60:" + future + ":999~" + bossZone + ":" + future + ":123";
            destination = new Zone_jump { ZoneID = bossZone, InstanceID = 60 };
            restored = (Instance_Lockouts)resolve.Invoke(null, new object[] { value, destination });
            Assert(restored.InstanceID == "~" + bossZone + ":" + future && restored.Bosseskilled == "123",
                "Gunbad entry restores its boss-map lockout, not the cave's shared dungeon ID");
            copy.Lockout = restored;
            Assert(copy.IsBossKilled(123) && !copy.IsBossKilled(999), "Only the destination map's dead bosses are suppressed");
        }
        Assert(resolve.Invoke(null, new object[] { value, null }) == null, "Missing destination cannot resolve a lockout");
    }

    private static PublicQuest KillQuestFixture(uint id, uint target, ushort zone)
    {
        var objective = new PQuest_Objective { Guid = id, StageName = "Stage I", Type = 2,
            ObjectId = target.ToString(), Creature = new Creature_proto { Entry = target },
            Count = 10, Spawns = new List<PQuest_Spawn>() };
        var quest = new PublicQuest(new PQuest_Info { Entry = id, Name = "Kill fixture", ZoneId = zone,
            Objectives = new List<PQuest_Objective> { objective } });
        quest.Loaded = true;
        quest.Start();
        return quest;
    }

    private static Player QuestContributor(uint id, PublicQuest quest)
    {
        var player = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        player.Info = new Character { CharacterId = id };
        player._Value = new Character_value { Level = 40 };
        player.QtsInterface = new QuestsInterface();
        player.QtsInterface.PublicQuest = quest;
        return player;
    }

    private static void CheckOrdinaryCreatureQuestCredit()
    {
        // No Tome subtype or killer is needed to credit damage contributors' PQs.
        var creature = new Creature();
        creature.Spawn = new Creature_spawn { Entry = 94104, ZoneId = 179,
            Proto = new Creature_proto { Entry = 94104, CreatureSubType = 0 } };
        var order = KillQuestFixture(901, 94104, 179);
        var destruction = KillQuestFixture(592, 94104, 179);
        var unrelated = KillQuestFixture(590, 12345, 179);
        var otherZone = KillQuestFixture(591, 94104, 191);
        var contributors = (Dictionary<Player, uint>)typeof(Unit).GetField("DamageSources",
            BindingFlags.Instance | BindingFlags.NonPublic).GetValue(creature);
        contributors.Add(QuestContributor(1, unrelated), 20);
        contributors.Add(QuestContributor(2, otherZone), 20);
        contributors.Add(QuestContributor(3, order), 20);
        contributors.Add(QuestContributor(4, order), 20);
        contributors.Add(QuestContributor(5, destruction), 20);
        var total = typeof(Unit).GetField("TotalDamageTaken", BindingFlags.Instance | BindingFlags.NonPublic);
        total.SetValue(creature, (uint)100);
        var credit = typeof(Creature).GetMethod("CreditQuestKill", BindingFlags.Instance | BindingFlags.NonPublic);
        credit.Invoke(creature, new object[] { null });
        Assert(order.Stage.Objectives[0].Count == 1, "Two contributors to one PQ count a subtype-zero kill once");
        Assert(destruction.Stage.Objectives[0].Count == 1, "Separate realm quest also receives the kill");
        Assert(unrelated.Stage.Objectives[0].Count == 0 && otherZone.Stage.Objectives[0].Count == 0,
            "Unmatched and cross-zone quests cannot consume or receive another quest's credit");
        total.SetValue(creature, (uint)0);
        credit.Invoke(creature, new object[] { null });
        Assert(order.Stage.Objectives[0].Count == 1, "No damage cannot create PQ credit");
        var pqCreature = (PQuestCreature)FormatterServices.GetUninitializedObject(typeof(PQuestCreature));
        pqCreature.Spawn = creature.Spawn;
        total.SetValue(pqCreature, (uint)100);
        credit.Invoke(pqCreature, new object[] { null });
        Assert(order.Stage.Objectives[0].Count == 1, "PQ-owned creatures retain their separate death-credit path");
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
