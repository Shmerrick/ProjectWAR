using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class CraftingTalInterface : BaseInterface
    {
        Player _myPlayer;
        byte _stage;
        ushort _container;
        ushort _slot1;
        ushort _slot2;
        ushort _slot3;
        ushort _slot4;
        byte _power;
        byte _duration;
        byte _multiplier;
        byte _critChance;
        bool locked;

        public override bool Load()
        {
            SetBasePower();
            _myPlayer.SendTradeSkill(_myPlayer._Value.CraftingSkill, _myPlayer._Value.CraftingSkillLevel);

            return base.Load();
        }

        public override void SetOwner(Object owner)
        {
            _Owner = owner;
            _myPlayer = owner as Player;
        }

        // The item each slot held when it was added; see CraftingApoInterface._added (BUG-196).
        private readonly Dictionary<ushort, Item> _added = new Dictionary<ushort, Item>();

        /// <summary>Runs a step under the lock and always releases it; errors reset the craft.</summary>
        private void RunLocked(Action step)
        {
            if (locked)
                return;
            locked = true;
            try
            {
                step();
            }
            catch (Exception e)
            {
                Log.Error("CraftingTalInterface", e.ToString());
                Reset();
            }
            finally
            {
                locked = false;
            }
        }

        /// <summary>The bonuses the item added to <paramref name="slot"/> contributed, whatever the slot holds now.</summary>
        private string[] AddedCrafts(ushort slot)
        {
            Item added;
            if (_added.TryGetValue(slot, out added) && added?.Info?.Crafts != null)
                return added.Info.Crafts.Split(';');
            return new string[0];
        }

        public void AddContainer(ushort slot)
        {
            RunLocked(() => AddContainerImpl(slot));
        }

        private void AddContainerImpl(ushort slot)
        {
            Item added = _myPlayer.ItmInterface.IsBagSlot(slot) ? _myPlayer.ItmInterface.GetItemInSlot(slot) : null;
            Item_Info itm = added?.Info;

            if (itm == null || itm.Crafts == null || itm.Unk27 == null)
                return;
            if (itm.Unk27.Length == 0 || ((itm.Unk27[15] == 1 || itm.Unk27[15] == 5) || ((GetCraft(8,itm.Crafts) == 14) && (itm.Craftresult == null || itm.Craftresult.Length < 2))))
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_CULTIVATION_BAD_SEED_DATA);
                Crafting();
                locked = false;
                return;
            }
            if (itm.Unk27[14] > _myPlayer._Value.CraftingSkillLevel)
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_CRAFT_SKILL_TOO_LOW);
                Crafting();
                locked = false;
                return;
            }
            if (_stage == 0 && (GetCraft(8, itm.Crafts) != 13 && GetCraft(8, itm.Crafts) != 5))
            {
                locked = false;
                return;
            }
            if (_stage == 1 && !(GetCraft(8, itm.Crafts) == 2 || GetCraft(8, itm.Crafts) == 8 || GetCraft(8, itm.Crafts) == 14))
            {
                locked = false;
                return;
            }
            
       
      
            switch(_stage)
            {
                case 0:
                    _container = slot;
                    _stage = 1;               
                    break;
                case 1:
                    _slot1 = slot;
                    if (_myPlayer._Value.CraftingSkill == 5 && (_slot2 == 0 || _slot3 == 0 || _slot4 == 0))
                    {
                        _stage = 2;
                    }
                    else
                        _stage = 3;               
                    break;
                case 2:
                        if (GetCraft(8, itm.Crafts) == 14)
                        {
                            if (_slot1 == 0)
                                _slot1 = slot;
                            else
                            {
                                RemoveBonuses(AddedCrafts(_slot1));
                                _slot1 = slot;
                            }
                        }

                        if (GetCraft(8, itm.Crafts) == 15)
                        {
                            if (_slot2 == 0)
                                _slot2 = slot;
                            else
                            {
                                RemoveBonuses(AddedCrafts(_slot2));
                                _slot2 = slot;
                            }
                        }

                        if (GetCraft(8, itm.Crafts) == 16)
                            if (_slot3 == 0)
                                _slot3 = slot;
                            else
                            {
                                RemoveBonuses(AddedCrafts(_slot3));
                                _slot3 = slot;
                            }

                        if (GetCraft(8, itm.Crafts) == 17)
                            if (_slot4 == 0)
                                _slot4 = slot;
                            else
                            {
                                RemoveBonuses(AddedCrafts(_slot4));
                                _slot4 = slot;
                            }
                        if (_slot2 != 0 && _slot3 != 0 && _slot4 != 0)
                            _stage = 3;
                    
                    break;
                case 3:
                        if (GetCraft(8, itm.Crafts) == 13)
                        {
                            if (_container == 0)
                                _container = slot;
                            else
                            {
                                RemoveBonuses(AddedCrafts(_container));
                                _container = slot;
                            }
                        }
                        if (GetCraft(8, itm.Crafts) == 14)
                        {
                            if (_slot1 == 0)
                                _slot1 = slot;
                            else
                            {
                                RemoveBonuses(AddedCrafts(_slot1));
                                _slot1 = slot;
                            }
                        }

                        if (GetCraft(8, itm.Crafts) == 15)
                        {
                            if (_slot2 == 0)
                                _slot2 = slot;
                            else
                            {
                                RemoveBonuses(AddedCrafts(_slot2));
                                _slot2 = slot;
                            }
                        }

                        if (GetCraft(8, itm.Crafts) == 16)
                            if (_slot3 == 0)
                                _slot3 = slot;
                            else
                            {
                                RemoveBonuses(AddedCrafts(_slot3));
                                _slot3 = slot;
                            }

                        if (GetCraft(8, itm.Crafts) == 17)
                            if (_slot4 == 0)
                                _slot4 = slot;
                            else
                            {
                                RemoveBonuses(AddedCrafts(_slot4));
                                _slot4 = slot;
                            }
                        if (_slot2 != 0 && _slot3 != 0 && _slot4 != 0)
                            _stage = 3;
                    break;

            }

            _added[slot] = added;
            AddBonuses(itm.Crafts.Split(';'));
            Crafting();
            locked = false;
         //   Log.Success("Power ", "" + _power);
        }

        public void RemoveContainer(ushort slot,Boolean consumeItem = false)
        {
            // consumeItem is Craft's own call, made while it already holds the lock.
            if (consumeItem)
                RemoveContainerImpl(slot, true);
            else
                RunLocked(() => RemoveContainerImpl(slot, false));
        }

        private void RemoveContainerImpl(ushort slot, Boolean consumeItem)
        {
            if (slot == 0)
                return;

           if (_container == slot)
           {
                if (consumeItem) 
                    _myPlayer.ItmInterface.DeleteItem(slot, 1);
                Reset();
                locked = false;
                return;
           } 
           else if(_slot1 == slot)
           {
               _stage = 1;
               _slot1=0;
           }
           else if (_slot2 == slot)
           {
               _slot2 = 0;
           }
           else if (_slot3 == slot)
           {
               _slot3 = 0;
           }
           else if (_slot4 == slot)
           {
               _slot4 = 0;
           }
           else
           {
                locked = false;
                return;
           }
            RemoveBonuses(AddedCrafts(slot));

            if(_myPlayer._Value.CraftingSkill == 5)
              if (_slot2 == 0 || _slot3 == 0 || _slot4 == 0)
                _stage = 2;


            if (consumeItem)
                _myPlayer.ItmInterface.DeleteItem(slot, 1);

                Crafting();
            if(!consumeItem)
            locked = false;
            //  Log.Success("Power ", "" + _power);
        }

        public void AddBonuses(String[] bonuses)
        {
           foreach(String st in bonuses)
           {
               if (st.Length>0)
               switch((UInt16.Parse(st.Split(':')[0])))
               {
                   case 2: _power += (byte)(UInt16.Parse(st.Split(':')[1])); break;
                   case 3: _duration += (byte)(UInt16.Parse(st.Split(':')[1])); break;
                   case 4: _multiplier += (byte)(UInt16.Parse(st.Split(':')[1])); break;
                   case 10: break; // time
                  // case 12: Pots.ElementAt(Pot).Crittchance += (byte)(UInt16.Parse(st.Split(':')[1])); break;
                  // case 13: Pots.ElementAt(Pot).Failchance -= (byte)(UInt16.Parse(st.Split(':')[1])); break;
                   case 14: _critChance += (byte)(UInt16.Parse(st.Split(':')[1])); break;
                 //  case 15: Power += (byte)(UInt16.Parse(st.Split(':')[1])); break;
               }
           }

        }

        public void RemoveBonuses(String[] bonuses)
        {
            foreach (String st in bonuses)
            {
                if (st.Length > 0)
                    switch ((UInt16.Parse(st.Split(':')[0])))
                    {
                        case 2: _power -= (byte)(UInt16.Parse(st.Split(':')[1])); break;
                        case 3: _duration -= (byte)(UInt16.Parse(st.Split(':')[1])); break;
                        case 4: _multiplier -= (byte)(UInt16.Parse(st.Split(':')[1])); break;
                        case 10: break; // time
                        // case 12: Pots.ElementAt(Pot).Crittchance += (byte)(UInt16.Parse(st.Split(':')[1])); break;
                        // case 13: Pots.ElementAt(Pot).Failchance -= (byte)(UInt16.Parse(st.Split(':')[1])); break;
                        case 14: _critChance -= (byte)(UInt16.Parse(st.Split(':')[1])); break;
                        //  case 15: Power += (byte)(UInt16.Parse(st.Split(':')[1])); break;
                    }
            }
        }

        public static byte GetCraft(byte id, String craft)
        {
            String[] crafts = craft.Split(';');

            foreach (String st in crafts)
            {
                if (st.Length > 0 && UInt16.Parse(st.Split(':')[0]) == id)
                    return (byte)UInt16.Parse(st.Split(':')[1]);
            }
            return 0;
        }

        public void Reset()
        {
            _stage = 0;
            _container = 0;
            _slot1 = 0;
            _slot2 = 0;
            _slot3 = 0;
            _slot4 = 0;
            _power = 0;
            _duration = 0;
            _multiplier = 0;
            _critChance = 0;
            _added.Clear();
            Crafting();
        }

        public void Craft()
        {
            RunLocked(CraftImpl);
        }

        private void CraftImpl()
        {
            if (_container == 0 || _slot1 == 0 || _slot2 == 0 || _slot3 == 0 || _slot4 == 0)
                return;

            // Consumption is by slot, so every slot must still hold the item whose bonuses were applied.
            foreach (ushort slot in new[] { _container, _slot1, _slot2, _slot3, _slot4 })
            {
                Item added;
                if (!_added.TryGetValue(slot, out added) || added == null || _myPlayer.ItmInterface.GetItemInSlot(slot) != added)
                {
                    _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_MISMATCH_TYPE);
                    Reset();
                    return;
                }
            }

            byte rarety = _myPlayer.ItmInterface.GetItemInSlot(_slot1).Info.Rarity;
            uint itemid = UInt32.Parse(_myPlayer.ItmInterface.GetItemInSlot(_slot1).Info.Craftresult.Split(';')[0]);
            Boolean purplespecialcritt = false;
            Boolean sppecialmoment = false;
            Boolean critt = false;

            float rand = ((float)StaticRandom.Instance.NextDouble() * 100f);

            if (rand <= 3)
            {
                sppecialmoment = true;
                if (rarety < 4)
                    rarety++;
                else
                    purplespecialcritt = true;

            }
            else if (rand <= 10)
            {
                _power += 6;
                critt = true;
            }

            byte powerlvl = 0;


            if (_power >= 50)
            {
                powerlvl = 10;
            }
            else if (_power >= 45)
            {
                powerlvl = 9;
            }
            else if (_power >= 39)
            {
                powerlvl = 8;
            }
            else if (_power >= 34)
            {
                powerlvl = 7;
            }
            else if (_power >= 29)
            {
                powerlvl = 6;
            }
            else if (_power >= 24)
            {
                powerlvl = 5;
            }
            else if (_power >= 19)
            {
                powerlvl = 4;
            }
            else if (_power >= 14)
            {
                powerlvl = 3;
            }
            else if (_power >= 9)
            {
                powerlvl = 2;
            }
            else
            { 
                powerlvl = 1;
            }

            itemid += (uint)((rarety - 1) *10);
                itemid += (uint)(powerlvl - 1);



            // The critical-success bonus applied only to this roll.
            if (critt)
                _power -= 6;

            // Deliver the talisman before anything is consumed. CreateItem's result was ignored, so a
            // full backpack used up every ingredient for nothing, and a result id naming no item
            // threw on its name with the ingredients still committed.
            Item_Info product = ItemService.GetItem_Info(itemid);
            if (product == null)
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_INVALID_PRODUCT);
                return;
            }
            if (_myPlayer.ItmInterface.CreateItem(product, 1) != ItemResult.RESULT_OK)
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_NO_ROOM_IN_BACKPACK);
                return;
            }

            if (sppecialmoment)
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_SPECIALMOMENT);
            else if (critt)
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_CRITICALSUCCESS);
            _myPlayer.SendLocalizeString(product.Name, ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_YOU_CREATED);


                if (_myPlayer._Value.CraftingSkillLevel < 200 && (((_myPlayer._Value.CraftingSkillLevel - _myPlayer.ItmInterface.GetItemInSlot(_slot1).Info.Unk27[14]) * 3) < ((float)StaticRandom.Instance.NextDouble() * 100f)))
                {
                    _myPlayer._Value.CraftingSkillLevel++;
                    SetBasePower();
                    _myPlayer.SendTradeSkill(_myPlayer._Value.CraftingSkill, _myPlayer._Value.CraftingSkillLevel);
                }
                    


                if (_myPlayer.ItmInterface.GetItemInSlot(_slot4).Count > 1)
                    _myPlayer.ItmInterface.DeleteItem(_slot4, 1);
                else
                {
                /*
                    List<ushort> newslots = _myPlayer.ItmInterface.GetStackItem(_myPlayer.ItmInterface.GetItemInSlot(_slot4).Info);
                    if (newslots.Count > 1)
                    {
                        foreach(ushort slot in newslots)
                        {
                            if(slot != _slot4)
                            {
                                _myPlayer.ItmInterface.DeleteItem(_slot4, 1);
                            Log.Info("", "oldslot " + _slot4 + "  newslot " + slot);
                                _slot4 = slot;

                            }
                        }
                    }
                    else
                    */
                    RemoveContainer(_slot4,true);
                }
                if (_myPlayer.ItmInterface.GetItemInSlot(_slot3).Count > 1)
                    _myPlayer.ItmInterface.DeleteItem(_slot3, 1);
                else
                {
                    RemoveContainer(_slot3,true);
                }
                if (_myPlayer.ItmInterface.GetItemInSlot(_slot2).Count > 1)
                    _myPlayer.ItmInterface.DeleteItem(_slot2, 1);
                else
                {
                    RemoveContainer(_slot2,true);
                }

                if (purplespecialcritt)
                {
                    purplespecialcritt = false;
                }
                else
                    {
                    if (_myPlayer.ItmInterface.GetItemInSlot(_slot1).Count > 1)
                        _myPlayer.ItmInterface.DeleteItem(_slot1, 1);
                    else
                    {
                        RemoveContainer(_slot1,true);
                    }
                }

                if (_myPlayer.ItmInterface.GetItemInSlot(_container).Count > 1)
                    _myPlayer.ItmInterface.DeleteItem(_container, 1);
                else 
                {
                    RemoveContainer(_container,true);
                }

            
            Crafting();

            locked = false;
        }

        public void SetBasePower()
        {
        }

        public void Crafting()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CRAFTING_STATUS, 32);
             Out.WriteByte(_myPlayer._Value.CraftingSkill);
             Out.WriteByte(0);
             Out.WriteByte(0);
             Out.WriteByte(_myPlayer._Value.CraftingSkillLevel);
             Out.WriteByte(_stage);  //_stage
             Out.WriteByte(0);
             Out.WriteUInt16(_container);
            if (_myPlayer._Value.CraftingSkill == 4)
            {
                Out.WriteUInt16(_slot1);
                Out.WriteUInt16(_slot2);
                Out.WriteUInt16(_slot3);
                Out.WriteUInt16(_slot4);
            }else
            {
                byte nuller = 0;
                if (_slot1 == 0)
                    nuller++;
                else
                    Out.WriteUInt16(_slot1);
                if (_slot2 == 0)
                    nuller++;
                else
                    Out.WriteUInt16(_slot2);
                if (_slot3 == 0)
                    nuller++;
                else
                    Out.WriteUInt16(_slot3);
                if (_slot4 == 0)
                    nuller++;
                else
                    Out.WriteUInt16(_slot4);

                for(byte x = 0;x<nuller ; x++)
                {
                    Out.WriteUInt16(0);
                }

            }

            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);  
             Out.WriteByte(0);
             Out.WriteByte(0);
             Out.WriteByte(_power);          // talis 0-49
             Out.WriteByte(0);
             _myPlayer.SendPacket(Out);
        }
    }
}
