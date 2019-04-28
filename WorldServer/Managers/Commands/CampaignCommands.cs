using Common.Database.World.BattleFront;
using FrameWork;
using GameData;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NLog;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using static WorldServer.Managers.Commands.GMUtils;
using BattleFrontConstants = WorldServer.World.Battlefronts.Apocalypse.BattleFrontConstants;

namespace WorldServer.Managers.Commands
{
    /// <summary>RvR campaign commmands under .campaign</summary>
    internal class CampaignCommands
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>Constant initials extractor<summary>
        private static readonly Regex INITIALS = new Regex(@"([A-Z])[A-Z1-9]*_?");

        [CommandAttribute(EGmLevel.AnyGM, "Provides campaign diagnostic info. Usage: .campaign diag")]
        public static void Diag(Player plr, string targetString = null)
        {
            if (plr.Zone == null || plr.Region.Campaign == null)
            {
                SendCsr(plr, "Must be in a RvR zone to use this command.");
                return;
            }
            plr.SendClientMessage($"Upper Tier {WorldMgr.UpperTierCampaignManager.ActiveBattleFrontName} is active.");
            plr.SendClientMessage($"  Campaign Status : \t {WorldMgr.GetRegion((ushort)WorldMgr.UpperTierCampaignManager.ActiveBattleFront.RegionId, false).Campaign.GetBattleFrontStatus()}");

            var destroKeep = plr.Region.Campaign.Keeps.FirstOrDefault(x => x.Info.KeepId == WorldMgr.UpperTierCampaignManager.ActiveBattleFront.DestroKeepId);
            if (destroKeep != null)
                DisplayKeepStatus(destroKeep, plr);
            var orderKeep = plr.Region.Campaign.Keeps.FirstOrDefault(x => x.Info.KeepId == WorldMgr.UpperTierCampaignManager.ActiveBattleFront.OrderKeepId);
            if (orderKeep != null)
                DisplayKeepStatus(orderKeep, plr);
        }

