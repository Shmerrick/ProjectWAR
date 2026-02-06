using System.Text;

namespace WarZoneLib;

public class CRC32
{
    private static uint[] table;

    public static uint ComputeChecksum(string text)
    {
        if (text == null || text.Length == 0)
            return 0;
        return ComputeChecksum(Encoding.ASCII.GetBytes(text));
    }

    public static uint ComputeChecksum(byte[] bytes)
    {
        if (table == null)
        {
            var num1 = 3988292384;
            table = new uint[256];
            for (uint index1 = 0; index1 < table.Length; ++index1)
            {
                var num2 = index1;
                for (var index2 = 8; index2 > 0; --index2)
                {
                    if (((int)num2 & 1) == 1)
                        num2 = num2 >> 1 ^ num1;
                    else
                        num2 >>= 1;
                }
                table[(int)index1] = num2;
            }
        }
        var num3 = uint.MaxValue;
        for (var index = 0; index < bytes.Length; ++index)
        {
            var num1 = (byte)(num3 & byte.MaxValue ^ bytes[index]);
            num3 = num3 >> 8 ^ table[num1];
        }
        return ~num3;
    }
}