using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    
    [FrameWork.DataTable(PreCache = false, TableName = "item_sets", DatabaseName = "World")]
    [Serializable]
    public class Item_Set : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement(AllowDbNull = false, Varchar = 255)]
        public string Name { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Unk { get; set; } // Spell modifier?

        [DataElement(AllowDbNull = false)]
        public string ItemsString
        {
            get
            {
                string Value = "";
                foreach (KeyValuePair<uint, string> Item in Items)
                    Value += Item.Key + ":" + Item.Value + "|";
                return Value;
            }
            set
            {
                if (value.Length <= 0)
                    return;

                string[] Objs = value.Split('|');

                foreach (string Obj in Objs)
                {
                    if (Obj.Length <= 0)
                        continue;

                    string[] St = Obj.Split(':');

                    uint bonusKey = uint.Parse(St[0]);

                    if (Items.ContainsKey(bonusKey))
                        Log.Error("Item set "+Name, "Duplicate bonus type in Items String ("+bonusKey+")");
                    else
                        Items.Add(bonusKey, St[1]);
                }
                Dirty = true;
            }
        }

        [DataElement(AllowDbNull = false)]
        public string BonusString
        {
            get
            {
                string Value = "";
                foreach (KeyValuePair<byte, string> B in Bonus)
                    Value += B.Key + ":" + B.Value + "|";
                return Value;
            }
            set
            {
                if (value.Length <= 0)
                    return;

                string[] objs = value.Split('|');

                foreach (string bonusString in objs)
                {
                    if (bonusString.Length <= 0)
                        continue;

                    string[] St = bonusString.Split(':');

                    Bonus.Add(byte.Parse(St[0]), St[1]);

                    // Update bonus list which is read by ItemsInterface
                    string[] bonuses = St[1].Split(',');

                    if (bonuses.Length == 1)
                        _bonusList.Add(new ItemSetBonusInfo(byte.Parse(St[0]), ushort.Parse(bonuses[0])));
                    else
                        _bonusList.Add(new ItemSetBonusInfo(byte.Parse(St[0]), ushort.Parse(bonuses[0]), ushort.Parse(bonuses[1]), ushort.Parse(bonuses[2])));
                }
                Dirty = true;
            }
        }

        [DataElement(AllowDbNull = false)]
        public int ClassId { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Comments { get; set; }

        [DataElement(AllowDbNull = false)]
        public string ItemSetList { get; set; }

        [DataElement(AllowDbNull = false)]
        public String ItemSetFullDescription { get; set; }

        public Dictionary<uint, string> Items = new Dictionary<uint, string>();
        public Dictionary<byte, string> Bonus = new Dictionary<byte, string>();

        private readonly List<ItemSetBonusInfo> _bonusList = new List<ItemSetBonusInfo>();

        public List<ItemSetBonusInfo> GetBonusList(byte setItemsEquipped)
        {
            List<ItemSetBonusInfo> bonusList = new List<ItemSetBonusInfo>();

            foreach (ItemSetBonusInfo info in _bonusList)
            {
                if (info.ItemsRequired > setItemsEquipped)
                    return bonusList;

                bonusList.Add(info);
            }

            return bonusList;
        }
    }

    public class ItemSetBonusInfo
    {
        public byte ItemsRequired;
        public byte ActionType;
        public ushort StatOrSpell;
        public ushort Value;
        public ushort Percentage;

        public ItemSetBonusInfo(ushort actionType, ushort spell)
        {
            // 8x -> x is the number of items required
            ItemsRequired = (byte)(actionType % 10);
            ActionType = (byte)(actionType / 10);
            StatOrSpell = spell;
        }

        public ItemSetBonusInfo(ushort actionType, ushort stat, ushort value, ushort percentage)
        {
            // 3x -> x-2 is the number of items required
            ItemsRequired = (byte)(actionType % 10 - 2);
            ActionType = (byte)(actionType / 10);
            StatOrSpell = stat;
            //switches all armor set damage bonuses to % based
            if (StatOrSpell == 24)
            {
                StatOrSpell = 25;
            }
            Value = value;
            Percentage = percentage;
        }
    }
}