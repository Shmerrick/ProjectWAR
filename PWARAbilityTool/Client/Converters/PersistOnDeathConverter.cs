using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class PersistOnDeathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            switch (value)
            {
                case 0:
                default:
                    return "RequiresTargetAlive";
                case 1:
                    return "AlwaysOn";
                case 2:
                    return "RequiresTargetDead";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            switch (value.ToString())
            {
                case "RequiresTargetAlive":
                default:
                    return 0;
                case "AlwaysOn":
                    return 1;
                case "RequiresTargetDead":
                    return 2;
            }
        }
    }
}
