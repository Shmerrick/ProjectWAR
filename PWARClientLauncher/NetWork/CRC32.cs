using System.Text;

namespace WarZoneLib
{
    public class CRC32
    {
        private static uint[] table;

        public static uint ComputeChecksum(string text)
        {
            if (text == null || text.Length == 0)
                return 0;
            return CRC32.ComputeChecksum(Encoding.ASCII.GetBytes(text));
        }

        public static uint ComputeChecksum(byte[] bytes)
        {
            if (CRC32.table == null)
            {
                uint num1 = 3988292384;
                CRC32.table = new uint[256];
                for (uint index1 = 0; (long)index1 < (long)CRC32.table.Length; ++index1)
                {
                    uint num2 = index1;
                    for (int index2 = 8; index2 > 0; --index2)
                    {
                        if (((int)num2 & 1) == 1)
                            num2 = num2 >> 1 ^ num1;
                        else
                            num2 >>= 1;
                    }
                    CRC32.table[(int)index1] = num2;
                }
            }
            uint num3 = uint.MaxValue;
            for (int index = 0; index < bytes.Length; ++index)
            {
                byte num1 = (byte)(num3 & (uint)byte.MaxValue ^ (uint)bytes[index]);
                num3 = num3 >> 8 ^ CRC32.table[(int)num1];
            }
            return ~num3;
        }
    }
}