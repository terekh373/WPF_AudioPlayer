using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF_EXAM
{
    public class TimeSpanToTimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null && value != DependencyProperty.UnsetValue && value is TimeSpan time && time != TimeSpan.Zero ? time.ToString(@"mm\:ss") : "00:00";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new NotImplementedException("TimeSpanToTimeStringConverter ConvertBack is not supported");
    }
}
