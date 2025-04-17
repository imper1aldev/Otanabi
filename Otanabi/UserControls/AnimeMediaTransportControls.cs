using System.Diagnostics;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Otanabi.ViewModels;

namespace Otanabi.UserControls;

public sealed partial class AnimeMediaTransportControls : MediaTransportControls
{
    private AppBarButton _serversButton;

    private readonly MenuFlyout _serverFlyout = new() { Placement = FlyoutPlacementMode.Top };

    public AnimeMediaTransportControls()
    {
        DefaultStyleKey = typeof(AnimeMediaTransportControls);
    }

    public static readonly DependencyProperty FullScreenCommandProperty = DependencyProperty.Register(
        nameof(FullScreenCommand),
        typeof(ICommand),
        typeof(AnimeMediaTransportControls),
        new PropertyMetadata(null)
    );
    public static readonly DependencyProperty PreviusCommandProperty = DependencyProperty.Register(
        nameof(PreviusCommand),
        typeof(ICommand),
        typeof(AnimeMediaTransportControls),
        new PropertyMetadata(null)
    );
    public static readonly DependencyProperty NextCommandProperty = DependencyProperty.Register(
        nameof(NextCommand),
        typeof(ICommand),
        typeof(AnimeMediaTransportControls),
        new PropertyMetadata(null)
    );

    public static readonly DependencyProperty OpenPanelCommandProperty = DependencyProperty.Register(
        nameof(OpenPanelCommand),
        typeof(ICommand),
        typeof(AnimeMediaTransportControls),
        new PropertyMetadata(null)
    );
    public static readonly DependencyProperty SkipIntroCommandProperty = DependencyProperty.Register(
        nameof(SkipIntroCommand),
        typeof(ICommand),
        typeof(AnimeMediaTransportControls),
        new PropertyMetadata(null)
    );

    public static readonly DependencyProperty IsNextEnabledProperty = DependencyProperty.Register(
        nameof(IsNextEnabled),
        typeof(bool),
        typeof(AnimeMediaTransportControls),
        new PropertyMetadata(
            default(bool),
            propertyChangedCallback: (d, e) =>
            {
                if (d is AnimeMediaTransportControls control)
                {
                    var button = control.GetTemplateChild("NextTrackButton") as AppBarButton;
                    if (button != null)
                    {
                        button.IsEnabled = (bool)e.NewValue;
                        if ((bool)e.NewValue)
                        {
                            VisualStateManager.GoToState((Control)control.GetTemplateChild("NextTrackButton"), "Normal", true);
                        }
                        else
                        {
                            VisualStateManager.GoToState((Control)control.GetTemplateChild("NextTrackButton"), "Disabled", true);
                        }
                    }
                }
            }
        )
    );
    public static readonly DependencyProperty IsPrevEnabledProperty = DependencyProperty.Register(
        nameof(IsPrevEnabled),
        typeof(bool),
        typeof(AnimeMediaTransportControls),
        new PropertyMetadata(
            default(bool),
            propertyChangedCallback: (d, e) =>
            {
                if (d is AnimeMediaTransportControls control)
                {
                    var button = control.GetTemplateChild("PreviousTrackButton") as AppBarButton;
                    if (button != null)
                    {
                        button.IsEnabled = (bool)e.NewValue;
                        if ((bool)e.NewValue)
                        {
                            VisualStateManager.GoToState((Control)control.GetTemplateChild("PreviousTrackButton"), "Normal", true);
                        }
                        else
                        {
                            VisualStateManager.GoToState((Control)control.GetTemplateChild("PreviousTrackButton"), "Disabled", true);
                        }
                    }
                }
            }
        )
    );
    public static readonly DependencyProperty IsPanelOpenProperty = DependencyProperty.Register(
        nameof(IsPanelOpen),
        typeof(bool),
        typeof(AnimeMediaTransportControls),
        new PropertyMetadata(false)
    );
    public static readonly DependencyProperty IsFullScreenProperty = DependencyProperty.Register(
        nameof(IsFullScreen),
        typeof(bool),
        typeof(AnimeMediaTransportControls),
        new PropertyMetadata(
            default(bool),
            new PropertyChangedCallback(
                (d, e) =>
                {
                    if (d is AnimeMediaTransportControls control)
                    {
                        var button = control.GetTemplateChild("FullScreenButton") as AppBarWindowPresenterStateButton;
                        if (button != null)
                        {
                            button.IsFullScreen = (bool)e.NewValue;
                        }
                    }
                }
            )
        )
    );
    public static readonly DependencyProperty ServersProperty = DependencyProperty.Register(
        nameof(Servers),
        typeof(IEnumerable<string>),
        typeof(AnimeMediaTransportControls),
        new PropertyMetadata(null, OnServersChanged)
    );

    public IEnumerable<string> Servers
    {
        get => (IEnumerable<string>)GetValue(ServersProperty);
        set => SetValue(ServersProperty, value);
    }

