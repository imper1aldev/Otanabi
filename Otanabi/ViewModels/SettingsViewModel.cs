﻿using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using Otanabi.Contracts.Services;
using Otanabi.Contracts.ViewModels;
using Otanabi.Core.Models;
using Otanabi.Core.Services;
using Otanabi.Helpers;
using Windows.ApplicationModel;

namespace Otanabi.ViewModels;

public partial class SettingsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly SearchAnimeService _searchAnimeService = new();
    private readonly AppUpdateService _appUpdateService = new();

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private string versionMessage = "";

    [ObservableProperty]
    private string versionExtensions = "";

    [ObservableProperty]
    private string _appName = "";

    [ObservableProperty]
    private int selectedThemeIndex;

    [ObservableProperty]
    private bool isNsfwEnabled;

    private bool updateAvailable = false;

    public ICommand SwitchThemeCommand
    {
        get;
    }

    [ObservableProperty]
    private Provider selectedProvider;
    public ObservableCollection<Provider> Providers { get; } = [];

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        ILocalSettingsService localSettingsService
    )
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _localSettingsService = localSettingsService;
        _versionDescription = GetVersionDescription();
        _appName = GetAppName();
        GetExtensionVersion();

        SwitchThemeCommand = new RelayCommand<ComboBox>(
            async (cb) =>
            {
                var selected = ElementTheme.Dark;
                selected = cb.SelectedIndex switch
                {
                    0 => ElementTheme.Light,
                    1 => ElementTheme.Dark,
                    2 => ElementTheme.Default,
                    _ => ElementTheme.Default,
                };
                await _themeSelectorService.SetThemeAsync(selected);
            }
        );
    }

    private void GetExtensionVersion()
    {
        var currv = _appUpdateService.GetExtensionVer().ToString();
        VersionExtensions = $"Extensions version : {currv}";
    }

    private string GetAppName()
    {
        return $"{"AppDisplayName".GetLocalized()}";
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(
                packageVersion.Major,
                packageVersion.Minor,
                packageVersion.Build,
                packageVersion.Revision
            );
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"Version - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    public async void OnNavigatedTo(object parameter)
    {
        //IsNsfwEnabled = await _localSettingsService.ReadSettingAsync<bool>("EnableNsfwContent");

        await GetProviders();

        var provdef = await _localSettingsService.ReadSettingAsync<int>("ProviderId");

        if (provdef != 0)
        {
            var tmp = Providers.FirstOrDefault(p => p.Id == provdef);
            SelectedProvider = tmp ?? new();
        }

        var currentTheme = _themeSelectorService.Theme;

        switch (currentTheme)
        {
            case ElementTheme.Light:
                SelectedThemeIndex = 0;
                break;
            case ElementTheme.Dark:
                SelectedThemeIndex = 1;
                break;
            case ElementTheme.Default:
                SelectedThemeIndex = 2;
                break;
        }

        if (parameter is bool isUpdate && isUpdate)
        {
            await CheckUpdates();
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private async Task GetProviders()
    {
        Providers.Clear();
        var bNsfwEnabled = await _localSettingsService.ReadSettingAsync<bool>("EnableNsfwContent");
        if (bNsfwEnabled)
        {
            IsNsfwEnabled = bNsfwEnabled;
        }
        var provs = _searchAnimeService.GetProviders();
        foreach (var item in provs.Where(p => IsNsfwEnabled || !p.IsNsfw).OrderBy(x => x.Id))
        {
            Providers.Add(item);
        }
        SelectedProvider = provs[0];

        var selectedProviderId = await _localSettingsService.ReadSettingAsync<int>("ProviderId");
        if (selectedProviderId != 0)
        {
            var tmp = Providers.FirstOrDefault(p => p.Id == selectedProviderId);
            if (tmp != null)
            {
                SelectedProvider = tmp;
            }
        }
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ChangedProvider()
    {
        if (SelectedProvider != null)
        {
            await _localSettingsService.SaveSettingAsync<int>("ProviderId", SelectedProvider.Id);
        }
    }

    partial void OnIsNsfwEnabledChanged(bool value)
    {
        EnableNsfwCommand.Execute(null);
    }

    [RelayCommand]
    private async Task EnableNsfw(object parameter)
    {
        await _localSettingsService.SaveSettingAsync<bool>("EnableNsfwContent", IsNsfwEnabled);
        await GetProviders();
    }

    [RelayCommand]
    private async Task CheckUpdates()
    {
        try
        {
            VersionMessage = "Searching for updates";

            var result = await _appUpdateService.CheckMainUpdates();
            var verEval = result.Item1;
            var version = result.Item2;
            if (verEval > 0)
            {
                VersionMessage =
                    "The running version is higher than the main version; it is not recommended to update in debug mode.";
                OnPatchNotes(
                    this,
                    (
                        "This action will break the application, be aware \n"
                        + "The running version is higher than the main version; it is not recommended to update in debug mode."
                        ,
                        version.ToString(),
                        true
                    )
                );
            }
            else if (verEval < 0)
            {
                VersionMessage = "Update Available";
                updateAvailable = true;
                var tmpNotes = await _appUpdateService.GetReleaseNotes();
                var patchNotes = (string)JObject.Parse(tmpNotes)["body"];

                OnPatchNotes(this, (patchNotes, version.ToString(), updateAvailable));
            }
            else
            {
                VersionMessage = "This version is the latest, no update required";
                updateAvailable = false;
            }
        }
        catch (Exception e)
        {
            VersionMessage = e.ToString();
        }
    }

    public async Task UpdateApp()
    {
        OnUpdatePressed(this, true);
        await _appUpdateService.UpdateApp();
    }

    [RelayCommand]
    private async Task CheckPatchNotes()
    {
        var version = _appUpdateService.GetCurrVersion();
        var tmpNotes = await _appUpdateService.GetReleaseNotes(version);
        var patchNotes = (string)JObject.Parse(tmpNotes)["body"];
        OnPatchNotes(this, (patchNotes, version.ToString(), false));
    }

    public event EventHandler<(string Notes, string version, bool IsAvaible)> OnPatchNotes;
    public event EventHandler<bool> OnUpdatePressed;
}
