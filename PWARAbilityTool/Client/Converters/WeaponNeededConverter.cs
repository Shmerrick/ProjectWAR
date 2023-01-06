using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class WeaponNeededConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (value)
            {
                case 0:
                default:
                    return "None";
                case 1:
                    return "MainHand";
                case 2:
                    return "OffHand";
                case 3:
                    return "Ranged";
                case 4:
                    return "TwoHander";
                case 5:
                    return "DualWield";
                case 6:
                    return "Shield";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            int returnValue = 0;
            switch (value.ToString())
            {
                case "None":
                    return returnValue;
                case "MainHand":
                    returnValue = 1;
                    break;
                case "OffHand":
                    returnValue = 2;
                    break;
                case "Ranged":
                    returnValue = 3;
                    break;
                case "TwoHander":
                    returnValue = 4;
                    break;
                case "DualWield":
                    returnValue = 5;
                    break;
                case "Shield":
                    returnValue = 6;
                    break;
            }

            return returnValue;
        }
    }
}
