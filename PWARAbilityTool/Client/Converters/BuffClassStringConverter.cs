using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class BuffClassStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            switch (value)
            {
                case "0":
                    return "Standard";
                case "Standard":
                case "Morale":
                case "Tactic":
                case "Career":
                case "Persist":
                case "MaxBuffClasses":
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            return value.ToString();
        }
    }
}
