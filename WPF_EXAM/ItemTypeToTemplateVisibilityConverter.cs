using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF_EXAM
{
    internal class ItemTypeToTemplateVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != DependencyProperty.UnsetValue && value != null && value is FileType fileType && parameter is FileType targetFileType && fileType == targetFileType ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new NotImplementedException("ItemTypeToTemplateVisibilityConverter ConvertBack is not supported.");
    }
}
