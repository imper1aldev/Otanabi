using Otanabi.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Otanabi.Views;

public sealed partial class HistoryPage : Page
{
    public HistoryViewModel ViewModel
    {
        get;
    }

    public HistoryPage()
    {
        ViewModel = App.GetService<HistoryViewModel>();
        InitializeComponent();
    }

    private void DeleteBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) => ViewModel.DeleteHistoryById((int)((Button)sender).Tag);
}
