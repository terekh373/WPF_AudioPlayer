using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WPF_EXAM
{
    internal class ItemTypeToTemplateBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != DependencyProperty.UnsetValue && value != null && value is FileType fileType && parameter is FileType targetFileType && fileType == targetFileType ? Color.FromRgb(0, 42, 111) : Color.FromRgb(255, 0, 0);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new NotImplementedException("ItemTypeToTemplateBackgroundConverter ConvertBack is not supported.");
    }
}
