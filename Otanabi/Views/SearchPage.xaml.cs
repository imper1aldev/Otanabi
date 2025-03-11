using Microsoft.UI.Xaml.Controls;
using Otanabi.ViewModels;

namespace Otanabi.Views;

public sealed partial class SearchPage : Page
{
    public SearchViewModel ViewModel
    {
        get;
    }

    public SearchPage()
    {
        ViewModel = App.GetService<SearchViewModel>();
        InitializeComponent();
    }
}
