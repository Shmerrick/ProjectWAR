using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using Item = WorldServer.World.Objects.Item;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class CultivationPlot
    {
        #region Constants

        private const int STAGE_EMPTY = 0;
        private const int STAGE_GERMINATION = 1;
        private const int STAGE_SEEDLING = 2;
        private const int STAGE_FLOWERING = 3;
        private const int STAGE_FLOWERED = 4;

        private const int TYPE_SEED = 1;
        private const int TYPE_SOIL = 2;
        private const int TYPE_WATER = 3;
        private const int TYPE_NUTRIENTS = 4;

        #endregion

        private byte _stage;
        private byte _stageTime, _currentStageTime, _stage2Time, _stage3Time;
        private long _nextUpdateTime;

        private byte _seedLvl;
        private uint _seedItemID, _soilItemID, _waterItemID, _nutrientItemID;

        private byte _criticalChance = 10, _specialMomentChance, _failureChance;
        private byte _result;

        private readonly CultivationInterface _ownerInterface;
        private readonly byte _index;

        public CultivationPlot(CultivationInterface cInterface, byte index)
        {
            _index = index;
            _ownerInterface = cInterface;
        }

        public void Update(long tick)
        {
            if (_stage == STAGE_EMPTY || _stage == STAGE_FLOWERED || tick <= _nextUpdateTime)
                return;

            _currentStageTime--;
            _nextUpdateTime = tick + 1000;

            if (_currentStageTime <= 0)
            {
                UpdateStage();
                SendPlotInfo();
            }
        }

        private void SetStageTime(byte value)
        {
            _stageTime = value;
            _stage3Time = value;
            _stage2Time = value;
            _currentStageTime = value;
        }

        private void UpdateStage()
        {
            ++_stage;

            if (_stage == STAGE_FLOWERED)
            {
                GenerateResult();
                _ownerInterface.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CULTIVATION_PLOT_FLOWERING_COMPLETED);
            }
            else
            {
                switch (_stage)
                {
                    case STAGE_GERMINATION: _nextUpdateTime = TCPManager.GetTimeStampMS() + 1000;
                        break;
                    case STAGE_SEEDLING: _currentStageTime = _stage2Time; _stage2Time = 0;
                        break;
                    case STAGE_FLOWERING: _currentStageTime = _stage3Time; _stage3Time = 0;
                        break;
                }
                _ownerInterface.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CULTIVATION_PLOT_FLOWERING_ADVANCED);
            }
        }

        #region Component Add

        public bool AddSeed(Item_Info seedInfo)
        {
            if (_stage != STAGE_EMPTY)
            { 
                _ownerInterface.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_CULTIVATION_PLOT_STILL_HAS_PLANTS);
                return false;
            }

            _seedItemID = seedInfo.Entry;
            _seedLvl = seedInfo.Unk27[14];

            if (_seedLvl >= 150)
                SetStageTime(60);
            else if (_seedLvl >= 100)
                SetStageTime(40);
            else if (_seedLvl >= 50)
                SetStageTime(30);
            else
                SetStageTime(20);

            UpdateStage();
            AddBonuses(TYPE_SEED, seedInfo.Crafts.Split(';'));

            return true;
        }

        public bool AddSoil(Item_Info soilInfo)
        {
            if (_soilItemID != 0 || _stage != STAGE_GERMINATION || _seedItemID == 0)
            {
                _ownerInterface.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_CULTIVATION_CANT_APPLY_TO_PLOT_NOW);
                return false;
            }

            _soilItemID = soilInfo.Entry;
            AddBonuses(TYPE_SOIL, soilInfo.Crafts.Split(';'));

            return true;
        }

        public bool AddWater(Item_Info waterInfo)
        {
            if (_waterItemID != 0 || _stage != STAGE_SEEDLING || _seedItemID == 0)
            {
                _ownerInterface.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_CULTIVATION_CANT_APPLY_TO_PLOT_NOW);
                return false;
            }

            _waterItemID = waterInfo.Entry;
            AddBonuses(TYPE_WATER, waterInfo.Crafts.Split(';'));

            return true;
        }

        public bool AddNutrients(Item_Info nutrientInfo)
        {
            if (_nutrientItemID != 0 || _stage != STAGE_FLOWERING || _seedItemID == 0)
            {
                _ownerInterface.GetPlayer().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_CULTIVATION_CANT_APPLY_TO_PLOT_NOW);
                return false;
            }

            _nutrientItemID = nutrientInfo.Entry;
            AddBonuses(TYPE_NUTRIENTS, nutrientInfo.Crafts.Split(';'));

            return true;
        }

        private void AddBonuses(byte itemType, string[] bonuses)
        {
            foreach (string st in bonuses)
            {
                if (st.Length <= 0)
                    continue;

                switch (ushort.Parse(st.Split(':')[0]))
                {
                    case 10:
                        byte reduce;
                        switch (itemType)
                        {
                            case TYPE_SOIL:
                                reduce = (byte)(_stageTime * (byte)ushort.Parse(st.Split(':')[1]) / 100);
                                if (_currentStageTime > reduce)
                                    _currentStageTime -= reduce;
                                else
                                    _currentStageTime = 1;
                                break;
                            case TYPE_WATER:
                                reduce = (byte)(_stageTime * (byte)ushort.Parse(st.Split(':')[1]) / 100);
                                if (_stage == STAGE_SEEDLING)
                                {
                                    if (_currentStageTime > reduce)
                                        _currentStageTime -= reduce;
                                    else
                                        _currentStageTime = 1;
                                }
                                break;
                            case TYPE_NUTRIENTS:
                                reduce = (byte)(_stageTime * (byte)ushort.Parse(st.Split(':')[1]) / 100);
                                if (_stage == STAGE_FLOWERING)
                                {
                                    if (_currentStageTime > reduce)
                                        _currentStageTime -= reduce;
                                    else
                                        _currentStageTime = 1;
                                }
                                break;
                        }
                        break; // time    phase time / 10 value / 100
                    case 12: _criticalChance += (byte)ushort.Parse(st.Split(':')[1]); break;
                    case 13:
                        if (itemType == 1)
                            _failureChance += (byte)ushort.Parse(st.Split(':')[1]);
                        else
                            _failureChance -= (byte)ushort.Parse(st.Split(':')[1]);
                        break;
                    case 14: _specialMomentChance += (byte)ushort.Parse(st.Split(':')[1]); break;
                }
            }
        }

        #endregion

        private void GenerateResult()
        {
            Player ownerPlayer = _ownerInterface.GetPlayer();

            if (ownerPlayer._Value.GatheringSkillLevel < 200)
            {
                if ((ownerPlayer._Value.GatheringSkillLevel - _seedLvl) * 3 <= (float)StaticRandom.Instance.NextDouble() * 100f)
                { 
                    ownerPlayer._Value.GatheringSkillLevel++;
                    _ownerInterface.PendingSkillUpdate = true;
                }
            }

            if ((float)StaticRandom.Instance.NextDouble() * 100f <= _failureChance)
                _result = 0;
            else
            {
                _result = 1;
                if ((float)StaticRandom.Instance.NextDouble() * 100f <= _criticalChance)
                    _result = 2;
                if ((float)StaticRandom.Instance.NextDouble() * 100f <= _specialMomentChance)
                    _result = 3;
            }
        }

        public void Harvest()
        {
            Player harvester = _ownerInterface.GetPlayer();

            if (_stage != STAGE_FLOWERED)
            {
                harvester.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CULTIVATION_PLOT_NOT_FINISHED_GROWING);
                return;
            }

            if (ItemService.GetItem_Info(_seedItemID).Rarity == 3)
                harvester.ItmInterface.CreateItem(_seedItemID, 1, true);

            Item_Info mainItem = ItemService.GetItem_Info(uint.Parse(ItemService.GetItem_Info(_seedItemID).Craftresult.Split(';')[0]));
            Item_Info subItem = ItemService.GetItem_Info(uint.Parse(ItemService.GetItem_Info(_seedItemID).Craftresult.Split(';')[1]));

            if (harvester.ItmInterface.GetTotalFreeInventorySlot(mainItem) < 4)
            { 
                harvester.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CULTIVATION_CANT_HARVEST_SEED_BACKPACK_FULL);
                return;
            }

            switch (_result)
            {
                case 0:
                    if (harvester.ItmInterface.CreateItem(84918, 1, true) != ItemResult.RESULT_OK)
                        return;

                    harvester.SendLocalizeString(ItemService.GetItem_Info(84918).Name, ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CRAFT_RECIPE_CRITFAILURE);
                    break;
                case 1:
                    if (harvester.ItmInterface.CreateItem(mainItem, 2, 0, true) != ItemResult.RESULT_OK)
                        return;

                    harvester.SendLocalizeString(new [] { "2",  mainItem.Name }, ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CRAFT_CULTIVATION_HARVEST);
                    break;
                case 2:
                    if (harvester.ItmInterface.CreateItem(mainItem, 3, 0, true) != ItemResult.RESULT_OK)
                        return;

                    harvester.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CRAFT_CRITICALSUCCESS);
                    harvester.SendLocalizeString(new[] { "3", mainItem.Name }, ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CRAFT_CULTIVATION_HARVEST);
                    break;
                case 3:
                    if (harvester.ItmInterface.CreateItem(uint.Parse(ItemService.GetItem_Info(_seedItemID).Craftresult.Split(';')[0]), 2, true) != ItemResult.RESULT_OK)
                        return;

                    if (harvester.ItmInterface.CreateItem(subItem, 1, 0, true) != ItemResult.RESULT_OK)
                    {
                        harvester.ItmInterface.DeleteItem(ushort.Parse(ItemService.GetItem_Info(_seedItemID).Craftresult.Split(';')[0]), 2);
                        return;
                    }

                    uint dyepigment = 199901;
                    byte seedlvl = CultivationInterface.GetCraft(9, ItemService.GetItem_Info(_seedItemID).Crafts);


                    if (seedlvl < 50)
                        harvester.ItmInterface.CreateItem(dyepigment, 1, true);
                    else if (seedlvl < 100)
                        harvester.ItmInterface.CreateItem(dyepigment += 1, 1, true);
                    else if (seedlvl < 150)
                        harvester.ItmInterface.CreateItem(dyepigment += 2, 1, true);
                    else if (seedlvl < 200)
                        harvester.ItmInterface.CreateItem(dyepigment += 3, 1, true);
                    else
                    {
                        if (50 > (float)StaticRandom.Instance.NextDouble() * 100f)
                            harvester.ItmInterface.CreateItem(dyepigment += 4, 1, true);
                        else
                            harvester.ItmInterface.CreateItem(dyepigment += 5, 1, true);
                    }
                    harvester.SendLocalizeString(new[] { "2", mainItem.Name }, ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CRAFT_CULTIVATION_HARVEST);
                    harvester.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CRAFT_SPECIALMOMENT);
                    harvester.SendLocalizeString(new[] { "1", subItem.Name }, ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CRAFT_CULTIVATION_HARVEST);
                    harvester.SendLocalizeString(new[] { "1", ItemService.GetItem_Info(dyepigment).Name}, ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CRAFT_CULTIVATION_HARVEST);
                    break;
            }

            Reset();
        }

        public void SendPlotInfo()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_GET_CULTIVATION_INFO);
            Out.WriteByte(_index);
            Out.WriteByte((byte)_ownerInterface.GetPlotCount());
            Out.WriteByte(_stage);
            Out.WriteByte(0);
            Out.WriteUInt32(_currentStageTime);
            Out.WriteUInt32((uint)(_stage3Time + _stage2Time + _currentStageTime));

            if (_seedItemID == 0)
                Out.WriteByte(0);
            else
                Item.BuildItem(ref Out, null, ItemService.GetItem_Info(_seedItemID), null, 0, 0);

            if (_soilItemID == 0)
                Out.Fill(0, 5);
            else
                Item.BuildItem(ref Out, null, ItemService.GetItem_Info(_soilItemID), null, 0, 0);

            if (_waterItemID == 0)
                Out.Fill(0, 5);
            else
                Item.BuildItem(ref Out, null, ItemService.GetItem_Info(_waterItemID), null, 0, 0);

            if (_nutrientItemID == 0)
                Out.Fill(0, 5);
            else
                Item.BuildItem(ref Out, null, ItemService.GetItem_Info(_nutrientItemID), null, 0, 0);

            Out.Fill(0, 7);

            _ownerInterface.GetPlayer().SendPacket(Out);
        }

        public void Reset()
        {
            _stage = 0;
            _seedItemID = 0;
            _seedLvl = 0;

            _soilItemID = 0;
            _waterItemID = 0;
            _nutrientItemID = 0;

            _stageTime = 0;
            _stage3Time = 0;
            _stage2Time = 0;
            _currentStageTime = 0;

            _criticalChance = 10;
            _specialMomentChance = 0;
            _failureChance = 0;

            _result = 0;

            SendPlotInfo();
        }
    }

    public class CultivationInterface : BaseInterface
    {
        private Player _myPlayer;
        private List<CultivationPlot> _plots;
        public bool PendingSkillUpdate;

        public override bool Load()
        {
            UpdateTradeSkills();
            return base.Load();
        }

        public override void SetOwner(Object owner)
        {
            _Owner = owner;
            _myPlayer = owner as Player;
        }

        public void UpdateTradeSkills()
        {
            _myPlayer.SendTradeSkill(_myPlayer._Value.GatheringSkill, _myPlayer._Value.GatheringSkillLevel);
            if (_plots == null || _myPlayer._Value.GatheringSkillLevel == 1)
            {
                _plots = new List<CultivationPlot> { new CultivationPlot(this, 0) };
                SendPlotInfo(0);
            }
            if (_myPlayer._Value.GatheringSkillLevel >= 50 && _plots.Count < 2)
            {
                _plots.Add(new CultivationPlot(this, (byte)_plots.Count));
                SendPlotInfo(1);
            }
            if (_myPlayer._Value.GatheringSkillLevel >= 100 && _plots.Count < 3)
            {
                _plots.Add(new CultivationPlot(this, (byte)_plots.Count));
                SendPlotInfo(2);
            }
            if (_myPlayer._Value.GatheringSkillLevel >= 150 && _plots.Count < 4)
            {
                _plots.Add(new CultivationPlot(this, (byte)_plots.Count));
                SendPlotInfo(3);
            }
        }

        public override void Update(long tick)
        {
            if (_plots == null)
                return;

            foreach (CultivationPlot plot in _plots)
                plot.Update(tick);

            if (PendingSkillUpdate)
            {
                UpdateTradeSkills();
                PendingSkillUpdate = false;
            }
        }

        public int GetPlotCount()
        {
            return _plots.Count;
        }

        public void AddComponentToPlot(byte index, ushort itemSlot)
        {
            if (index >= _plots.Count)
            {
                _myPlayer.SendLocalizeString(index.ToString(), ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CULTIVATION_ILLEGAL_PLOT_NUM);
                return;
            }

            Item itm = _myPlayer.ItmInterface.GetItemInSlot(itemSlot);

            if (itm == null)
            {
                _myPlayer.SendClientMessage("No item was found in the requested inventory slot.", ChatLogFilters.CHATLOGFILTERS_CRAFTING);
                return;
            }

            if (itm.Info.Unk27[14] > _myPlayer._Value.GatheringSkillLevel)
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_CULTIVATION_OBJECT_TOO_POWERFUL);
                return;
            }

            if (itm.Info.Unk27.Length == 0 || ((itm.Info.Unk27[15] == 1 || itm.Info.Unk27[15] == 5) && itm.Info.Craftresult.Split(';').Length < 2))
            {
                _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_CULTIVATION_BAD_SEED_DATA);
                return;
            }

            switch(itm.Info.Unk27[15])
            {
                case 5:
                case 1:
                    if (_plots[index].AddSeed(itm.Info))
                        _myPlayer.ItmInterface.DeleteItem(itemSlot, 1);
                    break;
                case 2:
                    if (_plots[index].AddSoil(itm.Info))
                        _myPlayer.ItmInterface.DeleteItem(itemSlot, 1);
                    break;
                case 3:
                    if (_plots[index].AddWater(itm.Info))
                        _myPlayer.ItmInterface.DeleteItem(itemSlot, 1);
                    break;
                case 4:
                    if (_plots[index].AddNutrients(itm.Info))
                        _myPlayer.ItmInterface.DeleteItem(itemSlot, 1);
                    break;
            }

            SendPlotInfo(index);
        }

        public void Uproot(byte index)
        {
            if (index >= _plots.Count)
            {
                _myPlayer.SendLocalizeString(index.ToString(), ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CULTIVATION_ILLEGAL_PLOT_NUM);
                return;
            }

            _plots[index].Reset();

            _myPlayer.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CULTIVATION_UPROOTED_PLANT);
        }

        public void ResetPlot(byte index)
        {
            if (index >= _plots.Count)
            {
                _myPlayer.SendLocalizeString(index.ToString(), ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CULTIVATION_ILLEGAL_PLOT_NUM);
                return;
            }

            _plots[index].Reset();
        }

        public void Harvest(byte index)
        {
            if (index >= _plots.Count)
            {
                _myPlayer.SendLocalizeString(index.ToString(), ChatLogFilters.CHATLOGFILTERS_CRAFTING, Localized_text.TEXT_CULTIVATION_ILLEGAL_PLOT_NUM);
                return;
            }

            _plots[index].Harvest();
        }

        public static void ReapResin(Player plr, ushort slot)
        {
            uint arborealResin = 84711;

            byte lvl = CraftingApoInterface.GetCraft(9, plr.ItmInterface.GetItemInSlot(slot).Info.Crafts);
            if(lvl > 1)
                arborealResin += (uint)(lvl / 25);

            plr.ItmInterface.CreateItem(arborealResin, 1, true);
            plr.ItmInterface.CreateItem(uint.Parse(plr.ItmInterface.GetItemInSlot(slot).Info.Craftresult.Split(';')[3]), 1, true);
            plr.ItmInterface.DeleteItem(slot, 1);
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

        public void SendPlotInfo(byte index)
        {
            if (_plots == null || index >= _plots.Count)
                return;

            _plots[index].SendPlotInfo();
        }
    }
}
