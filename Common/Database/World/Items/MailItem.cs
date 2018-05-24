using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class MailItem
    {
        public uint id;
        public List<Talisman> talisman = new List<Talisman>();
        public ushort primary_dye;
        public ushort secondary_dye;
        public ushort count;

        public MailItem(uint id, ushort count)
        {
            this.id = id;
            this.count = count;
        }

        public MailItem(uint id, String additonalinfo, ushort count)
        {
            this.id = id;
            this.count = count;

            if (additonalinfo != null)
            {
                string[] ItemInfo = additonalinfo.Split('!');
                string[] detail = ItemInfo[0].Split(';');
                for (int i = 0; i < detail.Length; i++)
                {
                    if (detail[i].Length < 2)
                        return;
                    talisman.Add(new Talisman(UInt32.Parse(detail[i].Split('\'')[0]), (byte)UInt16.Parse(detail[i].Split('\'')[1]), (byte)UInt16.Parse(detail[i].Split('\'')[2]), UInt32.Parse(detail[i].Split('\'')[3])));
                }
                    primary_dye = ushort.Parse(ItemInfo[1].Split('-')[0]);
                    secondary_dye = ushort.Parse(ItemInfo[1].Split('-')[1]);
            }
        }

        public MailItem(uint id, List<Talisman> talisman, ushort primary_dye, ushort secondary_dye, ushort count)
        {
            this.id = id;
            this.count = count;
            this.talisman = talisman;
            this.primary_dye = primary_dye;
            this.secondary_dye = secondary_dye;
        }


        public Talisman GetTalisman(byte i)
        {
            foreach (Talisman tali in talisman)
            {
                if (tali.Slot == i)
                    return tali;
            }
            return null;
        }
        public String GetSaveString()
        {

            if (talisman.Count == 0 && primary_dye == 0 && secondary_dye == 0)
                return null;
            String Str="";

            foreach (Talisman tali in talisman)
            {
                Str += tali.Entry + "'" + tali.Slot + "'" + tali.Fused + "'" + tali.Timer + ";";
            }
            Str += "!"+primary_dye + "-" + secondary_dye;
            
            return Str;
        }
    }
}
