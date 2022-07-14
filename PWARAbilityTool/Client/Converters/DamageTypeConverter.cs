using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class DamageTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            string returnValue = "";
            switch (value.ToString())
            {
                case "0":
                default:
                    returnValue = "Physical";
                    break;
                case "1":
                    returnValue = "Spiritual";
                    break;
                case "2":
                    returnValue = "Elemental";
                    break;
                case "3":
                    returnValue = "Corporeal";
                    break;
                case "4":
                    returnValue = "Healing";
                    break;
                case "254":
                    returnValue = "RawHealing";
                    break;
                case "255":
                    returnValue = "RawDamage";
                    break;
            }
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            return value.ToString();
            //switch (value.ToString())
            //{
            //    case "Physical":
            //    default:
            //        return 0;
            //    case "Spiritual":
            //        return 1;
            //    case "Elemental":
            //        return 2;
            //    case "Corporeal":
            //        return 3;
            //    case "Healing":
            //        return 4;
            //    case "RawHealing":
            //        return 254;
            //    case "RawDamage":
            //        return 255;
            //}
        }
    }
}
