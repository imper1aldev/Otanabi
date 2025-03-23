using Microsoft.UI.Xaml.Controls;

using Otanabi.ViewModels;

namespace Otanabi.Views;

public sealed partial class DetailPage : Page
{
    public DetailViewModel ViewModel
    {
        get;
    }

    public DetailPage()
    {
        ViewModel = App.GetService<DetailViewModel>();
        InitializeComponent();
    }
}
