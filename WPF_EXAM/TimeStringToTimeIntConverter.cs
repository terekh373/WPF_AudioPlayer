using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF_EXAM
{
    internal class TimeStringToTimeIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value != DependencyProperty.UnsetValue && value is string timeString)
            {
                var parts = timeString.Split(':');

                if (parts.Length == 2 && int.TryParse(parts[0], out int minutes) && int.TryParse(parts[1], out int seconds)) return minutes * 60 + seconds;
            }

            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new NotImplementedException("TimeStringToTimeIntConverter ConvertBack is not supported.");
    }
}
