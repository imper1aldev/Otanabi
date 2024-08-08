using AnimeWatcher.ViewModels;
using AnimeWatcher.Views.Embeddeds;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AnimeWatcher.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        ViewModel.onPatchNotes += OnPatchNotes;
        InitializeComponent();
    }


    private async void OnPatchNotes(object sender, (string notes, string version) e)
    {
        var dialog = new PatchNotesDialog();
        dialog.XamlRoot = this.XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = $"Patch notes Version {e.version}";
        dialog.Content = e.notes;
        dialog.CloseButtonText = "Ok";

        await dialog.ShowAsync();
    }
}
