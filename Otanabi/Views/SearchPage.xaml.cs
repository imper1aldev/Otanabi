using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Otanabi.ViewModels;

namespace Otanabi.Views;

public sealed partial class SearchPage : Page
{
    public SearchViewModel ViewModel { get; }

    public SearchPage()
    {
        ViewModel = App.GetService<SearchViewModel>();
        InitializeComponent();
    }

    private void SelectorGenres_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listView = sender as ListView;
        if (listView != null)
        {
            ViewModel.UpdateCollection(listView.SelectedItems, "Genres");
        }
    }

    private void SelectorFormats_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listView = sender as ListView;
        if (listView != null)
        {
            ViewModel.UpdateCollection(listView.SelectedItems, "Formats");
        }
    }
}
