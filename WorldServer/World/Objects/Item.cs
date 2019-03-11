using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Interfaces;

namespace WorldServer.World.Objects
{
    public enum EquipSlot
    {
        NONE = 0,
        MAIN_HAND = 10,
        OFF_HAND = 11,
        RANGED_WEAPON = 12,
        EITHER_HAND = 13,
        STANDARD = 14,

        TROPHY_1 = 15,
        TROPHY_2 = 16,
        TROPHY_3 = 17,
        TROPHY_4 = 18,
        TROPHY_5 = 19,

        BODY = 20,
        GLOVES = 21,
        BOOTS = 22,
        HELM = 23,
        SHOULDER = 24,
        POCKET_1 = 25,
        POCKET_2 = 26,
        BACK = 27,
        BELT = 28,

        JEWELLERY_1 = 31,
        JEWELLERY_2 = 32,
        JEWELLERY_3 = 33,
        JEWELLERY_4 = 34
    };

    public class Item
    {
        // Créature ou Player
        public Object Owner;
        public uint _ModelId;
        private uint previewModelID;
        public ushort _SlotId;
        public ushort _Count = 1;
        public uint _EffectId;
        public ushort _PrimaryColor;
        public ushort _SecondaryColor;
        public long Cooldown;
        private bool _isCreature = false;
        // Player Uniquement
        public Item_Info Info;
        public CharacterItem CharSaveInfo;
        public Creature_item CreatureItem { get; protected set; }

        public enum ItemType
        {
            Quest = 21,
            Dye = 27,
            Crafting = 34,
            Currency = 36
        };

        public Item(Object Owner)
        {
            this.Owner = Owner;
        }
        public Item(Creature_item CItem)
        {
            _SlotId = CItem.SlotId;
            _ModelId = CItem.ModelId;
            _EffectId = CItem.EffectId;
            _PrimaryColor = CItem.PrimaryColor;
            _SecondaryColor = CItem.SecondaryColor;
            _Count = 1;
            _isCreature = true;
            CreatureItem = CItem;
        }

        public bool Load(CharacterItem itemSaveInfo)
        {
            Info = ItemService.GetItem_Info(itemSaveInfo.Entry);

            if (Info == null)
            {
                Log.Error("ItemInterface", "Load : info==null,entry=" + itemSaveInfo.Entry);
                return false;
            }

            CharSaveInfo = itemSaveInfo;
            return true;
        }

        public bool Load(Item_Info info, ushort slotId, ushort count)
        {
            Info = info;

            if (info == null)
                return false;

            if (count <= 0)
                count = 1;

            _SlotId = slotId;
            _ModelId = info.ModelId;
            _Count = count;

            Player player = Owner as Player;

            if (player != null)
            {
                if (CharSaveInfo == null)
                    CreateCharSaveInfo(player.CharacterId);

                if (info.SpellId != 0)
                    player.AbtInterface.AssignItemCooldown(this);
            }

            return true;
        }

        //public bool LoadExisting(Item_Info info, ushort slotId, ushort count, Item itm)
        //{
        //    Info = info;

        //    if (info == null)
        //        return false;

        //    if (count <= 0)
        //        count = 1;

        //    _SlotId = slotId;
        //    _ModelId = info.ModelId;
        //    _Count = count;

        //    Player player = Owner as Player;

        //    if (player != null)
        //    {
        //        CharMgr.CreateItem(itm.CharSaveInfo);

        //        if (info.SpellId != 0)
        //            player.AbtInterface.AssignItemCooldown(this);
        //    }

        //    return true;
        //}

        public void Delete()
        {
            if (CharSaveInfo != null)
                CharMgr.DeleteItem(CharSaveInfo);
        }

