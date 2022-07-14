using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class WeaponDmgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            string returnValue = "";
            switch (value.ToString())
            {
                case "1":
                case "MainHand":
                    returnValue = "MainHand";
                    break;
                default:
                    returnValue = value.ToString();
                    break;
            }
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            return value.ToString();
        }
    }
}
