using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class AbilityTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!int.TryParse(value.ToString(), out int parsedValue))
                throw new ArgumentException("Value is no int.");
            return parsedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            int returnVal = 0;
            switch (value.ToString())
            {
                case "None":
                    return returnVal;
                case "Melee":
                    returnVal = 1;
                    break;
                case "Ranged":
                    returnVal = 2;
                    break;
                case "Verbal":
                    returnVal = 3;
                    break;
                case "Effect":
                    returnVal = 255;
                    break;
            }

            return returnVal;
        }
    }
}
