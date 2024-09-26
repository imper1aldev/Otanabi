using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Otanabi.Core.Models;
using Windows.UI;

namespace Otanabi.UserControls;

public sealed partial class AnimePaneControl : UserControl
{
    public AnimePaneControl()
    {
        this.InitializeComponent();
    }

    private readonly SolidColorBrush NormalStateColor = new(Color.FromArgb(255, 0, 0, 0));
    private readonly SolidColorBrush FocusStateColor = new(Color.FromArgb(255, 0, 0, 0));

    public ObservableCollection<Anime> Items { get; } = new ObservableCollection<Anime>();
    public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        "ItemsSource",
        typeof(ObservableCollection<Anime>),
        typeof(AnimePaneControl),
        null
    );

    public ObservableCollection<Anime> ItemsSource
    {
        get => (ObservableCollection<Anime>)GetValue(ItemsSourceProperty);
        set
        {
            SetValue(ItemsSourceProperty, value);
            Items.Clear();
            foreach (var item in value)
            {
                Items.Add(item);
            }
        }
    }

    private void Card_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            var bg = (Border)this.Resources["HoverBg"];
            border.Background = bg.Background;
            var prev = (Grid)border.Parent;
            border.TabIndex = 2;
            var dd = typeof(Border);
            SetScaleAnimation(border, 1, 1.025, 0.2);
        }
    }

    private void Card_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            var bg = (Border)this.Resources["NormalBg"];
            border.Background = bg.Background;
            SetScaleAnimation(border, 1.025, 1, 0.2);
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
            AutoReverse = false
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
            AutoReverse = false
        };

        Storyboard.SetTarget(scaleYAnimation, target);
        Storyboard.SetTargetProperty(scaleYAnimation, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
        storyboard.Children.Add(scaleYAnimation);

        // Start the animation
        storyboard.Begin();
    }
}
