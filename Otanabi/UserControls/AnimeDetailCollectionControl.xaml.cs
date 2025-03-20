using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Otanabi.Core.Models;
using Otanabi.Core.Services;

namespace Otanabi.UserControls;

public sealed partial class AnimeDetailCollectionControl
{
    public AnimeDetailCollectionControl()
    {
        this.InitializeComponent();
    }

    public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        "ItemsSource",
        typeof(ObservableCollection<Anime>),
        typeof(AnimeDetailCollectionControl),
        null
    );

    public ObservableCollection<Anime> ItemsSource
    {
        get => (ObservableCollection<Anime>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
}
