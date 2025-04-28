using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Otanabi.Core.Models;

namespace Otanabi.Converters;

public class VideoSourceToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is VideoSource currentItem && parameter is VideoSource selectedServer)
        {
            return currentItem.Id == selectedServer.Id ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}