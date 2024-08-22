using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Helpers;
using AnimeWatcher.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Windows.System;

namespace AnimeWatcher.Views;

// TODO: Update NavigationViewItem titles and icons in ShellPage.xaml.
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);
         
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        App.MainWindow.SetIcon("Assets/AnimeWatcher_icon.ico");
        var setting=(NavigationViewItem)NavigationViewControl.SettingsItem;
        if(setting != null){
            setting.Content = "Settings";
        }
        AppTitleBarText.Text = "AppDisplayName".GetLocalized(); 
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
        var setting=(NavigationViewItem)NavigationViewControl.SettingsItem;
        setting.Content = "Settings";
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        App.AppTitlebar = AppTitleBarText as UIElement;
        var setting=(NavigationViewItem)NavigationViewControl.SettingsItem;
        if(setting != null){
            setting.Content = "Settings";
        }
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
        var setting=(NavigationViewItem)NavigationViewControl.SettingsItem;
        if(setting != null){
            setting.Content = "Settings";
        }
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }
}
