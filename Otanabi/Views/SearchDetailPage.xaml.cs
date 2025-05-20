using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Otanabi.Core.Models;
using Otanabi.Core.Services;
using Otanabi.ViewModels;

namespace Otanabi.Views;

public sealed partial class SearchDetailPage : Page
{
    private int AnimeId;
    private List<FavoriteList> favoriteLists;
    private List<FavoriteList> selectedFList = new();
    DatabaseService dbService = new();

    public SearchDetailViewModel ViewModel
    {
        get;
    }

    public SearchDetailPage()
    {
        ViewModel = App.GetService<SearchDetailViewModel>();
        InitializeComponent();
    }

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {

        if (e.Parameter is Anime anime)
        {
            AnimeId = anime.Id;
            // await LoadAnimeFavList();
        }
        base.OnNavigatedTo(e);
    }

    private async Task LoadAnimeFavList()
    {

        var sFList1 = await dbService.GetFavoriteListByAnime(AnimeId);
        var tmpLit = new List<FavoriteList>();

        foreach (var item in sFList1)
        {
            foreach (var item1 in favListbox.Items)
            {
                if (item1 is FavoriteList ls && ls.Id == item.Id)
                {
                    favListbox.SelectedItems.Add(item1);
                }
            }
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        if (e.NavigationMode == NavigationMode.Back)
        {

        }
    }

    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem != null)
        {
            ViewModel.OpenPlayer((Chapter)e.ClickedItem);
        }

    }

    private async void FavoriteCombo_IsEnabledChanged(object sender, Microsoft.UI.Xaml.DependencyPropertyChangedEventArgs e)
    {
        if (animetxtid.Tag is int aid)
        {
            AnimeId = aid;
        }
        if (sender is Button cb && cb.IsEnabled)
        {
            await LoadAnimeFavList();
        }
    }

    private void ImageExControl_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ImagePopup.IsOpen = true;
    }

    private void ClosePopup_Click(object sender, RoutedEventArgs e)
    {
        ImagePopup.IsOpen = false;
    }

    private void ListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {// Obtén la posición del clic en el ListView
        var position = e.GetPosition(sender as UIElement);

        // Obtén el elemento que fue tocado (en este caso un FrameworkElement)
        var originalSource = e.OriginalSource as FrameworkElement;

        // Obtén el objeto 'Chapter' asociado al elemento
        if (originalSource?.DataContext is Chapter chapter)
        {
            // Crear el MenuFlyout
            var menuFlyout = new MenuFlyout();

            menuFlyout.Opening += MenuFlyout_Opening;

            // Agregar elementos al menú contextual
            //var viewDetailsItem = new MenuFlyoutItem { Text = "View Details" };
            //viewDetailsItem.Click += (s, args) => ViewDetails_Click(chapter);
            //menuFlyout.Items.Add(viewDetailsItem);

            var markAsWatchedItem = new MenuFlyoutItem { Text = "Mark as Watched" };
            markAsWatchedItem.Click += (s, args) => MarkAsWatched_Click(chapter);
            menuFlyout.Items.Add(markAsWatchedItem);

            //var addToFavoritesItem = new MenuFlyoutItem { Text = "Add to Favorites" };
            //addToFavoritesItem.Click += (s, args) => AddToFavorites_Click(chapter);
            //menuFlyout.Items.Add(addToFavoritesItem);

            // Mostrar el menú contextual en la posición donde se hizo clic
            menuFlyout.ShowAt(sender as UIElement, position);
        }
    }

    private void MenuFlyout_Opening(object sender, object e)
    {
        // Asegúrate de que el código se ejecute en el hilo correcto y que la ventana esté disponible
        var coreWindow = Window.Current?.CoreWindow;

        if (coreWindow != null)
        {
            // Restablecer el cursor al valor predeterminado
            coreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }
    }

    private void ViewDetails_Click(Chapter chapter)
    {
        // Lógica para ver detalles del capítulo
        // Por ejemplo, navegar a una nueva página con más información
        Debug.WriteLine($"Ver detalles del capítulo: {chapter.ChapterNumber}");
    }

    private void MarkAsWatched_Click(Chapter chapter)
    {
        // Lógica para marcar el capítulo como visto
        Debug.WriteLine($"Marcar el capítulo {chapter.ChapterNumber} como visto");
    }

    private void AddToFavorites_Click(Chapter chapter)
    {
        // Lógica para agregar el capítulo a los favoritos
        Debug.WriteLine($"Agregar el capítulo {chapter.ChapterNumber} a favoritos");
    }
}
