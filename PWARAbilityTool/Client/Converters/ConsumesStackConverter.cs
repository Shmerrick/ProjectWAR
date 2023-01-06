using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class ConsumesStackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (value.ToString())
            {
                case "1":
                    return "yes";
                case "0":
                default:
                    return "no";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (value.ToString())
            {
                case "yes":
                    return 1;
                case "no":
                default:
                    return 0;
            }
        }
    }
}
