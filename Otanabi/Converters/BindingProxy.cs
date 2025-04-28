using Microsoft.UI.Xaml;

namespace Otanabi.Converters;

public class BindingProxy : DependencyObject
{
    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), null);
}