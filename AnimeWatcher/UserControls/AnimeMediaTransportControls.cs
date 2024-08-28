using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls; 
using AnimeWatcher.ViewModels;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace AnimeWatcher.UserControls;

public sealed partial class AnimeMediaTransportControls : MediaTransportControls
{
    private VideoPlayerViewModel MViewModel => App.GetService<VideoPlayerViewModel>();

    public AnimeMediaTransportControls()
    {
        DefaultStyleKey = typeof(AnimeMediaTransportControls);
    }

    public static readonly DependencyProperty FullScreenCommandProperty =
        DependencyProperty.Register(
            nameof(FullScreenCommand),
            typeof(ICommand),
            typeof(AnimeMediaTransportControls),
            new PropertyMetadata(null)
        );
    public static readonly DependencyProperty PreviusCommandProperty =
        DependencyProperty.Register(
            nameof(PreviusCommand),
            typeof(ICommand),
            typeof(AnimeMediaTransportControls),
            new PropertyMetadata(null)
        );
    public static readonly DependencyProperty NextCommandProperty =
        DependencyProperty.Register(
            nameof(NextCommand),
            typeof(ICommand),
            typeof(AnimeMediaTransportControls),
            new PropertyMetadata(null)
        );

    public static readonly DependencyProperty OpenPanelCommandProperty =
        DependencyProperty.Register(
            nameof(OpenPanelCommand),
            typeof(ICommand),
            typeof(AnimeMediaTransportControls),
            new PropertyMetadata(null)
        );

    public static readonly DependencyProperty IsNextEnabledProperty =
        DependencyProperty.Register(
            nameof(IsNextEnabled),
            typeof(bool),
            typeof(AnimeMediaTransportControls),
            new PropertyMetadata(false)
        );
    public static readonly DependencyProperty IsPrevEnabledProperty =
                DependencyProperty.Register(
            nameof(IsPrevEnabled),
            typeof(bool),
            typeof(AnimeMediaTransportControls),
            new PropertyMetadata(false)
        );
     public static readonly DependencyProperty IsPanelOpenProperty =
                DependencyProperty.Register(
            nameof(IsPanelOpen),
            typeof(bool),
            typeof(AnimeMediaTransportControls),
            new PropertyMetadata(false)
        );

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
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if(GetTemplateChild("FullScreenButton") is Button fullScreenButton)
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
    }
}
