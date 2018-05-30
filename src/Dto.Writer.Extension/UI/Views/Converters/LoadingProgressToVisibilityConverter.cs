using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Dto.Writer.UI.ViewModels;

namespace Dto.Writer.UI.Views.Converters
{
  public class LoadingProgressToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is LoadingViewModel vm)
      {
        return vm.IsCompleted ? Visibility.Hidden : Visibility.Visible;
      }

      return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return null;
    }
  }
}
