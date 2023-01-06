using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class UndefendableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
