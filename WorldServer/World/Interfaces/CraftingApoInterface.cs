using System;
using System.Collections.Generic;
using SystemData;
using FrameWork;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class CraftingApoInterface : BaseInterface
    {
        Player _myPlayer;
        byte _stage;
        ushort _container;
        ushort _mainIngredient;
        ushort _slot2;
        ushort _slot3;
        ushort _slot4;
        bool _dyecrafting;
        short _stability;
        byte _power;
        byte _duration;
        byte _multiplier;
        byte _critChance;

        byte _baseStability;
        byte _basePower;
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

        // Which item each slot held when it was added. Craft() consumes slots, not items, so an item
        // swapped into a slot after its bonuses were applied was consumed in its place.
        private readonly Dictionary<ushort, Item> _added = new Dictionary<ushort, Item>();

        /// <summary>
        /// Runs a crafting step under the interface lock and always releases it. Several paths
        /// (an empty slot, Craft's power check, any exception) returned or threw with the lock
        /// still held, which disabled crafting for the player until relog.
        /// </summary>
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
                Log.Error("CraftingApoInterface", e.ToString());
                Reset(0);
            }
            finally
            {
                locked = false;
            }
        }

        public void AddContainer(ushort slot)
        {
            RunLocked(() => AddContainerImpl(slot));
        }

         private void AddContainerImpl(ushort slot)
         {
            Item Itm = _myPlayer.ItmInterface.IsBagSlot(slot) ? _myPlayer.ItmInterface.GetItemInSlot(slot) : null;

            if (Itm == null || Itm.Info.Crafts == null)
                return;

            byte crafttype = GetCraft(8, Itm.Info.Crafts);

            try
            {
                if (crafttype == 2 && Itm.Info.Craftresult.Length < 2)
                {
                    _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CULTIVATION_BAD_SEED_DATA);
                    Crafting();
                    locked = false;
                    return;
                }
            }
            catch
            {
                locked = false;
                return;
            }

            if (GetCraft(9,Itm.Info.Crafts) > _myPlayer._Value.CraftingSkillLevel)
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_SKILL_TOO_LOW);
                Crafting();
                locked = false;
                return;
            }
            if (_stage == 0 && (!(crafttype == 6) && !(crafttype == 5)))     // containers
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_CONTAINER_FIRST);
                locked = false;
                return;
            }
            if (_stage != 0 && ((crafttype == 6) || (crafttype == 5)))     // containers
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_ALREADY_HAS_CONTAINER);
                locked = false;
                return;
            }
            if (_stage == 1 && !(crafttype == 2 || crafttype == 8))
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_MAIN_INGREDIENT_FIRST);
                locked = false;
                return;
            }
            if (_stage != 1 && (crafttype == 2 || crafttype == 8 || crafttype == 14))
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_ALREADY_HAS_MAIN_INGREDIENT);
                locked = false;
                return;
            }
            if (_dyecrafting && !(crafttype == 8 || crafttype == 9))
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_MISMATCH_TYPE);
                Crafting();
                locked = false;
                return;
            }
            if (!_dyecrafting && (crafttype == 8 || crafttype == 9))
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_MISMATCH_TYPE);
                Crafting();
                locked = false;
                return;
            }
            if (!_dyecrafting && (_slot2 != 0 && _slot3 != 0 && _slot4!=0))
            {
                Crafting();
                locked = false;
                return;
            }
            if (crafttype == 6)
            {
                _dyecrafting = true;
            }


            // One stack may fill several roles only if it holds one item per role. The container and
            // main ingredient were not counted, so a single item could be both main and additive.
            if (slot == _container || slot == _mainIngredient || slot == _slot2 || slot == _slot3 || slot == _slot4)
            {
                byte count = 1;
                if (slot == _container)
                    count++;
                if (slot == _mainIngredient)
                    count++;
                if (slot == _slot2)
                    count++;
                if (slot == _slot3)
                    count++;
                if (slot == _slot4)
                    count++;

                if(count > Itm.Count)
                {
                    Crafting();
                    locked = false;
                    return;
                }
            }
            switch(_stage)
            {
                case 0:
                    _container = slot;
                    _stage = 1;               
                    break;
                case 1:
                    _mainIngredient = slot;
                    if (_dyecrafting)
                        _stage = 2;
                    else
                        _stage = 3;
                    break;
                case 2:
                        if (_slot2 == 0)
                        {
                            _slot2 = slot;
                            _stage = 3;
                        }
                    break;
                   
                case 3:
                        if (_dyecrafting)
                            break;
                    if (_slot2 == 0)
                        _slot2 = slot;
                    else if (_slot3 == 0)
                        _slot3 = slot;
                    else if (_slot4 == 0)
                        _slot4 = slot;
                break;
            }

            _added[slot] = Itm;
            AddBonuses(Itm.Info.Crafts.Split(';'));
            Crafting();
            locked = false;
             //_Owner.GetPlayer().SendMessage(_Owner.Oid,"Power ", "" + (_power + _basePower) + "  stabi  " + (_stability+ _baseStability) + "  crittchance "+_critChance+"  duration  "+_duration+"  multi "+_multiplier ,SystemData.ChatLogFilters.CHATLOGFILTERS_SHOUT);
        }

        public void RemoveContainer(ushort slot,bool consumeItem = false,byte count = 1)
        {
            // consumeItem is Craft's own call, made while it already holds the lock.
            if (consumeItem)
                RemoveContainerImpl(slot, true, count);
            else
                RunLocked(() => RemoveContainerImpl(slot, false, count));
        }

        private void RemoveContainerImpl(ushort slot, bool consumeItem, byte count)
        {
            // Only a slot in this craft carries bonuses to remove. Any other slot's item used to have
            // its bonuses subtracted anyway, so "removing" a negative-stability item raised stability.
            if (slot == 0 || (slot != _container && slot != _mainIngredient && slot != _slot2 && slot != _slot3 && slot != _slot4))
                return;

           if(_container == slot)
           {
                if (consumeItem)
                    _myPlayer.ItmInterface.DeleteItem(slot, 1);
                Reset(0);
                return;
           }
           if(_mainIngredient == slot)
           {
                // These were RemoveContainer calls, which returned at once because the lock was
                // held, so the additives' bonuses outlived the main ingredient they belonged to.
                // Clear the main slot first: a stack serving as main and additive would otherwise
                // re-enter this branch from the additive's removal without end.
                _stage = 1;
                _mainIngredient = 0;
                RemoveContainerImpl(_slot2, false, 1);
                RemoveContainerImpl(_slot3, false, 1);
                RemoveContainerImpl(_slot4, false, 1);
           }
           if (_slot2 == slot)
           {
               if (_dyecrafting)
                   _stage = 2;
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
            // Remove the bonuses that were added, from the item recorded at the time; the slot may
            // now hold something else, or nothing (which threw here with the lock held).
            Item added;
            if (_added.TryGetValue(slot, out added) && added?.Info?.Crafts != null)
                RemoveBonuses(added.Info.Crafts.Split(';'));

            if (consumeItem)
                _myPlayer.ItmInterface.DeleteItem(slot, count);

                Crafting();
            //_Owner.GetPlayer().SendMessage(_Owner.Oid, "Power ", "" + (_power + _basePower) + "  stabi  " + (_stability+ _baseStability) + "  crittchance " + _critChance + "  duration  " + _duration + "  multi " + _multiplier, SystemData.ChatLogFilters.CHATLOGFILTERS_SHOUT);
            //Log.Success("Stage: "+ _stage +"   Power ", "" + (_power+_basePower) + "  stabi  " + _stability);
        }

        public void AddBonuses(string[] bonuses)
        {
           foreach(string st in bonuses)
           {
               if (st.Length>0)
               switch((ushort.Parse(st.Split(':')[0])))
               {
                   case 1: _stability += (short)(ushort.Parse(st.Split(':')[1])); break;
                   case 2: _power += (byte)(ushort.Parse(st.Split(':')[1])); break;
                   case 3: _duration += (byte)(ushort.Parse(st.Split(':')[1])); break;
                   case 4: _multiplier += (byte)(ushort.Parse(st.Split(':')[1])); break;
                   case 10: break; // time
                  // case 12: Pots.ElementAt(Pot).Crittchance += (byte)(UInt16.Parse(st.Split(':')[1])); break;
                  // case 13: Pots.ElementAt(Pot).Failchance -= (byte)(UInt16.Parse(st.Split(':')[1])); break;
                   case 14: _critChance += (byte)(ushort.Parse(st.Split(':')[1])); break;
                 //  case 15: Power += (byte)(UInt16.Parse(st.Split(':')[1])); break;
               }
           }

        }

        public void RemoveBonuses(string[] bonuses)
        {
            foreach (string st in bonuses)
            {
                if (st.Length > 0)
                    switch ((ushort.Parse(st.Split(':')[0])))
                    {
                        case 1: _stability -= (short)(ushort.Parse(st.Split(':')[1])); break;
                        case 2: _power -= (byte)(ushort.Parse(st.Split(':')[1])); break;
                        case 3: _duration -= (byte)(ushort.Parse(st.Split(':')[1])); break;
                        case 4: _multiplier -= (byte)(ushort.Parse(st.Split(':')[1])); break;
                        case 10: break; // time
                        // case 12: Pots.ElementAt(Pot).Crittchance += (byte)(UInt16.Parse(st.Split(':')[1])); break;
                        // case 13: Pots.ElementAt(Pot).Failchance -= (byte)(UInt16.Parse(st.Split(':')[1])); break;
                        case 14: _critChance -= (byte)(ushort.Parse(st.Split(':')[1])); break;
                        //  case 15: Power += (byte)(UInt16.Parse(st.Split(':')[1])); break;
                    }
            }
        }

        public static byte GetCraft(byte id, string craft)
        {
            string[] crafts = craft.Split(';');

            foreach (string st in crafts)
            {
                if (st.Length > 0 && ushort.Parse(st.Split(':')[0]) == id)
                    return (byte)ushort.Parse(st.Split(':')[1]);
            }
            return 0;
        }

        public void Reset(byte pot)
        {
            _stage = 0;
            _container = 0;
            _mainIngredient = 0;
            _slot2 = 0;
            _slot3 = 0;
            _slot4 = 0;
            _dyecrafting = false;
            _stability = 0;
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

        /// <summary>Every slot in the craft still holds the item that was added to it.</summary>
        private bool SlotsUnchanged()
        {
            foreach (ushort slot in new[] { _container, _mainIngredient, _slot2, _slot3, _slot4 })
            {
                if (slot == 0)
                    continue;
                Item added;
                if (!_added.TryGetValue(slot, out added) || added == null || _myPlayer.ItmInterface.GetItemInSlot(slot) != added)
                    return false;
            }
            return _container != 0 && _mainIngredient != 0;
        }

        private void CraftImpl()
        {
            if (!SlotsUnchanged())
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_MISMATCH_TYPE);
                Reset(0);
                return;
            }

            uint itemid;
            byte multiplierlvl = 1;


            if(_power > 25 || _duration > 25 || _multiplier > 25  )
            {
                _Owner.GetPlayer().SendMessage("An Error in the crafting system occured if the problem is repeatable please report it", SystemData.ChatLogFilters.CHATLOGFILTERS_SHOUT);
               Reset(1);
                return;
            }



            if (!_dyecrafting && _stability + _baseStability < 0)
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_CRITICALFAILURE);
                _myPlayer.ItmInterface.DeleteItem(_container, 1);
                _myPlayer.ItmInterface.DeleteItem(_slot2, 1);
                _myPlayer.ItmInterface.DeleteItem(_slot3, 1);
                _myPlayer.ItmInterface.DeleteItem(_slot4, 1);
                _myPlayer.ItmInterface.DeleteItem(_mainIngredient, 1);
                Reset(0);
                Crafting();
                locked = false;
                return;
            }
            else if (_dyecrafting)
            {
                itemid = uint.Parse(_myPlayer.ItmInterface.GetItemInSlot(_mainIngredient).Info.Craftresult.Split(';')[0]);
            }
            else
            {
                byte powerlvl = 0;
                byte durationlvl = 0;

                switch (_power + _basePower)
                {
                    case 1:
                    case 2: powerlvl = 1; break;
                    case 3:
                    case 4:
                    case 5: powerlvl = 2; break;
                    case 6:
                    case 7: powerlvl = 3; break;
                    case 8:
                    case 9: powerlvl = 4; break;
                    case 10:
                    case 11: powerlvl = 5; break;
                    case 12:
                    case 13: powerlvl = 6; break;
                    case 14:
                    case 15: powerlvl = 7; break;
                    case 16:
                    case 17: powerlvl = 8; break;
                    case 18:
                    case 19: powerlvl = 9; break;
                    case 20:
                    case 21: powerlvl = 10; break;
                    case 22:
                    case 23: powerlvl = 11; break;
                }

                if (_duration >= 16)
                    durationlvl = 5;
                else if (_duration >= 8)
                    durationlvl = 4;
                else if (_duration >= 4)
                    durationlvl = 3;
                else if (_duration >= 2)
                    durationlvl = 2;
                else
                    durationlvl = 1;

                if (_multiplier >= 16)
                    multiplierlvl = 5;
                else if (_multiplier >= 8)
                    multiplierlvl = 4;
                else if (_multiplier >= 4)
                    multiplierlvl = 3;
                else if (_multiplier >= 2)
                    multiplierlvl = 2;
                else
                    multiplierlvl = 1;

                byte result = 0;


                if (_stability + _baseStability == 0)
                {
                    result = 2;

                    if (((float)StaticRandom.Instance.NextDouble() * 100f) <= 50)
                    {
                        _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_CRITICALFAILURE);
                        _myPlayer.ItmInterface.DeleteItem(_container, 1);
                        Reset(0);
                        Crafting();
                        locked = false;
                        return;
                    }
                }
                else if (((float)StaticRandom.Instance.NextDouble() * 100f) <= (_critChance + 10))
                {
                    result = 1;
                    _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_CRITICALSUCCESS);
                }

                itemid = uint.Parse(_myPlayer.ItmInterface.GetItemInSlot(_mainIngredient).Info.Craftresult.Split(';')[result]);

                if (itemid == 3000200 || itemid == 3000255 || itemid == 3000310 || itemid == 3000400 || itemid == 3000455 || itemid == 3000510)
                {
                    itemid += (uint)((powerlvl - 1));
                    durationlvl = 1;
                }
                else
                    itemid += (uint)((powerlvl - 1) * 5);
                itemid += ((uint)durationlvl - 1);

            }
            

                    if (_myPlayer._Value.CraftingSkillLevel < 200 && ((byte)((_myPlayer._Value.CraftingSkillLevel - _myPlayer.ItmInterface.GetItemInSlot(_mainIngredient).Info.Unk27[14]) * 3) < ((float)StaticRandom.Instance.NextDouble() * 100f)))
                    {
                        _myPlayer._Value.CraftingSkillLevel++;
                        SetBasePower();
                        _myPlayer.SendTradeSkill(_myPlayer._Value.CraftingSkill, _myPlayer._Value.CraftingSkillLevel);
                    }


                    _myPlayer.ItmInterface.CreateItem(itemid, multiplierlvl);


                    if (_slot2 != 0 && _slot2 == _slot3 && _slot2 == _slot4)
                    {
                        ushort count = _myPlayer.ItmInterface.GetItemInSlot(_slot2).Count;

                        if (count >= 6)
                            _myPlayer.ItmInterface.DeleteItem(_slot2, 3);
                        else if (count >= 5)
                        {
                            RemoveContainer(_slot2,true);
                            _myPlayer.ItmInterface.DeleteItem(_slot3, 2);
                        }
                        else if (count >= 4)
                        {
                            RemoveContainer(_slot2,true);
                            RemoveContainer(_slot3,true);
                            _myPlayer.ItmInterface.DeleteItem(_slot4, 1);
                        }
                        else
                        {
                            ushort sl = _slot2;
                            RemoveContainer(_slot2,true);
                            RemoveContainer(_slot3,true);
                            RemoveContainer(_slot4,true);

                        }
                    }

                    else if (_slot2 != 0 && _slot2 == _slot3)
                    {
                        ushort count = _myPlayer.ItmInterface.GetItemInSlot(_slot2).Count;

                        if (count >= 4)
                            _myPlayer.ItmInterface.DeleteItem(_slot2, 2);
                        else if (count >= 3)
                        {
                            RemoveContainer(_slot2,true);
                            _myPlayer.ItmInterface.DeleteItem(_slot3, 1);
                        }
                        else
                        {
                            ushort sl = _slot2;
                            RemoveContainer(_slot2,true);
                            RemoveContainer(_slot3,true);
                        }

                        if (_myPlayer.ItmInterface.GetItemInSlot(_slot4) == null || _myPlayer.ItmInterface.GetItemInSlot(_slot4).Count == 1)
                        {
                            RemoveContainer(_slot4, true);
                        }
                        else
                        {
                            _myPlayer.ItmInterface.DeleteItem(_slot4, 1);
                        }

            }
                    else if (_slot2 != 0 && _slot2 == _slot4)
                    {
                        ushort count = _myPlayer.ItmInterface.GetItemInSlot(_slot2).Count;

                        if (count >= 4)
                            _myPlayer.ItmInterface.DeleteItem(_slot2, 2);
                        else if (count >= 3)
                        {
                            RemoveContainer(_slot2,true);
                            _myPlayer.ItmInterface.DeleteItem(_slot4, 1);
                        }
                        else
                        {
                            ushort sl = _slot2;
                            RemoveContainer(_slot2,true);
                            RemoveContainer(_slot4,true);
                        }

                        if (_myPlayer.ItmInterface.GetItemInSlot(_slot3) == null || _myPlayer.ItmInterface.GetItemInSlot(_slot3).Count == 1)
                        {
                            RemoveContainer(_slot3, true);
                        }
                        else
                        {
                            _myPlayer.ItmInterface.DeleteItem(_slot3, 1);
                        }

                    }
                    else if (_slot3 != 0 && _slot3 == _slot4)
                    {
                        ushort count = _myPlayer.ItmInterface.GetItemInSlot(_slot3).Count;

                        if (count >= 4)
                            _myPlayer.ItmInterface.DeleteItem(_slot3, 2);
                        else if (count >= 3)
                        {
                            RemoveContainer(_slot3,true);
                            _myPlayer.ItmInterface.DeleteItem(_slot4, 1);
                        }
                        else
                        {
                            ushort sl = _slot3;
                            RemoveContainer(_slot3,true);
                            RemoveContainer(_slot4,true);
                        }


                        if (_myPlayer.ItmInterface.GetItemInSlot(_slot2) == null || _myPlayer.ItmInterface.GetItemInSlot(_slot2).Count == 1)
                        {
                            RemoveContainer(_slot2, true);
                        }
                        else
                        {
                            _myPlayer.ItmInterface.DeleteItem(_slot2, 1);
                        }
            }
                    else
                    {
                        if (_slot2 != 0)
                            if (_slot2 != 0 && _myPlayer.ItmInterface.GetItemInSlot(_slot2) == null || _myPlayer.ItmInterface.GetItemInSlot(_slot2).Count == 1)
                            {
                                RemoveContainer(_slot2, true);
                            }
                            else
                            {
                                _myPlayer.ItmInterface.DeleteItem(_slot2, 1);
                            }
                        if (_slot3 != 0)
                            if (_myPlayer.ItmInterface.GetItemInSlot(_slot3) == null || _myPlayer.ItmInterface.GetItemInSlot(_slot3).Count == 1)
                            {
                                RemoveContainer(_slot3, true);
                            }
                            else
                            {
                                _myPlayer.ItmInterface.DeleteItem(_slot3, 1);
                            }
                        if (_slot4 != 0)
                            if (_myPlayer.ItmInterface.GetItemInSlot(_slot4) == null || _myPlayer.ItmInterface.GetItemInSlot(_slot4).Count == 1)
                            {
                                RemoveContainer(_slot4, true);
                            }
                            else
                            {
                                _myPlayer.ItmInterface.DeleteItem(_slot4, 1);
                            }
                    }

                    if (_myPlayer.ItmInterface.GetItemInSlot(_mainIngredient) == null || _myPlayer.ItmInterface.GetItemInSlot(_mainIngredient).Count == 1)
                    {
                        RemoveContainer(_mainIngredient, true);
                    }
                    else
                    {
                        _myPlayer.ItmInterface.DeleteItem(_mainIngredient, 1);
                    }

                    if (_myPlayer.ItmInterface.GetItemInSlot(_container) == null || _myPlayer.ItmInterface.GetItemInSlot(_container).Count == 1)
                    {
                        RemoveContainer(_container, true);
                    }
                    else
                    {
                        _myPlayer.ItmInterface.DeleteItem(_container, 1);
                    }


                
            
            
            Crafting();
            locked = false;
        }

        public void SetBasePower()
        {
            if (_myPlayer._Value.CraftingSkillLevel == 200)
            {
                _basePower = 3;
                _baseStability = 1;
            }
            else if (_myPlayer._Value.CraftingSkillLevel >= 174)
            {
                _basePower = 2;
                _baseStability = 1;
            }
            else if (_myPlayer._Value.CraftingSkillLevel >= 150)
            {
                _basePower = 1;
                _baseStability = 1;
            }
        }


        public void Crafting()
        {
             PacketOut Out;
             Out = new PacketOut((byte)Opcodes.F_CRAFTING_STATUS, 32);
             Out.WriteByte(_myPlayer._Value.CraftingSkill);
             Out.WriteByte(0);
             Out.WriteByte(0);
             Out.WriteByte(_myPlayer._Value.CraftingSkillLevel);
             Out.WriteByte(_stage);  //stage
             Out.WriteByte(0);
             Out.WriteUInt16(_container);
            if (_myPlayer._Value.CraftingSkill == 4)
            {
                Out.WriteUInt16(_mainIngredient);
                Out.WriteUInt16(_slot2);
                Out.WriteUInt16(_slot3);
                Out.WriteUInt16(_slot4);
            }else
            {
                byte nuller = 0;
                if (_mainIngredient == 0)
                    nuller++;
                else
                    Out.WriteUInt16(_mainIngredient);
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
             if (_stability + _baseStability == 0)
                 Out.WriteByte(2);     // apo 0 normal  1 bad    3 good
             else if (_stability + _baseStability > 0)
                 Out.WriteByte(3);
             else if (_stability + _baseStability < 0)
                 Out.WriteByte(1);  
             Out.WriteByte(0);
             Out.WriteByte(0);
             Out.WriteByte(_power);          // talis 0-49
             Out.WriteByte(0);
             _myPlayer.SendPacket(Out);
        }
    }
}
