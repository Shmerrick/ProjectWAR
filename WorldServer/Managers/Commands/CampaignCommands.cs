using Common;
using System;
using System.Collections.Generic;
using SystemData;
using static WorldServer.Managers.Commands.GMUtils;
using WorldServer.World.BattleFronts;
using FrameWork;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using GameData;
using Common.Database.World.BattleFront;
using WorldServer.World.BattleFronts.Keeps;
using WorldServer.World.BattleFronts.Objectives;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using BattleFrontConstants = WorldServer.World.Battlefronts.Apocalypse.BattleFrontConstants;

namespace WorldServer.Managers.Commands
{
    /// <summary>RvR campaign commmands under .campaign</summary>
    internal class CampaignCommands
    {
        /// <summary>Constant initials extractor<summary>
        private static readonly Regex INITIALS = new Regex(@"([A-Z])[A-Z1-9]*_?");

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Provides campaign diagnostic info. Usage: .campaign diag (zone|region) or .campaign diag")]
        public static void Diag(Player plr, string targetString = null)
        {
            // Weird algorithm but it's for legacy purpose only
            bool bLocalZone = true;
            var BattleFront = plr.Region.BattleFront;
            switch (targetString)
            {
                case "zone":
                    bLocalZone = true;
                    break;
                case "region":
                    bLocalZone = false;
                    break;
                default:
                    bLocalZone = false;
                    ushort regionId;
                    if (targetString != null && UInt16.TryParse(targetString, out regionId))
                    {
                        RegionMgr region = WorldMgr.GetRegion(regionId, false);
                        if (region == null)
                        {
                            SendCsr(plr, "Unkown region : ", regionId.ToString());
                            return;
                        }
                        BattleFront = region.BattleFront;
                    }
                    else
                        SendCsr(plr, "Please enter a valid regionID");
                    break;
            }

            BattleFront.CampaignDiagnostic(plr, bLocalZone);
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

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Locks the current BattleFront (under t4)")]
        public static void Lock(Player plr, Realms realm, string noReward)
        {
            if (noReward == "0" || noReward == "1")
            {
                plr.SendClientMessage($"Attempting to lock the {plr.Region.RegionId} campaign...");

                IBattleFront BattleFront = plr.Region.Bttlfront;

                bool b = false;
                if (noReward == "0")
                    b = true;

                if (GameData.Constants.DoomsdaySwitch == 2)
                {
                    ProximityProgressingBattleFront pBttlfront = BattleFront as ProximityProgressingBattleFront;
                    if (pBttlfront != null)
                    {
                        pBttlfront.LockZone(realm, (int)plr.ZoneId, true, false, b); // Reset changed to false
                    }
                    else
                        BattleFront.LockPairing(realm, false, false, b);

                }
                else if (GameData.Constants.DoomsdaySwitch > 0)
                {
                    ProgressingBattleFront pBttlfront = BattleFront as ProgressingBattleFront;
                    if (pBttlfront != null)
                    {
                        pBttlfront.LockZone(realm, (int)plr.ZoneId, true, true, b);
                    }
                    else
                        BattleFront.LockPairing(realm, false, false, b);

                }
                else
                    BattleFront.LockPairing(realm, true);
            }
            else
                plr.SendClientMessage("Second parameter must be 0 or 1 - 0 no rewards, 1 grants rewards.");
        }


        [CommandAttribute(EGmLevel.EmpoweredStaff, "Locks the pairing the player is in for the given realm (1 - Order, 2 - Dest). 0 - no reward, 1 - reward")]
        public static void LockPairing(Player plr, Realms realm, string noReward)
        {
            if (noReward == "0" || noReward == "1")
            {
                plr.SendClientMessage($"Attempting to lock the {plr.Region.RegionId} campaign...");

                IBattleFront BattleFront = plr.Region.Bttlfront;

                bool b = noReward == "0";

                WorldMgr.GetRegion(plr.Region.RegionId, false).BattleFront.BattleFrontManager.LockActiveBattleFront(realm);
            }
            else
                plr.SendClientMessage("Second parameter must be 0 or 1 - 0 no rewards, 1 grants rewards.");
        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Advances the pairing the player is in ")]
        public static void AdvancePairing(Player plr, Realms realm, int tier)
        {

            if (tier == 1)
            {
                var progression = WorldMgr.LowerTierBattleFrontManager.AdvanceBattleFront(realm);
                WorldMgr.LowerTierBattleFrontManager.OpenActiveBattlefront();
                plr.SendClientMessage($"{realm.ToString()} pushes - next battle is in {progression.Description}");
            }
            else
            {
                var progression = WorldMgr.UpperTierBattleFrontManager.AdvanceBattleFront(realm);
                WorldMgr.UpperTierBattleFrontManager.OpenActiveBattlefront();
                plr.SendClientMessage($"{realm.ToString()} pushes - next battle is in {progression.Description}");

            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Sends server commands to the client")]
        public static void SendComms(Player player, int destVP, int orderVP, int realm)
        {
            var vpp = new VictoryPointProgress();
            vpp.DestructionVictoryPoints = destVP;
            vpp.OrderVictoryPoints = orderVP;

            Realms lockingRealm;

            if (realm == 1)
                lockingRealm = Realms.REALMS_REALM_ORDER;
            if (realm == 2)
                lockingRealm = Realms.REALMS_REALM_DESTRUCTION;
            else
                lockingRealm = Realms.REALMS_REALM_NEUTRAL;

            //new ApocCommunications().UpdateRegionCaptureStatus(player, lockingRealm, vpp);

           // new ApocCommunications().SendCampaignStatus(player, vpp, lockingRealm);
        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Locks a battle objective for the given realm (1 - Order, 2 - Dest).")]
        public static void LockObj(Player plr, Realms realm, int values)
        {
            plr.SendClientMessage($"Attempting to lock objective...");

            var objectiveToLock = values;

            WorldMgr.GetRegion(plr.Region.RegionId, false).BattleFront.LockBattleObjective(realm, objectiveToLock);

        }


        [CommandAttribute(EGmLevel.EmpoweredStaff, "Locks the current BattleFront, no winners")]
        public static void Draw(Player plr)
        {
            plr.SendClientMessage($"Attempting to lock the {plr.Region.RegionId} campaign...");
            IBattleFront BattleFront = plr.Region.Bttlfront;

            Random random = new Random();
            Realms realm;

            switch (random.Next(1, 3))
            {
                case 1:
                    realm = Realms.REALMS_REALM_ORDER;
                    break;
                case 2:
                    realm = Realms.REALMS_REALM_DESTRUCTION;
                    break;
                default:
                    realm = Realms.REALMS_REALM_ORDER;
                    break;
            }

            if (GameData.Constants.DoomsdaySwitch == 2)
            {
                ProximityProgressingBattleFront pBttlfront = BattleFront as ProximityProgressingBattleFront;
                if (pBttlfront != null)
                    pBttlfront.LockZone(realm, (int)plr.ZoneId, true, false, false, true); // Reset changed to false
                else
                    BattleFront.LockPairing(realm, false, false, false, true);
            }
            else
                BattleFront.LockPairing(realm, true);
        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Enables or disables grace.")]
        public static void Grace(Player plr)
        {
            IBattleFront BattleFront = plr.Region.Bttlfront;
            if (GameData.Constants.DoomsdaySwitch == 2)
            {
                ProximityProgressingBattleFront pBttlfront = BattleFront as ProximityProgressingBattleFront;

                if (pBttlfront != null)
                {
                    if (pBttlfront.GraceDisabled)
                        pBttlfront.StartGrace();
                    else
                        pBttlfront.EndGrace();

                    plr.SendClientMessage("Current Value of Grace: " + pBttlfront.GraceDisabled, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
                }
                else
                {
                    ProximityBattleFront bttlfront = BattleFront as ProximityBattleFront;

                    if (bttlfront.GraceDisabled)
                        bttlfront.StartGrace();
                    else
                        bttlfront.EndGrace();

                    plr.SendClientMessage("Current Value of Grace: " + bttlfront.GraceDisabled, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
                }
            }
            else
            {
                ProgressingBattleFront pBttlfront = BattleFront as ProgressingBattleFront;

                if (pBttlfront != null)
                {
                    if (pBttlfront.GraceDisabled)
                        pBttlfront.StartGrace();
                    else
                        pBttlfront.EndGrace();

                    plr.SendClientMessage("Current Value of Grace: " + pBttlfront.GraceDisabled);
                }
                else
                {
                    BattleFront bttlfront = BattleFront as BattleFront;

                    if (bttlfront.GraceDisabled)
                        bttlfront.StartGrace();
                    else
                        bttlfront.EndGrace();

                    plr.SendClientMessage("Current Value of Grace: " + pBttlfront.GraceDisabled);
                }
            }
        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Makes keep safe again.")]
        public static void SafeKeep(Player plr)
        {
            IBattleFront BattleFront = plr.Region.Bttlfront;
            if (GameData.Constants.DoomsdaySwitch == 2)
            {
                ProximityProgressingBattleFront pBttlfront = BattleFront as ProximityProgressingBattleFront;

                if (pBttlfront != null)
                {
                    foreach (Keep keep in pBttlfront._Keeps)
                        keep.SafeKeep();

                    plr.SendClientMessage("Keeps set to status safe");
                }
                else
                {
                    ProximityBattleFront bttlfront = BattleFront as ProximityBattleFront;

                    foreach (Keep keep in bttlfront._Keeps)
                        keep.SafeKeep();

                    plr.SendClientMessage("Keeps set to status safe");
                }
            }
            else
            {
                ProgressingBattleFront pBttlfront = BattleFront as ProgressingBattleFront;

                if (pBttlfront != null)
                {
                    foreach (Keep keep in pBttlfront._Keeps)
                        keep.SafeKeep();

                    plr.SendClientMessage("Keeps set to status safe");
                }
                else
                {
                    BattleFront bttlfront = BattleFront as BattleFront;

                    foreach (Keep keep in bttlfront._Keeps)
                        keep.SafeKeep();

                    plr.SendClientMessage("Keeps set to status safe");
                }
            }
        }



        [CommandAttribute(EGmLevel.DatabaseDev, "Adds a resource spawn point at the current location for the nearest objective - legacy")]
        public static void Point(Player plr)
        {
            if (plr.Zone == null)
            {
                SendCsr(plr, "CAMPAIGN POINT: Must be in a zone to use this command.");
                return;
            }

            IBattleFrontFlag closestFlag = plr.Region.Bttlfront.GetClosestFlag(plr.WorldPosition);

            if (closestFlag == null)
            {
                SendCsr(plr, "CAMPAIGN POINT: Must be in an open-world RvR zone.");
                return;
            }
            else if (!(closestFlag is BattleFrontFlag))
            {
                SendCsr(plr, "CAMPAIGN POINT: This command is supported in legacy RvR.");
                return;
            }

            plr.SendClientMessage("Flag: " + closestFlag.ObjectiveName);
            GameObject_proto proto = GameObjectService.GetGameObjectProto(429);

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldX = plr.WorldPosition.X,
                WorldY = plr.WorldPosition.Y,
                WorldZ = plr.WorldPosition.Z,
                WorldO = plr.Heading,
                ZoneId = plr.Zone.ZoneId
            };

            spawn.BuildFromProto(proto);
            plr.Region.CreateGameObject(spawn);

            BattleFrontResourceSpawn res = new BattleFrontResourceSpawn
            {
                Entry = ((BattleFrontFlag)closestFlag).ID,
                X = plr.X,
                Y = plr.Y,
                Z = plr.Z,
                O = plr.Heading
            };

            WorldMgr.Database.AddObject(res);
        }

        [CommandAttribute(EGmLevel.DatabaseDev, "Adds a resource return point at the current location - legacy")]
        public static void Drop(Player plr, int realmIndex)
        {
            if (plr.Zone == null)
            {
                SendCsr(plr, "CAMPAIGN DROP: Must be in a zone to use this command.");
                return;
            }
            else if (!(plr.Region.Bttlfront is BattleFront))
            {
                SendCsr(plr, "CAMPAIGN DROP: This command is supported in legacy RvR.");
                return;
            }

            Keep closestKeep = ((BattleFront)plr.Region.Bttlfront).GetZoneKeep(plr.Zone.ZoneId, realmIndex);

            if (closestKeep == null)
            {
                SendCsr(plr, "CAMPAIGN DROP: Must be in an open-world RvR zone.");
                return;
            }

            plr.SendClientMessage("Keep: " + closestKeep.Info.Name);

            BattleFrontResourceSpawn res = new BattleFrontResourceSpawn
            {
                Entry = closestKeep.Info.KeepId,
                X = plr.X,
                Y = plr.Y,
                Z = plr.Z,
                O = plr.Heading
            };

            WorldMgr.Database.AddObject(res);

            closestKeep.SupplyReturnPoints.Clear();
            closestKeep.SupplyReturnPoints.Add(res);
            closestKeep.CreateSupplyDrops();
        }

        [CommandAttribute(EGmLevel.DatabaseDev, "Toggles supply system for current map - legacy")]
        public static void SupplyToggle(Player plr)
        {
            if (plr.Zone == null || plr.Region.Bttlfront == null)
            {
                SendCsr(plr, "CAMPAIGN SUPPLY: Must be in a RvR zone to use this command.");
                return;
            }
            else if (!(plr.Region.Bttlfront is BattleFront))
            {
                SendCsr(plr, "CAMPAIGN SUPPLY: This command is supported in legacy RvR.");
                return;
            }

            BattleFront BattleFront = (BattleFront)plr.Region.Bttlfront;

            BattleFront.ToggleSupplies();

            plr.SendClientMessage("Supplies " + (BattleFront.NoSupplies ? "disabled." : "enabled."));
        }

        [CommandAttribute(EGmLevel.DatabaseDev, "Sets the number of VP for a realm")]
        public static void SetVictoryPoints(Player plr, Realms realm, int points)
        {
            if (plr.Zone == null || plr.Region.BattleFront == null)
            {
                SendCsr(plr, "CAMPAIGN SUPPLY: Must be in a RvR zone to use this command.");
                return;
            }
            var BattleFront = (ApocBattleFront)plr.Region.BattleFront;

            if (realm == Realms.REALMS_REALM_ORDER)
                BattleFront.VictoryPointProgress.OrderVictoryPoints = points;
            if (realm == Realms.REALMS_REALM_DESTRUCTION)
                BattleFront.VictoryPointProgress.DestructionVictoryPoints = points;

            plr.SendClientMessage($"Victory Points set to {points} for realm {realm}");
        }


        [CommandAttribute(EGmLevel.DatabaseDev, "Returns the World Campaign Status")]
        public static void Status(Player plr)
        {
            if (plr.Zone == null || plr.Region.BattleFront == null)
            {
                SendCsr(plr, "Must be in a RvR zone to use this command.");
                return;
            }
            plr.SendClientMessage($"Lower Tier {WorldMgr.LowerTierBattleFrontManager.ActiveBattleFrontName} is active.");

            plr.SendClientMessage($"  BattleFront Status : \t {WorldMgr.GetRegion((ushort)WorldMgr.LowerTierBattleFrontManager.ActiveBattleFront.RegionId, false).BattleFront.GetBattleFrontStatus()}");

            plr.SendClientMessage($"Upper Tier {WorldMgr.UpperTierBattleFrontManager.ActiveBattleFrontName} is active.");

            plr.SendClientMessage($"  BattleFront Status : \t {WorldMgr.GetRegion((ushort)WorldMgr.UpperTierBattleFrontManager.ActiveBattleFront.RegionId, false).BattleFront.GetBattleFrontStatus()}");

            foreach (var flag in plr.Region.BattleFront.Objectives)
                plr.SendClientMessage($"{flag.ToString()}");
        }

        [CommandAttribute(EGmLevel.DatabaseDev, "Sets an objective flag portal location, flag to warcamp if no objective name / otherwise warcamp to flag")]
        public static void Portal(Player plr, string objectiveName = null)
        {
            // Current player BattleFront
            IBattleFront BattleFront;
            if (plr.Region.GetTier() == 1)
                BattleFront = plr.Region.Bttlfront as RoRBattleFront;
            else if (plr.Region.GetTier() > 1)
                BattleFront = plr.Region.Bttlfront as ProximityBattleFront;
            else
                return;

            if (BattleFront == null)
            {
                SendCsr(plr, "CAMPAIGN PORTAL: Must be in a BattleFront.");
                return;
            }

            // Current flag
            ProximityFlag flag;
            string notFoundMessage;
            string successMessage;
            BattleFrontObjectType type;
            if (objectiveName == null)
            {
                flag = BattleFront.GetClosestFlag(plr.WorldPosition) as ProximityFlag;
                type = BattleFrontObjectType.WARCAMP_PORTAL;
                notFoundMessage = "CAMPAIGN PORTAL: Must be near a proximity flag in rvr lake if no name is given.";
                successMessage = $"CAMPAIGN PORTAL: Portal from {(flag != null ? flag.Name : "")} to warcamp has been set.";
            }
            else
            {
                flag = GetFlag(BattleFront, objectiveName);
                notFoundMessage = "CAMPAIGN PORTAL: Could not find objective of name : " + objectiveName;
                type = BattleFrontObjectType.OBJECTIVE_PORTAL;
                successMessage = $"CAMPAIGN PORTAL: Portal from warcamp to {(flag != null ? flag.Name : "")} has been set for ";
            }

            if (flag == null)
            {
                SendCsr(plr, notFoundMessage);
                return;
            }

            // Database update
            BattleFrontService.LoadBattleFrontObjects();
            BattleFrontObject newObject = new BattleFrontObject()
            {
                Type = (byte)type,
                ObjectiveID = flag.ID,
            };

            BattleFrontObject oldObject;

            if (type == BattleFrontObjectType.OBJECTIVE_PORTAL)
            {
                // Compute realm, depending on its location - quite a hack but will avoid parameter errors
                Realms realm;

                int orderDistance = GetWarcampDistance(plr, Realms.REALMS_REALM_ORDER);
                int destroDistance = GetWarcampDistance(plr, Realms.REALMS_REALM_DESTRUCTION);
                if (orderDistance < destroDistance) // In order warcamp
                {
                    realm = Realms.REALMS_REALM_ORDER;
                    successMessage += "Order.";
                }
                else // In destro warcamp
                {
                    realm = Realms.REALMS_REALM_DESTRUCTION;
                    successMessage += "Destruction.";
                }

                newObject.Realm = (ushort)realm;
                oldObject = BattleFrontService.GetPortalToObjective(plr.ZoneId.Value, flag.ID, realm);
            }
            else
            {
                oldObject = BattleFrontService.GetPortalToWarcamp(plr.ZoneId.Value, flag.ID);
            }

            if (oldObject != null)
            {
                WorldMgr.Database.DeleteObject(oldObject);
                oldObject.Dirty = true;
                WorldMgr.Database.ForceSave();
            }
            AddObject(plr, newObject);
            SendCsr(plr, successMessage);
        }

        [CommandAttribute(EGmLevel.DatabaseDev, "Get or sets warcamp entrance, use realm parameter order|destruction or 1|2 to update entrabce coordinate")]
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
        /// Gets an objective flag in current BattleFront depending on given id.
        /// </summary>
        /// <param name="BattleFront">BattleFront to search in</param>
        /// <param name="name">Name of the searched objective (starts with)</param>
        /// <returns>flag or null</returns>
        private static ProximityFlag GetFlag(IBattleFront BattleFront, String name)
        {
            if (name == null)
                return null;
            name = name.ToLowerInvariant();
            foreach (ProximityFlag existing in BattleFront.Objectives)
                if (existing.Name.ToLowerInvariant().Contains(name))
                    return existing;
            return null;
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
        /// <param name="newObject">Object to add with BattleFront specific properties configured</param>
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
    }
}
