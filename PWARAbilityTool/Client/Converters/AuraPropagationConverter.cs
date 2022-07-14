using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class AuraPropagationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            switch (value.ToString())
            {
                default:
                    return "None";
                case "None":
                case "Group":
                case "Foe":
                case "Foe20":
                case "Foe30":
                case "Foe40":
                case "Foe250":
                case "All":
                case "HTL":
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
