using Common;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Abilities.Objects;
using WorldServer.World.Objects;
using Item = WorldServer.World.Objects.Item;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public enum ItemResult
    {
        RESULT_OK = 0,
        RESULT_MAX_BAG = 1, // Sac Plein
        RESULT_ITEMID_INVALID = 2,// Item incorrect
    };

    public class ItemsInterface : BaseInterface
    {
        #region Define

        public static ushort MY_TRADE_SLOT = 232;
        public static ushort OTHER_TRADE_SLOT = 241;
        public static ushort TALISMAN_SLOT_WINDOW = 255;
        public static ushort BUY_BACK_SLOT = 20;
        public static ushort BASE_BAG_PRICE = 100;
        public static ushort MAX_TRADE_SLOT = 9;
        public static ushort INVENTORY_SLOT_COUNT = 16;
        public static ushort MAX_EQUIPMENT_SLOT = 40;
        public static ushort INVENTORY_START_SLOT = 40;
        public static ushort CRAFTING_START_SLOT = 400;
        public static ushort CURRENCY_START_SLOT = 500;
        public static ushort AUTO_EQUIP_SLOT = 600;
        public static ushort AUTO_BANK_SLOT = 603;
        public static ushort AUTO_GUILD_BANK_SLOT = 606;
        public static ushort BANK_START_SLOT = 800;
        public static ushort BANK_END_SLOT = 1039;
        public static ushort OVERFLOW_START_SLOT = 1100;
        public static ushort OVERFLOW_END_SLOT = 1150;
        public static ushort BANK_SLOT_COUNT = 8;
        public static ushort BASE_BANK_PRICE = 60000;
        public static ushort BASE_BANK_INTERVAL = 20000;
        public static ushort DELETE_SLOT = 1040;
        public static ushort QUEST_START_SLOT = 700;

        #endregion

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public float BolsterFactor;

        public Item[] Items;
        private long _movedItems = 0;
        private byte _bagBuy;
        public byte BagBuy
        {
            get
            {
                if (_playerOwner != null)
                    return _playerOwner._Value.BagBuy;
                return _bagBuy;
            }
            set
            {
                if (_playerOwner != null)
                    _playerOwner._Value.BagBuy = value;
                else
                    _bagBuy = value;
            }

        }

        private byte _bankBuy;
        public byte BankBuy
        {
            get
            {
                if (_playerOwner != null)
                    return _playerOwner._Value.BankBuy;
                return _bankBuy;
            }
            set
            {
                if (_playerOwner != null)
                    _playerOwner._Value.BankBuy = value;
                else
                    _bankBuy = value;
            }

        }

        private Player _playerOwner;

        public void Load(List<CharacterItem> playerItems)
        {
            if (IsLoad)
                return;

            _playerOwner = (Player)_Owner;

            Items = new Item[OVERFLOW_END_SLOT];

            foreach (CharacterItem item in playerItems)
                if (item.SlotId < Items.Length)
                {
                    Item itm = new Item(_Owner);
                    if (!itm.Load(item))
                        continue;

                    if (itm.SlotId == 0)
                    {
                        BuyBack.Add(itm);
                        continue;
                    }

                    // Because item restrictions can change in patches
                    if (IsEquipmentSlot(itm.SlotId) && (!CanUse(itm.Info, _playerOwner, false, false) /*|| (itm.Info.UniqueEquiped == 1 && _uniqueEquipped.Contains(item.Entry))*/ || (EquipSlot)itm.SlotId == EquipSlot.OFF_HAND && itm.Info.TwoHanded) && _playerOwner.GmLevel == 1)
                        SlotCollidingItems.Add(itm);

                    else if (Items[itm.SlotId] == null)
                    {
                        Items[itm.SlotId] = itm;

                        if (IsEquipmentSlot(itm.SlotId))
                            EquipItem(itm);
                    }

                    else
                        SlotCollidingItems.Add(itm);
                }

            int buyLen = BuyBack.Count;

            // Rescue items with no sell price from the buy back list...
            for (int i = 0; i < buyLen; ++i)
            {
                if (BuyBack[i].Info.SellPrice > 0)
                    continue;

                for (ushort slot = MAX_EQUIPMENT_SLOT; slot < GetMaxInventorySlot(); ++slot)
                {
                    if (Items[slot] != null)
                        continue;

                    Items[slot] = BuyBack[i];
                    Items[slot].SlotId = slot;
                    BuyBack.RemoveAt(i);
                    --buyLen;
                    --i;
                    goto Continue;
                }

                for (ushort slot = BANK_START_SLOT; slot < GetMaxBankSlot(); ++slot)
                {
                    if (Items[slot] != null)
                        continue;

                    Items[slot] = BuyBack[i];
                    Items[slot].SlotId = slot;
                    BuyBack.RemoveAt(i);
                    --buyLen;
                    --i;
                    goto Continue;
                }

                break;

            Continue:
                ;
            }

            HandleSlotCollisions();
            base.Load();
        }
        public void Load(List<Creature_item> creatureItems)
        {
            if (IsLoad)
                return;

            if (_playerOwner != null)
                Items = new Item[OVERFLOW_END_SLOT];
            else
                Items = new Item[MAX_EQUIPMENT_SLOT];


            foreach (Creature_item Item in creatureItems)
            {
                if (Item.SlotId < Items.Length)
                {
                    Item Itm = new Item(Item);

                    if (Itm.SlotId == 0)
                    {
                        BuyBack.Add(Itm);
                        continue;
                    }

                    Items[Itm.SlotId] = Itm;
                }
            }
        }

        public void AddCreatureItem(Creature_item item)
        {
            if (item.SlotId < Items.Length)
            {
                Item Itm = new Item(item);
                AddCreatureItem(Itm);
            }
        }

        public void AddCreatureItem(Item item)
        {
            if (item.SlotId < Items.Length)
            {
                if (item.SlotId == 0)
                {
                    BuyBack.Add(item);
                    return;
                }

                Items[item.SlotId] = item;
            }

            if (IsEquipmentSlot(item.SlotId))
                SendEquipped(null);
        }

        public Item RemoveCreatureItem(ushort slot)
        {
            Item item = Items[slot];

            Items[slot] = null;

            if (item != null)
            {
                if (IsEquipmentSlot(slot))
                    SendEquipped(null, slot);
            }

            return item;
        }

        public override void Update(long tick)
        {

        }

        public override void Save()
        {
            if (_playerOwner == null || Items == null || !IsLoad)
                return;

            List<Item> itms = new List<Item>(Items);

            CharMgr.SaveItems(_playerOwner.Info.CharacterId, itms);
        }

        #region Slot Collisions

        /// <summary>
        /// Contains items which, upon loading, occupied the same slot as an existing item.
        /// </summary>
        public List<Item> SlotCollidingItems = new List<Item>();

        /// <summary>
        /// Moves, if possible, any displaced items into a free slot in the inventory.
        /// </summary>
        public void HandleSlotCollisions()
        {
            foreach (Item itm in SlotCollidingItems)
            {
                Log.Success("ItemInterface", "Found duplicated item " + itm.Info.Name);
                itm.SlotId = GetFreeInventorySlot(itm.Info);
                Items[itm.SlotId] = itm;
            }
        }

        #endregion

        private readonly HashSet<uint> _uniqueEquipped = new HashSet<uint>();

        #region Stats

        public bool CanUseItemType(Item_Info item, ushort slotId = 0, byte career = 0)
        {
            // Career restrictions.
            if (item.Career != 0 && (item.Career & (1 << _playerOwner.Info.CareerLine - 1)) == 0)
                return false;

            // Race restrictions. 
            if (item.Race != 0 && (item.Race & (1 << _playerOwner.Info.Race - 1)) == 0)
                return false;

            if (item.UniqueEquiped == 1 && _uniqueEquipped.Contains(item.Entry))
                return false;

            //check profession 

            // todo

            if (slotId >= 15 && slotId <= 19 || slotId == 27)
                return true;

            if (item.Type == 0 || item.Type == 21 || item.Type > 23)
                return true;

            if (career == 0 && _playerOwner != null)
                career = _playerOwner.Info.CareerLine;

            uint skills = item.Skills << 1;
            long playerSkill = 0;
            if (_playerOwner != null)
                playerSkill = _playerOwner._Value.Skills;

            if (skills != 0 && (playerSkill & skills) != skills)
                return false;

            // FIXME replace with check on Skills
            switch (career)
            {
                case 1:
                    if (slotId == 11 && item.Type != 5)
                        return false;
                    if (item.Type == 2 || item.Type == 5 || item.Type == 3 || item.Type == 20)
                        return true;
                    break;
                case 2:
                    if (slotId == 11 && !(item.SlotId == 13 || item.SlotId == 11))
                        return false;
                    if (item.Type == 2 || item.Type == 3 || item.Type == 19)
                        return true;
                    break;
                case 3:
                    if (slotId == 11)
                        return false;
                    if (item.Type == 11 || item.Type == 6)
                        return true;
                    break;
                case 4:
                    if (slotId == 11)
                        return false;
                    if (item.Type == 3 || item.Type == 9 || item.Type == 18)
                        return true;
                    break;
                case 5:
                    if (slotId == 11 && item.Type != 5)
                        return false;
                    if (item.Type == 1 || item.Type == 5 || item.Type == 2 || item.Type == 20)
                        return true;
                    break;
                case 6:
                    if (slotId == 11 && !(item.SlotId == 13 || item.SlotId == 11))
                        return false;
                    if (item.Type == 1 || item.Type == 2 || item.Type == 3 || item.Type == 19)
                        return true;
                    break;
                case 7:
                    if (slotId == 11)
                        return false;
                    if (item.Type == 11 || item.Type == 6)
                        return true;
                    break;
                case 8:
                    if (slotId == 11)
                        return false;
                    if (item.Type == 14 || item.Type == 7 || item.Type == 18)
                        return true;
                    break;
                case 9:
                    if (slotId == 11 && item.Type != 15)
                        return false;
                    if (item.Type == 1 || item.Type == 15 || item.Type == 18)
                        return true;
                    break;
                case 10:
                    if (slotId == 11 && item.Type != 5)
                        return false;
                    if (item.Type == 1 || item.Type == 5 || item.Type == 20)
                        return true;
                    break;
                case 11:
                    if (slotId == 11)
                        return false;
                    if (item.Type == 11 || item.Type == 6)
                        return true;
                    break;
                case 12:
                    if (slotId == 11 && item.Type != 25)
                        return false;
                    if (item.Type == 3 || item.Type == 22 || item.Type == 25)
                        return true;
                    break;
                case 13:
                    if (slotId == 11 && item.Type != 5)
                        return false;
                    if (item.Type == 1 || item.Type == 2 || item.Type == 5 || item.Type == 20)
                        return true;
                    break;
                case 14:
                    if (slotId == 11 && !(item.SlotId == 13 || item.SlotId == 11))
                        return false;
                    if (item.Type == 3 || item.Type == 2 || item.Type == 19)
                        return true;
                    break;
                case 15:
                    if (slotId == 11 && item.Type != 25)
                        return false;
                    if (item.Type == 6 || item.Type == 12 || item.Type == 25)
                        return true;
                    break;
                case 16:
                    if (slotId == 11)
                        return false;
                    if (item.Type == 11 || item.Type == 6)
                        return true;
                    break;

                case 17:
                    if (slotId == 11 && item.Type != 5)
                        return false;
                    if (item.Type == 1 || item.Type == 5 || item.Type == 20)
                        return true;
                    break;
                case 18:
                    if (slotId == 11)
                        return false;
                    if (item.Type == 1 || item.Type == 7 || item.Type == 18)
                        return true;
                    break;
                case 19:
                    if (slotId == 11)
                        return false;
                    if (item.Type == 2 || item.Type == 19)
                        return true;
                    break;
                case 20:
                    if (slotId == 11)
                        return false;
                    if (item.Type == 11 || item.Type == 6)
                        return true;
                    break;
                case 21:
                    if (slotId == 11 && item.Type != 5)
                        return false;
                    if (item.Type == 1 || item.Type == 14 || item.Type == 5 || item.Type == 20)
                        return true;
                    break;
                case 22:
                    if (slotId == 11 && !(item.SlotId == 13 || item.SlotId == 11))
                        return false;
                    if (item.Type == 12 || item.Type == 18)
                        return true;
                    break;
                case 23:
                    if (slotId == 11 && !(item.SlotId == 13 || item.SlotId == 11 || item.SlotId == 25))
                        return false;
                    if (item.Type == 1 || item.Type == 25 || item.Type == 22)
                        return true;
                    break;
                case 24:
                    if (slotId == 11)
                        return false;
                    if (item.Type == 11 || item.Type == 6)
                        return true;
                    break;
            }
            return false;
        }

        public void OpenBox(ushort slot, Item item)
        {
            switch (item.Info.Entry)
            {
                case 208424:
                    CreateItem(208400, 5, true);
                    break;
                case 208425:
                    CreateItem(208400, 25, true);
                    break;
                case 208426:
                    CreateItem(208401, 5, true);
                    break;
                case 208427:
                    CreateItem(208401, 25, true);
                    break;
                case 208428:
                    CreateItem(208402, 5, true);
                    break;
                case 208447:
                    CreateItem(208431, 5, true);
                    break;
                case 208448:
                    CreateItem(208431, 25, true);
                    break;
                case 208449:
                    CreateItem(208432, 5, true);
                    break;
                case 208450:
                    CreateItem(208432, 25, true);
                    break;
                case 208451:
                    CreateItem(208433, 5, true);
                    break;
                case 1298378389:
                    CreateItem(208403, 5, true);
                    break;
                case 1298378390:
                    CreateItem(208434, 5, true);
                    break;
                case 1298378391:
                    CreateItem(208403, 25, true);
                    break;
                case 1298378392:
                    CreateItem(208434, 25, true);
                    break;
                case 1298378521:   // Black market warlord box

                    var random = StaticRandom.Instance.Next(0, 10000);
                    var count = 0;

                    if (random >= 9950)
                    {
                        count = StaticRandom.Instance.Next(200, 235);
                    }
                    else
                    {
                        if (random >= 9900)
                            count = StaticRandom.Instance.Next(130, 160);
                        else
                        {
                            if (random >= 9700)
                                count = StaticRandom.Instance.Next(95, 105);
                            else
                            {
                                if (random >= 9400)
                                    count = StaticRandom.Instance.Next(75, 85);
                                else
                                {
                                    if (random >= 9100)
                                        count = StaticRandom.Instance.Next(65, 75);
                                    else
                                    {
                                        if (random >= 8500)
                                            count = StaticRandom.Instance.Next(55, 65);
                                        else
                                        { if (random >= 7500)
                                                count = StaticRandom.Instance.Next(45, 55);
                                            else
                                            {
                                                if (random >= 6500)
                                                    count = StaticRandom.Instance.Next(35, 45);
                                                else
                                                {
                                                    if (random >= 0)
                                                        count = StaticRandom.Instance.Next(15, 25);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    CreateItem(208454, (ushort)count, true);
                    (_Owner as Player).SendClientMessage($"The package contains {count} Warlord Crests!", ChatLogFilters.CHATLOGFILTERS_LOOT);
                    break;

                case 1298378528:
                    CreateItem(208470, 5, true);
                    break;
                case 1298378529:
                    CreateItem(208454, 5, true);
                    break;
                case 1298378530:
                    CreateItem(208470, 25, true);
                    break;
                case 1298378531:
                    CreateItem(208453, 5, true);
                    break;
                case 1298378532:
                    CreateItem(208453, 25, true);
                    break;
                case 1298378533:
                    CreateItem(208470, 5, true);
                    break;
                case 1298378534:
                    CreateItem(208470, 25, true);
                    break;

                default:
                    return;
            }
            _playerOwner.ItmInterface.DeleteItem(slot, 1);
        }

        public List<Chapter_Reward> GetChapterRewards(byte Tier, Chapter_Info Info)
        {
            List<Chapter_Reward> items = new List<Chapter_Reward>();

            if (Tier == 1)
            {
                if (Info.T1Rewards == null)
                    return items;

                foreach (Chapter_Reward CR in Info.T1Rewards)
                {
                    if (CR.Item.Race == 0 || ((CR.Item.Race & (1 << _playerOwner.Info.Race - 1)) > 0))
                        if (CR.Item.Career == 0 || ((CR.Item.Career & (1 << _playerOwner.Info.CareerLine - 1)) > 0))
                        {
                            if (CanUseItemType(CR.Item))
                            {
                                if (CR.InfluenceCount == (Info.Tier1InfluenceCount))
                                    items.Add(CR);
                            }
                        }
                }
            }
            if (Tier == 2)
            {
                if (Info.T2Rewards == null)
                    return items;

                foreach (Chapter_Reward CR in Info.T2Rewards)
                {
                    if (CR.Item.Race == 0 || ((CR.Item.Race & (1 << _playerOwner.Info.Race - 1)) > 0))
                        if (CR.Item.Career == 0 || ((CR.Item.Career & (1 << _playerOwner.Info.CareerLine - 1)) > 0))
                        {
                            if (CanUseItemType(CR.Item))
                            {
                                if (CR.InfluenceCount == (Info.Tier2InfluenceCount))
                                    items.Add(CR);
                            }
                        }
                }
            }
            if (Tier == 3)
            {
                if (Info.T3Rewards == null)
                    return items;

                foreach (Chapter_Reward CR in Info.T3Rewards)
                {
                    if (CR.Item.Race == 0 || ((CR.Item.Race & (1 << _playerOwner.Info.Race - 1)) > 0))
                        if (CR.Item.Career == 0 || ((CR.Item.Career & (1 << _playerOwner.Info.CareerLine - 1)) > 0))
                        {
                            if (CanUseItemType(CR.Item))
                            {
                                if (CR.InfluenceCount == (Info.Tier3InfluenceCount))
                                    items.Add(CR);
                            }
                        }
                }
            }
            return items;
        }

        public ushort GetAttackTime(EquipSlot slot)
        {
            if (((Unit)_Owner).IsPolymorphed)
                return 100;

            Item Itm = Items[(ushort)slot];
            if (Itm?.Info == null || Itm.Info.Speed == 0)
                return 200;
            return Itm.Info.Speed;
        }

        public ushort GetWeaponDamage(EquipSlot slot)
        {
            Creature owner = _Owner as Creature;
            if (owner != null)
                return (ushort)(owner.GetStrikeDamage() * 0.1f);

            Item Itm = Items[(ushort)slot];
            if (Itm?.Info == null)
                return 0;
            return (ushort)((Itm.Info.Dps + (Itm.Info.TwoHanded ? 15 : 10) * ((Unit)_Owner).StsInterface.BolsterFactor) * 0.1f);
        }

        public byte GetAttackSpeed()
        {
            Item OffHand = GetItemInSlot((ushort)EquipSlot.OFF_HAND);
            Item MainHand = GetItemInSlot((ushort)EquipSlot.MAIN_HAND);
            Item Distance = GetItemInSlot((ushort)EquipSlot.RANGED_WEAPON);

            byte Speed = 0;
            if (OffHand != null) Speed += (byte)(OffHand.Info.Speed * 0.1);
            if (MainHand != null) Speed += (byte)(MainHand.Info.Speed * 0.1);
            if (Distance != null) Speed += (byte)(Distance.Info.Speed * 0.1);

            return Speed;
        }

        public float GetWeaponDamage(WeaponDamageContribution weaponMod)
        {
            Creature crea = _Owner as Creature;
            if (crea != null)
                return (ushort)(crea.GetStrikeDamage() * 0.1f);

            Item offHand = GetItemInSlot((ushort)EquipSlot.OFF_HAND); // offhand
            Item mainHand = GetItemInSlot((ushort)EquipSlot.MAIN_HAND); // main
            Item distance = GetItemInSlot((ushort)EquipSlot.RANGED_WEAPON); // ranged

            float damage = 0;
            switch (weaponMod)
            {
                case WeaponDamageContribution.MainHand:
                    if (mainHand != null)
                    {
                        damage += mainHand.Info.Dps;
                        if (BolsterFactor > 0f) damage += (mainHand.Info.TwoHanded ? 15 : 10) * BolsterFactor;
                    }
                    break;

                case WeaponDamageContribution.OffHand:
                    if (offHand != null)
                    {
                        damage += offHand.Info.Dps;
                        if (BolsterFactor > 0f) damage += 10 * BolsterFactor;
                    }
                    break;

                case WeaponDamageContribution.Ranged:
                    if (distance != null)
                    {
                        damage += distance.Info.Dps;
                        if (BolsterFactor > 0f) damage += 10 * BolsterFactor;
                    }
                    break;

                case WeaponDamageContribution.DualWield:
                    if (mainHand != null)
                    {
                        damage += mainHand.Info.Dps;
                        if (BolsterFactor > 0f) damage += 10 * BolsterFactor;
                    }
                    if (offHand != null)
                    {
                        damage += offHand.Info.Dps * 0.45f;
                        if (BolsterFactor > 0f) damage += 4.5f * BolsterFactor;
                    }
                    break;

                case WeaponDamageContribution.MainAndRanged:
                    if (mainHand != null)
                    {
                        damage += mainHand.Info.Dps * 0.45f;
                        if (BolsterFactor > 0f) damage += 4.5f * BolsterFactor;
                    }
                    if (distance != null)
                    {
                        damage += distance.Info.Dps + 10 * BolsterFactor;
                        if (BolsterFactor > 0f) damage += 10 * BolsterFactor;
                    }
                    break;
            }
            return damage / 10f;
        }

        public void EquipItem(Item Itm)
        {
            if (Itm == null || _playerOwner == null)
                return;

            if (Itm.Info.Bind == 2 && !Itm.BoundtoPlayer)
                Itm.BoundtoPlayer = true;

            if (Itm.Info.UniqueEquiped == 1)
                _uniqueEquipped.Add(Itm.Info.Entry);

            Player Plr = _playerOwner;

            foreach (KeyValuePair<byte, ushort> Stats in Itm.Info._Stats)
                Plr.StsInterface.AddItemBonusStat((Stats)Stats.Key, Stats.Value);

            for (byte i = 0; i < Itm.Info.TalismanSlots; i++)
            {
                Talisman tali = Itm.GetTalisman(i);
                if (tali != null)
                {
                    Item_Info taliInfo = ItemService.GetItem_Info(tali.Entry);
                    foreach (KeyValuePair<byte, ushort> Stats in taliInfo._Stats)
                        Plr.StsInterface.AddItemBonusStat((Stats)Stats.Key, Stats.Value);

                    ApplyItemEffects(taliInfo);
                }
            }

            if (Itm.Info.Armor > 0 && (Itm.Info.SlotId < 10 || Itm.Info.SlotId > 12))
                Plr.StsInterface.AddItemBonusStat(Stats.Armor, Itm.Info.Armor);

            ApplyItemEffects(Itm.Info);

            CheckItemSets();

            Plr.StsInterface.ApplyStats();

            if (Itm.SlotId == (byte)EquipSlot.MAIN_HAND)
                Plr.BuffInterface.NotifyItemEvent((byte)BuffCombatEvents.MainWeaponChanged, Itm.Info);
            else if (Itm.Info.Type == 5)
                Plr.BuffInterface.NotifyItemEvent((byte)BuffCombatEvents.ShieldChanged, Itm.Info);

            if (Plr.AdjustedLevel < Plr.Level)
                Plr.CheckDebolsterValid();
        }

        public void UnEquipItem(Item Itm, bool blockSetCheck = false)
        {
            if (Itm == null || _playerOwner == null)
                return;

            if (Itm.Info.UniqueEquiped == 1)
                _uniqueEquipped.Remove(Itm.Info.Entry);

            Player Plr = _playerOwner;

            foreach (KeyValuePair<byte, ushort> Stats in Itm.Info._Stats)
                Plr.StsInterface.RemoveItemBonusStat((Stats)Stats.Key, Stats.Value);

            for (byte i = 0; i < Itm.Info.TalismanSlots; i++)
            {
                Talisman tali = Itm.GetTalisman(i);
                if (tali != null)
                {
                    Item_Info taliInfo = ItemService.GetItem_Info(tali.Entry);
                    foreach (KeyValuePair<byte, ushort> Stats in taliInfo._Stats)
                        Plr.StsInterface.RemoveItemBonusStat((Stats)Stats.Key, Stats.Value);

                    RemoveItemEffects(Itm.Info);
                }
            }

            if (Itm.Info.Armor > 0 && (Itm.Info.SlotId < 10 || Itm.Info.SlotId > 12))
                Plr.StsInterface.RemoveItemBonusStat(Stats.Armor, Itm.Info.Armor);

            if (!blockSetCheck)
                CheckItemSets();

            RemoveItemEffects(Itm.Info);

            Plr.StsInterface.ApplyStats();

            if (Itm.SlotId == (byte)EquipSlot.MAIN_HAND)
                Plr.BuffInterface.NotifyItemEvent((byte)BuffCombatEvents.MainWeaponChanged, Itm.Info);
            else if (Itm.Info.Type == 5)
                Plr.BuffInterface.NotifyItemEvent((byte)BuffCombatEvents.ShieldChanged, null);
        }

        /// <summary>
        /// Iterates over the given item to apply its effects on player.
        /// </summary>
        /// <param name="item">Item to apply effects of</param>
        private void ApplyItemEffects(Item_Info item)
        {
            foreach (ushort effectEntry in item.EffectsList)
            {
                // Skip Intimidating Repent, visual only
                if (effectEntry == 8281)
                    continue;
                _playerOwner.BuffInterface.QueueBuff(new BuffQueueInfo(_playerOwner, _playerOwner.EffectiveLevel, AbilityMgr.GetBuffInfo(effectEntry)));
            }
        }

        /// <summary>
        /// Iterates over the given item to remove its effects on player.
        /// </summary>
        /// <param name="item">Item to remove effects of</param>
        private void RemoveItemEffects(Item_Info item)
        {
            foreach (ushort effectEntry in item.EffectsList)
            {
                // Skip Intimidating Repent, visual only
                if (effectEntry == 8281)
                    continue;
                _playerOwner.BuffInterface.RemoveBuffByEntry(effectEntry);
            }
        }
        #endregion

        #region Accessor

        public Item GetItemInSlot(ushort slotID)
        {
            if (Items == null || slotID >= Items.Length)
                return null;

            return Items[slotID];
        }

        public ushort GetItemCount(uint entry)
        {
            ushort count = 0;
            foreach (Item Itm in Items)
            {
                if (Itm?.Info != null && Itm.Info.Entry == entry)
                    count += Itm.Count;
            }
            return count;
        }

        public bool HasItemCountInInventory(uint entry, ushort soughtCount)
        {
            ushort count = 0;

            // Purge all matching items from the standard inventory slots.
            for (ushort slot = MAX_EQUIPMENT_SLOT; slot < GetMaxInventorySlot(); ++slot)
            {
                if (Items[slot] == null)
                    continue;
                if (Items[slot].Info.Entry != entry)
                    continue;

                count += Items[slot].Count;

                if (count >= soughtCount)
                    return true;
            }

            // Continue on to the craft bags.
            for (ushort slot = CURRENCY_START_SLOT; slot < CURRENCY_START_SLOT + GetMaxCurrencyItemSlots() && soughtCount > 0; ++slot)
            {
                if (Items[slot] == null || Items[slot].Info.Entry != entry)
                    continue;

                count += Items[slot].Count;

                if (count >= soughtCount)
                    return true;
            }

            return false;
        }

        public bool HasMaxBag => BagBuy >= 3;
        public bool HasMaxBank => BankBuy >= 20;

        private byte GetTotalSlot() => (byte)(GetMaxInventorySlot() - INVENTORY_START_SLOT);
        private byte GetMaxInventorySlot() => (byte)(INVENTORY_START_SLOT + 32 + BagBuy * INVENTORY_SLOT_COUNT);
        private ushort GetTotalBankSlot() => (ushort)(GetMaxBankSlot() - BANK_START_SLOT);
        private ushort GetMaxBankSlot() => (ushort)(BANK_START_SLOT + 80 + BankBuy * BANK_SLOT_COUNT);

        public uint GetBagPrice()
        {
            double bag = BagBuy;
            double price = BASE_BAG_PRICE;

            price *= Math.Pow(10, bag);
            return (uint)price;
        }
        public uint GetBankPrice()
        {
            double bank = BankBuy;
            double price = BASE_BANK_PRICE;

            price += bank * BASE_BANK_INTERVAL;
            return (uint)price;
        }

        /// <summary>
        /// Returns the number of free inventory slots.
        /// </summary>
        /// <param name="info">If null then this will search only the main inventory; otherwise, this will also search the preferred bags (if any) for the item type 'info.Type'.</param>
        /// <param name="overflow">Whether or not to include the overflow slots.</param>
        public ushort GetTotalFreeInventorySlot(Item_Info info = null, bool overflow = false)
        {
            ushort count = 0;
            SlotRange range;

            if (info != null && GetItemPreferredRange(out range, info))
                for (ushort slot = range.Start; slot < range.End; ++slot)
                    if (Items[slot] == null)
                        ++count;

            for (ushort slot = MAX_EQUIPMENT_SLOT; slot < GetMaxInventorySlot(); ++slot)
                if (Items[slot] == null)
                    ++count;

            if (overflow)
                for (ushort slot = OVERFLOW_START_SLOT; slot < OVERFLOW_END_SLOT; ++slot)
                    if (Items[slot] == null)
                        ++count;

            return count;
        }

        ///<summary>
        /// Searches for a free inventory slot.
        /// </summary>
        public ushort GetFreeMainBagInventorySlot()
        {
            ushort slot;

            // main bag
            for (slot = MAX_EQUIPMENT_SLOT; slot < GetMaxInventorySlot(); ++slot)
                if (Items[slot] == null)
                    return slot;

            return 0;
        }

        public ushort GetFreeInventorySlot(Item_Info info, bool overflow = false)
        {
            ushort slot;

            if (info == null)
                return 0;

            // Check for a preferred range for this item, and if one
            // exists, try to find a free slot in that range
            SlotRange range;
            if (GetItemPreferredRange(out range, info))
                if ((slot = GetFreeItemSlotInRange(range.Start, range.End)) != 0)
                    return slot;

            // no slot found go to main bag
            for (slot = MAX_EQUIPMENT_SLOT; slot < GetMaxInventorySlot(); ++slot)
                if (Items[slot] == null)
                    return slot;

            // no slot found go to overflow
            if (overflow)
                for (slot = OVERFLOW_START_SLOT; slot < OVERFLOW_END_SLOT; ++slot)
                    if (Items[slot] == null)
                        return slot;

            return 0;
        }

        /*
         * AddStacksInRange
         *    Appends to 'stacks' all item stacks matching 'info' and in the range given
         *    by startIndex, SlotCount.
         */
        private void AddStacksInRange(List<ushort> stacks, Item_Info info, ushort startIndex, ushort stopIndex)
        {
            for (ushort slot = startIndex; slot < stopIndex; slot++)
                if (Items[slot] != null && Items[slot].Info.Entry == info.Entry)
                    if (Items[slot].Count < Items[slot].Info.MaxStack)
                        stacks.Add(slot);
        }

        private ushort GetMaxCurrencyItemSlots() => 32;
        public ushort GetMaxCraftingItemSlots()
        {
            Player player = (Player)_Owner;

            ushort space = 16;
            space += (ushort)(16 * player._Value.CraftingBags);
            return space;
        }
        private ushort GetMaxQuestItemSlots() => 100;

        private bool IsMainInventorySlot(ushort slot)
        {
            return slot >= MAX_EQUIPMENT_SLOT && slot < MAX_EQUIPMENT_SLOT + GetMaxInventorySlot();
        }
        private bool IsCraftingInventorySlot(ushort slot)
        {
            return slot >= CRAFTING_START_SLOT && slot < CRAFTING_START_SLOT + GetMaxCraftingItemSlots();
        }
        private bool IsCurrencyInventorySlot(ushort slot)
        {
            return slot >= CURRENCY_START_SLOT && slot < CURRENCY_START_SLOT + GetMaxCurrencyItemSlots();
        }
        private bool IsQuestItemInventorySlot(ushort slot)
        {
            return slot >= QUEST_START_SLOT && slot < QUEST_START_SLOT + GetMaxQuestItemSlots();
        }
        private bool IsBankInventorySlot(ushort slot)
        {
            return slot >= BANK_START_SLOT && slot <= BANK_END_SLOT;
        }

        private ushort GetFreeItemSlotInRange(ushort startIndex, ushort count)
        {
            for (ushort Slot = startIndex; Slot < count; Slot++)
                if (Items[Slot] == null)
                    return Slot;
            return 0;
        }

        private struct SlotRange
        {
            public ushort Start, End;
        };

        ///<Summary>
        /// Some types of items go into specific inventory areas by default;
        /// this function tells which area (if any) is best for the given item.
        ///</Summary>
        private bool GetItemPreferredRange(out SlotRange range, Item_Info info)
        {
            if (info.Type == (byte)Item.ItemType.Currency)
            {
                range.Start = CURRENCY_START_SLOT;
                range.End = (ushort)(CURRENCY_START_SLOT + GetMaxCurrencyItemSlots());
                return true;
            }
            if (info.Type == (byte)Item.ItemType.Crafting)
            {
                range.Start = CRAFTING_START_SLOT;
                range.End = (ushort)(range.Start + GetMaxCraftingItemSlots());
                return true;
            }
            if (info.Type == (byte)Item.ItemType.Quest)
            {
                range.Start = QUEST_START_SLOT;
                range.End = (ushort)(QUEST_START_SLOT + GetMaxQuestItemSlots());
                return true;
            }
            range.Start = 0;
            range.End = 0;
            return false;
        }

        public bool EquippedGearAbove(byte level)
        {
            for (int i = 0; i < MAX_EQUIPMENT_SLOT; ++i)
            {
                if (Items[i] != null && Items[i].Info.MinRank > level)
                    return true;
            }

            return false;
        }

        public bool HasTalismansInGear()
        {
            for (int i = 0; i < MAX_EQUIPMENT_SLOT; ++i)
            {
                if (Items[i] != null && Items[i].CharSaveInfo?._Talismans?.Count > 0)
                    return true;
            }

            return false;
        }

        #endregion

        #region Packets

        public void BuildStats(ref PacketOut Out)
        {
            Out.WriteByte((byte)Stats.BaseStatCount);
            // This is for tactic slots. (Career slots + Renown slots << 2 + Tome slots << 4)
            Out.WriteByte((GetPlayer().Level > 10) ? (byte)(GetPlayer().Level / 10) : (byte)0);
            //Out.WriteByte(127);  shows 7 slots
            Out.WriteByte(1);
            Out.WriteByte(0xF4);
            Out.WriteUInt16((ushort)(GetPlayer().StsInterface.GetTotalStat(Stats.Armor)));
        }
        public void SendMaxInventory(Player plr) // 1.3.5
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_BAG_INFO, 18);
            Out.WriteByte(0x0F);
            Out.WriteUInt16R(GetTotalSlot());               // GameData.Player.numBackpackSlots
            Out.WriteUInt16R(INVENTORY_SLOT_COUNT);         // GameData.Player.backpackExpansionSlots
            Out.WriteUInt32R(GetBagPrice());                // GameData.Player.backpackExpansionSlotsCost

            Out.WriteUInt16R(GetTotalBankSlot());           // GameData.Player.numBankSlots
            Out.WriteUInt16R(BANK_SLOT_COUNT);              // GameData.Player.bankExpansionSlots
            Out.WriteUInt32R(GetBankPrice());               // GameData.Player.bankExpansionSlotsCost
            plr.SendPacket(Out);
        }
        private static void SendBuffer(Player plr, ref PacketOut buffer, ref byte Count)
        {
            // On Envoi le Packet des items
            byte[] ArrayBuf = buffer.ToArray();
            PacketOut Packet = new PacketOut((byte)Opcodes.F_GET_ITEM);
            Packet.WriteByte(Count);
            Packet.Fill(0, 3);
            Packet.Write(ArrayBuf, 0, ArrayBuf.Length);

            plr.SendPacket(Packet);

            // On Remet le compteur a zero
            Count = 0;

            // On Initalise un nouveau buffer
            buffer = new PacketOut(0);
            buffer.Position = 0;
        }

        public void SendItemInSlot(Player plr, ushort slot)
        {
            Item item = GetItemInSlot(slot);

            PacketOut Out = new PacketOut((byte)Opcodes.F_GET_ITEM);
            Out.WriteByte(1);
            Out.Fill(0, 3);
            Item.BuildItem(ref Out, item, null, null, slot, 0, plr);
            plr.SendPacket(Out);

            // If the item is in a set send its info
            if (item?.Info != null)
                SendItemSetInfo(plr, item.Info.ItemSet);
        }
        private void SendItemsInSlots(Player plr, List<ushort> slots)
        {
            if (slots.Count <= 0)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_GET_ITEM);
            Out.WriteByte((byte)slots.Count);
            Out.Fill(0, 3);
            for (int i = 0; i < slots.Count; ++i)
                Item.BuildItem(ref Out, GetItemInSlot(slots[i]), null, null, slots[i], 0, _Owner.GetPlayer());
            plr.SendPacket(Out);

            // If the item is in a set send its info
            for (int i = 0; i < slots.Count; ++i)
                SendItemSetInfo(plr, GetItemInSlot(slots[i]));
        }
        private void SendItemSwap(Player plr, ushort destination, ushort source)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_GET_ITEM);
            Out.WriteByte(2);
            Out.Fill(0, 3);
            Item.BuildItem(ref Out, GetItemInSlot(destination), null, null, destination, 0, plr);
            Item.BuildItem(ref Out, GetItemInSlot(source), null, null, source, 0, plr);
            plr.SendPacket(Out);

            // If the item is in a set send its info
            SendItemSetInfo(plr, GetItemInSlot(destination));
            SendItemSetInfo(plr, GetItemInSlot(source));
        }
        public void SendAllItems(Player plr)
        {
            SendMaxInventory(plr); // 1.3.5

            // On Envoi les items 16 par 16
            byte count = 0;


            //item sets to send
            List<uint> itemSets = new List<uint>();

            PacketOut buffer = new PacketOut(0, Items.Length * 150) { Position = 0 };

            for (int i = 0; i < Items.Length; ++i)
            {
                if (count >= 8)
                    SendBuffer(plr, ref buffer, ref count);

                if (Items[i] != null)
                {
                    Item.BuildItem(ref buffer, Items[i], null, null, 0, 0, plr);
                    ++count;

                    // If the item is in a set send its info
                    if (Items[i].Info.ItemSet != 0)
                        if (!itemSets.Contains(Items[i].Info.ItemSet))
                            itemSets.Add(Items[i].Info.ItemSet);
                }
            }

            if (count > 0)
                SendBuffer(plr, ref buffer, ref count);

            // If the item is in a set send its info
            foreach (uint setId in itemSets)
                SendItemSetInfo(plr, setId);

        }
        private static void SendItemToTradeWindow(Player plr, ushort slot, Item info, ushort count)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_GET_ITEM);
            Out.WriteByte(1);
            Out.Fill(0, 3);
            Item.BuildItem(ref Out, info, null, null, slot, count, plr);
            plr.SendPacket(Out);

            // If the item is in a set send its info
            SendItemSetInfo(plr, info.Info.ItemSet);
        }

        private static void SendItemSetInfo(Player plr, Item item)
        {
            if (item?.Info == null)
                return;

            SendItemSetInfo(plr, item.Info.ItemSet);
        }
        private static void SendItemSetInfo(Player plr, uint id)
        {
            if (id == 0)
                return;

            Item_Set set = ItemService.GetItem_Set(id);

            if (set == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_ITEM_SET_DATA);
            Out.WriteUInt32(set.Entry); // Item set id
            Out.WritePascalString(set.Name); // Item set name
            Out.WriteByte(set.Unk); // modifies the spell :S
            Out.WriteByte((byte)set.Items.Count); // Number of items in set
            foreach (KeyValuePair<uint, string> Item in set.Items)
            {
                Out.WriteUInt32(Item.Key); // Item Id
                Out.WritePascalString(Item.Value); // Item Name
            }

            Out.WriteByte((byte)set.Bonus.Count); // Bonus count - Spells/Stats
            foreach (KeyValuePair<byte, string> Bonus in set.Bonus)
            {
                Out.WriteByte(Bonus.Key); // bonus id
                if (Bonus.Key < 80) // If the bonus is below 80 it is a stat
                {
                    string[] bonusInfo = Bonus.Value.Split(',');
                    Out.WriteByte(byte.Parse(bonusInfo[0])); // stat id
                    Out.WriteUInt16(ushort.Parse(bonusInfo[1])); // value
                    Out.WriteByte(byte.Parse(bonusInfo[2])); // percentage?
                }
                else
                    Out.WriteUInt16(ushort.Parse(Bonus.Value)); // spell id
            }
            plr.SendPacket(Out);
        }

        public void SendItemSetInfoToPlayer(Player plr, uint id)
        {
            if (id == 0)
                return;

            Item_Set set = ItemService.GetItem_Set(id);

            if (set == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_ITEM_SET_DATA);
            Out.WriteUInt32(set.Entry); // Item set id
            Out.WritePascalString(set.Name); // Item set name
            Out.WriteByte(set.Unk); // modifies the spell :S
            Out.WriteByte((byte)set.Items.Count); // Number of items in set
            foreach (KeyValuePair<uint, string> Item in set.Items)
            {
                Out.WriteUInt32(Item.Key); // Item Id
                Out.WritePascalString(Item.Value); // Item Name
            }

            Out.WriteByte((byte)set.Bonus.Count); // Bonus count - Spells/Stats
            foreach (KeyValuePair<byte, string> Bonus in set.Bonus)
            {
                Out.WriteByte(Bonus.Key); // bonus id
                if (Bonus.Key < 80) // If the bonus is below 80 it is a stat
                {
                    string[] bonusInfo = Bonus.Value.Split(',');
                    Out.WriteByte(byte.Parse(bonusInfo[0])); // stat id
                    Out.WriteUInt16(ushort.Parse(bonusInfo[1])); // value
                    Out.WriteByte(byte.Parse(bonusInfo[2])); // percentage?
                }
                else
                    Out.WriteUInt16(ushort.Parse(Bonus.Value)); // spell id
            }
            plr.SendPacket(Out);
        }

        public void SendBackpack(Player plr, ushort slot, uint backpackModelId)
        {
            slot = 13; // Back

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_INVENTORY, 32);
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte((byte)(_playerOwner?.WeaponStance ?? 0));
            Out.WriteByte(1);

            Item curItem = Items[13];

            if (curItem == null)
            {
                Out.WriteByte(0);
                Out.WriteByte((byte)slot);
                Out.WriteUInt16(0);
                //continue;
            }

            //use old color scheme handling for monsters. New scheme uses Pri/Seconday colors in creature_items table
            if (_playerOwner == null && (curItem._PrimaryColor == 0 && curItem._SecondaryColor == 0))
            {
                Out.WriteByte((byte)(curItem._EffectId > 0 ? 1 : 0));
                Out.WriteByte((byte)slot);
                if (backpackModelId == 0)
                    Out.WriteUInt16((ushort)plr.CloakModel); // backpackModelId
                else
                    Out.WriteUInt16((ushort)backpackModelId); // backpackModelId
                if (curItem._EffectId > 0)
                    Out.WriteUInt16((ushort)curItem._EffectId);

                //continue;
            }

            byte flags = curItem.GetInventoryFlags();
            Out.WriteByte(flags);
            Out.WriteByte((byte)slot);

            if (backpackModelId == 0)
                Out.WriteUInt16((ushort)plr.CloakModel); // backpackModelId
            else
                Out.WriteUInt16((ushort)backpackModelId); // backpackModelId

            if (Utils.HasFlag(flags, (byte)ItemFlags.ColorOverride))
            {
                if (Utils.HasFlag(flags, (byte)ItemFlags.PriColorExpansion))
                    Out.WriteUInt16(curItem.GetPrimaryColor());
                else
                    Out.WriteByte((byte)curItem.GetPrimaryColor());

                if (Utils.HasFlag(flags, (byte)ItemFlags.SecColorExpansion))
                    Out.WriteUInt16(curItem.GetSecondaryColor());
                else
                    Out.WriteByte((byte)curItem.GetSecondaryColor());
            }

            if (Utils.HasFlag(flags, (byte)ItemFlags.Heraldry))
            {
                if (_playerOwner.GldInterface.Guild != null && backpackModelId == 0)
                {
                    Out.WriteByte(2); // HasHeraldry
                    _playerOwner.GldInterface.Guild.BuildHeraldry(Out);
                    Out.WriteByte(1); // HeraldryUnk1
                    Out.WriteByte(2); // HeraldryUnk2
                }
                else
                    Out.WriteByte(0);
            }

            Out.WriteByte(0);

            if (plr != null)
                plr.SendPacket(Out);
            else
                _Owner.DispatchPacket(Out, false);
        }

        public void SendEquipped(Player plr)
        {
            if (_Owner is GameObject || _Owner is BuffHostObject || _Owner is GroundTarget || _Owner is Standard || Items == null)
                return;

            List<ushort> itms = new List<ushort>();
            for (ushort i = 0; i < MAX_EQUIPMENT_SLOT; ++i)
                if (Items[i] != null)
                    itms.Add(i);


            SendEquipped(plr, itms);
        }
        private void SendEquipped(Player plr, ushort slot)
        {
            if (!IsEquipmentSlot(slot))
                return;

            List<ushort> items = new List<ushort> { slot };

            SendEquipped(plr, items);
        }
        private void SendEquipped(Player plr, ushort destination, ushort source)
        {
            int Invalide = !IsEquipmentSlot(destination) ? 1 : 0;
            Invalide += !IsEquipmentSlot(source) ? 1 : 0;

            if (Invalide == 2)
                return;

            List<ushort> Itms = new List<ushort>();
            if (IsEquipmentSlot(destination))
                Itms.Add(destination);
            if (IsEquipmentSlot(source))
                Itms.Add(source);

            SendEquipped(plr, Itms);
        }
        private void SendEquipped(Player plr, List<ushort> items)
        {
            if (_Owner is Pet)
            {
                PacketOut petStance = new PacketOut((byte)Opcodes.F_PLAYER_INVENTORY, 5);
                petStance.WriteUInt16(_Owner.Oid);
                petStance.WriteHexStringBytes("01 01 00 0C 15 48 00".Replace(" ", ""));

                if (plr != null)
                    plr.SendPacket(petStance);
                else
                    _Owner.DispatchPacket(petStance, false);
                return;
            }

            items.RemoveAll(slot => !IsEquipmentSlot(slot));

            if (items.Count == 0)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_INVENTORY, 32 * items.Count);
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte((byte)(_playerOwner?.WeaponStance ?? 0));
            Out.WriteByte((byte)items.Count);

            foreach (ushort slot in items)
            {
                Item curItem = Items[slot];

                if (curItem == null)
                {
                    Out.WriteByte(0);
                    Out.WriteByte((byte)slot);
                    Out.WriteUInt16(0);
                    continue;
                }

                //use old color scheme handling for monsters. New scheme uses Pri/Seconday colors in creature_items table
                if (_playerOwner == null && (curItem._PrimaryColor == 0 && curItem._SecondaryColor == 0))
                {
                    Out.WriteByte((byte)(curItem._EffectId > 0 ? 1 : 0));
                    Out.WriteByte((byte)slot);
                    Out.WriteUInt16((ushort)curItem.ModelId);
                    if (curItem._EffectId > 0)
                        Out.WriteUInt16((ushort)curItem._EffectId);

                    continue;
                }

                byte flags = curItem.GetInventoryFlags();
                Out.WriteByte(flags);
                Out.WriteByte((byte)slot);

                if (flags == (int)ItemFlags.Trophy)
                {
                    Out.WriteUInt16((ushort)curItem.ModelId);
                    Out.WriteUInt16((ushort)curItem.AltAppearanceEntry);
                }

                else
                {
                    Out.WriteUInt16(curItem.GetModel());

                    if (Utils.HasFlag(flags, (byte)ItemFlags.ColorOverride))
                    {
                        if (Utils.HasFlag(flags, (byte)ItemFlags.PriColorExpansion))
                            Out.WriteUInt16(curItem.GetPrimaryColor());
                        else
                            Out.WriteByte((byte)curItem.GetPrimaryColor());

                        if (Utils.HasFlag(flags, (byte)ItemFlags.SecColorExpansion))
                            Out.WriteUInt16(curItem.GetSecondaryColor());
                        else
                            Out.WriteByte((byte)curItem.GetSecondaryColor());
                    }

                    if (Utils.HasFlag(flags, (byte)ItemFlags.Heraldry))
                    {
                        if (_playerOwner.GldInterface.Guild != null)
                        {
                            Out.WriteByte(2); // HasHeraldry
                            _playerOwner.GldInterface.Guild.BuildHeraldry(Out);
                            Out.WriteByte(1); // HeraldryUnk1
                            Out.WriteByte(2); // HeraldryUnk2
                        }
                        else
                            Out.WriteByte(0);
                    }
                }
            }
            Out.WriteByte(0);

            if (plr != null)
                plr.SendPacket(Out);
            else
                _Owner.DispatchPacket(Out, false);
        }

        public void SendInspect(Player plr)
        {
            byte Count = 0;
            for (ushort i = 0; i < MAX_EQUIPMENT_SLOT; ++i)
                if (Items[i] != null)
                    ++Count;

            PacketOut Out = new PacketOut((byte)Opcodes.F_SOCIAL_NETWORK);
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(7); // Inspect Code
            Out.WriteByte(Count);

            for (ushort i = 0; i < MAX_EQUIPMENT_SLOT; ++i)
                if (Items[i] != null)
                {
                    Item.BuildItem(ref Out, Items[i], null, null, 0, 0);
                    if (Items[i] != null && Items[i].Info != null && Items[i].Info.ItemSet != 0 && plr.ItmInterface != null)
                        plr.ItmInterface.SendItemSetInfoToPlayer(plr, Items[i].Info.ItemSet);

                }
            Out.WriteByte(0);
            plr.SendPacket(Out);
        }

        #endregion

        #region Creation

        ///<summary>
        /// Creates a list of all stacks containing the given item.
        /// stacks are identified by inventory slot index.
        ///</summary>
        private List<ushort> GetStackItem(Item_Info info)
        {
            List<ushort> infos = new List<ushort>();

            // Add stacks from preferred bags first so that those stacks fill
            // up before stacks in the main inventory
            SlotRange range;
            if (GetItemPreferredRange(out range, info))
                AddStacksInRange(infos, info, range.Start, range.End);

            // Now add stacks from main inventory
            AddStacksInRange(infos, info, MAX_EQUIPMENT_SLOT, GetMaxInventorySlot());

            return infos;
        }

        public ItemResult CreateItem(Item_Info info, ushort count, ushort bagSlot = 0, bool overflow = false)
        {
            return CreateItem(info, count, new List<Talisman>(), 0, 0, false, bagSlot, overflow);
        }

        public ItemResult CreateItem(Item_Info info, ushort count, List<Talisman> talismans, ushort PrimaryDye, ushort SecondaryDye, bool boundToPlayer, ushort bagSlot = 0, bool overflow = false)
        {
            if (info == null)
                return ItemResult.RESULT_ITEMID_INVALID;

            if (bagSlot > 0 && GetItemInSlot(bagSlot) != null)
                return ItemResult.RESULT_ITEMID_INVALID;

            lock (_toSend)
            {
                _stacked.Clear();
                _toSend.Clear();
                ushort toCreate = 0;

                if (count != 1 || info.MaxStack > 1)
                {
                    if (info.MaxStack > 1) // Seek existing stacks within the inventory, for addition.
                    {
                        _stacked = GetStackItem(info);

                        foreach (ushort stackableSlot in _stacked)
                        {
                            Item Itm = Items[stackableSlot];

                            if (Itm == null || Itm.Count >= Itm.Info.MaxStack)
                                continue;

                            if (count > Itm.Info.MaxStack - Itm.Count)
                            {
                                count -= (ushort)(Itm.Info.MaxStack - Itm.Count);
                                Itm.Count = Itm.Info.MaxStack;
                                _toSend.Add(stackableSlot);
                            }
                            else
                            {
                                Itm.Count += count;
                                count = 0;
                                _toSend.Add(stackableSlot);
                            }

                        }

                        toCreate = NumStacksRequiredToHold(count, info.MaxStack); // Nombre d'objet qui doit être créé count/MaxStack
                    }
                }
                else
                {
                    toCreate = 1;
                }

                ushort totalFreeSlot = GetTotalFreeInventorySlot(info, overflow); // Nombre de slots total dont je dispose

                //Log.info("ItemsInterface", "count=" + count + ",FreeSlot=" + TotalFreeSlot + ",ToCreate=" + ToCreate+",CanStack="+CanStack);


                if (totalFreeSlot < toCreate) // Je n'ai pas assez de slots disponible pour créer ces objets
                    return ItemResult.RESULT_MAX_BAG;

                for (int i = 0; i < toCreate && count > 0; ++i)
                {
                    ushort freeSlot;

                    if (bagSlot > 0)
                        freeSlot = bagSlot;
                    else
                        freeSlot = GetFreeInventorySlot(info, overflow);

                    if (freeSlot == 0)
                        return ItemResult.RESULT_MAX_BAG;

                    ushort ToAdd = Math.Min(count, info.MaxStack);
                    count -= ToAdd;

                    Item itm = new Item(_Owner);
                    if (!itm.Load(info, freeSlot, ToAdd))
                        return ItemResult.RESULT_ITEMID_INVALID;

                    itm.CharSaveInfo._Talismans = talismans;
                    itm.CharSaveInfo.PrimaryDye = PrimaryDye;
                    itm.CharSaveInfo.SecondaryDye = SecondaryDye;
                    itm.CharSaveInfo.BoundtoPlayer = boundToPlayer;

                    //Log.info("ItemsInterface", "New Item ToAdd : " + ToAdd + ",count=" + count);
                    Items[freeSlot] = itm;
                    _toSend.Add(freeSlot);
                }

                _playerOwner?.QtsInterface.HandleEvent(Objective_Type.QUEST_GET_ITEM, info.Entry, count);

                SendItemsInSlots(_playerOwner, _toSend);
            }

            return ItemResult.RESULT_OK;
        }
        public ItemResult CreateItem(uint itemId, ushort count, bool overflow = false, ushort bagSlot = 0)
        {
            Item_Info info = ItemService.GetItem_Info(itemId);

            return CreateItem(info, count, bagSlot, overflow);
        }

        //public ItemResult CreateExistingItem(Item_Info info, ushort count, CharacterItem itm, ushort bagSlot = 0, bool overflow = false)
        //{
        //    return CreateExistingItem(info, count, itm, new List<Talisman>(), 0, 0, bagSlot, overflow);
        //}

        //public ItemResult CreateExistingItem(Item_Info info, ushort count, CharacterItem itm, List<Talisman> talismans, ushort PrimaryDye, ushort SecondaryDye, ushort bagSlot = 0, bool overflow = false)
        //{
        //    if (info == null)
        //        return ItemResult.RESULT_ITEMID_INVALID;

        //    if (bagSlot > 0 && GetItemInSlot(bagSlot) != null)
        //        return ItemResult.RESULT_ITEMID_INVALID;

        //    lock (_toSend)
        //    {
        //        _stacked.Clear();
        //        _toSend.Clear();
        //        ushort toCreate = 0;

        //        if (count != 1 || info.MaxStack > 1)
        //        {
        //            if (info.MaxStack > 1) // Seek existing stacks within the inventory, for addition.
        //            {
        //                _stacked = GetStackItem(info);

        //                foreach (ushort stackableSlot in _stacked)
        //                {
        //                    Item Itm = Items[stackableSlot];

        //                    if (Itm == null || Itm.Count >= Itm.Info.MaxStack)
        //                        continue;

        //                    if (count > Itm.Info.MaxStack - Itm.Count)
        //                    {
        //                        count -= (ushort)(Itm.Info.MaxStack - Itm.Count);
        //                        Itm.Count = Itm.Info.MaxStack;
        //                        _toSend.Add(stackableSlot);
        //                    }
        //                    else
        //                    {
        //                        Itm.Count += count;
        //                        count = 0;
        //                        _toSend.Add(stackableSlot);
        //                    }

        //                }

        //                toCreate = NumStacksRequiredToHold(count, info.MaxStack); // Number of objects to be created count/MaxStack
        //            }
        //        }
        //        else
        //        {
        //            toCreate = 1;
        //        }
        //        ushort totalFreeSlot = GetTotalFreeInventorySlot(info, overflow); // Number of total slots available

        //        if (totalFreeSlot < toCreate) // not enough slots available to create the items
        //            return ItemResult.RESULT_MAX_BAG;

        //        for (int i = 0; i < toCreate && count > 0; ++i)
        //        {
        //            ushort freeSlot;

        //            if (bagSlot > 0)
        //                freeSlot = bagSlot;
        //            else
        //                freeSlot = GetFreeInventorySlot(info, overflow);

        //            if (freeSlot == 0)
        //                return ItemResult.RESULT_MAX_BAG;

        //            ushort ToAdd = Math.Min(count, info.MaxStack);
        //            count -= ToAdd;

        //            Item New = new Item(_Owner);
        //            //New.LoadExisting(item.Info, 0, count, itm);
        //            if (!New.Load(info, freeSlot, ToAdd))
        //                return ItemResult.RESULT_ITEMID_INVALID;

        //            New.CharSaveInfo.BoundtoPlayer = itm.BoundtoPlayer;
        //            New.CharSaveInfo._Talismans = talismans;
        //            New.CharSaveInfo.PrimaryDye = PrimaryDye;
        //            New.CharSaveInfo.SecondaryDye = SecondaryDye;

        //            //Log.info("ItemsInterface", "New Item ToAdd : " + ToAdd + ",count=" + count);
        //            Items[freeSlot] = New;
        //            _toSend.Add(freeSlot);
        //        }
        //        _playerOwner?.QtsInterface.HandleEvent(Objective_Type.QUEST_GET_ITEM, info.Entry, count);

        //        SendItemsInSlots(_playerOwner, _toSend);
        //    }
        //    return ItemResult.RESULT_OK;
        //}

        #endregion

        #region Moving

        private static bool IsEquipmentSlot(ushort slot) => slot < MAX_EQUIPMENT_SLOT;

        public static bool CanUse(Item_Info info, Player plr, bool ignoreTempRestrictions, bool sendMessage)
        {
            uint skills = info.Skills << 1;
            long playerSkill = plr._Value.Skills;

            if (skills != 0 && (playerSkill & skills) != skills)
                return false;

            if (info.Career != 0)
            {
                int playerCareer = 1 << (plr.Info.CareerLine - 1);

                if ((info.Career & playerCareer) == 0)
                {
                    if (sendMessage)
                        plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEST_ITEM_PLAYER_INVALID_CAREER);
                    return false;
                }
            }

            if (info.Race != 0)
            {
                int playerRace = 1 << (plr.Info.Race - 1);

                if ((info.Race & playerRace) == 0)
                {
                    if (sendMessage)
                        plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEST_ITEM_PLAYER_INVALID_RACE);
                    return false;
                }
            }

            /*if (info.Type == 25 && (plr.Info.CareerLine == (int)CareerLine.CAREERLINE_WARRIOR_PRIEST || plr.Info.CareerLine == (int)CareerLine.CAREERLINE_DISCIPLE) && plr.CrrInterface.ExperimentalMode)
            {
                if (sendMessage)
                    plr.SendClientMessage("You may not equip a tome or chalice when in Experimental Mode.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return false;
            }*/

            if (ignoreTempRestrictions)
                return true;

            if (plr.AdjustedLevel < info.MinRank)
            {
                if (sendMessage)
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEST_ITEM_PLAYER_LEVEL_TOO_LOW);
                return false;
            }

            if (plr.RenownRank < info.MinRenown)
            {
                if (sendMessage)
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEST_ITEM_PLAYER_RENOWN_TOO_LOW);
                return false;
            }

            return true;
        }

        public bool CanMoveToSlot(Item itm, ushort slot)
        {
            if (itm == null || _playerOwner == null)
                return true;

            if (slot == TALISMAN_SLOT_WINDOW && GetItemInSlot(TALISMAN_SLOT_WINDOW) == null)
                return true;

            // Destination slot is an equippable slot.
            if (IsEquipmentSlot(slot))
            {
                if (!(slot == 15 || slot == 16 || slot == 17 || slot == 18 || slot == 19))
                    if (!CanUse(itm.Info, _playerOwner, false, true))
                        return false;

                EquipSlot equipSlot = (EquipSlot)slot;
                EquipSlot itemSlot = (EquipSlot)itm.Info.SlotId;

                if (equipSlot == EquipSlot.TROPHY_2 && _playerOwner.Level < 10) // Level 10 pour le trophee_2
                    return false;
                if (equipSlot == EquipSlot.TROPHY_3 && _playerOwner.Level < 20) // Level 20 pour le trophee_3
                    return false;
                if (equipSlot == EquipSlot.TROPHY_4 && _playerOwner.Level < 30) // Level 30 pour le trophee_4
                    return false;
                if (equipSlot == EquipSlot.TROPHY_5 && _playerOwner.Level < 40) // Level 40 pour le trophee_5
                    return false;

                // Not allowed to equip a 2 handed when you have an off hand
                if (itm.Info.TwoHanded && equipSlot == EquipSlot.MAIN_HAND && Items[(ushort)EquipSlot.OFF_HAND] != null)
                {
                    _playerOwner.SendLocalizeString(itm.Info.Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ITEM_ERR_CANT_EQUIP_X);
                    return false;
                }

                // Not allowed to equip a off hand when you have a 2 handed
                if (equipSlot == EquipSlot.OFF_HAND && Items[(ushort)EquipSlot.MAIN_HAND] != null && Items[(ushort)EquipSlot.MAIN_HAND].Info.TwoHanded)
                {
                    _playerOwner.SendLocalizeString(itm.Info.Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ITEM_ERR_CANT_EQUIP_X);
                    return false;
                }

                if (slot == itm.Info.SlotId)
                    return true;
                if (itemSlot == EquipSlot.EITHER_HAND && (equipSlot == EquipSlot.MAIN_HAND || equipSlot == EquipSlot.OFF_HAND))
                    return true;
                if ((itemSlot == EquipSlot.POCKET_1 || itemSlot == EquipSlot.POCKET_2) && (equipSlot == EquipSlot.POCKET_1 || equipSlot == EquipSlot.POCKET_2))
                    return true;
                if (itm.Info.Type == 24 && equipSlot >= EquipSlot.TROPHY_1 && equipSlot <= EquipSlot.TROPHY_5)
                    return true;
                if (itemSlot >= EquipSlot.JEWELLERY_1 && itemSlot <= EquipSlot.JEWELLERY_4 && equipSlot >= EquipSlot.JEWELLERY_1 && equipSlot <= EquipSlot.JEWELLERY_4)
                    return true;

                _playerOwner.SendLocalizeString(itm.Info.Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ITEM_CANT_GO_THERE);
                return false;
            }

            // Destination slot is in inventory.
            Item.ItemType itemType = (Item.ItemType)itm.Info.Type;

            if (itemType == Item.ItemType.Quest)
            {
                // Quest items must never be moved outside of the quest item inventory range
                return IsQuestItemInventorySlot(slot);
            }

            if (IsBankInventorySlot(slot))
            {
                // Anything that is not a quest item can be moved into the bank
                return true;
            }
            if (IsCurrencyInventorySlot(slot))
            {
                // Only currency items can go in currency inventory
                return itemType == Item.ItemType.Currency;
            }
            if (IsCraftingInventorySlot(slot))
            {
                // Only crafting items can go in the crafting inventory
                return itemType == Item.ItemType.Crafting;
            }
            if (IsQuestItemInventorySlot(slot))
            {
                // Only quest items can go in the quest inventory.  This check is redundant because quest items
                // should have already been special-cased above, but go ahead and check again anyway.
                return itemType == Item.ItemType.Quest;
            }
            if (slot >= GetMaxInventorySlot())
            {
                _playerOwner.SendLocalizeString(itm.Info.Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ITEM_ERR_CANT_MOVE_X);
                return false;
            }

            return true;
        }

        public ushort GenerateAutoSlot(ushort sourceSlot)
        {
            ushort slotId = 0;

            Item sourceItem = GetItemInSlot(sourceSlot);

            if (sourceItem == null)
                return slotId;

            if (IsEquipmentSlot(sourceSlot)) // The item was already equipped, so a free inventory slot is found.
                slotId = GetFreeInventorySlot(sourceItem.Info);
            else
            {
                EquipSlot itemEquipSlot = (EquipSlot)sourceItem.Info.SlotId;

                if (itemEquipSlot >= EquipSlot.JEWELLERY_1 && itemEquipSlot <= EquipSlot.JEWELLERY_4) // Place jewellery in any free slot.
                {
                    if (Items[(int)EquipSlot.JEWELLERY_1] == null)
                        slotId = (ushort)EquipSlot.JEWELLERY_1;
                    else if (Items[(int)EquipSlot.JEWELLERY_2] == null)
                        slotId = (ushort)EquipSlot.JEWELLERY_2;
                    else if (Items[(int)EquipSlot.JEWELLERY_3] == null)
                        slotId = (ushort)EquipSlot.JEWELLERY_3;
                    else if (Items[(int)EquipSlot.JEWELLERY_4] == null)
                        slotId = (ushort)EquipSlot.JEWELLERY_4;
                    else
                        slotId = (ushort)itemEquipSlot;
                }
                else if (itemEquipSlot == EquipSlot.EITHER_HAND)
                {
                    if (Items[(int)EquipSlot.MAIN_HAND] == null)
                        slotId = (ushort)EquipSlot.MAIN_HAND;
                    else if (Items[(int)EquipSlot.OFF_HAND] == null)
                        slotId = (ushort)EquipSlot.OFF_HAND;
                    else
                        slotId = (ushort)EquipSlot.MAIN_HAND;
                }
                else if (itemEquipSlot >= EquipSlot.POCKET_1 && itemEquipSlot <= EquipSlot.POCKET_2)
                {
                    if (Items[(int)EquipSlot.POCKET_1] == null)
                        slotId = (ushort)EquipSlot.POCKET_1;
                    else if (Items[(int)EquipSlot.POCKET_2] == null)
                        slotId = (ushort)EquipSlot.POCKET_2;
                    else
                        slotId = (ushort)itemEquipSlot;
                }
                else if (itemEquipSlot >= EquipSlot.TROPHY_1 && itemEquipSlot <= EquipSlot.TROPHY_5)
                {
                    if (Items[(int)EquipSlot.TROPHY_1] == null)
                        slotId = (ushort)EquipSlot.TROPHY_1;
                    else if (Items[(int)EquipSlot.TROPHY_2] == null)
                        slotId = (ushort)EquipSlot.TROPHY_2;
                    else if (Items[(int)EquipSlot.TROPHY_3] == null)
                        slotId = (ushort)EquipSlot.TROPHY_3;
                    else if (Items[(int)EquipSlot.TROPHY_4] == null)
                        slotId = (ushort)EquipSlot.TROPHY_4;
                    else if (Items[(int)EquipSlot.TROPHY_5] == null)
                        slotId = (ushort)EquipSlot.TROPHY_5;
                    else
                        slotId = (ushort)itemEquipSlot;
                }
                else
                    slotId = sourceItem.Info.SlotId;
            }

            //Log.Success("Generate", "ItemSlot=" + IFrom.info.SlotId + ",generated=" + SlotId);
            return slotId;
        }

        public void AssignTrophy(byte trophySlot, ushort value)
        {
            if (GetItemInSlot(trophySlot) != null)
            {
                GetItemInSlot(trophySlot).AltAppearanceEntry = value;
                SendItemInSlot(_playerOwner, trophySlot);
            }
        }

        public void HandleAltAppearance(byte slot, ushort appearanceItemEntry)
        {
            Item existingItem = GetItemInSlot(slot);

            if (existingItem == null)
                return;

            if (appearanceItemEntry != 0)
            {
                Item appearanceItem = GetItemInSlot(appearanceItemEntry);

                if (appearanceItem == null)
                    return;

                if (CanMoveToSlot(appearanceItem, slot) && CanUseItemType(appearanceItem.Info, slot, _playerOwner.Info.CareerLine) && appearanceItem.Info.Type == existingItem.Info.Type && appearanceItem.Info.TwoHanded == existingItem.Info.TwoHanded)
                    existingItem.AltAppearanceEntry = appearanceItem.Info.Entry;
            }
            else
                existingItem.AltAppearanceEntry = 0;

            SendItemInSlot(_playerOwner, slot);
            SendEquipped(null, slot);
        }

        public bool MoveSlot(ushort sourceSlot, ushort destinationSlot, ushort count)
        {
            Item sourceItem = GetItemInSlot(sourceSlot);
            bool moveSuccess = false;

            if (TCPManager.GetTimeStampMS() > _movedItems)
            {
                if (sourceItem == null)
                    return false;

                // Clients can provide false information here.
                if (count > sourceItem.Count)
                {
                    _playerOwner?.SendClientMessage("Item move failed: attempted to move " + count + " of the item " + sourceItem.Info.Name + " while only having " + sourceItem.Count + ".", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return false;
                }

                if (destinationSlot == DELETE_SLOT)
                {
                    _playerOwner?.SendLocalizeString(new[] { sourceItem.Info.Name, count.ToString() }, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_PLAYER_DESTROYED_ITEM);
                    DeleteItem(sourceSlot, count);
                    return true;
                }

                if (destinationSlot == AUTO_EQUIP_SLOT)
                {
                    // This block runs when the user right clicks an item to transfer it between bags.
                    if (IsCraftingInventorySlot(sourceSlot) || IsCurrencyInventorySlot(sourceSlot))
                    {
                        // Moving a crafting or currency item to main inventory
                        destinationSlot = GetFreeMainBagInventorySlot();
                    }
                    else if (IsMainInventorySlot(sourceSlot))
                    {
                        switch (sourceItem.Info.Type)
                        {
                            case (byte)Item.ItemType.Currency:
                                // Moving a currency item into currency bag
                                destinationSlot = GetFreeItemSlotInRange(CURRENCY_START_SLOT, (ushort)(CURRENCY_START_SLOT + GetMaxCurrencyItemSlots()));
                                break;
                            case (byte)Item.ItemType.Crafting:
                                // Moving a crafting item from main inventory to crafting bag
                                destinationSlot = GetFreeItemSlotInRange(CRAFTING_START_SLOT, (ushort)(CRAFTING_START_SLOT + GetMaxCraftingItemSlots()));
                                break;
                        }
                    }
                }
                if (destinationSlot == AUTO_EQUIP_SLOT)
                    destinationSlot = GenerateAutoSlot(sourceSlot);

                if (destinationSlot == AUTO_BANK_SLOT)
                    destinationSlot = GetFreeItemSlotInRange(BANK_START_SLOT, GetMaxBankSlot());

                if (_playerOwner.CbtInterface.IsInCombat && (IsEquipmentSlot(destinationSlot) || IsEquipmentSlot(sourceSlot)))
                {
                    _playerOwner.SendLocalizeString(sourceItem.Info.Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ITEM_ERR_CANT_MOVE_X);
                    return false;
                }

                if (destinationSlot == 0)
                {
                    _playerOwner.SendLocalizeString(sourceItem.Info.Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ITEM_ERR_CANT_MOVE_X);
                    return false;
                }

                if (!CanMoveToSlot(sourceItem, destinationSlot))
                    return false;

                Item destinationItem = GetItemInSlot(destinationSlot);

                if (!CanMoveToSlot(sourceItem, destinationSlot))
                    return false;

                if (IsEquipmentSlot(destinationSlot) && !CanUseItemType(sourceItem.Info, destinationSlot))
                    return false;

                if (destinationItem != null)
                {
                    if (!CanMoveToSlot(destinationItem, sourceSlot))
                        return false;

                    if (IsEquipmentSlot(sourceSlot) && !CanUseItemType(destinationItem.Info, sourceSlot))
                        return false;

                    // Attempt to stack the item.
                    if (sourceItem.Info.Entry == destinationItem.Info.Entry && destinationItem.Info.MaxStack > 1 && destinationItem.Count < destinationItem.Info.MaxStack)
                    {
                        if (sourceItem.Count <= destinationItem.Info.MaxStack - destinationItem.Count)
                        {   // Complete addition to an existing stack.
                            destinationItem.Count += sourceItem.Count;
                            DeleteItem(sourceSlot);
                            moveSuccess = true;
                        }

                        else
                        {   // Partial addition.
                            sourceItem.Count -= (ushort)(destinationItem.Info.MaxStack - destinationItem.Count);
                            destinationItem.Count = destinationItem.Info.MaxStack;
                            moveSuccess = true;
                        }
                    }
                    else // Perform a swap.
                    {
                        if (CanMoveToSlot(destinationItem, sourceSlot))
                        {
                            Items[sourceSlot] = destinationItem;
                            destinationItem.SlotId = sourceSlot;

                            Items[destinationSlot] = sourceItem;
                            sourceItem.SlotId = destinationSlot;

                            if (IsEquipmentSlot(sourceSlot) && !IsEquipmentSlot(destinationSlot))
                            {
                                UnEquipItem(sourceItem, true);
                                EquipItem(destinationItem);
                            }

                            if (IsEquipmentSlot(destinationSlot) && !IsEquipmentSlot(sourceSlot))
                            {
                                UnEquipItem(destinationItem, true);
                                EquipItem(sourceItem);
                            }
                            moveSuccess = true;
                            SendEquipped(null, destinationSlot, sourceSlot);

                            if (_playerOwner != null && IsEquipmentSlot(destinationSlot) && sourceItem.Info.TokUnlock > 0)
                                // the 2nd value here is true because this is item we currently equipped and this might trigger 
                                // set unlock
                                _playerOwner.TokInterface.AddTok(sourceItem.Info.TokUnlock, true);
                        }

                        else
                            _playerOwner?.SendLocalizeString(sourceItem.Info.Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, IsEquipmentSlot(destinationSlot) ?
                                Localized_text.TEXT_ITEM_ERR_CANT_EQUIP_X
                                : Localized_text.TEXT_ITEM_ERR_CANT_MOVE_X);
                    }
                }
                else // Direct move.
                {
                    if (count == sourceItem.Count)
                    {   // Move all.
                        sourceItem.SlotId = destinationSlot;
                        Items[sourceSlot] = null;
                        Items[destinationSlot] = sourceItem;
                    }
                    else
                    {   // Split the stack.
                        sourceItem.Count -= count;
                        Item newDestItem = new Item(_Owner);
                        if (newDestItem.Load(sourceItem.Info, destinationSlot, count))
                            Items[destinationSlot] = newDestItem;
                    }


                    if (IsEquipmentSlot(sourceSlot) && !IsEquipmentSlot(destinationSlot))
                        UnEquipItem(sourceItem);
                    else if (IsEquipmentSlot(destinationSlot) && !IsEquipmentSlot(sourceSlot))
                        EquipItem(sourceItem);

                    SendEquipped(null, destinationSlot, sourceSlot);

                    if (_playerOwner != null && IsEquipmentSlot(destinationSlot) && sourceItem.Info.TokUnlock > 0)
                        // the 2nd value here is true because this is item we currently equipped and this might trigger 
                        // set unlock
                        _playerOwner.TokInterface.AddTok(sourceItem.Info.TokUnlock, true);

                    moveSuccess = true;
                }

                if (!moveSuccess)
                    return false;

                if (_playerOwner != null)
                    SendItemSwap(_playerOwner, destinationSlot, sourceSlot);
                _movedItems = TCPManager.GetTimeStampMS() + 5;
                return true;
            }
            else
                return false;

        }

        private List<ushort> _stacked = new List<ushort>(); // List des Objets stackable
        private readonly List<ushort> _toSend = new List<ushort>(); // List des Objets mis a jours

        private static ushort NumStacksRequiredToHold(ushort itemCount, ushort maxStack)
        {
            if (maxStack <= 1)
                return itemCount;
            return (ushort)((itemCount + maxStack - 1) / maxStack);
        }

        #endregion

        #region Removal / Deletion

        /// <summary>
        /// Deletes a certain number of items of the given item ID from the player's inventory. Used to handle item currencies.
        /// </summary>
        /// <param name="itemId">The item ID to delete.</param>
        /// <param name="count">The number to delete.</param>
        /// <returns>The number of items successfully removed.</returns>
        public bool RemoveItems(uint itemId, ushort count)
        {
            List<ushort> removed = new List<ushort>();
            int result = RemoveItemsImpl(itemId, count, ref removed);
            SendItemsInSlots(_playerOwner, removed);
            return result != 0;
        }

        /// <summary>
        /// Removes all items of the specified item ID from the player's quest inventory.
        /// </summary>
        /// <param name="itemId">The item ID of the quest item(s) to delete.</param>
        public void RemoveQuestItems(uint itemId)
        {
            ushort count = GetItemCount(itemId);

            List<ushort> removed = new List<ushort>();

            for (ushort slot = QUEST_START_SLOT; slot < QUEST_START_SLOT + GetMaxQuestItemSlots() && count > 0; ++slot)
            {
                if (Items[slot] != null && Items[slot].Info.Entry == itemId)
                {
                    if (Items[slot].Count >= count)
                    {
                        removed.Add(slot);
                        DeleteItem(slot, count);
                        count = 0;
                    }
                    else
                    {
                        removed.Add(slot);
                        count -= Items[slot].Count;
                        DeleteItem(slot, Items[slot].Count);
                    }
                }
            }

            SendItemsInSlots(_playerOwner, removed);
        }

        /// <summary>
        /// Removes items from the player's inventory.
        /// </summary>
        /// <param name="entry">The item ID to remove.</param>
        /// <param name="count">The number of items to remove.</param>
        /// <param name="removed">Reference list containing the inventory slots that have been modified by a call to this function.</param>
        /// <param name="checkCount">If true, the function will return immediately if the total number of matching items present is less than Count.</param>
        /// <returns>The number of items removed.</returns>
        private int RemoveItemsImpl(uint entry, ushort count, ref List<ushort> removed, bool checkCount = true)
        {
            if (checkCount && GetItemCount(entry) < count)
                return 0;

            ushort originalCount = count;

            // Purge all matching items from the standard inventory slots.
            for (ushort slot = MAX_EQUIPMENT_SLOT; slot < GetMaxInventorySlot() && count > 0; ++slot)
            {
                if (Items[slot] == null || Items[slot].Info.Entry != entry)
                    continue;

                removed.Add(slot);

                if (Items[slot].Count >= count)
                {
                    DeleteItem(slot, count);
                    count = 0;
                }
                else
                {
                    count -= Items[slot].Count;
                    DeleteItem(slot, Items[slot].Count);
                }
            }

            // Continue on to the craft bags.
            for (ushort slot = CURRENCY_START_SLOT; slot < CURRENCY_START_SLOT + GetMaxCurrencyItemSlots() && count > 0; ++slot)
            {
                if (Items[slot] == null || Items[slot].Info.Entry != entry)
                    continue;

                removed.Add(slot);

                if (Items[slot].Count >= count)
                {
                    DeleteItem(slot, count);
                    count = 0;
                }
                else
                {
                    count -= Items[slot].Count;
                    DeleteItem(slot, Items[slot].Count);
                }
            }

            return originalCount - count;
        }

        /// <summary>
        /// Handles the deletion of the item in a given slot from the inventory and the database.
        /// </summary>
        /// <param name="slotId">The slot to delete from.</param>
        /// <param name="count">If nonzero, the specific number of items to remove from the slot.</param>
        public void DeleteItem(ushort slotId, ushort count = 0)
        {
            //Log.Success("DeleteItem", "SlotId=" + SlotId);
            Item sourceItem = GetItemInSlot(slotId);

            if (sourceItem == null)
                return;

            if (count == 0)
            {
                Items[slotId] = null;
                sourceItem.Delete();
            }
            else if (count <= sourceItem.Count)
            {
                sourceItem.Count -= count;
                if (sourceItem.Count <= 0)
                {
                    Items[slotId] = null;
                    sourceItem.Delete();
                }
            }

            if (_playerOwner != null)
            {
                SendItemInSlot(_playerOwner, slotId);
                _playerOwner.QtsInterface.HandleEvent(Objective_Type.QUEST_GET_ITEM, sourceItem.Info.Entry, count);
            }

            if (IsEquipmentSlot(slotId))
            {
                UnEquipItem(sourceItem);
                SendEquipped(null, slotId);
            }
        }

        #endregion

        #region Trading

        public Player Trading;
        public uint TradingMoney;
        public byte TradingAccepted;
        public bool TradingUpdated;
        public byte TradingUpdate;
        public KeyValuePair<ushort, Item>[] TradeItems;

        public void HandleTrade(PacketIn packet)
        {
            try
            {


                TradingUpdated = false;

                byte Status = packet.GetUint8();
                byte Unk = packet.GetUint8();
                ushort Oid = packet.GetUint16();

                if (!_Owner.IsInWorld())
                    return;

                if (_playerOwner == null)
                    return;

                Player Plr = _playerOwner;

                if (Oid <= 0)
                {
                    Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_TRADE, Localized_text.TEXT_TRADE_ERR_NO_TARGET);
                    SendTradeClose(Oid);
                    return;
                }

                if (Oid == _Owner.Oid)
                {
                    Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_TRADE, Localized_text.TEXT_TRADE_ERR_CANT_TRADE_WITH_YOURSELF);
                    SendTradeClose(Oid);
                    return;
                }



                //Log.Success("HandleTrade", "Status=" + Status + ",oid=" + oid);

                Trading = Plr.Region.GetPlayer(Oid);

                if (Trading == null)
                {
                    SendTradeClose(Oid);
                    return;
                }

                if (Status == 0 && TradingAccepted == 0) // New Trade
                {
                    if (!CanOpenTrade(Trading))
                    {
                        Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_TRADE, Localized_text.TEXT_TRADE_ERR_TARGET_ALREADY_TRADING);
                        CloseTrade();
                        return;
                    }

                    TradeItems = new KeyValuePair<ushort, Item>[MAX_TRADE_SLOT];
                    SendTradeInfo(this);
                    Trading.ItmInterface.SendTradeInfo(this);
                    TradingAccepted = 1;
                }
                else if (Status == 1 && IsTrading()) // Update trade
                {
                    uint Money = packet.GetUint32();
                    byte Update = packet.GetUint8();
                    byte ItemCounts = packet.GetUint8();

                    //Log.info("Trade", "Money=" + Money + ",Update=" + Update + ",Items=" + ItemCounts);
                    if (TradeItems == null)
                        TradeItems = new KeyValuePair<ushort, Item>[MAX_TRADE_SLOT];

                    for (int i = 0; i < MAX_TRADE_SLOT; i++)
                    {
                        ushort itemslot = packet.GetUint16();
                        Item item = GetItemInSlot(itemslot);

                        if (item != null)
                        {
                            if (item.BoundtoPlayer || item.Info.Bind == 1)
                            {
                                Plr.SendLocalizeString(item.Info.Name, ChatLogFilters.CHATLOGFILTERS_TRADE, Localized_text.TEXT_TRADE_ERR_OBJECT_CANT_BE_TRADED);
                                return;
                            }
                            TradeItems[i] = new KeyValuePair<ushort, Item>(itemslot, item);
                            SendItemToTradeWindow(Plr, (ushort)(MY_TRADE_SLOT + i), item, item.Count);
                            SendItemToTradeWindow(Trading, (ushort)(OTHER_TRADE_SLOT + i), item, item.Count);
                        }
                        else
                            TradeItems[i] = new KeyValuePair<ushort, Item>(0, null);
                    }

                    Trading.ItmInterface.TradingAccepted = 1;
                    TradingAccepted = 1;

                    TradingMoney = Money;
                    if (TradingMoney > Plr.GetMoney())
                    {
                        TradingMoney = Plr.GetMoney();
                        Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_TRADE, Localized_text.TEXT_TRADE_ERR_INSUFFICIENT_MONEY);
                    }

                    SendTradeInfo(this);
                    Trading.ItmInterface.SendTradeInfo(this);

                }
                else if (Status == 2 && IsTrading()) // Accept trade
                {
                    TradingAccepted = 2;

                    Trading.ItmInterface.SendTradeInfo(this);

                    if (TradingAccepted == 2 && Trading.ItmInterface.TradingAccepted == 2)
                        Trade(Trading.ItmInterface);
                }
                else if (Status == 3 && IsTrading()) // Close trade
                {
                    Trading.ItmInterface.SendTradeClose(_Owner.Oid);
                    SendTradeClose(Oid);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Handletrade error {e.Message} {e.StackTrace}");
            }
        }
        public void Trade(ItemsInterface distInter)
        {
            //Log.Success("Trade", "TRADE !");

            Player Me = GetPlayer();
            Player Other = distInter.GetPlayer();

            bool AllOk = true;

            if (distInter.TradingMoney > 0)
                if (!Other.HasMoney(distInter.TradingMoney))
                    AllOk = false;

            if (TradingMoney > 0)
                if (!Me.HasMoney(TradingMoney))
                    AllOk = false;

            // TODO : CheckItem more
            Item itm;

            for (int i = 0; i < MAX_TRADE_SLOT; i++)
            {
                itm = _Owner.GetPlayer().ItmInterface.GetItemInSlot(TradeItems[i].Key);

                if (TradeItems[i].Value != null)
                {
                    if (itm == null)
                    {
                        AllOk = false;
                        continue;
                    }

                    if (TradeItems[i].Value.Owner != _Owner)
                        AllOk = false;

                    if (itm.Info.Entry != TradeItems[i].Value.Info.Entry)
                        AllOk = false;

                    if (itm.Count < TradeItems[i].Value.Count)
                        AllOk = false;
                }

                itm = distInter.GetPlayer().ItmInterface.GetItemInSlot(distInter.TradeItems[i].Key);

                if (distInter.TradeItems[i].Value != null)
                {
                    if (distInter.TradeItems[i].Value.Owner != distInter._Owner)
                        AllOk = false;

                    if (itm == null)
                    {
                        AllOk = false;
                        continue;
                    }

                    if (itm.Info.Entry != distInter.TradeItems[i].Value.Info.Entry)
                        AllOk = false;

                    if (itm.Count < distInter.TradeItems[i].Value.Count)
                        AllOk = false;
                }
            }

            if (AllOk)
            {
                if (Other.RemoveMoney(distInter.TradingMoney))
                    Me.AddMoney(distInter.TradingMoney);

                if (Me.RemoveMoney(TradingMoney))
                    Other.AddMoney(TradingMoney);

                for (int i = 0; i < MAX_TRADE_SLOT; i++)
                {
                    if (TradeItems[i].Value != null)
                    {
                        distInter.CreateItem(TradeItems[i].Value.Info, TradeItems[i].Value.Count, TradeItems[i].Value.CharSaveInfo._Talismans, TradeItems[i].Value.GetPrimaryDye(), TradeItems[i].Value.GetSecondaryDye(), TradeItems[i].Value.BoundtoPlayer);
                        DeleteItem(TradeItems[i].Value.SlotId, TradeItems[i].Value.Count);
                    }

                    if (distInter.TradeItems[i].Value != null)
                    {
                        CreateItem(distInter.TradeItems[i].Value.Info, distInter.TradeItems[i].Value.Count, distInter.TradeItems[i].Value.CharSaveInfo._Talismans, distInter.TradeItems[i].Value.GetPrimaryDye(), distInter.TradeItems[i].Value.GetSecondaryDye(), distInter.TradeItems[i].Value.BoundtoPlayer);
                        distInter.DeleteItem(distInter.TradeItems[i].Value.SlotId, distInter.TradeItems[i].Value.Count);
                    }
                }
            }

            SendTradeClose(Other.Oid);
            distInter.SendTradeClose(Me.Oid);

            CloseTrade();
        }
        public void SendTradeInfo(ItemsInterface distInterface)
        {
            try
            {


                PacketOut Out = new PacketOut((byte)Opcodes.F_TRADE_STATUS);
                Out.WriteByte(distInterface.TradingAccepted);
                Out.WriteByte(0);
                Out.WriteUInt16(distInterface != this ? distInterface._Owner.Oid : (ushort)0);

                if (distInterface.TradingAccepted == 2)
                    Out.Fill(0, 24);
                else
                {
                    Out.WriteUInt32(distInterface.TradingMoney);
                    Out.WriteByte(3); // ?
                    Out.WriteByte((byte)distInterface.TradeItems.Count(i => i.Value != null));
                    for (int i = 0; i < MAX_TRADE_SLOT; i++)
                        Out.WriteUInt16((ushort)(distInterface.TradeItems[i].Value == null ? 0 : 1));
                }

                _playerOwner.SendPacket(Out);
            }
            catch (Exception e)
            {
                _logger.Error($"Exception : {e.Message} {e.StackTrace} {e.Source}");
            }
        }
        public bool IsTrading()
        {
            return Trading != null;
        }
        public bool CanOpenTrade(Player plr)
        {
            return plr.ItmInterface.Trading == null || plr.ItmInterface.Trading == _playerOwner;
        }
        public void CloseTrade()
        {
            Trading = null;
            TradingMoney = 0;
            TradingAccepted = 0;
            TradingUpdate = 0;
        }
        public void SendTradeClose(ushort oid)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_TRADE_STATUS);
            Out.WriteByte(3);
            Out.WriteByte(0);
            Out.WriteUInt16(oid);
            Out.Fill(0, 24);
            _playerOwner.SendPacket(Out);

            CloseTrade();
        }

        #endregion

        #region BuyBack

        public List<Item> BuyBack = new List<Item>();
        public void SendBuyBack()
        {
            if (_playerOwner == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_STORE_BUY_BACK);
            Out.WriteByte((byte)BuyBack.Count); // count
            for (int i = BuyBack.Count - 1; i >= 0; --i)
            {
                Out.WriteUInt32(BuyBack[i].Info.SellPrice);
                Item.BuildItem(ref Out, BuyBack[i], null, null, 0, 0, _Owner.GetPlayer());
            }
            Out.WriteByte(0);
            _playerOwner.SendPacket(Out);
        }
        public void SellItem(InteractMenu menu)
        {
            ushort slotId = (ushort)(menu.Num + (menu.Page * 256));
            ushort count = menu.Count;

            List<ushort> toSend = new List<ushort>();
            Item itm = GetItemInSlot(slotId);

            if (itm == null || itm.Info.SellPrice <= 0)
                return;

            if (count == 0 || count > itm.Count)
                count = itm.Count;

            toSend.Add(slotId);

            if (count == itm.Count)
            {
                Items[slotId] = null;
                AddBuyBack(itm);
            }
            else if (count < itm.Count)
            {
                itm.Count -= count;

                Item New = new Item(_Owner);
                if (!New.Load(itm.Info, 0, count))
                    return;
                AddBuyBack(New);
            }

            _playerOwner.AddMoney(itm.Info.SellPrice * count);

            SendItemsInSlots(_playerOwner, toSend);
            SendBuyBack();
        }

        public void RepairItem(InteractMenu menu)
        {
            ushort slotId = (ushort)(menu.Num + (menu.Page * 256));
            ushort count = menu.Count;
            Item itm = GetItemInSlot(slotId);

            if (itm == null)
                return;

            string[] items = itm.Info.Craftresult.Split(';');
            Item_Info RepItemInfo = null;
            uint itemlvl = 0;
            byte rarety = 0;

            foreach (string ritem in items)
            {
                Item_Info RitemInfo = ItemService.GetItem_Info(uint.Parse(ritem));
                rarety = RitemInfo.Rarity;
                itemlvl = RitemInfo.MinRank;
                if (ItemsInterface.CanUse(RitemInfo, _Owner.GetPlayer(), false, false))
                {
                    RepItemInfo = RitemInfo;
                    break;
                }
            }
            if (rarety < 1)
                rarety = 1;
            if (itemlvl < 1)
                itemlvl = 1;

            if (RepItemInfo == null)
                return;

            if (!_playerOwner.RemoveMoney(20 * itemlvl * 6 * rarety * 2))
            {
                _playerOwner.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_INSUFFICIENT_MONEY_TO_BUY);
                return;
            }

            if (CreateItem(RepItemInfo, 1) == ItemResult.RESULT_OK)
            {
                DeleteItem(slotId);
            }
            else
                _playerOwner.AddMoney(20 * itemlvl * 6 * rarety * 2);
        }

        public void BuyBackItem(InteractMenu menu)
        {
            ushort slotId = (ushort)(BuyBack.Count - 1 - (menu.Num + (menu.Page * 256)));
            Item itm = GetBuyBack(slotId, menu.Packet.GetUint16());

            if (itm == null || !_playerOwner.RemoveMoney(itm.Count * itm.Info.SellPrice))
            {
                _playerOwner.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_INSUFFICIENT_MONEY_TO_BUY);
                return;
            }
            //CreateExistingItem(itm.Info, itm.Count, itm.CharSaveInfo);
            CreateItem(itm.Info, itm.Count, itm.GetTalismans(), itm.GetPrimaryDye(), itm.GetSecondaryDye(), itm.BoundtoPlayer);
            SendBuyBack();
        }
        public void AddBuyBack(Item itm)
        {
            itm.SlotId = 0;
            CharMgr.Database.DeleteObject(itm.CharSaveInfo);
            if (BuyBack.Count >= BUY_BACK_SLOT)
            {
                Item toDel = BuyBack[0];
                BuyBack.Remove(toDel);
                //toDel?.Delete();
            }
            BuyBack.Add(itm);
        }
        public Item GetBuyBack(ushort num, ushort count)
        {
            if (num >= BuyBack.Count)
                return null;

            Item itm = BuyBack[num];

            if (itm == null || !_playerOwner.HasMoney(itm.Info.SellPrice * count))
                return null;

            if (itm.Count <= count)
            {
                BuyBack.Remove(itm);
                return itm;
            }

            itm.Count -= count;
            Item New = new Item(_Owner);
            // New.LoadExisting(itm.Info, 0, count, itm);
            New.Load(itm.Info, 0, count);
            return New;
        }

        #endregion

        #region Dye

        public void RemoveDye(Item itm, bool primary, bool secondary)
        {
            if (_playerOwner == null)
                return;

            if (primary)
                itm.CharSaveInfo.PrimaryDye = 0;
            if (secondary)
                itm.CharSaveInfo.SecondaryDye = 0;

            SendItemInSlot(_playerOwner, itm.SlotId);

            if (IsEquipmentSlot(itm.SlotId))
                SendEquipped(null, itm.SlotId);
        }


        public void DyeItem(Item itm, ushort primary, ushort secondary)
        {
            if (_playerOwner == null)
                return;

            if (primary != 0)
                itm.CharSaveInfo.PrimaryDye = primary;

            if (secondary != 0)
                itm.CharSaveInfo.SecondaryDye = secondary;

            SendItemInSlot(_playerOwner, itm.SlotId);

            if (IsEquipmentSlot(itm.SlotId))
                SendEquipped(null, itm.SlotId);
        }

        #endregion

        #region ItemSets
        private Dictionary<uint, byte> _itemSets;

        private bool _strikethroughActive, _dualParryActive;

        public void CheckItemSets()
        {
            RemoveItemSetBonus();

            _itemSets = new Dictionary<uint, byte>();
            for (ushort i = 0; i < MAX_EQUIPMENT_SLOT; ++i)
                if (Items[i] != null && Items[i].Info != null && Items[i].Info.ItemSet != 0)
                {
                    if (_itemSets.ContainsKey(Items[i].Info.ItemSet))
                        _itemSets[Items[i].Info.ItemSet] += 1;
                    else
                        _itemSets.Add(Items[i].Info.ItemSet, 1);
                }

            //10 = mainhand // 11 offhand 

            AddItemSetBonus();
        }

        public void RemoveItemSetBonus()
        {
            if (_itemSets != null)
                foreach (KeyValuePair<uint, byte> itemSet in _itemSets)
                {
                    Item_Set itemSetInfo = ItemService.GetItem_Set(itemSet.Key);

                    if (itemSetInfo == null)
                    {
                        Log.Error("Item sets", "Missing Item_Set entry for key " + itemSet.Key);
                        continue;
                    }

                    List<ItemSetBonusInfo> bonusList = itemSetInfo.GetBonusList(itemSet.Value);

                    if (bonusList.Count <= 0)
                        continue;

                    foreach (ItemSetBonusInfo info in bonusList)
                    {
                        if (info.ActionType == 3)  // remove stats
                            _playerOwner.StsInterface.RemoveItemBonusStat((Stats)info.StatOrSpell, info.Value, info.Percentage);
                        else
                        {
                            BuffInfo buffInfo = AbilityMgr.GetBuffInfo(info.StatOrSpell);
                            if (buffInfo != null && buffInfo.BuffClass != BuffClass.Tactic) // To allow signalling of Intimidating Repent without casting it
                                _playerOwner.BuffInterface.RemoveBuffByEntry(info.StatOrSpell);
                        }
                    }
                }

            if (_dualParryActive)
            {
                _dualParryActive = false;
                _playerOwner.StsInterface.RemoveItemBonusStat(Stats.Parry, 10);
            }
            else if (_strikethroughActive)
            {
                _strikethroughActive = false;
                _playerOwner.StsInterface.RemoveItemBonusStat(Stats.BlockStrikethrough, 10);
            }
        }

        public void AddItemSetBonus()
        {
            if (_itemSets != null)
                foreach (KeyValuePair<uint, byte> itemSet in _itemSets)
                {
                    Item_Set itemSetInfo = ItemService.GetItem_Set(itemSet.Key);

                    if (itemSetInfo == null)
                    {
                        Log.Error("Item sets", "Missing Item_Set entry for key " + itemSet.Key);
                        continue;
                    }

                    List<ItemSetBonusInfo> bonusList = itemSetInfo.GetBonusList(itemSet.Value);

                    if (bonusList.Count <= 0)
                        continue;

                    foreach (ItemSetBonusInfo info in bonusList)
                    {
                        //switches all armor set damage bonuses to % based
                        if (info.StatOrSpell == 24)
                        {
                            info.StatOrSpell = 25;
                        }
                        if (info.ActionType == 3)
                            _playerOwner.StsInterface.AddItemBonusStat((Stats)info.StatOrSpell, info.Value, info.Percentage);
                        else
                        {
                            _logger.Debug($"{_playerOwner.Name} finding buff {info.StatOrSpell} ");
                            BuffInfo buffInfo = AbilityMgr.GetBuffInfo(info.StatOrSpell);
                            if (buffInfo != null && buffInfo.BuffClass != BuffClass.Tactic
                            ) // To allow signalling of Intimidating Repent without casting it
                            {
                                // Unk is desired level.
                                _logger.Debug($"{_playerOwner.Name} queueing buff {itemSetInfo.Unk}, {buffInfo.Entry} {buffInfo.Name} ");
                                _playerOwner.BuffInterface.QueueBuff(new BuffQueueInfo(_playerOwner, itemSetInfo.Unk,
                                    buffInfo));
                            }
                        }
                    }
                }

            if (Items[10] != null && Items[11] != null && !(Items[11].Info.Type == (byte)ItemTypes.ITEMTYPES_CHARM || Items[11].Info.Type == (byte)ItemTypes.ITEMTYPES_SHIELD))
            {
                _dualParryActive = true;
                _playerOwner.StsInterface.AddItemBonusStat(Stats.Parry, 10);
            }

            if (Items[10] != null && Items[10].Info.TwoHanded && (Items[10].Info.Type == (byte)ItemTypes.ITEMTYPES_HAMMER || Items[10].Info.Type == (byte)ItemTypes.ITEMTYPES_AXE || Items[10].Info.Type == (byte)ItemTypes.ITEMTYPES_SWORD || Items[10].Info.Type == (byte)ItemTypes.ITEMTYPES_SPEAR))
            {
                _strikethroughActive = true;
                _playerOwner.StsInterface.AddItemBonusStat(Stats.BlockStrikethrough, 10);
            }
        }

        #endregion

        public void TalismanCheck()
        {
            Item modifiedItem = GetItemInSlot(255);
            if (modifiedItem == null)
                return;
            List<uint> unfused = modifiedItem.AbortFuseTalisman();

            foreach (uint entry in unfused)
            {
                CreateItem(entry, 1);
            }

            MoveSlot(255, GetFreeInventorySlot(GetItemInSlot(255).Info, true), 1);
        }

        #region Ability

        public AbilityResult WeaponCheck(WeaponRequirements weaponNeeded)
        {
            AbilityResult result = AbilityResult.ABILITYRESULT_OK;
            switch (weaponNeeded)
            {
                case WeaponRequirements.MainHand:
                    if (GetItemInSlot((ushort)EquipSlot.MAIN_HAND) == null)
                        result = AbilityResult.ABILITYRESULT_NEED_MELEE_WEAPON;
                    break;
                case WeaponRequirements.OffHand:
                    if (GetItemInSlot((ushort)EquipSlot.OFF_HAND) == null)
                        result = AbilityResult.ABILITYRESULT_NEED_MELEE_WEAPON;
                    break;
                case WeaponRequirements.Ranged:
                    if (GetItemInSlot((ushort)EquipSlot.RANGED_WEAPON) == null)
                        result = AbilityResult.ABILITYRESULT_NEEDRANGED;
                    break;
                case WeaponRequirements.TwoHander:
                    if (GetItemInSlot((ushort)EquipSlot.MAIN_HAND) == null || GetItemInSlot((ushort)EquipSlot.MAIN_HAND).Info.TwoHanded == false)
                        result = AbilityResult.ABILITYRESULT_WRONG_WEAPON_TYPE;
                    break;
                case WeaponRequirements.DualWield:
                    if (GetItemInSlot((ushort)EquipSlot.MAIN_HAND) == null || GetItemInSlot((ushort)EquipSlot.OFF_HAND) == null)
                        result = AbilityResult.ABILITYRESULT_WRONG_WEAPON_TYPE;
                    break;
                case WeaponRequirements.Shield:
                    if (GetItemInSlot((ushort)EquipSlot.OFF_HAND) == null || GetItemInSlot((ushort)EquipSlot.OFF_HAND).Info.Type != (byte)ItemTypes.ITEMTYPES_SHIELD)
                        result = AbilityResult.ABILITYRESULT_WRONG_WEAPON_TYPE;
                    break;
            }
            return result;
        }

        private readonly HashSet<uint> _foundItemEntries = new HashSet<uint>();
        private readonly List<Item> _foundItems = new List<Item>();

        public void SendItemGroupCooldown(ushort groupEntry, ushort cooldown)
        {
            long nextUseTime = TCPManager.GetTimeStamp() + cooldown;

            foreach (var item in Items)
            {
                if (item?.Info != null && item.Info.Unk27[19] == groupEntry && !_foundItemEntries.Contains(item.Info.Entry))
                {
                    _foundItemEntries.Add(item.Info.Entry);
                    _foundItems.Add(item);
                    item.CharSaveInfo.NextAllowedUseTime = nextUseTime;
                }
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_ITEM_COOLDOWN, _foundItems.Count * 20 + 1);

            Out.WriteByte((byte)_foundItems.Count);

            foreach (var item in _foundItems)
            {
                Out.WriteUInt32(item.Info.Entry);
                Out.WriteUInt32(0);
                Out.WriteUInt16(0);

                Out.WriteUInt16(cooldown);
            }

            ((Player)_Owner).SendPacket(Out);

            _foundItemEntries.Clear();
            _foundItems.Clear();
        }


        public void SendMysteryBag(ushort slot)
        {
            Item itm = _Owner.GetPlayer().ItmInterface.GetItemInSlot(slot);

            if (itm == null)
                return;

            List<KeyValuePair<uint, byte>> items = GetBagCraftingItems(itm.GetPrimaryDye());
            foreach (Talisman item in itm.GetTalismans())
            {
                if (item.Entry > 0)
                    items.Add(new KeyValuePair<uint, byte>(item.Entry, item.Slot));
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_MYSTERY_BAG);
            Out.Fill(0, 2);
            Out.WriteUInt16(slot);
            Out.WriteUInt32(itm.GetSecondaryDye());
            Out.WriteByte((byte)items.Count);

            foreach (KeyValuePair<uint, byte> itemid in items)
            {
                Out.WriteByte(1);
                Item.BuildItem(ref Out, null, ItemService.GetItem_Info(itemid.Key), null, 0, itemid.Value);
            }

            ((Player)_Owner).SendPacket(Out);
        }

        public void GetItemfromMysterybag(ushort slot, byte mode)
        {
            Item itm = _Owner.GetPlayer().ItmInterface.GetItemInSlot(slot);

            if (itm == null)
                return;

            List<KeyValuePair<uint, byte>> items = GetBagCraftingItems(itm.GetPrimaryDye());
            foreach (Talisman item in itm.GetTalismans())
            {
                if (item.Entry > 0)
                    items.Add(new KeyValuePair<uint, byte>(item.Entry, item.Slot));
            }

            if (mode == 255)
            {
                ((Player)_Owner).AddMoney(itm.GetSecondaryDye());
                ((Player)_Owner).ItmInterface.DeleteItem(slot, 0);
            }
            else
            {
                if (((Player)_Owner).ItmInterface.CreateItem(items[mode - 1].Key, items[mode - 1].Value) == ItemResult.RESULT_OK)
                    ((Player)_Owner).ItmInterface.DeleteItem(slot, 0);
            }
        }

        public List<KeyValuePair<uint, byte>> GetBagCraftingItems(ushort Craftingtype)
        {
            List<KeyValuePair<uint, byte>> items = new List<KeyValuePair<uint, byte>>();
            var results = from pqitems in PQuestService._PQLoot_Crafting where (pqitems.PQCraftingBag_ID == Craftingtype) select pqitems;

            foreach (PQuest_Loot_Crafting crafts in results)
                items.Add(new KeyValuePair<uint, byte>(crafts.ItemID, crafts.Count));

            return items;
        }


        public void SendItemCooldown(ushort spellEntry, ushort cooldown)
        {
            long nextUseTime = TCPManager.GetTimeStamp() + cooldown;

            foreach (Item item in Items)
            {
                if (item?.Info != null && item.Info.SpellId == spellEntry && !_foundItemEntries.Contains(item.Info.Entry))
                {
                    _foundItemEntries.Add(item.Info.Entry);
                    _foundItems.Add(item);
                    item.CharSaveInfo.NextAllowedUseTime = nextUseTime;
                }
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_ITEM_COOLDOWN, _foundItems.Count * 20 + 1);

            Out.WriteByte((byte)_foundItems.Count);

            foreach (var item in _foundItems)
            {
                Out.WriteUInt32(item.Info.Entry);
                Out.WriteUInt32(0);
                Out.WriteUInt16(0);

                Out.WriteUInt16(cooldown);
            }
            if (_Owner is Player)
                ((Player)_Owner).SendPacket(Out);

            _foundItemEntries.Clear();
            _foundItems.Clear();
        }

        #endregion
    }
}
