﻿using Microsoft.UI.Xaml.Controls;
using Otanabi.Core.Services;
using Otanabi.ViewModels;

namespace Otanabi.Views;

public sealed partial class FavoritesPage : Page
{
    readonly DatabaseService dbService = new();
    public FavoritesViewModel ViewModel
    {
        get;
    }

    public FavoritesPage()
    {
        ViewModel = App.GetService<FavoritesViewModel>();
        InitializeComponent();
        ViewModel.FavoreListChanged += async (s, o) => await AnimePanel.LoadFavorites();
    }

    private async void OpenConfigDialog(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ConfigFavsDialog.XamlRoot = this.XamlRoot;
        await ConfigFavsDialog.ShowAsync();
    }

    private void NoButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        DeleteFlyout?.Hide();
    }
}
