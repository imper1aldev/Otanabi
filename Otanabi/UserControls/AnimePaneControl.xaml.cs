using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Otanabi.Core.Models;
using Otanabi.Core.Services;

namespace Otanabi.UserControls;

public sealed partial class AnimePaneControl : UserControl
{
    private readonly DatabaseService dbService = new();
    private readonly DispatcherQueue _dispatcherQueue;

    public AnimePaneControl()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        this.InitializeComponent();
    }

    public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        "ItemsSource",
        typeof(ObservableCollection<Anime>),
        typeof(AnimePaneControl),
        null
    );

    public ObservableCollection<Anime> ItemsSource
    {
        get => (ObservableCollection<Anime>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static DependencyProperty FavoriteListIdProperty = DependencyProperty.Register(
        "FavoriteListId",
        typeof(int),
        typeof(AnimePaneControl),
        null
    );
    public int FavoriteListId
    {
        get => (int)GetValue(FavoriteListIdProperty);
        set => SetValue(FavoriteListIdProperty, value);
    }
    private FavoriteList[] Favorites = [];

    private void Card_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            var bg = (Border)this.Resources["HoverBg"];
            border.Background = bg.Background;
            //SetScaleAnimation(border, 1, 1.025, 0.2);
        }
    }

    private void Card_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            //if (border.ContextFlyout != null && border.ContextFlyout.IsOpen)
            //{
            //    return;
            //}
            var bg = (Border)this.Resources["NormalBg"];
            border.Background = bg.Background;
            //SetScaleAnimation(border, 1.025, 1, 0.2);
        }
    }

    private void SetScaleAnimation(Border target, double from, double to, double duration)
    {
        var scaleTransform = new ScaleTransform();
        target.RenderTransform = scaleTransform;

        var storyboard = new Storyboard();

        // Define the animation for ScaleX
        var scaleXAnimation = new DoubleAnimation()
        {
            From = from,
            To = to,
            Duration = new Duration(TimeSpan.FromSeconds(duration)),
            AutoReverse = false,
        };
        Storyboard.SetTarget(scaleXAnimation, target);
        Storyboard.SetTargetProperty(scaleXAnimation, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        storyboard.Children.Add(scaleXAnimation);

        // Define the animation for ScaleY
        var scaleYAnimation = new DoubleAnimation()
        {
            From = from,
            To = to,
            Duration = new Duration(TimeSpan.FromSeconds(duration)),
            AutoReverse = false,
        };

        Storyboard.SetTarget(scaleYAnimation, target);
        Storyboard.SetTargetProperty(scaleYAnimation, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
        storyboard.Children.Add(scaleYAnimation);

        // Start the animation
        storyboard.Begin();
    }

    public event EventHandler<Anime> AnimeSelected;
    public event EventHandler<Anime> AddToFavorites;

    private async void SubMenuOpen(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Anime anime)
        {
            if (Favorites.Length == 0)
            {
                await LoadFavorites();
            }

            var mFlyout = new MenuFlyout();
            var _maxLength = 15;
            var animeTitle = anime.Title.ToString();
            if (anime.Title.Length > _maxLength)
            {
                animeTitle = animeTitle.Substring(0, _maxLength) + "...";
            }

            mFlyout.Items.Add(new MenuFlyoutItem() { Text = animeTitle, Command = new RelayCommand(() => AnimeSelected?.Invoke(this, anime)) });
            mFlyout.Items.Add(new MenuFlyoutSeparator());

            var fListMenu = new MenuFlyoutSubItem()
            {
                Text = "Add to Favorites",
                Icon = new FontIcon() { Glyph = "\uE710" },
            };

            var selectedFavs = await dbService.GetFavoriteListByUrl(anime.Url, anime.Provider.Id);

            foreach (var fav in Favorites)
            {
                var fItem = new ToggleMenuFlyoutItem() { Text = fav.Name };
                if (selectedFavs != null)
                {
                    var selectedFav = selectedFavs.FirstOrDefault(x => x.Id == fav.Id);
                    if (selectedFav != null)
                    {
                        fItem.IsChecked = true;
                    }
                    else
                    {
                        fItem.IsChecked = false;
                    }
                }
                fListMenu.Items.Add(fItem);
            }

            mFlyout.Items.Add(fListMenu);
            mFlyout.ShowAt(btn);
            Debug.WriteLine($"Submenu open tt{DateTime.Now}");
        }
    }

    private void Card_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        Debug.WriteLine($"Card pressed emiting AnimeSelected tt{DateTime.Now}");
        if (sender is Border bd && bd.DataContext is Anime anime)
        {
            AnimeSelected?.Invoke(this, anime);
        }
    }

    public async Task LoadFavorites()
    {
        var favs = await dbService.GetFavoriteLists();
        Favorites = favs;
    }
}
