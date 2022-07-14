using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class BuffGroupsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            switch (value)
            {
                case 1:
                    return "SelfClassBuff";
                case 2:
                    return "OtherClassBuff";
                case 3:
                    return "SelfClassSecondaryBuff";
                case 5:
                    return "Aura";
                case 6:
                    return "Vanity";
                case 7:
                    return "Resurrection";
                case 10:
                    return "Detaunt";
                case 20:
                    return "HealPotion";
                case 21:
                    return "StatPotion";
                case 22:
                    return "DefensePotion";
                case 23:
                    return "Caltrops";
                case 24:
                    return "SharedCooldown1";
                case 30:
                    return "ItemProc";
                case 50:
                    return "HoldTheLine";
                case 51:
                    return "Guard";
                case 52:
                    return "OathFriend";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            switch (value)
            {
                case "SelfClassBuff":
                    return 1;
                case "OtherClassBuff":
                    return 2;
                case "SelfClassSecondaryBuff":
                    return 3;
                case "Aura":
                    return 5;
                case "Vanity":
                    return 6;
                case "Resurrection":
                    return 7;
                case "Detaunt":
                    return 20;
                case "HealPotion":
                    return 10;
                case "StatPotion":
                    return 21;
                case "DefensePotion":
                    return 22;
                case "Caltrops":
                    return 23;
                case "SharedCooldown1":
                    return 24;
                case "ItemProc":
                    return 30;
                case "HoldTheLine":
                    return 50;
                case "Guard":
                    return 51;
                case "OathFriend":
                    return 52;
                default:
                    return 0;
            }
        }
    }
}
