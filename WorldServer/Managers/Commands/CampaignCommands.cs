using Common;
using System;
using System.Collections.Generic;
using SystemData;
using static WorldServer.Managers.Commands.GMUtils;
using WorldServer.World.Battlefronts;
using FrameWork;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using GameData;
using Common.Database.World.Battlefront;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Battlefronts.Objectives;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.NewDawn;
using BattlefrontConstants = WorldServer.World.Battlefronts.NewDawn.BattlefrontConstants;

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
            var battlefront = plr.Region.ndbf;
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
                        battlefront = region.ndbf;
                    }
                    else
                        SendCsr(plr, "Please enter a valid regionID");
                    break;
            }

            battlefront.CampaignDiagnostic(plr, bLocalZone);
        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Updates some constant parameters. Give no arg to list constants.")]
        public static void Constants(Player plr, string nameOrShortcut = "", string newValue = "")
        {
            Type type = typeof(BattlefrontConstants);

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

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Locks the current battlefront (under t4)")]
        public static void Lock(Player plr, Realms realm, string noReward)
        {
            if (noReward == "0" || noReward == "1")
            {
                plr.SendClientMessage($"Attempting to lock the {plr.Region.RegionId} campaign...");

                IBattlefront battlefront = plr.Region.Bttlfront;

                bool b = false;
                if (noReward == "0")
                    b = true;

                if (GameData.Constants.DoomsdaySwitch == 2)
                {
                    ProximityProgressingBattlefront pBttlfront = battlefront as ProximityProgressingBattlefront;
                    if (pBttlfront != null)
                    {
                        pBttlfront.LockZone(realm, (int)plr.ZoneId, true, false, b); // Reset changed to false
                    }
                    else
                        battlefront.LockPairing(realm, false, false, b);

                }
                else if (GameData.Constants.DoomsdaySwitch > 0)
                {
                    ProgressingBattlefront pBttlfront = battlefront as ProgressingBattlefront;
                    if (pBttlfront != null)
                    {
                        pBttlfront.LockZone(realm, (int)plr.ZoneId, true, true, b);
                    }
                    else
                        battlefront.LockPairing(realm, false, false, b);

                }
                else
                    battlefront.LockPairing(realm, true);
            }
            else
                plr.SendClientMessage("Second parameter must be 0 or 1 - 0 no rewards, 1 grants rewards.");
        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Locks the current battlefront, no winners")]
        public static void Draw(Player plr)
        {
            plr.SendClientMessage($"Attempting to lock the {plr.Region.RegionId} campaign...");
            IBattlefront battlefront = plr.Region.Bttlfront;

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
                ProximityProgressingBattlefront pBttlfront = battlefront as ProximityProgressingBattlefront;
                if (pBttlfront != null)
                    pBttlfront.LockZone(realm, (int)plr.ZoneId, true, false, false, true); // Reset changed to false
                else
                    battlefront.LockPairing(realm, false, false, false, true);
            }
            else
                battlefront.LockPairing(realm, true);
        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Enables or disables grace.")]
        public static void Grace(Player plr)
        {
            IBattlefront battlefront = plr.Region.Bttlfront;
            if (GameData.Constants.DoomsdaySwitch == 2)
            {
                ProximityProgressingBattlefront pBttlfront = battlefront as ProximityProgressingBattlefront;

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
                    ProximityBattlefront bttlfront = battlefront as ProximityBattlefront;

                    if (bttlfront.GraceDisabled)
                        bttlfront.StartGrace();
                    else
                        bttlfront.EndGrace();

                    plr.SendClientMessage("Current Value of Grace: " + bttlfront.GraceDisabled, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
                }
            }
            else
            {
                ProgressingBattlefront pBttlfront = battlefront as ProgressingBattlefront;

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
                    Battlefront bttlfront = battlefront as Battlefront;

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
            IBattlefront battlefront = plr.Region.Bttlfront;
            if (GameData.Constants.DoomsdaySwitch == 2)
            {
                ProximityProgressingBattlefront pBttlfront = battlefront as ProximityProgressingBattlefront;

                if (pBttlfront != null)
                {
                    foreach (Keep keep in pBttlfront._Keeps)
                        keep.SafeKeep();

                    plr.SendClientMessage("Keeps set to status safe");
                }
                else
                {
                    ProximityBattlefront bttlfront = battlefront as ProximityBattlefront;

                    foreach (Keep keep in bttlfront._Keeps)
                        keep.SafeKeep();

                    plr.SendClientMessage("Keeps set to status safe");
                }
            }
            else
            {
                ProgressingBattlefront pBttlfront = battlefront as ProgressingBattlefront;

                if (pBttlfront != null)
                {
                    foreach (Keep keep in pBttlfront._Keeps)
                        keep.SafeKeep();

                    plr.SendClientMessage("Keeps set to status safe");
                }
                else
                {
                    Battlefront bttlfront = battlefront as Battlefront;

                    foreach (Keep keep in bttlfront._Keeps)
                        keep.SafeKeep();

                    plr.SendClientMessage("Keeps set to status safe");
                }
            }
        }

        // Temporary until new rvr is stabilized
        [CommandAttribute(EGmLevel.EmpoweredStaff, "Resets the current battlefront (under t4)")]
        public static void Reset(Player plr)
        {
            plr.SendClientMessage($"Attempting to reset the {plr.Region.RegionId} campaign...");

            IBattlefront battlefront = plr.Region.Bttlfront;
            //NEWDAWN
            if (battlefront != null)
                battlefront.ResetPairing();
            else
            {
                plr.Region.ndbf.ResetPairing();
            }
        }

        [CommandAttribute(EGmLevel.EmpoweredStaff, "Attempts to reset the T4 campaign")]
        public static void ResetT4(Player plr)
        {
            plr.SendClientMessage("This command is disabled, as it will break T4 RvR...");
            //plr.SendClientMessage("Attempting to reset the T4 campaign...");

            /*IBattlefront DvG = WorldMgr.GetRegion(2, false).Bttlfront;
            IBattlefront EvC = WorldMgr.GetRegion(11, false).Bttlfront;
            IBattlefront HEvDE = WorldMgr.GetRegion(4, false).Bttlfront;

            DvG.ResetPairing();
            EvC.ResetPairing();
            HEvDE.ResetPairing();*/
        }

        [CommandAttribute(EGmLevel.DatabaseDev, "Adds a resource spawn point at the current location for the nearest objective - legacy")]
        public static void Point(Player plr)
        {
            if (plr.Zone == null)
            {
                SendCsr(plr, "CAMPAIGN POINT: Must be in a zone to use this command.");
                return;
            }

            IBattlefrontFlag closestFlag = plr.Region.Bttlfront.GetClosestFlag(plr.WorldPosition);

            if (closestFlag == null)
            {
                SendCsr(plr, "CAMPAIGN POINT: Must be in an open-world RvR zone.");
                return;
            }
            else if (!(closestFlag is BattlefrontFlag))
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

            BattlefrontResourceSpawn res = new BattlefrontResourceSpawn
            {
                Entry = ((BattlefrontFlag)closestFlag).ID,
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
            else if (!(plr.Region.Bttlfront is Battlefront))
            {
                SendCsr(plr, "CAMPAIGN DROP: This command is supported in legacy RvR.");
                return;
            }

            Keep closestKeep = ((Battlefront)plr.Region.Bttlfront).GetZoneKeep(plr.Zone.ZoneId, realmIndex);

            if (closestKeep == null)
            {
                SendCsr(plr, "CAMPAIGN DROP: Must be in an open-world RvR zone.");
                return;
            }

            plr.SendClientMessage("Keep: " + closestKeep.Info.Name);

            BattlefrontResourceSpawn res = new BattlefrontResourceSpawn
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
            else if (!(plr.Region.Bttlfront is Battlefront))
            {
                SendCsr(plr, "CAMPAIGN SUPPLY: This command is supported in legacy RvR.");
                return;
            }

            Battlefront battlefront = (Battlefront)plr.Region.Bttlfront;

            battlefront.ToggleSupplies();

            plr.SendClientMessage("Supplies " + (battlefront.NoSupplies ? "disabled." : "enabled."));
        }

        [CommandAttribute(EGmLevel.DatabaseDev, "Sets the number of VP for a realm")]
        public static void SetVictoryPoints(Player plr, Realms realm, int points)
        {
            if (plr.Zone == null || plr.Region.ndbf == null)
            {
                SendCsr(plr, "CAMPAIGN SUPPLY: Must be in a RvR zone to use this command.");
                return;
            }
            var battlefront = (NewDawnBattlefront)plr.Region.ndbf;

            if (realm == Realms.REALMS_REALM_ORDER)
                battlefront.VictoryPointProgress.OrderVictoryPoints = points;
            if (realm == Realms.REALMS_REALM_DESTRUCTION)
                battlefront.VictoryPointProgress.DestructionVictoryPoints = points;

            plr.SendClientMessage($"Victory Points set to {points} for realm {realm}");
        }


        [CommandAttribute(EGmLevel.DatabaseDev, "Returns the World Campaign Status")]
        public static void Status(Player plr)
        {
            if (plr.Zone == null || plr.Region.ndbf == null)
            {
                SendCsr(plr, "Must be in a RvR zone to use this command.");
                return;
            }
            plr.SendClientMessage($"Lower Tier {WorldMgr.LowerTierBattlefrontManager.GetActivePairing().PairingName} is active.");

            plr.SendClientMessage($"  Battlefront Status : {WorldMgr.GetRegion((ushort)WorldMgr.LowerTierBattlefrontManager.GetActivePairing().RegionId, false).GetBattleFrontStatus()}");

            plr.SendClientMessage($"Upper Tier {WorldMgr.UpperTierBattlefrontManager.GetActivePairing().PairingName} is active.");

            plr.SendClientMessage($"  Battlefront Status : {WorldMgr.GetRegion((ushort)WorldMgr.UpperTierBattlefrontManager.GetActivePairing().RegionId, false).GetBattleFrontStatus()}");

        }

        [CommandAttribute(EGmLevel.DatabaseDev, "Sets an objective flag portal location, flag to warcamp if no objective name / otherwise warcamp to flag")]
        public static void Portal(Player plr, string objectiveName = null)
        {
            // Current player battlefront
            IBattlefront battlefront;
            if (plr.Region.GetTier() == 1)
                battlefront = plr.Region.Bttlfront as RoRBattlefront;
            else if (plr.Region.GetTier() > 1)
                battlefront = plr.Region.Bttlfront as ProximityBattlefront;
            else
                return;

            if (battlefront == null)
            {
                SendCsr(plr, "CAMPAIGN PORTAL: Must be in a battlefront.");
                return;
            }

            // Current flag
            ProximityFlag flag;
            string notFoundMessage;
            string successMessage;
            BattlefrontObjectType type;
            if (objectiveName == null)
            {
                flag = battlefront.GetClosestFlag(plr.WorldPosition) as ProximityFlag;
                type = BattlefrontObjectType.WARCAMP_PORTAL;
                notFoundMessage = "CAMPAIGN PORTAL: Must be near a proximity flag in rvr lake if no name is given.";
                successMessage = $"CAMPAIGN PORTAL: Portal from {(flag != null ? flag.Name : "")} to warcamp has been set.";
            }
            else
            {
                flag = GetFlag(battlefront, objectiveName);
                notFoundMessage = "CAMPAIGN PORTAL: Could not find objective of name : " + objectiveName;
                type = BattlefrontObjectType.OBJECTIVE_PORTAL;
                successMessage = $"CAMPAIGN PORTAL: Portal from warcamp to {(flag != null ? flag.Name : "")} has been set for ";
            }

            if (flag == null)
            {
                SendCsr(plr, notFoundMessage);
                return;
            }

            // Database update
            BattlefrontService.LoadBattlefrontObjects();
            BattlefrontObject newObject = new BattlefrontObject()
            {
                Type = (byte)type,
                ObjectiveID = flag.ID,
            };

            BattlefrontObject oldObject;

            if (type == BattlefrontObjectType.OBJECTIVE_PORTAL)
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
                oldObject = BattlefrontService.GetPortalToObjective(plr.ZoneId.Value, flag.ID, realm);
            }
            else
            {
                oldObject = BattlefrontService.GetPortalToWarcamp(plr.ZoneId.Value, flag.ID);
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
                plr.SendClientMessage("Order spawn (1) : " + BattlefrontService.GetWarcampEntrance(zoneId, Realms.REALMS_REALM_ORDER));
                plr.SendClientMessage("Destruction spawn (2) : " + BattlefrontService.GetWarcampEntrance(zoneId, Realms.REALMS_REALM_DESTRUCTION));
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

            BattlefrontObject oldObject = WorldMgr.Database.SelectObject<BattlefrontObject>($"ZoneId = {zoneId} AND Realm = {(int)newRealm}");
            if (oldObject != null)
            {
                WorldMgr.Database.DeleteObject(oldObject);
                oldObject.Dirty = true;
                WorldMgr.Database.ForceSave();
            }

            BattlefrontObject newObject = new BattlefrontObject
            {
                Type = (ushort)BattlefrontObjectType.WARCAMP_ENTRANCE,
                ObjectiveID = 0,
                Realm = (ushort)newRealm,
            };
            AddObject(plr, newObject);
            BattlefrontService.LoadBattlefrontObjects();

            SendCsr(plr, $"CAMPAIGN WARCAMP: {(newRealm == Realms.REALMS_REALM_ORDER ? "order" : "destruction")} warcamp is set");
        }

        /// <summary>
        /// Gets an objective flag in current battlefront depending on given id.
        /// </summary>
        /// <param name="battlefront">Battlefront to search in</param>
        /// <param name="name">Name of the searched objective (starts with)</param>
        /// <returns>flag or null</returns>
        private static ProximityFlag GetFlag(IBattlefront battlefront, String name)
        {
            if (name == null)
                return null;
            name = name.ToLowerInvariant();
            foreach (ProximityFlag existing in battlefront.Objectives)
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
            Point3D warcampLoc = BattlefrontService.GetWarcampEntrance(zoneId, realm);
            return warcampLoc != null ? plr.GetDistanceTo(warcampLoc) : int.MaxValue;
        }

        /// <summary>
        /// Adds the given object at player location.
        /// </summary>
        /// <param name="plr">Player providing the location or created object.</param>
        /// <param name="newObject">Object to add with battlefront specific properties configured</param>
        private static void AddObject(Player plr, BattlefrontObject newObject)
        {
            int max = (int)WorldMgr.Database.GetMaxColValue<BattlefrontObject>("Entry");
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
