using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class AttackingStatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            string returnValue = "";
            switch (value.ToString())
            {
                case "1":
                    returnValue = "Strength and Weaponskill";
                    break;
                case "8":
                    returnValue = "Ballistic and Initiative";
                    break;
                case "9":
                    returnValue = "Intelligence and Willpower";
                    break;
                default:
                    break;
            }
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            int returnValue = -1;
            switch (value.ToString())
            {
                case "Strength and Weaponskill":
                    returnValue = 1;
                    break;
                case "Ballistic and Initiative":
                    returnValue = 8;
                    break;
                case "Intelligence and Willpower":
                    returnValue = 9;
                    break;
                default:
                    break;
            }
            return returnValue;
        }
    }
}