    public bool IsNextEnabled
    {
        get => (bool)GetValue(IsNextEnabledProperty);
        set => SetValue(IsNextEnabledProperty, value);
    }
    public bool IsPrevEnabled
    {
        get => (bool)GetValue(IsPrevEnabledProperty);
        set => SetValue(IsPrevEnabledProperty, value);
    }
    public bool IsPanelOpen
    {
        get => (bool)GetValue(IsPanelOpenProperty);
        set => SetValue(IsPanelOpenProperty, value);
    }
    public bool IsFullScreen
    {
        get => (bool)GetValue(IsFullScreenProperty);
        set => SetValue(IsFullScreenProperty, value);
    }
    public ICommand FullScreenCommand
    {
        get => (ICommand)GetValue(FullScreenCommandProperty);
        set => SetValue(FullScreenCommandProperty, value);
    }
    public ICommand PreviusCommand
    {
        get => (ICommand)GetValue(PreviusCommandProperty);
        set => SetValue(PreviusCommandProperty, value);
    }
    public ICommand NextCommand
    {
        get => (ICommand)GetValue(NextCommandProperty);
        set => SetValue(NextCommandProperty, value);
    }
    public ICommand OpenPanelCommand
    {
        get => (ICommand)GetValue(OpenPanelCommandProperty);
        set => SetValue(OpenPanelCommandProperty, value);
    }
    public ICommand SkipIntroCommand
    {
        get => (ICommand)GetValue(SkipIntroCommandProperty);
        set => SetValue(SkipIntroCommandProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild("FullScreenButton") is AppBarButton fullScreenButton)
        {
            fullScreenButton.Click += (s, e) => FullScreenCommand?.Execute(null);
        }
        if (GetTemplateChild("PreviousTrackButton") is AppBarButton prevButton)
        {
            prevButton.Click += (s, e) => PreviusCommand?.Execute(null);
        }
        if (GetTemplateChild("NextTrackButton") is AppBarButton nextButton)
        {
            nextButton.Click += (s, e) => NextCommand?.Execute(null);
        }
        if (GetTemplateChild("OpenPanelButton") is ToggleButton openPanelButton)
        {
            openPanelButton.Click += (s, e) => OpenPanelCommand?.Execute(null);
        }
        if (GetTemplateChild("SkipIntroButton") is Button skipIntroButton)
        {
            skipIntroButton.Click += (s, e) => SkipIntroCommand?.Execute(null);
        }
        _serversButton = GetTemplateChild("ServersButton") as AppBarButton;
        if (_serversButton != null)
        {
            _serversButton.Flyout = _serverFlyout;
        }
    }

    private static void OnServersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Debug.WriteLine("OnServersChanged ejecutado.");

        var mtc = d as AnimeMediaTransportControls;
        var flyout = mtc._serverFlyout;

        if (mtc._serversButton is null)
        {
            Debug.WriteLine("QualitiesButton es null.");
            return;
        }

        Debug.WriteLine($"Número de servidores: {(e.NewValue as IEnumerable<string>)?.Count()}");

        foreach (var item in flyout.Items.OfType<MenuFlyoutItem>())
        {
            item.Click -= mtc.ServerFlyoutItem_Click;
        }

        flyout.Items.Clear();

        if (e.NewValue is IEnumerable<string> servers)
        {
            var serverList = servers.ToList();
            if (serverList.Count == 1)
            {
                mtc._serversButton.Visibility = Visibility.Collapsed;
            }
            else if (serverList.Count > 1)
            {
                mtc._serversButton.IsEnabled = true;
                mtc._serversButton.Visibility = Visibility.Visible;
                foreach (var server in serverList)
                {
                    var flyoutItem = new MenuFlyoutItem { Text = server };
                    flyoutItem.Click += mtc.ServerFlyoutItem_Click;
                    flyout.Items.Add(flyoutItem);
                }
            }
        }
    }

    private void ServerFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var selectedServer = (sender as MenuFlyoutItem).Text;
        if (DataContext is VideoPlayerViewModel viewModel)
        {
            viewModel.SelectServerCommand.Execute(selectedServer);
        }
    }

    public void UpdateServers(IEnumerable<string> servers, string selectedServer)
    {
        if (_serversButton != null)
        {
            UpdateFlyout(servers, selectedServer);
        }
    }

    private void UpdateFlyout(IEnumerable<string> servers, string selectedServer)
    {
        foreach (var item in _serverFlyout.Items.OfType<MenuFlyoutItem>())
        {
            item.Click -= ServerFlyoutItem_Click;
        }

        _serverFlyout.Items.Clear();

        var serverList = servers.ToList();
        if (serverList.Count == 1)
        {
            _serversButton.Visibility = Visibility.Collapsed;
        }
        else if (serverList.Count > 1)
        {
            _serversButton.IsEnabled = true;
            _serversButton.Visibility = Visibility.Visible;
            foreach (var server in serverList)
            {
                var flyoutItem = new MenuFlyoutItem { Text = server };
                if (server == selectedServer)
                {
                    flyoutItem.IsEnabled = false;
                    flyoutItem.Icon = new SymbolIcon(Symbol.Accept);
                }
                flyoutItem.Click += ServerFlyoutItem_Click;
                _serverFlyout.Items.Add(flyoutItem);
            }
        }
    }
}
