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

         public void AddContainer(ushort slot)
         {
            if (locked)
                return;
            locked = true;

            Item Itm = _myPlayer.ItmInterface.GetItemInSlot(slot);
            byte crafttype = GetCraft(8, Itm.Info.Crafts);

            if (Itm == null)
            {
                locked = false;
                return;
            }

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


            if(slot == _slot2 || slot == _slot3 || slot == _slot4)
            {
                byte count = 1;
                if (slot == _slot2)
                    count++;
                if (slot == _slot3)
                    count++;
                if (slot == _slot4)
                    count++;

                if(count > _myPlayer.ItmInterface.GetItemInSlot(slot).Count)
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

            AddBonuses(_myPlayer.ItmInterface.GetItemInSlot(slot).Info.Crafts.Split(';'));
            Crafting();
            locked = false;
             //_Owner.GetPlayer().SendMessage(_Owner.Oid,"Power ", "" + (_power + _basePower) + "  stabi  " + (_stability+ _baseStability) + "  crittchance "+_critChance+"  duration  "+_duration+"  multi "+_multiplier ,SystemData.ChatLogFilters.CHATLOGFILTERS_SHOUT);
        }

        public void RemoveContainer(ushort slot,bool consumeItem = false,byte count = 1)
        {

            if (!consumeItem)
            {
                if (locked)
                    return;
                locked = true;
            }
            if (slot == 0)
            {
                locked = false;
                return;
            }
           if(_container == slot)
           {
                if (consumeItem) 
                    _myPlayer.ItmInterface.DeleteItem(slot, 1);
                Reset(0);
                locked = false;
                return;
           } 
           if(_mainIngredient == slot)
           {
                RemoveContainer(_slot2);
                RemoveContainer(_slot3);
                RemoveContainer(_slot4);
                _stage = 1;
                _mainIngredient = 0;
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
            RemoveBonuses(_myPlayer.ItmInterface.GetItemInSlot(slot).Info.Crafts.Split(';'));

           
            if (consumeItem)
                _myPlayer.ItmInterface.DeleteItem(slot, count);
           
                Crafting();

            if (!consumeItem)
                locked = false;
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
            Crafting();
        }

        public void Craft()
        {
            try
            {
                if (locked)
                    return;
                locked = true;
            }
            catch
            {
                locked = false;
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