        public CharacterItem CreateCharSaveInfo(uint characterId)
        {
            CharSaveInfo = new CharacterItem
            {
                CharacterId = characterId,
                Counts = _Count,
                Entry = Info.Entry,
                ModelId = _ModelId,
                SlotId = _SlotId,
                PrimaryDye = 0,
                SecondaryDye = 0,
                BoundtoPlayer = false
            };
            CharMgr.CreateItem(CharSaveInfo);

            return CharSaveInfo;
        }
        public CharacterItem Save(uint characterId)
        {
            if (CharSaveInfo != null)
            {
                if (CharSaveInfo.CharacterId != characterId)
                    CharSaveInfo.CharacterId = characterId;
                CharMgr.Database.SaveObject(CharSaveInfo);
            }
            else // This should no longer be necessary.
                CreateCharSaveInfo(characterId);

            return CharSaveInfo;
        }

        public Talisman GetTalisman(byte SlotId)
        {
            if (CharSaveInfo == null)
                return null;

            Talisman tal = null;

            foreach (Talisman tali in CharSaveInfo._Talismans)
            {
                if (tali.Slot == SlotId && (tal == null || tali.Fused == 1))
                    tal = tali;
            }

            return tal;
        }

        public bool AddTalisman(uint entry, byte SlotId)
        {
            if (CharSaveInfo == null)
                return false;

            Item_Info info = ItemService.GetItem_Info(entry);

            if (info.Type != 23)
                return false;

            if (Info.ObjectLevel < info.MinRank)
                return false;

            foreach (Talisman tali in CharSaveInfo._Talismans)
            {
                if (tali.Slot == SlotId && tali.Fused == 1 || info.Crafts == ItemService.GetItem_Info(tali.Entry).Crafts && tali.Slot != SlotId)
                    return false;
            }
            CharSaveInfo._Talismans.Add(new Talisman(entry, SlotId, 1, 0));
            return true;
        }

        public uint RemoveTalisman(byte SlotId)
        {
            uint item = 0;
            foreach (Talisman tali in CharSaveInfo._Talismans)
            {
                if (tali.Slot == SlotId && tali.Fused == 1)
                    item = tali.Entry;
            }

            for (int i = 0; i < CharSaveInfo._Talismans.Count; i++)
            {
                if (CharSaveInfo._Talismans[i].Slot == SlotId && CharSaveInfo._Talismans[i].Fused == 1)
                    CharSaveInfo._Talismans.RemoveAt(i);
            }
            return item;
        }

        public void FuseTalisman()
        {
            List<Talisman> unfused = new List<Talisman>();

            foreach (Talisman tali in CharSaveInfo._Talismans)
            {
                if (tali.Fused == 1)
                {
                    unfused.Add(tali);
                }
            }
            for (int i = 0; i < CharSaveInfo._Talismans.Count; i++)
            {
                foreach (Talisman tali in unfused)
                {
                    if (CharSaveInfo._Talismans[i].Slot == tali.Slot && CharSaveInfo._Talismans[i].Fused == 0)
                        CharSaveInfo._Talismans.RemoveAt(i);
                }
            }
            foreach (Talisman tali in CharSaveInfo._Talismans)
            {
                if (tali.Fused == 1)
                {
                    tali.Fused = 0;
                }
            }

        }

        public List<uint> AbortFuseTalisman()
        {
            List<uint> unfused = new List<uint>();

            if (CharSaveInfo == null)
                return unfused;

            foreach (Talisman tali in CharSaveInfo._Talismans)
            {
                if (tali.Fused == 1)
                    unfused.Add(tali.Entry);
            }

            for (int i = CharSaveInfo._Talismans.Count - 1; i >= 0; --i)
            {
                if (CharSaveInfo._Talismans[i].Fused == 1)
                    CharSaveInfo._Talismans.RemoveAt(i);
            }

            return unfused;
        }

        public List<Talisman> GetTalismans()
        {
            if (CharSaveInfo == null)
                return null;

            return CharSaveInfo._Talismans;

        }

        public ushort GetPrimaryDye()
        {
            if (CharSaveInfo == null)
                return 0;

            return CharSaveInfo.PrimaryDye;
        }

        public ushort GetSecondaryDye()
        {
            if (CharSaveInfo == null)
                return 0;

            return CharSaveInfo.SecondaryDye;
        }

