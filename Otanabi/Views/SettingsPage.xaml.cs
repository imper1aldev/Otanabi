using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Otanabi.ViewModels;

namespace Otanabi.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        ViewModel.OnPatchNotes += OnPatchNotes;
        ViewModel.OnUpdatePressed += OnUpdatePressed;
        InitializeComponent();
    }

    private async void OnPatchNotes(object sender, (string notes, string version, bool avaible) e)
    {
        var content = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new TextBlock
            {
                Width = 490,
                Text = e.notes,
                TextWrapping = TextWrapping.Wrap,
            },
        };

        var dialog = PatchNotesDialog;
        dialog.Width = 500;
        dialog.XamlRoot = this.XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = $"Patch notes Version {e.version}";
        dialog.Content = content;
        dialog.CloseButtonText = "Close";

        if (e.avaible)
        {
            dialog.IsPrimaryButtonEnabled = true;
            dialog.PrimaryButtonClick += async (s, e) => await ViewModel.UpdateApp();
            dialog.PrimaryButtonText = "Update";
        }
        else
        {
            dialog.IsPrimaryButtonEnabled = false;
        }

        await dialog.ShowAsync();
    }

    private async void OnUpdatePressed(object sender, bool e)
    {
        var dialog = PatchNotesDialog;
        if (dialog != null)
        {
            dialog.Hide();
            await Task.Delay(2000); // Wait for the dialog to close
        }
        var content = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,

            Children =
            {
                new TextBlock
                {
                    Text = "Updating",
                    FontSize = 20,
                    Margin = new Thickness(0, 0, 0, 20),
                },
                new ProgressRing
                {
                    IsActive = true,
                    Width = 50,
                    Height = 50,
                },
            },
        };
        if (dialog != null)
        {
            dialog.Content = content;
            dialog.Title = "";
            dialog.IsPrimaryButtonEnabled = false;
            dialog.CloseButtonText = "Please wait ...";
            dialog.IsEnabled = false;
            dialog.Closing += DialogClosingEvent;
            await dialog.ShowAsync();
        }
    }

    private void DialogClosingEvent(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        // This mean user does click on Primary or Secondary button
        if (args.Result == ContentDialogResult.None)
        {
            args.Cancel = true;
        }
    }

    private void CloseDeleteFlyout(object sender, RoutedEventArgs e)
    {
        deletedbfly.Hide();
    }
}
