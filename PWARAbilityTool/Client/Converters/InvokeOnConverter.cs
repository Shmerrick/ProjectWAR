using System;
using System.Globalization;
using System.Windows.Data;

namespace PWARAbilityTool.Client.Converters
{
    public class InvokeOnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            switch (value.ToString())
            {
                case "0":
                default:
                    return "EventOnly";
                case "1":
                    return "Start";
                case "2":
                    return "Tick";
                case "3":
                    return "StartAndTick";
                case "4":
                    return "End";
                case "5":
                    return "StartAndEnd";
                case "6":
                    return "TickAndEnd";
                case "7":
                    return "All";
                case "8":
                    return "ChannelOverride";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            int returnVal = 0;
            switch (value.ToString())
            {
                case "EventOnly":
                    return returnVal;
                case "Start":
                    returnVal = 1;
                    break;
                case "Tick":
                    returnVal = 2;
                    break;
                case "StartAndTick":
                    returnVal = 3;
                    break;
                case "End":
                    returnVal = 4;
                    break;
                case "StartAndEnd":
                    returnVal = 5;
                    break;
                case "TickAndEnd":
                    returnVal = 6;
                    break;
                case "All":
                    returnVal = 7;
                    break;
                case "ChannelOverride":
                    returnVal = 8;
                    break;
            }

            return returnVal;
        }
    }
}
