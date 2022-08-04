using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class DmgHealIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (value.ToString())
            {
                case "0":
                default:
                    return "Normal Damage Info";
                case "1":
                    return "Buff Damage Info";
                case "2":
                    return "Extra Damage Info";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (value.ToString())
            {
                case "Normal Damage Info":
                default:
                    return 0;
                case "Buff Damage Info":
                    return 1;
                case "Extra Damage Info":
                    return 2;
            }
        }
    }
}
