namespace WorldServer.World.Objects
{
    public enum CreatureState
    {
        Merchant = 0,
        SkillTrainer = 1,
        KillCollector = 2,
        Dead = 3,
        QuestFinishable = 4,
        QuestAvailable = 5,
        RepeatableQuestAvailable = 6,
        QuestInProgress = 7,
        Lootable = 8,
        // 9 and 10 do not appear in logs
        Banker = 11,
        Auctioneer = 12,
        Influence = 13,
        GuildRegistrar = 14,
        FlightMaster = 15,
        RallyMasterIcon = 16,
        Healer = 17,
        RvRFlagged = 18,
        // 19 and 20 do not appear in logs
        Butcherable = 21,
        Scavengeable = 22,
        StandardBanner = 23,
        Effects = 24,
        UnkOmnipresent = 25,
        DyeMerchant = 26,
        NameRegistrar = 27,
        ConsistentAppearance = 28,
        PhysicalEffects = 29, // when set, fig leaf data is very long... probably reads some stuff for modifying appearance to be "special" - traits, perhaps? set on player corpses
        // 30 does not appear in logs
        QuestEventAvailable = 32
    }
}