        // Writes approximately: (100 + info._Stats.Count * 8 + info.EffectsList.Count * 6 + info.SpellId > 0 ? 8 : 1 + info.Type == 23 ? info.Crafts.Count * 3 : 1 + info.Description.Length + info.TalismanSlots * 47)
        public static void BuildItem(ref PacketOut Out, Item itm, Item_Info info, MailItem mail, ushort SlotId, ushort Count, Player Plr = null,bool frombuildrepairitem = false)
        {
            SlotId = SlotId == 0 ? (itm?.SlotId ?? SlotId) : SlotId;
            Count = Count == 0 ? (itm?.Count ?? Count) : Count;
            info = info ?? itm?.Info;
            bool lootbag = false;

            
           

                if (mail != null)
                    info = ItemService.GetItem_Info(mail.id);

                if (SlotId != 0 && !frombuildrepairitem)
                    Out.WriteUInt16(SlotId);
                
            if (info != null && info.Entry >= 2500000 && info.Entry < 2600000)
            {
                BuildRepairableItem(ref Out, itm, info, mail, SlotId, Count, Plr);
                return;
            }
            if(!frombuildrepairitem)
            Out.WriteByte(0);    // repairable item
                Out.WriteUInt32(info?.Entry ?? 0);

                if (info == null)
                    return;

                //if (info.Bind == 1 || (info.Bind == 2 && itm != null && itm.BoundtoPlayer))
                //    Out.WriteUInt32(info.Entry << 2);
                //else
                //    Out.WriteUInt32(info.Entry);

                if (info.Entry == 9980 || info.Entry == 9940 || info.Entry == 9941 || info.Entry == 9942 || info.Entry == 9943)  // lootbags
                    lootbag = true;


                Out.WriteUInt16((ushort)info.ModelId);  // Valid 1.4.8

                //Appearance
                if (itm != null && itm.AltAppearanceEntry > 0 && itm.Info.Type != 24)
                {
                    Item_Info tmp = ItemService.GetItem_Info(itm.AltAppearanceEntry);

                    if (tmp == null)
                    {
                        itm.AltAppearanceEntry = 0;
                        Out.Fill(0, 7);
                    }
                    else
                    {
                        Out.WriteUInt16((ushort)tmp.ModelId); // DisplayId
                        Out.WriteUInt32(itm.AltAppearanceEntry); // Id
                        Out.WritePascalString(tmp.Name); //name
                    }
                }
                else
                {
                    Out.Fill(0, 7);
                }

                Out.WriteUInt16(info.SlotId);  // Valid 1.4.8
                Out.WriteByte(info.Type);  // Valid 1.4.8

                Out.WriteByte(info.MinRank); // Min Level
                Out.WriteByte(info.ObjectLevel); // 1.3.5, Object Level
                Out.WriteByte(info.MinRenown); // 1.3.5, Min Renown
                Out.WriteByte(info.MinRenown); // ?
                Out.WriteByte(info.UniqueEquiped); // Unique - Equiped

                Out.WriteByte(info.Rarity);
                Out.WriteByte(info.Bind);  // This byte should be part of race byte
                Out.WriteByte(info.Race);


                // Trophys have some extra bytes
                if (info.Type == (byte)ItemTypes.ITEMTYPES_TROPHY)
                {
                    Out.WriteUInt32(0);
                    Out.WriteUInt16(0x00);
                }

                if (info.Type == (byte)ItemTypes.ITEMTYPES_ENHANCEMENT)
                {
                    Out.WriteUInt32(0);
                }

                if (SlotId != 0 && info.Type == 24) // Trophy 
                {
                    Out.WriteUInt16(0);
                    Out.WriteUInt16((ushort)itm.AltAppearanceEntry);
                }
                else
                    Out.WriteUInt32(info.Career);


                Out.WriteUInt16(info.BaseColor1);  // basecolor
                Out.WriteUInt16(info.BaseColor2);


                Out.WriteUInt32(info.SellPrice);

                Out.WriteUInt16(info.MaxStack);

                if (mail != null)
                    Out.WriteUInt16((ushort)(mail.count > 0 ? mail.count : 1));
                else
                    Out.WriteUInt16((ushort)(Count > 0 ? Count : 1));

                Out.WriteUInt32(info.ItemSet);

                Out.WriteUInt32(info.Skills);  // Valid 1.4.8
                Out.WriteUInt16(info.Dps > 0 ? info.Dps : info.Armor);  // Valid 1.4.8
                Out.WriteUInt16(info.Speed);  // Valid 1.4.8

                Out.WritePascalString(info.Name);  // Valid 1.4.8  + (info.Bind == 1 || (info.Bind == 2 && itm != null && itm.BoundtoPlayer) ? "_" : "" )

                //66 Bytes + info.Name.Length

                Out.WriteByte((byte)info._Stats.Count);  // Valid 1.4.8
                foreach (KeyValuePair<byte, ushort> Key in info._Stats)
                {
                    if (Key.Key == (byte)Stats.AutoAttackSpeed)
                    {
                        Out.WriteByte(Key.Key);
                        Out.WriteUInt16(Key.Value);
                        Out.Fill(1, 5);
                    }
                    else
                    {
                        Out.WriteByte(Key.Key);
                        Out.WriteUInt16(Key.Value);
                        Out.Fill(0, 5);
                    }
                }

                Out.WriteByte((byte)info.EffectsList.Count);
                foreach (ushort effect in info.EffectsList)
                {
                    Out.WriteUInt16(effect);
                    Out.WriteUInt32(0);
                }

                if (info.SpellId == 0)
                    Out.WriteByte(0);
                else
                {
                    Out.WriteByte(1); // (byte)info._Spells.Count OK

                    Out.WriteUInt32(info.SpellId);
                    Out.WriteUInt16(AbilityMgr.GetCooldownFor(info.SpellId));   // cooldown time info

                    if (Plr == null || itm?.CharSaveInfo == null)
                        Out.WriteUInt16(0);   // current cooldown
                    else
                        Out.WriteUInt16(itm.CharSaveInfo.RemainingCooldown);
                }

                // (uint32)entry, uint16 X, uint16 Y

                if (info.Type == 23)   // talisman use craft to store its buff type 
                    Out.WriteByte(0);
                else
                {
                    Out.WriteByte((byte)info._Crafts.Count); // OK
                    foreach (KeyValuePair<byte, ushort> Kp in info._Crafts)
                    {
                        Out.WriteByte(Kp.Key);
                        Out.WriteUInt16(Kp.Value);
                    }
                }
                Out.WriteByte(0); // ??

                if (lootbag)
                {
                    Out.WriteByte(0);
                }
                else
                {
                    Out.WriteByte(info.TalismanSlots);
                    Talisman talis = null;
                    for (int i = 0; i < info.TalismanSlots; ++i)
                    {
                        if (itm != null)
                            talis = itm.GetTalisman((byte)i);
                        else if (mail != null)
                        {
                            talis = mail.GetTalisman((byte)i);
                        }

                        if (talis == null)
                            Out.WriteUInt32(0); // entry;
                        else
                        {
                            Item_Info talismanInfo = ItemService.GetItem_Info(talis.Entry);

                            // Out.Fill(0, 2);

                            Out.WriteByte(0); // slot ???
                            Out.WriteByte(0);
                            Out.WriteUInt16((ushort)talismanInfo.ModelId);
                            Out.WriteByte(talis.Fused); // 0 fused 1 unfused
                            Out.WriteByte(0);
                            Out.WritePascalString(talismanInfo.Name);
                            Out.WriteByte((byte)talismanInfo._Stats.Count); // Valid 1.4.8
                            foreach (KeyValuePair<byte, ushort> Key in talismanInfo._Stats)
                            {
                                Out.WriteByte(Key.Key);
                                Out.WriteUInt16(Key.Value);
                                Out.WriteUInt32(talis.Timer);
                                Out.WriteByte(0);
                                //  Out.Fill(0, 5);
                            }
                            Out.WriteByte((byte)talismanInfo.EffectsList.Count);
                            foreach (ushort effect in talismanInfo.EffectsList)
                            {
                                Out.WriteUInt16(effect);
                                Out.WriteUInt32(0);
                            }
                            //if (talismanInfo.SpellId == 0)
                            //    Out.WriteByte(0);
                            //else
                            //{
                            //    Out.WriteByte(1); // (byte)talismanInfo._Spells.Count

                            //    Out.WriteUInt16(talismanInfo.SpellId);
                            //    Out.WriteUInt32(AbilityMgr.GetCooldownFor(talismanInfo.SpellId));
                            //}
                            Out.Fill(0, 3);
                            Out.WriteUInt16(0x041C);
                        }

                    }
                }

                Out.WritePascalString(info.Description);

                // Note from wash : this algorithm updates shared 

                byte[] Unks = info.Unk27;

                //if (info.Bind == 1 && (itm == null || !itm.BoundtoPlayer))
                //    Unks[5] = (byte)(4);  // bind on pickup, if set to true for one item, all items with have this flag active client side
                //else if (info.Bind == 2 && (itm == null || !itm.BoundtoPlayer)) //
                //    Unks[5] = (byte)(8);   // bind on equip, if set to true for one item, all items with have this flag active client side
                //else
                //    Unks[5] = 0;

                //Unks[5] = 8;

                if (info.DyeAble)
                    Unks[6] = (byte)(Unks[6] | 1); // dyeable
                if (info.Salvageable)
                    Unks[6] = (byte)(Unks[6] | 2); // scavangable
                                                   // Prevents sale
                                                   //   Unks[6] = (byte) (Unks[6] | 32);
                                                   // Allow Conversion text (Ctrl+Right Click)
                                                   //   Unks[6] = (byte) (Unks[6] | 128);


                //if (itm != null && itm.BoundtoPlayer) // info.Bind == 1
                //    Unks[8] = (byte)(1); // bound to player, if set to true for one item, all items with have this flag active client side
                //else
                //    Unks[8] = (byte)(0);


                Out.WriteByte(Unks[0]); // londo : wut ?
                Out.WriteByte(Unks[1]); // londo : getUnk7
                Out.WriteByte(Unks[2]); // londo : getUnk8
                Out.WriteByte(Unks[3]); // londo : getNoChargeLeftDontDelete
                Out.WriteByte(Unks[4]); // londo : flag count

                if (info.Bind == 1 && (itm == null || !itm.BoundtoPlayer))
                    Out.WriteByte(4);  // bind on pickup, if set to true for one item, all items with have this flag active client side
                else if (info.Bind == 2 && (itm == null || !itm.BoundtoPlayer)) //
                    Out.WriteByte(8);   // bind on equip, if set to true for one item, all items with have this flag active client side
                else
                    Out.WriteByte(0);

                Out.WriteByte(Unks[6]); // dyeable from londo (+ scavangable from RoR ?)
                Out.WriteByte(Unks[7]); // 0 from londo

                if (info.Bind == 2 && itm != null && itm.BoundtoPlayer)
                    Out.WriteByte(1);
                else
                    Out.WriteByte(0);

                //Unks[19] = (byte)(Unks[19] | 5); // potion cd

                // 4 can crash the client if changed
                // 5: if 1, hides the crafting level requirement. if 2, shows it.
                // 7 with value of 8 can suppress the Item Level text
                //for (int i = 0; i < 9; i++) //9
                //{
                //    Out.WriteByte(Unks[i]);
                //}

                if (lootbag)
                {
                    Out.WriteUInt16(0);
                    Out.WriteUInt16(0);
                }
                else
                {
                    if (mail != null)
                    {
                        Out.WriteUInt16(mail.primary_dye);
                        Out.WriteUInt16(mail.secondary_dye);
                    }
                    else
                    {
                        Out.WriteUInt16(itm?.GetPrimaryDye() ?? 0);
                        Out.WriteUInt16(itm?.GetSecondaryDye() ?? 0);
                    }
                }

                // Overwrite Unks27 with the TwoHanded flag
                if (info.TwoHanded)
                    Unks[26] = (byte)(Unks[26] | 1); // bitwise? oh well better to be safe.



                if ((info.SlotId == 14 || info.SlotId == 27) && Plr?.GldInterface.Guild != null)
                {
                    Out.Fill(0, 7);
                    Out.WriteByte(1); // 1, Out, 1, 2
                    Plr.GldInterface.Guild.BuildHeraldry(Out);
                    Out.WriteByte(1);
                    Out.WriteByte(1);
                    Out.Fill(0, 6);
                }
                else
                {
                    // 14: Skill level 
                    // 15: Flags for Cultivating
                    // 20: Crashes the client if nonzero... is set on the Fleet Stag Mantle
                    // 21-24: Seconds until decayed
                    // 26: two-handed flag
                    for (int i = 13; i < /*21*/ 27; i++)
                        Out.WriteByte(Unks[i]);

                    /*
                    21-26:
                    Out.WriteUInt32(0); // Seconds until decayed. Set on the Stag Mantle but doesn't show, possible double meaning based on value 20

                    Out.WriteByte(Unks[25]);
                    Out.WriteByte(Unks[26]);
                    */
                }


                //Out.Write(Unks);

                /*Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(0);

                Out.WriteUInt16(0x0302);

                Out.Fill(0, 8);
                Out.WriteByte(0); // Type , Culture, etc etc
                Out.WriteByte(0); // Type, Recipian , Soil , etc etc
                Out.Fill(0, 11);*/
           
        }