        private static void DisplayKeepStatus(BattleFrontKeep keep, Player plr)
        {
            plr.SendClientMessage($"Keep Status : {keep.KeepStatus}");
            plr.SendClientMessage($"Ram Deployed : {keep.RamDeployed}");
            plr.SendClientMessage($"Players killed in range : {keep.PlayersKilledInRange}");
            plr.SendClientMessage($"Players in range : {keep.PlayersInRange.Count}");
            foreach (var keepDoorRepairTimer in keep.DoorRepairTimers)
            {
                plr.SendClientMessage($"Door Repair Timer : {keepDoorRepairTimer.Key}:{keepDoorRepairTimer.Value.Value}/{keepDoorRepairTimer.Value.Length}");
            }

            foreach (var keepHardPoint in keep.HardPoints)
            {
                plr.SendClientMessage($"Siege : {keepHardPoint.CurrentWeapon} {keepHardPoint.SiegeType}");
            }
            keep.SendDiagnostic(plr);
            foreach (var door in keep.Doors)
            {
                plr.SendClientMessage($"DoorId : {door.Info.DoorId} Interact:{door.GameObject.InteractState} AutoAttack:{door.GameObject.CanAutoAttack}");
                plr.SendClientMessage("Occlusion_Visible:" + Occlusion.GetFixtureVisible(door.GameObject.DoorId));
            }

        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Updates some constant parameters. Give no arg to list constants.")]
        public static void Constants(Player plr, string nameOrShortcut = "", string newValue = "")
        {
            Type type = typeof(BattleFrontConstants);

            nameOrShortcut = nameOrShortcut.ToUpperInvariant();
            bool match = false;
            if (newValue == "")
            {
                // Print existing value(s)
                SendCsr(plr, "<FIELD_NAME> (<shortcut>) : <value>");
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    string initials = INITIALS.Replace(field.Name, "$1");
                    if (nameOrShortcut == "" || initials == nameOrShortcut || field.Name == nameOrShortcut)
                    {
                        plr.SendClientMessage(string.Concat(
                            field.Name, " (", INITIALS.Replace(field.Name, "$1"), ") : ", field.GetValue(null)));
                        match = true;
                    }
                }
            }
            else
            {
                // Set new value
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    string initials = INITIALS.Replace(field.Name, "$1");
                    if (field.Name == nameOrShortcut || initials == nameOrShortcut)
                    {
                        object value = TryParse(field.FieldType, newValue);
                        if (value == null)
                        {
                            SendCsr(plr, "Illegal value for this constant : " + newValue);
                            return;
                        }
                        field.SetValue(null, value);
                        plr.SendClientMessage($"Constant {field.Name} has beeen set to {value}");
                        match = true;
                        break;
                    }
                }
            }
            if (!match)
                SendCsr(plr, "Please enter a valid constant name");
        }




        [CommandAttribute(EGmLevel.EmpoweredStaff, "Locks the pairing the player is in for the given realm (1 - Order, 2 - Dest). forceNumberOfBags = 0 for default.")]
        public static void LockPairing(Player plr, Realms realm, int forceNumberOfBags = 0)
        {
            plr.SendClientMessage($"Attempting to lock the {plr.Region.Campaign.ActiveCampaignName} campaign... (call AdvancePairing <realm> <tier> to move ahead)");

            if (WorldMgr.GetRegion(plr.Region.RegionId, false) == null)
                plr.SendClientMessage("Region does not exist.");

            if (WorldMgr.GetRegion(plr.Region.RegionId, false).Campaign == null)
                plr.SendClientMessage("Region / Campaign does not exist.");

            WorldMgr.GetRegion(plr.Region.RegionId, false).Campaign.BattleFrontManager.LockActiveBattleFront(realm, (byte)forceNumberOfBags);
        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Advances the pairing the player is in ")]
        public static void AdvancePairing(Player plr, Realms realm, int tier)
        {

            if (tier == 1)
            {
                var progression = WorldMgr.LowerTierCampaignManager.AdvanceBattleFront(realm);
                WorldMgr.LowerTierCampaignManager.OpenActiveBattlefront();
                WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);
                plr.SendClientMessage(realm == Realms.REALMS_REALM_DESTRUCTION
                    ? $"Destruction vanquishes Order, the campaign moves to {progression.Description}"
                    : $"Order conquers Destruction, the campaign moves to {progression.Description}");
            }
            else
            {
                var progression = WorldMgr.UpperTierCampaignManager.AdvanceBattleFront(realm);
                WorldMgr.UpperTierCampaignManager.OpenActiveBattlefront();
                WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);
                plr.SendClientMessage(realm == Realms.REALMS_REALM_DESTRUCTION
                    ? $"Destruction vanquishes Order, the campaign moves to {progression.Description}"
                    : $"Order conquers Destruction, the campaign moves to {progression.Description}");
            }
        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Updates the clients with current region capture status")]
        public static void UpdateRegionCaptureStatus(Player plr)
        {
            WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);
        }

        //.campaign SetRegionCaptureStatus 111111110 9    -- lock all to order except Caledor. Make it the active BF
        [CommandAttribute(EGmLevel.EmpoweredStaff, "Force lock and advance on all progressions. Params : T4progression string, activeBattlefrontId")]
        public static void SetRegionCaptureStatus(Player plr, string T4Progression, int activeBattleFrontId)
        {
            var lockingRealm = Realms.REALMS_REALM_NEUTRAL;
            foreach (var status in WorldMgr.UpperTierCampaignManager.BattleFrontStatuses)
            {

                if (status.BattleFrontId == BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_KADRIN_VALLEY)
                {
                    lockingRealm = GetLockRealmFromT4Progression(T4Progression[0].ToString());
                    WorldMgr.UpperTierCampaignManager.LockBattleFrontStatus(status.BattleFrontId, lockingRealm, new VictoryPointProgress());
                }
                if (status.BattleFrontId == BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_THUNDER_MOUNTAIN)
                {
                    lockingRealm = GetLockRealmFromT4Progression(T4Progression[1].ToString());
                    WorldMgr.UpperTierCampaignManager.LockBattleFrontStatus(status.BattleFrontId, lockingRealm, new VictoryPointProgress());
                }
                if (status.BattleFrontId == BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_BLACK_CRAG)
                {
                    lockingRealm = GetLockRealmFromT4Progression(T4Progression[2].ToString());
                    WorldMgr.UpperTierCampaignManager.LockBattleFrontStatus(status.BattleFrontId, lockingRealm, new VictoryPointProgress());
                }

                if (status.BattleFrontId == BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_REIKLAND)
                {
                    lockingRealm = GetLockRealmFromT4Progression(T4Progression[3].ToString());
                    WorldMgr.UpperTierCampaignManager.LockBattleFrontStatus(status.BattleFrontId, lockingRealm, new VictoryPointProgress());
                }
                if (status.BattleFrontId == BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_PRAAG)
                {
                    lockingRealm = GetLockRealmFromT4Progression(T4Progression[4].ToString());
                    WorldMgr.UpperTierCampaignManager.LockBattleFrontStatus(status.BattleFrontId, lockingRealm, new VictoryPointProgress());
                }
                if (status.BattleFrontId == BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_CHAOS_WASTES)
                {
                    lockingRealm = GetLockRealmFromT4Progression(T4Progression[5].ToString());
                    WorldMgr.UpperTierCampaignManager.LockBattleFrontStatus(status.BattleFrontId, lockingRealm, new VictoryPointProgress());
                }

                if (status.BattleFrontId == BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_EATAINE)
                {
                    lockingRealm = GetLockRealmFromT4Progression(T4Progression[6].ToString());
                    WorldMgr.UpperTierCampaignManager.LockBattleFrontStatus(status.BattleFrontId, lockingRealm, new VictoryPointProgress());
                }
                if (status.BattleFrontId == BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_DRAGONWAKE)
                {
                    lockingRealm = GetLockRealmFromT4Progression(T4Progression[7].ToString());
                    WorldMgr.UpperTierCampaignManager.LockBattleFrontStatus(status.BattleFrontId, lockingRealm, new VictoryPointProgress());
                }
                if (status.BattleFrontId == BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_CALEDOR)
                {
                    lockingRealm = GetLockRealmFromT4Progression(T4Progression[8].ToString());
                    WorldMgr.UpperTierCampaignManager.LockBattleFrontStatus(status.BattleFrontId, lockingRealm, new VictoryPointProgress());
                }
            }

            WorldMgr.UpperTierCampaignManager.ActiveBattleFront = WorldMgr.UpperTierCampaignManager.GetBattleFrontByBattleFrontId(activeBattleFrontId);
            WorldMgr.UpperTierCampaignManager.OpenActiveBattlefront();

            WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);
        }

        private static Realms GetLockRealmFromT4Progression(string str)
        {
            switch (str)
            {
                case "1":
                    return Realms.REALMS_REALM_ORDER;
                case "0":
                    return Realms.REALMS_REALM_NEUTRAL;
                case "2":
                    return Realms.REALMS_REALM_DESTRUCTION;
            }
            return Realms.REALMS_REALM_NEUTRAL;
        }

        // Example : .campaign ResetProgressionCommunications 0 100 1 102102102
        [CommandAttribute(EGmLevel.SourceDev, "Sends server commands to the client. forceT4 is a 9 char setting (1 order, 2 dest). eg 102110102")]
        public static void ResetProgressionCommunications(Player player, int destVP, int orderVP, int realm, string forceT4)
        {
            var vpp = new VictoryPointProgress();
            vpp.DestructionVictoryPoints = destVP;
            vpp.OrderVictoryPoints = orderVP;

            Realms lockingRealm;

            if (realm == 1)
                lockingRealm = Realms.REALMS_REALM_ORDER;
            else
            {
                if (realm == 2)
                    lockingRealm = Realms.REALMS_REALM_DESTRUCTION;
                else
                    lockingRealm = Realms.REALMS_REALM_NEUTRAL;
            }

            new ApocCommunications().ResetProgressionCommunications(player, lockingRealm, vpp, forceT4);

        }




        [CommandAttribute(EGmLevel.SourceDev, "Report on the status of the t4 progression")]
        public static void ProgressionStatus(Player plr)
        {
            var lockStr = $"BC :{WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_BLACK_CRAG).LockStatus.ToString()}";
            lockStr += $"TM :{WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_THUNDER_MOUNTAIN).LockStatus.ToString()}";
            lockStr += $"KV :{WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_KADRIN_VALLEY).LockStatus.ToString()}";
            lockStr += $"CW :{WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_CHAOS_WASTES).LockStatus.ToString()}";
            lockStr += $"PR :{WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_PRAAG).LockStatus.ToString()}";
            lockStr += $"RK :{WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_REIKLAND).LockStatus.ToString()}";
            lockStr += $"CA :{WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_CALEDOR).LockStatus.ToString()}";
            lockStr += $"DW :{WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_DRAGONWAKE).LockStatus.ToString()}";
            lockStr += $"EA :{ WorldMgr.UpperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_EATAINE).LockStatus.ToString()}";

            plr.SendClientMessage($"Porgression Status : {lockStr}");
        }


        [CommandAttribute(EGmLevel.EmpoweredStaff, "Locks a battle objective for the given realm (1 - Order, 2 - Dest), values = 0 (all), otherwise objectiveid")]
        public static void LockObj(Player plr, Realms realm, int values)
        {
            plr.SendClientMessage($"Attempting to lock objective...");

            var objectiveToLock = values;
            if (objectiveToLock == 0)
            {
                foreach (var flag in WorldMgr.GetRegion(plr.Region.RegionId, false).Campaign.Objectives)
                {
                    flag.OwningRealm = realm;
                    flag.SetObjectiveLocked();
                }
            }
            else
            {
                WorldMgr.GetRegion(plr.Region.RegionId, false).Campaign.LockBattleObjective(realm, objectiveToLock);
            }
        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Sets objectives in the players current realm as Guarded")]
        public static void MarkObjectivesAsGuarded(Player plr)
        {
            plr.SendClientMessage($"Attempting to lock objective...");

            foreach (var flag in WorldMgr.GetRegion(plr.Region.RegionId, false).Campaign.Objectives)
            {
                if (flag.ZoneId == plr.ZoneId)
                {
                    flag.OwningRealm = plr.Realm;
                    flag.SetObjectiveGuarded();
                }
            }
            // For VPP update
            WorldMgr.UpperTierCampaignManager.GetActiveCampaign().
                VictoryPointProgress.UpdateStatus(WorldMgr.UpperTierCampaignManager.GetActiveCampaign());
        }





        [CommandAttribute(EGmLevel.DatabaseDev, "Sets the number of VP for a realm")]
        public static void SetVictoryPoints(Player plr, Realms realm, int points)
        {
            if (plr.Zone == null || plr.Region.Campaign == null)
            {
                SendCsr(plr, "CAMPAIGN SUPPLY: Must be in a RvR zone to use this command.");
                return;
            }
            var BattleFront = (Campaign)plr.Region.Campaign;

            if (realm == Realms.REALMS_REALM_ORDER)
                BattleFront.VictoryPointProgress.OrderVictoryPoints = points;
            if (realm == Realms.REALMS_REALM_DESTRUCTION)
                BattleFront.VictoryPointProgress.DestructionVictoryPoints = points;

            plr.SendClientMessage($"Victory Points set to {points} for realm {realm}");
            _logger.Info($"{plr.Name} set Victory Points set to {points} for realm {realm}");
        }


        [CommandAttribute(EGmLevel.DatabaseDev, "Returns the World Campaign Status")]
        public static void Status(Player plr)
        {
            if (plr.Zone == null || plr.Region.Campaign == null)
            {
                SendCsr(plr, "Must be in a RvR zone to use this command.");
                return;
            }
            plr.SendClientMessage($"Lower Tier {WorldMgr.LowerTierCampaignManager.ActiveBattleFrontName} is active.");

            plr.SendClientMessage($"  Campaign Status : \t {WorldMgr.GetRegion((ushort)WorldMgr.LowerTierCampaignManager.ActiveBattleFront.RegionId, false).Campaign.GetBattleFrontStatus()}");

            plr.SendClientMessage($"Upper Tier {WorldMgr.UpperTierCampaignManager.ActiveBattleFrontName} is active.");

            plr.SendClientMessage($"  Campaign Status : \t {WorldMgr.GetRegion((ushort)WorldMgr.UpperTierCampaignManager.ActiveBattleFront.RegionId, false).Campaign.GetBattleFrontStatus()}");

            plr.SendClientMessage($"  Order RC : \t {WorldMgr.GetRegion((ushort)WorldMgr.UpperTierCampaignManager.ActiveBattleFront.RegionId, false).Campaign.ActiveBattleFrontStatus.OrderRealmCaptain?.Name}");
            plr.SendClientMessage($"  Dest RC : \t {WorldMgr.GetRegion((ushort)WorldMgr.UpperTierCampaignManager.ActiveBattleFront.RegionId, false).Campaign.ActiveBattleFrontStatus.DestructionRealmCaptain?.Name}");

            foreach (var flag in plr.Region.Campaign.Objectives)
                plr.SendClientMessage($"{flag.ToString()}");



        }


        [CommandAttribute(EGmLevel.DatabaseDev, "Get or sets warcamp entrance, use realm parameter order|destruction or 1|2 to update entrance coordinate")]
        public static void Warcamp(Player plr, string realm = "")
        {
            ushort zoneId = plr.Zone.ZoneId;

            if (realm == "")
            {
                // Display current entrances locations
                plr.SendClientMessage("Order spawn (1) : " + BattleFrontService.GetWarcampEntrance(zoneId, Realms.REALMS_REALM_ORDER));
                plr.SendClientMessage("Destruction spawn (2) : " + BattleFrontService.GetWarcampEntrance(zoneId, Realms.REALMS_REALM_DESTRUCTION));
                return;
            }

            Realms newRealm = Realms.REALMS_REALM_NEUTRAL;
            if (realm == "order" || realm == "1")
                newRealm = Realms.REALMS_REALM_ORDER;
            else if (realm == "destruction" || realm == "2")
                newRealm = Realms.REALMS_REALM_DESTRUCTION;
            else
            {
                SendCsr(plr, "CAMPAIGN WARCAMP: illegal realm argument, must be order|destruction or 1|2.");
                return;
            }

            BattleFrontObject oldObject = WorldMgr.Database.SelectObject<BattleFrontObject>($"ZoneId = {zoneId} AND Realm = {(int)newRealm}");
            if (oldObject != null)
            {
                WorldMgr.Database.DeleteObject(oldObject);
                oldObject.Dirty = true;
                WorldMgr.Database.ForceSave();
            }

            BattleFrontObject newObject = new BattleFrontObject
            {
                Type = (ushort)BattleFrontObjectType.WARCAMP_ENTRANCE,
                ObjectiveID = 0,
                Realm = (ushort)newRealm,
            };
            AddObject(plr, newObject);
            BattleFrontService.LoadBattleFrontObjects();

            SendCsr(plr, $"CAMPAIGN WARCAMP: {(newRealm == Realms.REALMS_REALM_ORDER ? "order" : "destruction")} warcamp is set");
        }



        /// <summary>
        /// Computes distance of player to warcamp of given realm.
        /// </summary>
        /// <param name="plr">Player to compute distance from</param>
        /// <param name="realm">Realm of the warcamp to compute distance to</param>
        /// <returns>Distance or int.MaxValue if not found in current zone</returns>
        private static int GetWarcampDistance(Player plr, Realms realm)
        {
            ushort zoneId = plr.ZoneId.Value;
            Point3D warcampLoc = BattleFrontService.GetWarcampEntrance(zoneId, realm);
            return warcampLoc != null ? plr.GetDistanceTo(warcampLoc) : int.MaxValue;
        }

        /// <summary>
        /// Adds the given object at player location.
        /// </summary>
        /// <param name="plr">Player providing the location or created object.</param>
        /// <param name="newObject">Object to add with Campaign specific properties configured</param>
        private static void AddObject(Player plr, BattleFrontObject newObject)
        {
            int max = (int)WorldMgr.Database.GetMaxColValue<BattleFrontObject>("Entry");
            newObject.Entry = max + 1;

            newObject.RegionId = plr.Region.RegionId;
            newObject.ZoneId = plr.ZoneId.Value;
            newObject.X = plr.X;
            newObject.Y = plr.Y;
            newObject.Z = plr.Z;
            newObject.O = plr.Heading;
            newObject.Dirty = true;
            newObject.IsValid = true;

            WorldMgr.Database.AddObject(newObject);
            WorldMgr.Database.ForceSave();
        }

        [CommandAttribute(EGmLevel.DatabaseDev, "Resets the World Campaign to default values")]
        public static void ResetAllCampaign(Player plr)
        {
            


            foreach (var progression in WorldMgr.UpperTierCampaignManager.BattleFrontProgressions)
            {
                if (progression.Tier == 4)
                {
                    progression.DestroVP = 0;
                    progression.OrderVP = 0;
                    progression.LastOpenedZone = 0;
                    progression.LastOwningRealm = progression.DefaultRealmLock;

                    if (progression.BattleFrontId == 2) // PRAAG
                    {
                        progression.LastOpenedZone = 1;
                        WorldMgr.UpperTierCampaignManager.ActiveBattleFront = progression;
                        WorldMgr.UpperTierCampaignManager.GetActiveCampaign().Keeps.SingleOrDefault(x=>x.Info.KeepId == progression.OrderKeepId).Realm = Realms.REALMS_REALM_ORDER;
                        WorldMgr.UpperTierCampaignManager.GetActiveCampaign().Keeps.SingleOrDefault(x => x.Info.KeepId == progression.OrderKeepId).SetKeepSafe();
                        WorldMgr.UpperTierCampaignManager.GetActiveCampaign().Keeps.SingleOrDefault(x => x.Info.KeepId == progression.OrderKeepId).Realm = Realms.REALMS_REALM_DESTRUCTION;
                        WorldMgr.UpperTierCampaignManager.GetActiveCampaign().Keeps.SingleOrDefault(x => x.Info.KeepId == progression.DestroKeepId).SetKeepSafe();
                        var objectives = WorldMgr.UpperTierCampaignManager.GetActiveCampaign().Objectives
                            .Where(x => x.ZoneId == progression.ZoneId);
                        foreach (var battlefieldObjective in objectives)
                        {
                            battlefieldObjective.SetObjectiveSafe();
                        }
                    }

                    var status = WorldMgr.UpperTierCampaignManager.GetBattleFrontStatusList().SingleOrDefault(x => x.BattleFrontId == progression.BattleFrontId);
                    if (status != null)
                    {
                        status.Locked = true;
                        if (status.BattleFrontId == 2)
                        {
                            status.Locked = false;
                        }
                        status.OpenTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                        status.LockingRealm = (Realms) progression.DefaultRealmLock;
                        status.FinalVictoryPoint = new VictoryPointProgress();
                        status.LockTimeStamp = 0;
                        // Reset the population for the battle front status
                        WorldMgr.UpperTierCampaignManager.GetActiveCampaign().InitializePopulationList(status.BattleFrontId);

                        
                    }
                }
            }

            // Unlock the next Progression
            WorldMgr.UpperTierCampaignManager.OpenActiveBattlefront();

            // This is kind of nasty, should use an event to signal the WorldMgr
            // Tell the server that the RVR status has changed.
            WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);
            _logger.Info($"{plr.Name} set RESET ALL");
        }
    }
}
