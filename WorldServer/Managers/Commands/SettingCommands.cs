using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using GameData;
using static System.UInt16;
using static WorldServer.Managers.Commands.GMUtils;
using WorldServer.Services.World;
using WorldServer.World.Objects;

namespace WorldServer.Managers.Commands
{
    /// <summary>RvR campaign commmands under .campaign</summary>
    internal class SettingCommands
    {
        /// <summary>Constant initials extractor<summary>
        private static readonly Regex INITIALS = new Regex(@"([A-Z])[A-Z1-9]*_?");

        [CommandAttribute(EGmLevel.SourceDev, "Allows setting supply value scaler")]
        public static void SuppliesScaler(Player plr, string targetString = null)
        {
            ushort scaler;
            if (targetString != null && UInt16.TryParse(targetString, out scaler))
            {
                if (scaler > 0)
                {
                    WorldMgr.WorldSettingsMgr.SetSuppliesScaler(scaler);
                    plr.SendClientMessage("Changed Supplies Scaler to " + scaler, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
                }
                else
                    plr.SendClientMessage("Scaler cannot be set to 0 or less.", ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);

            }
            else
            {
                plr.SendClientMessage("Current Supplies Scaler is equal to " + WorldMgr.WorldSettingsMgr.GetSuppliesScaler(), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Allows setting Door Regen value")]
        public static void DoorRegen(Player plr, string targetString = null)
        {
            int doorRegen;
            if (targetString != null && Int32.TryParse(targetString, out doorRegen))
            {
                if (doorRegen > 0)
                {
                    WorldMgr.WorldSettingsMgr.SetDoorRegenValue(doorRegen);
                    plr.SendClientMessage("Changed Door Regen to " + doorRegen + " which amounts to " + (float)doorRegen / 10000f * 100f + "% per 1 tick from BattlefieldObjective.", ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
                }
                else
                    plr.SendClientMessage("Door Regen cannot be set to 0 or less.", ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);

            }
            else
            {
                plr.SendClientMessage("Current Door Regen is equal to " + WorldMgr.WorldSettingsMgr.GetDoorRegenValue() + " which amounts to " + (float)WorldMgr.WorldSettingsMgr.GetDoorRegenValue() / 10000f * 100f + "% per 1 tick from BattlefieldObjective.", ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Enables Movement Packet Throtle")]
        public static void MovementThrottle(Player plr, string targetString = null)
        {
            int throtleValue;
            if (targetString != null && Int32.TryParse(targetString, out throtleValue))
            {
                if (throtleValue == 0)
                {
                    WorldMgr.WorldSettingsMgr.SetMovementPacketThrotle(throtleValue);
                    plr.SendClientMessage("Changed Movement Packet Throtle to " + throtleValue, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
                }
                else if (throtleValue == 1)
                {
                    WorldMgr.WorldSettingsMgr.SetMovementPacketThrotle(throtleValue);
                    plr.SendClientMessage("Changed Movement Packet Throtle to " + throtleValue, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
                }
                else 
                    plr.SendClientMessage("Incorrect value for Movement Packet Throtle, 0 - disabled 1 - enabled", ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);

            }
            else
            {
                plr.SendClientMessage("Movement Packet Throtle is equal to " + WorldMgr.WorldSettingsMgr.GetMovementPacketThrotle(), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Set amount of refreshed ammunition per 1 minute from 1 BattlefieldObjective, provided value is divided by 10 to allow for fractures")]
        public static void AmmoRefresh(Player plr, string targetString = null)
        {
            int ammoRefreshRate;
            if (targetString != null && Int32.TryParse(targetString, out ammoRefreshRate))
            {
                if (ammoRefreshRate > 0)
                {
                    WorldMgr.WorldSettingsMgr.SetAmmoRefresh(ammoRefreshRate);
                    plr.SendClientMessage("Changed Ammo Refresh to " + ammoRefreshRate, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
                }
                else
                    plr.SendClientMessage("Ammo Refresh cannot be set to 0 or less.", ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);

            }
            else
            {
                plr.SendClientMessage("Current Ammo Refresh is equal to " + WorldMgr.WorldSettingsMgr.GetAmmoRefresh(), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Set decay value for keeps")]
        public static void KeepDecay(Player plr, string targetString = null)
        {
            int keepDecay;
            if (targetString != null && Int32.TryParse(targetString, out keepDecay))
            {
                if (keepDecay > 0)
                {
                    WorldMgr.WorldSettingsMgr.SetGenericSetting(8, keepDecay);
                    plr.SendClientMessage("Set Supplies Decay for Keeps to " + keepDecay, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
                }
                else
                    plr.SendClientMessage("Supplies Decay for Keeps cannot be set to 0 or less.", ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("Supplies Decay for Keeps is equal to " + WorldMgr.WorldSettingsMgr.GetGenericSetting(8), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Set minimum number of BOs to prevent decay")]
        public static void KeepDecayBOHold(Player plr, string targetString = null)
        {
            int boHold;
            if (targetString != null && Int32.TryParse(targetString, out boHold))
            {
                    WorldMgr.WorldSettingsMgr.SetGenericSetting(9, boHold);
                    plr.SendClientMessage("Set minimum BattlefieldObjective hold to " + boHold, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("Minimum BattlefieldObjective hold is equal to " + WorldMgr.WorldSettingsMgr.GetGenericSetting(9), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Enables (1) or disables (0) supplies from abandoned BOs")]
        public static void SuppliesFromAbandonedBO(Player plr, string targetString = null)
        {
            int suppliesSwitch;
            if (targetString != null && Int32.TryParse(targetString, out suppliesSwitch))
            {
                WorldMgr.WorldSettingsMgr.SetGenericSetting(10, suppliesSwitch);
                plr.SendClientMessage("Supplies from abandoned BattlefieldObjective set to " + suppliesSwitch, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("Supplies from abandoned BattlefieldObjective are set to " + WorldMgr.WorldSettingsMgr.GetGenericSetting(10), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Enables (1) or disables (0) new aggro system, with aggro from healing")]
        public static void HealAggro(Player plr, string targetString = null)
        {
            int aggroSwitch;
            if (targetString != null && Int32.TryParse(targetString, out aggroSwitch))
            {
                WorldMgr.WorldSettingsMgr.SetGenericSetting(11, aggroSwitch);
                plr.SendClientMessage("New aggro set to " + aggroSwitch, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("New aggro is set to " + WorldMgr.WorldSettingsMgr.GetGenericSetting(11), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Sets the weights for Gold Bags")]
        public static void SetGoldBagWeights (Player plr, string targetString = null)
        {
            int bagWeights;
            if (targetString != null && Int32.TryParse(targetString, out bagWeights))
            {
                WorldMgr.WorldSettingsMgr.SetGenericSetting(12, bagWeights);
                plr.SendClientMessage("Gold Bag: " + bagWeights, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("Gold Bag is set to: " + WorldMgr.WorldSettingsMgr.GetGenericSetting(12), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Sets the weights for Purple Bags")]
        public static void SetPurpleBagWeights(Player plr, string targetString = null)
        {
            int bagWeights;
            if (targetString != null && Int32.TryParse(targetString, out bagWeights))
            {
                WorldMgr.WorldSettingsMgr.SetGenericSetting(13, bagWeights);
                plr.SendClientMessage("Purple Bag: " + bagWeights, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("Purple Bag is set to: " + WorldMgr.WorldSettingsMgr.GetGenericSetting(13), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Sets the weights for Blue Bags")]
        public static void SetBlueBagWeights(Player plr, string targetString = null)
        {
            int bagWeights;
            if (targetString != null && Int32.TryParse(targetString, out bagWeights))
            {
                WorldMgr.WorldSettingsMgr.SetGenericSetting(14, bagWeights);
                plr.SendClientMessage("Blue Bag: " + bagWeights, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("Blue Bag is set to: " + WorldMgr.WorldSettingsMgr.GetGenericSetting(14), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Sets the weights for Green Bags")]
        public static void SetGreenBagWeights(Player plr, string targetString = null)
        {
            int bagWeights;
            if (targetString != null && Int32.TryParse(targetString, out bagWeights))
            {
                WorldMgr.WorldSettingsMgr.SetGenericSetting(15, bagWeights);
                plr.SendClientMessage("Green Bag: " + bagWeights, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("Green Bag is set to: " + WorldMgr.WorldSettingsMgr.GetGenericSetting(15), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Disables Personal Roll System if set to 1.")]
        public static void DisablePersonalRollSystem(Player plr, string targetString = null)
        {
            int Disabled;
            if (targetString != null && Int32.TryParse(targetString, out Disabled))
            {
                WorldMgr.WorldSettingsMgr.SetGenericSetting(16, Disabled);
                plr.SendClientMessage("Disabled Personal Role System Successfully: " + Disabled, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("Disable Personal Role System is set to: " + WorldMgr.WorldSettingsMgr.GetGenericSetting(16), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Disables Group Fix System if set to 1.")]
        public static void DisableGroupFixSystem(Player plr, string targetString = null)
        {
            int Disabled;
            if (targetString != null && Int32.TryParse(targetString, out Disabled))
            {
                WorldMgr.WorldSettingsMgr.SetGenericSetting(17, Disabled);
                plr.SendClientMessage("Set Disable GroupFix System flag Successfully: " + Disabled, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("Disable GroupFix System flag is set to: " + WorldMgr.WorldSettingsMgr.GetGenericSetting(17), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Disables Pet Anti-AOE Splash System if set to 1.")]
        public static void DisableAntiPetSplash(Player plr, string targetString = null)
        {
            int Disabled;
            if (targetString != null && Int32.TryParse(targetString, out Disabled))
            {
                WorldMgr.WorldSettingsMgr.SetGenericSetting(18, Disabled);
                plr.SendClientMessage("Set Pet Anti-AOE Splash System flag Successfully: " + Disabled, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("Disable Pet Anti-AOE Splash System flag is set to: " + WorldMgr.WorldSettingsMgr.GetGenericSetting(18), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Disables Pet Modifier System if set to 1.")]
        public static void DisablePetModifiers(Player plr, string targetString = null)
        {
            int Disabled;
            if(targetString != null && Int32.TryParse(targetString, out Disabled))
            {
                WorldMgr.WorldSettingsMgr.SetGenericSetting(19, Disabled);
                plr.SendClientMessage("Set Pet Modifier System flag Successfully: " + Disabled, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);

                lock (Player._Players)
                {
                    foreach (Player player in Player._Players)
                        player.SendClientMessage("[System] Recaching pet tables...", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                }

                CharMgr.ReloadPetModifiers();

                lock (Player._Players)
                {
                    foreach (Player player in Player._Players)
                    {
                        player.SendClientMessage("[System] Pet tables successfully recached.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                        if (player != null && player.CrrInterface != null)
                        {
                            if (player.CrrInterface.GetTargetOfInterest() != null && player.CrrInterface.GetTargetOfInterest() is Pet)
                            {
                                Pet myPet = (Pet)player.CrrInterface.GetTargetOfInterest();
                                if (myPet != null)
                                {
                                    myPet.Dismiss(null, null);
                                    player.SendClientMessage("[System] Pet tables were recached. Your pet has been dismissed.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                                }
                            }
                        }
                    }
                }

            }
            else
            {
                plr.SendClientMessage("Disable Pet Mastery System flag is set to: " + WorldMgr.WorldSettingsMgr.GetGenericSetting(19), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }

        [CommandAttribute(EGmLevel.SourceDev, "Disables PeekPet System if set to 1.")]
        public static void DisablePeekPet(Player plr, string targetString = null)
        {
            int Disabled;
            if (targetString != null && Int32.TryParse(targetString, out Disabled))
            {
                WorldMgr.WorldSettingsMgr.SetGenericSetting(20, Disabled);
                plr.SendClientMessage("Set PeekPet System flag Successfully: " + Disabled, ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
            else
            {
                plr.SendClientMessage("Disable PeekPet System System flag is set to: " + WorldMgr.WorldSettingsMgr.GetGenericSetting(20), ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE);
            }
        }


    }
}