        public static void BuildRepairableItem(ref PacketOut Out, Item itm, Item_Info info, MailItem mail, ushort SlotId, ushort Count, Player Plr = null)
        {
            Out.WriteByte(1);    // repairable item
            Out.WritePascalString(info.Name);


            string[] items = info.Craftresult.Split(';');
            Item_Info RepItemInfo = null;

            uint itemlvl=0;
            byte rarety=0;

            foreach(string ritem in items)
            {
                
                Item_Info RitemInfo = ItemService.GetItem_Info(uint.Parse(ritem));
                rarety = RitemInfo.Rarity;
                itemlvl = RitemInfo.MinRank;

                if (ItemsInterface.CanUse(RitemInfo, Plr,false,false))
                {
                    RepItemInfo = RitemInfo;
                    break;
                }
            }
            if(rarety < 1)
                rarety = 1;
            if (itemlvl < 1)
                itemlvl = 1;

            if (RepItemInfo != null)
            {

                Out.WritePacketString(@"|00 00 0F 67|......b.L...]...|"); //Icon

                Out.WriteUInt32(20 * itemlvl * 6 * rarety * 2);  // repair costs
                Out.WritePacketString(@"|00 00 07 62|......b.L...]...|"); //?? 00 00 07 62

                BuildItem(ref Out,null, RepItemInfo,null,0,1,Plr,true);
            }
            else
            {
                Out.WritePacketString(@" | 00 00 0F 67 |........|");
                Out.WriteUInt32(20 * itemlvl * 6 * rarety * 2);  // repair costs

                Out.WritePacketString(@"|00 00 07 62 | apon...g.......b |
                                        | 00 4C B0 A5 0F 67 00 00 00 00 00 00 00 00 00 00 |.L...g..........|
                                        | 00 24 00 00 00 02 00 00 00 00 00 00 00 00 00 00 |.$..............|
                                        | 00 00 02 1C 00 01 00 01 00 00 00 00 00 00 00 00 |................|
                                        | 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 |................|
                                        | 03 42 00 00|........|");
                if (info.Bind == 1)
                    Out.WriteByte(4);  // bind on pickup, if set to true for one item, all items with have this flag active client side
                else if (info.Bind == 2) //
                    Out.WriteByte(8);   // bind on equip, if set to true for one item, all items with have this flag active client side
                else
                    Out.WriteByte(0);
                Out.WritePacketString(@" | 00 00 00 00 00 00 00 00 00 00 00 |.B..............|
                                        | 00 00 00 00 00 00 00|.B..............|");
            }
        }

            public bool CanBeUsedBy(Player player)
        {
            // Career restrictions.
            if (Info.Career != 0 && (Info.Career & (1 << player.Info.CareerLine - 1)) == 0)
            {
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEST_ITEM_PLAYER_INVALID_CAREER);
                return false;
            }

            // Race restrictions. 
            if (Info.Race != 0 && (Info.Race & (1 << player.Info.Race - 1)) == 0)
            {
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEST_ITEM_PLAYER_INVALID_RACE);
                return false;
            }

            if (Info.MinRank > 0 && player.AdjustedLevel < Info.MinRank)
            {
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEST_ITEM_PLAYER_LEVEL_TOO_LOW);
                return false;
            }

