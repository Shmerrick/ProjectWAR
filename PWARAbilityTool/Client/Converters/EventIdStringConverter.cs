using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class EventIdStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            return getConvertValue(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return value.ToString();
        }

        private string getConvertValue(object value)
        {
            switch (value.ToString())
            {
                case "0":
                    return "DealingDamage";
                default:
                    return value.ToString();
            }
        }
    }
}
