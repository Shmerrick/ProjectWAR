using System;
using System.Collections.Generic;
using SystemData;
using FrameWork;
using WorldServer.NetWork;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class GatheringInterface : BaseInterface
    {
        Player _myPlayer;

        public override bool Load()
        {
            _myPlayer.SendTradeSkill(_myPlayer._Value.GatheringSkill, _myPlayer._Value.GatheringSkillLevel);
            return base.Load();
        }

        public override void SetOwner(Object owner)
        {
            _Owner = owner;
            _myPlayer = owner as Player;
        }

        public void Gather(byte itemlevel)
        {

            if (_myPlayer._Value.GatheringSkillLevel <= 25 && (StaticRandom.Instance.Next(100) >= (((float)_myPlayer._Value.GatheringSkillLevel / (3 * itemlevel)) * 10)))
            //Beispiel skillevel = 25 , itemlevel = 1 -> sollte 16.66667% chance auf gain sein
            {
                _myPlayer._Value.GatheringSkillLevel++;
                _myPlayer.SendTradeSkill(_myPlayer._Value.GatheringSkill, _myPlayer._Value.GatheringSkillLevel);
            }
            else if (25 < _myPlayer._Value.GatheringSkillLevel && _myPlayer._Value.GatheringSkillLevel <= 75 && (StaticRandom.Instance.Next(100) >= (((float)_myPlayer._Value.GatheringSkillLevel / (1.5 * itemlevel)) * 10)))
            //Beispiel skillevel = 26 , itemlevel = 1 -> sollte 0% chance auf gain sein
            //Beispiel skillevel = 75 , itemlevel = 10 -> sollte 50% chance auf gain sein
            //Beispiel skillevel = 75 , itemlevel = 20 -> sollte 75% chance auf gain sein
            {
                _myPlayer._Value.GatheringSkillLevel++;
                _myPlayer.SendTradeSkill(_myPlayer._Value.GatheringSkill, _myPlayer._Value.GatheringSkillLevel);
            }
            else if (75 < _myPlayer._Value.GatheringSkillLevel && _myPlayer._Value.GatheringSkillLevel <= 150 && (StaticRandom.Instance.Next(100) >= (((float)_myPlayer._Value.GatheringSkillLevel / (itemlevel)) * 10)))
            //Beispiel skillevel = 76 , itemlevel = 10 -> sollte 24% chance auf gain sein
            //Beispiel skillevel = 100 , itemlevel = 10 -> sollte 0% chance auf gain sein
            //Beispiel skillevel = 76 , itemlevel = 20 -> sollte 62% chance auf gain sein
            //Beispiel skillevel = 150 , itemlevel = 20 -> sollte 25% chance auf gain sein
            {
                _myPlayer._Value.GatheringSkillLevel++;
                _myPlayer.SendTradeSkill(_myPlayer._Value.GatheringSkill, _myPlayer._Value.GatheringSkillLevel);
            }
            else if (150 < _myPlayer._Value.GatheringSkillLevel && _myPlayer._Value.GatheringSkillLevel < 200 && (StaticRandom.Instance.Next(100) >= (((1.5 * (float)_myPlayer._Value.GatheringSkillLevel) / (itemlevel)) * 10)))
            //Beispiel skillevel = 151 , itemlevel = 20 -> sollte 0% chance auf gain sein
            //Beispiel skillevel = 151 , itemlevel = 30 -> sollte 25% chance auf gain sein
            //Beispiel skillevel = 190 , itemlevel = 40 -> sollte 29% chance auf gain sein
            {
                _myPlayer._Value.GatheringSkillLevel++;
                _myPlayer.SendTradeSkill(_myPlayer._Value.GatheringSkill, _myPlayer._Value.GatheringSkillLevel);
            }
                /*
                if (_myPlayer._Value.GatheringSkillLevel < 200 && ((((float)_myPlayer._Value.GatheringSkillLevel / npclvl) * 10) < ((float)StaticRandom.Instance.NextDouble() * 100f)))
                {
                    _myPlayer._Value.GatheringSkillLevel++;
                    _myPlayer.SendTradeSkill(_myPlayer._Value.GatheringSkill, _myPlayer._Value.GatheringSkillLevel);
                }
                */
            }
        public bool CanGather(byte profession ,byte npclvl)
        {
            if (_myPlayer._Value.GatheringSkill != profession)
                return false;


            if (npclvl >= 40)
            {
                if (_myPlayer._Value.GatheringSkillLevel == 200)
                    return true;
            }
            else if (npclvl <= _myPlayer._Value.GatheringSkillLevel / 5)
            {
                return true;
            }
            else if (npclvl < 5)
                return true;

            _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_CRAFT_SKILL_TOO_LOW);
            return false;
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_SALVAGE_ITEM, (int)eClientState.Playing, "F_SALVAGE_ITEM")]
        public static void F_SALVAGE_ITEM(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null)
                return;

            ushort Slot = packet.GetUint16();
            ushort Stat = packet.GetUint16();

            cclient.Plr.GatherInterface.Salvage_Item(Slot,Stat);
        }

        public void Salvage_Item(ushort Slot,ushort Stat)
        {
            uint Talid = 0;
            uint Essence = 907701;
            uint Dust = 907638;
            Boolean fail = false;

            Item itm = _Owner.GetPlayer().ItmInterface.GetItemInSlot(Slot);

            if (itm == null)
                return;

            int chance;

            if (_Owner.GetPlayer()._Value.GatheringSkillLevel == 200)
                chance = 100;
            else
            {
                if (itm.Info.ObjectLevel > 40)
                    chance = 81 - (40 - 1) * 5;
                else
                    chance = 81 - (itm.Info.ObjectLevel - 1) * 5;

                chance += _Owner.GetPlayer()._Value.GatheringSkillLevel - 1;

                if (chance > 100)
                    chance = 100;
                if (chance < 0)
                    chance = 0;
            }


            if (chance < ((float)StaticRandom.Instance.NextDouble() * 100f))
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_CRITICALFAILURE);
                fail = true;
            }

                

            // double check never trust the client
            if (Stat > 0)
            {
                Boolean Realstat = false;
                foreach (KeyValuePair<byte, ushort> Key in itm.Info._Stats)
                {
                    if (Key.Key == Stat)
                        Realstat = true;
                }

                if (Realstat == false)
                    return;



                switch (Stat)
                {
                    case 1: // str
                        Talid = 908261;
                        break;

                    case 3: // willpo
                        Talid = 908221;
                        break;

                    case 4: // though
                        Talid = 908061;
                        break;

                    case 5: // wounds
                        Talid = 908341;
                        break;

                    case 6: // inititative
                        Talid = 908301;
                        break;

                    case 7: // Weaponskill
                        Talid = 908141;
                        break;

                    case 8: // Ballistic
                        Talid = 908101;
                        break;

                    case 9: // Intelligence
                        Talid = 908181;
                        break;

                    case 14: //spirit
                        Talid = 908021;
                        break;

                    case 15: //elemental
                        Talid = 907981;
                        break;
                        
                    case 16: //corp
                        Talid = 907941;
                        break;

                    default:
                        _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_CRAFT_INVALID_PRODUCT);
                        return;
                       
                }
            }

            Gather(itm.Info.ObjectLevel);

                       
            if (itm.Info.ObjectLevel >= 40)
                Essence += 8;
            else
                Essence += (uint)(itm.Info.ObjectLevel / 5);

            if (itm.Info.ObjectLevel >= 40)
                Dust += 8;
            else
                Dust += (uint)(itm.Info.ObjectLevel / 5);


            if (Talid > 0)
            {
                if (!fail) { 

                    if (Stat >= 14 && itm.Info.Rarity > 3)
                        Talid += 2;
                    else if (itm.Info.Rarity > 4)
                        Talid += 3;
                    else
                        Talid += (uint)(itm.Info.Rarity - 1);
                }

                byte statjump = 4;

                if (Stat >= 14)
                    statjump = 3;

                    if (itm.Info.ObjectLevel >= 40)
                    Talid += (uint)8 * statjump;
                else
                    Talid += (uint)(itm.Info.ObjectLevel / 5 * statjump);

                _Owner.GetPlayer().ItmInterface.CreateItem(Talid, 1, true);
                _myPlayer.SendLocalizeString(ItemService.GetItem_Info(Talid).Name, ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_YOU_CREATED);

                if (!fail)
                {

                    if (((float)StaticRandom.Instance.NextDouble() * 100f) <= 50)
                    {
                        _Owner.GetPlayer().ItmInterface.CreateItem(Dust, (ushort)(itm.Info.Rarity), true);
                        _myPlayer.SendLocalizeString(ItemService.GetItem_Info(Dust).Name, ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_YOU_CREATED);
                    }
                    else
                    {
                        _Owner.GetPlayer().ItmInterface.CreateItem(Essence, (ushort)(itm.Info.Rarity), true);
                        _myPlayer.SendLocalizeString(ItemService.GetItem_Info(Essence).Name, ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_YOU_CREATED);
                    }
                }
            }
            else
            {
                if (((float)StaticRandom.Instance.NextDouble() * 100f) <= 50)
                {
                    _Owner.GetPlayer().ItmInterface.CreateItem(Dust, 1, true);
                    _myPlayer.SendLocalizeString(ItemService.GetItem_Info(Dust).Name, ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_YOU_CREATED);
                }
                else
                {
                    _Owner.GetPlayer().ItmInterface.CreateItem(Essence, 1, true);
                    _myPlayer.SendLocalizeString(ItemService.GetItem_Info(Essence).Name, ChatLogFilters.CHATLOGFILTERS_CRAFTING, GameData.Localized_text.TEXT_CRAFT_YOU_CREATED);
                }
            }

            _Owner.GetPlayer().ItmInterface.DeleteItem(Slot, 1);
        }
    }
}