            if (Info.MinRenown > 0 && player.RenownRank < Info.MinRenown)
            {
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEST_ITEM_PLAYER_RENOWN_TOO_LOW);
                return false;
            }

            return true;
        }

        #region Accessor

        public ushort SlotId
        {
            get
            {
                return CharSaveInfo?.SlotId ?? _SlotId;
            }
            set
            {
                if (CharSaveInfo != null)
                    CharSaveInfo.SlotId = value;
                else
                    _SlotId = value;
            }
        }

        public ushort Count
        {
            get
            {
                return CharSaveInfo?.Counts ?? _Count;
            }
            set
            {
                if (CharSaveInfo != null)
                    CharSaveInfo.Counts = value;
                else
                    _Count = value;
            }
        }

        public uint ModelId
        {
            get
            {
                if (PreviewModelID == 0)
                    return Info?.ModelId ?? _ModelId;
                else
                    return PreviewModelID;
            }
            set { _ModelId = value; }
        }

        public bool BoundtoPlayer
        {
            get
            {
                return CharSaveInfo != null && CharSaveInfo.BoundtoPlayer;
            }
            set
            {
                if (CharSaveInfo != null)
                    CharSaveInfo.BoundtoPlayer = value;
            }
        }
        public uint AltAppearanceEntry
        {
            get
            {
                return CharSaveInfo?.Alternate_AppereanceEntry ?? 0;
            }
            set
            {
                if (CharSaveInfo != null)
                    CharSaveInfo.Alternate_AppereanceEntry = value;
            }
        }

        public uint PreviewModelID
        {
            get
            {
                return previewModelID;
            }

            set
            {
                previewModelID = value;
            }
        }



        #endregion

        public byte GetInventoryFlags()
        {
            byte flags = 0;

            if (!_isCreature)
            {
                // Trophy
                if (SlotId > 14 && SlotId < 20)
                {
                    flags |= (byte) ItemFlags.Trophy;
                    return flags;
                }

                if (SlotId == (int) EquipSlot.STANDARD)
                {
                    flags |= (byte) ItemFlags.Heraldry;
                    return flags;
                }

                if (SlotId == (int) EquipSlot.BACK)
                {
                    Player player = Owner as Player;

                    if (player?.GldInterface.Guild != null)
                    {
                        flags |= (byte) ItemFlags.Heraldry;
                        return flags;
                    }
                }
            }

            ushort priColor = GetPrimaryColor();
            ushort secColor = GetSecondaryColor();

            if (priColor > 0 || secColor > 0)
                flags |= (byte)ItemFlags.ColorOverride;
            if (priColor > 0xFF)
                flags |= (byte)ItemFlags.PriColorExpansion;
            if (secColor > 0xFF)
                flags |= (byte)ItemFlags.SecColorExpansion;

            return flags;
        }

        public ushort GetModel()
        {
            if (_isCreature)
                return (ushort)_ModelId;

            return (ushort)(AltAppearanceEntry > 0 ? (ushort)ItemService.GetItem_Info(AltAppearanceEntry).ModelId : ModelId);
        }

        public ushort GetPrimaryColor()
        {
            if (_isCreature)
                return _PrimaryColor;

            ushort priColor = GetPrimaryDye();

            if (priColor == 0 && Info != null)
                priColor = Info.BaseColor1;

            return priColor;
        }

        public ushort GetSecondaryColor()
        {
            if (_isCreature)
                return _SecondaryColor;

            ushort secColor = GetSecondaryDye();

            if (secColor == 0 && Info != null)
                secColor = Info.BaseColor2;

            return secColor;
        }
    }

    /*
     * public struct ItemSpells
{
    public UInt32 entry;
    public UInt16 X,Y;
}

public struct ItemArtisana
{
    public byte Unk1,Unk2,Unk3;
}

public struct ItemStat
{
    public byte Type;
    public UInt16 Count;
}

public struct ItemTalisman
{
    public UInt32 entry;
    public UInt16 Unk;
    public string Name;
}
     * 
     * static public ItemInformation DecodeItem(PacketIn Packet)
    {
        ItemInformation info = new ItemInformation();

        Packet.GetUint16(); // SlotId
        Packet.GetUint8();

        info.entry = Packet.GetUint32();

        if (info.entry == 0)
            return info;

        info.ModelId = Packet.GetUint16();
        info.UnknownId = Packet.GetUint16();
        info.UnknownEntry = Packet.GetUint32();
        info.UnknownText = Packet.GetPascalString();

        info.SlotId = Packet.GetUint16();
        info.Type = Packet.GetUint8();
        info.MinRank = Packet.GetUint8();
        Packet.GetUint8();
        info.MinRenown = Packet.GetUint8();
        Packet.Skip(2);
        info.Rarity = Packet.GetUint8();
        info.Bind = Packet.GetUint8();
        info.Race = Packet.GetUint8();
        info.Career = Packet.GetUint32();

        UInt32 ObjectPrice = Packet.GetUint32(); /// ?
        if (ObjectPrice != 0)
            Packet.Skip(2);

        info.Price = Packet.GetUint32();
        Packet.GetUint32(); // Count / MaxCount
        Packet.GetUint32(); // ?
        info.Skill = Packet.GetUint32();
        info.Dps = Packet.GetUint16();
        info.Speed = Packet.GetUint16();
        info.Name = Packet.GetPascalString();

        int TempCount = Packet.GetUint8();
        info.Stats = new List<ItemStat>(TempCount);
        for (int i = 0; i < TempCount; ++i)
        {
            ItemStat Stat = new ItemStat();
            Stat.Type = Packet.GetUint8();
            Stat.Count = Packet.GetUint16();
            Packet.Skip(5);
            info.Stats.Add(Stat);
        }

        TempCount = Packet.GetUint8(); // Equip Effects
        for (int i = 0; i < TempCount; ++i)
        {
            Packet.GetUint16(); // Effect Id
            Packet.GetUint32(); // ?
            Packet.GetUint32(); // ?

            string Txt = Packet.GetPascalString();
            if (Txt.Length == 0)
                Packet.Skip(3);
        }

        TempCount = Packet.GetUint8();
        info.Spells = new List<ItemSpells>(TempCount);
        for (int i = 0; i < TempCount; ++i)
        {
            ItemSpells Spell = new ItemSpells();
            Spell.entry = Packet.GetUint32();
            Spell.X = Packet.GetUint16();
            Spell.Y = Packet.GetUint16();
            info.Spells.Add(Spell);
            
        }

        TempCount = Packet.GetUint8();
        info.Artisanas = new List<ItemArtisana>(TempCount);
        for (int i = 0; i < TempCount; ++i)
        {
            ItemArtisana Art = new ItemArtisana();
            Art.Unk1 = Packet.GetUint8();
            Art.Unk2 = Packet.GetUint8();
            Art.Unk3 = Packet.GetUint8();
            info.Artisanas.Add(Art);
        }

        Packet.GetUint8();

        TempCount = Packet.GetUint8();
        info.TalismanSlots = new List<ItemTalisman>(TempCount);
        for (int i = 0; i < TempCount; ++i)
        {
            ItemTalisman Talisman = new ItemTalisman();
            Talisman.entry = Packet.GetUint32();
            if (Talisman.entry != 0)
            {
                Talisman.Unk = Packet.GetUint16();
                Talisman.Name = Packet.GetPascalString();
                Packet.Skip(15);
            }
            else
                Talisman.Name = "";

            info.TalismanSlots.Add(Talisman);
        }

        info.Description = Packet.GetPascalString();

        Packet.Skip(27); // Culture, recipian , soil, etc...
        return info;
    }*/
}
