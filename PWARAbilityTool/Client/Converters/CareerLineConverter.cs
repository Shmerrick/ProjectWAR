using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class CareerLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return getConvertValue(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return getConvertBackValue(value);
        }

        private string getConvertValue(object value)
        {
            switch (value.ToString())
            {
                case "0":
                    return "All";
                case "1":
                    return "Iron Breaker";
                case "2":
                    return "Slayer";
                case "4":
                    return "Rune Priest";
                case "8":
                    return "Engineer";
                case "16":
                    return "Black Orc";
                case "32":
                    return "Choppa";
                case "64":
                    return "Shaman";
                case "128":
                    return "Squig Herder";
                case "256":
                    return "Witch Hunter";
                case "512":
                    return "Knight of the Blazing Sun";
                case "1024":
                    return "Bright Wizard";
                case "2048":
                    return "Warrior Priest";
                case "4096":
                    return "Chosen";
                case "8192":
                    return "Marauder";
                case "16384":
                    return "Zealot";
                case "32768":
                    return "Magus";
                case "65536":
                    return "Sword Master";
                case "131072":
                    return "Shadow Warrior";
                case "262144":
                    return "White Lion";
                case "524288":
                    return "Archmage";
                case "1048576":
                    return "Black Guard";
                case "2097152":
                    return "Witch Elf";
                case "4194304":
                    return "Disciple Of Khaine";
                case "8388608":
                    return "Sorceress";
                case "25":
                    return "Pet - Lion";
                case "26":
                    return "Pet - Squig";
                case "27":
                    return "Pet - Horned Squig";
                case "28":
                    return "Pet - Gas Squig";
                case "29":
                    return "Pet - Spiked Squig";
                default:
                    return "undefined";
            }
        }

        private int getConvertBackValue(object value)
        {
            switch (value.ToString())
            {
                case "All":
                    return 0;
                case "Iron Breaker":
                    return 1;
                case "Slayer":
                    return 2;
                case "Rune Priest":
                    return 4;
                case "Engineer":
                    return 8;
                case "Black Orc":
                    return 16;
                case "Choppa":
                    return 32;
                case "Shaman":
                    return 64;
                case "Squig Herder":
                    return 128;
                case "Witch Hunter":
                    return 256;
                case "Knight of the Blazing Sun":
                    return 512;
                case "Bright Wizard":
                    return 1024;
                case "Warrior Priest":
                    return 2048;
                case "Chosen":
                    return 4096;
                case "Marauder":
                    return 8192;
                case "Zealot":
                    return 16384;
                case "Magus":
                    return 32768;
                case "Sword Master":
                    return 65536;
                case "Shadow Warrior":
                    return 131072;
                case "White Lion":
                    return 262144;
                case "Archmage":
                    return 524288;
                case "Black Guard":
                    return 1048576;
                case "Witch Elf":
                    return 2097152;
                case "Disciple Of Khaine":
                    return 4194304;
                case "Sorceress":
                    return 8388608;
                case "Pet - Lion":
                    return 25;
                case "Pet - Squig":
                    return 26;
                case "Pet - Horned Squig":
                    return 27;
                case "Pet - Gas Squig":
                    return 28;
                case "Pet - Spiked Squig":
                    return 29;
                case "undefined":
                default:
                    return -1;
            }
        }
    }
}
