using System.Linq;
using SystemData;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using Item = WorldServer.World.Objects.Item;

namespace WorldServer.NetWork.Handler
{
    public class InventoryHandlers : IPacketHandler
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_BAG_INFO, (int)eClientState.Playing, "onBagInfo")]
        public static void F_BAG_INFO(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (!cclient.IsPlaying())
                return;
            

            byte Type = packet.GetUint8();

            Player Plr = cclient.Plr;

            switch (Type)
            {
                case 3: // Toggle Pvp
                    ((CombatInterface_Player)Plr.CbtInterface).TogglePvPFlag();
                    break;

                case 16: // Buy more bag space
                    {
                        byte Price = packet.GetUint8();
                        if (!Plr.ItmInterface.HasMaxBag)
                        {
                            if (Plr.HasMoney(Plr.ItmInterface.GetBagPrice()))
                            {
                                if (Plr.RemoveMoney(Plr.ItmInterface.GetBagPrice()))
                                {
                                    ++Plr.ItmInterface.BagBuy;
                                    Plr.ItmInterface.SendMaxInventory(Plr);
                                }
                            }
                        }
                    }
                    break;

                case 17: // Buy more bank space
                    {
                        uint Price = packet.GetUint32R();
                        if (!Plr.ItmInterface.HasMaxBank)
                        {
                            if (Plr.HasMoney(Plr.ItmInterface.GetBankPrice()))
                            {
                                if (Plr.RemoveMoney(Plr.ItmInterface.GetBankPrice()))
                                {
                                    ++Plr.ItmInterface.BankBuy;
                                    Plr.ItmInterface.SendMaxInventory(Plr);
                                }
                            }
                        }
                    }
                    break;
                case 18: // Alternate Appereance
                    {
                        byte Slot = packet.GetUint8();
                        packet.Skip(2);
                        ushort SlotItem = packet.GetUint16();

                        Plr.ItmInterface.HandleAltAppearance(Slot, SlotItem);
                    }
                    break;

                case 19: // Alternate Currency
                {
                    packet.Skip(7);
                    //var a = packet.GetUint8();
                    //var b = packet.GetUint8();
                    //var c = packet.GetUint8();
                    //var d = packet.GetUint8();
                    //var e = packet.GetUint8();
                    //var f = packet.GetUint8();
                    //var g = packet.GetUint8();
                    var h = packet.GetUint16();
                    //var i = packet.GetUint8();
                    //var j = packet.GetUint8();
                    //var k = packet.GetUint8();
                    //var l = packet.GetUint8();
                    //var m = packet.GetUint8();
                    //var n = packet.GetUint8();
                    
                    // ushort SlotItem = packet.GetUint8();

                    var item = Plr.ItmInterface.GetItemInSlot(h);

                   // Plr.ItmInterface.DeleteItem(h, 1);



                }
                    break;

                case 27: // Barber
                    {
                        packet.Skip(5);


                        byte[] Traits = new byte[8];
                        packet.Read(Traits, 0, Traits.Length);
                        Plr.Info.bTraits = Traits;
                        CharMgr.Database.SaveObject(Plr.Info);


                    }
                    break;
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_TRANSFER_ITEM, (int)eClientState.Playing, "onTransferItem")]
        public static void F_TRANSFER_ITEM(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null || !cclient.Plr.IsInWorld())
                return;

            ushort Oid = packet.GetUint16();
            ushort To = packet.GetUint16();
            ushort From = packet.GetUint16();
            ushort Count = packet.GetUint16();

            // Log.Info("","move item " + cclient.Plr.ItmInterface.GetItemInSlot(From).Info.Entry + " slot " + From + " count "+Count+"    to slot  " + To +" item "+cclient.Plr.ItmInterface.GetItemInSlot(To).Info.Entry);
            cclient.Plr.ItmInterface.MoveSlot(From, To, Count);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_TRADE_STATUS, (int)eClientState.Playing, "onTradeStatus")]
        public static void F_TRADE_STATUS(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;
            if (!cclient.IsPlaying())
                return;

            cclient.Plr.ItmInterface.HandleTrade(packet);
        }
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_TROPHY_SETLOCATION, (int)eClientState.Playing, "onTrophySelection")]
        public static void F_TROPHY_SETLOCATION(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;
            if (!cclient.IsPlaying())
                return;

            packet.GetUint8();
            byte Trophyslot = packet.GetUint8();
            ushort value = packet.GetUint16();

            cclient.Plr.ItmInterface.AssignTrophy(Trophyslot, value);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_USE_ITEM, (int)eClientState.Playing, "onUseItem")]
        public static void F_USE_ITEM(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;
            if (!cclient.IsPlaying())
                return;

            Player Plr = cclient.Plr;

            if (Plr.AbtInterface.IsOnGlobalCooldown())
                return;

            ushort slot = packet.GetUint16();

            Item item = Plr.ItmInterface.GetItemInSlot(slot);

            if (item == null || item._Count == 0)
                return;

            if (!item.CanBeUsedBy(Plr))
                return;

            if (item.Info.Entry == 1337)
            {
                Plr.SetLevel((byte)40);
                Plr.ItmInterface.DeleteItem(slot, 1);
            }

            // Honor rewards
            var honorReward = HonorService.HonorRewards.SingleOrDefault(x => x.ItemId == item.Info.Entry);
            if (honorReward !=null)
            {
                if (Plr.Info.HonorRank < honorReward.HonorRank)
                {
                    Plr.SendClientMessage("You can no longer use this item, as you do not have a high enough Honor Rank.");
                    return;
                }
            }

            if ((item.Info.Entry == 208477) || (item.Info.Entry == 208474))
            {
                Plr.ItmInterface.CreateItem(208470, 6);
                Plr.ItmInterface.DeleteItem(slot, 1);
            }

            // Oil
            if (item.Info.Entry == 86203 || item.Info.Entry == 86207 || item.Info.Entry == 86211 || item.Info.Entry == 86215 || item.Info.Entry == 86219 || item.Info.Entry == 86223) // siege oil
            {
                BattleFrontKeep keep = Plr.Region.Campaign.GetClosestFriendlyKeep(Plr.WorldPosition, Plr.Realm);

                if (keep.Realm == Plr.Realm)
                    keep.SpawnOil(Plr, slot);
            }

            if (item.ModelId == 1566 || item.ModelId == 3850)  // currency conversion boxes
                Plr.ItmInterface.OpenBox(slot, item);

            #region Loot bags

            if (item.Info.Entry == 9980 || item.Info.Entry == 9940 || item.Info.Entry == 9941 || item.Info.Entry == 9942 || item.Info.Entry == 9943)  // lootbags
            {
                packet.Skip(5);
                byte mode = packet.GetUint8();

                if (mode == 0)
                    Plr.ItmInterface.SendMysteryBag(slot);
                else
                    Plr.ItmInterface.GetItemfromMysterybag(slot, mode);
            }

            #endregion

            // Banner hack for standards.
            if (item.ModelId >= 6188 && item.ModelId < 6194)
                Plr.Standard(item.Info.SpellId, true);

            if (item.Info.Crafts.Length > 0 && CraftingApoInterface.GetCraft(5, item.Info.Crafts) == 4 && (CraftingApoInterface.GetCraft(8, item.Info.Crafts) < 5 || CraftingApoInterface.GetCraft(8, item.Info.Crafts) == 18))
                CultivationInterface.ReapResin(Plr, slot);

            #region Dye
            if (item.Info.Type == 27)
            {
                Item dye = Plr.ItmInterface.GetItemInSlot(slot);
                if (dye == null)
                    return;

                byte prisek = packet.GetUint8();
                packet.Skip(4);
                byte Slot = packet.GetUint8();

                Item itemtodye = Plr.ItmInterface.GetItemInSlot(Slot);


                if (dye.Info.BaseColor1 == 0)
                {
                    Plr.ItmInterface.RemoveDye(itemtodye, prisek == 1, prisek == 2);

                }
                else
                {
                    if (prisek == 1)
                        Plr.ItmInterface.DyeItem(itemtodye, dye.Info.BaseColor1, 0);
                    else
                        Plr.ItmInterface.DyeItem(itemtodye, 0, dye.Info.BaseColor1);
                }
                Plr.ItmInterface.DeleteItem(slot, 1);
            }

            #endregion

            if (item.Info.SpellId == 0)
                return;

            var numberSiegeTypeBeforeCast = 0;
            var numberSiegeTypeAfterCast = 0;
            var siegeType = Siege.GetSiegeType(item.Info.Entry);

            if (item.Info.IsSiege)
            {
                if (siegeType == null)
                {
                    _logger.Warn($"Could not locate SiegeType for {item.Info.Entry}");
                    return;
                }

                if (item.Info.IsSiege)
                {
                    numberSiegeTypeBeforeCast = Plr.Region.Campaign.SiegeManager.GetNumberByType(siegeType.Value, Plr.Realm);
                }
            }

            #region Ability Cast

            if (!Plr.AbtInterface.CanCastCooldown(item.Info.SpellId))
            {
                Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ITEM_CANT_BE_USED_YET);
                return;
            }

            if (!Plr.AbtInterface.StartCast(Plr, item.Info.SpellId, 1, item.Info.Unk27?[19] ?? 0, item.Info.ObjectLevel))
                return;

            if (item.Info.MaxStack > 1)
                Plr.ItmInterface.DeleteItem(slot, 1);

            // Determine whether to remove the siege item from inventory.
            if (item.Info.IsSiege)
            {
                // If the siege exists, and the cast was not blocked or interrupted.
                numberSiegeTypeAfterCast = Plr.Region.Campaign.SiegeManager.GetNumberByType(siegeType.Value, Plr.Realm);

                if ((item.Owner as Player).CharacterId == Plr.CharacterId)
                {
                    if (item.Info.IsValid)
                    {
                        // If there is a Ram now, but there wasnt one before, remove it from inventory
                        if (numberSiegeTypeBeforeCast + 1 == numberSiegeTypeAfterCast)
                        {
                            Plr.ItmInterface.DeleteItem(slot, 1);
                        }
                    }
                }
            }

            WorldMgr.GeneralScripts.OnWorldPlayerEvent("USE_ITEM", Plr, item);

            #endregion
        }
    }
}