using AnimeWatcher.Activation;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Core.Contracts.Services;
using AnimeWatcher.Core.Services;
using AnimeWatcher.Core.Database;
using AnimeWatcher.Helpers;
using AnimeWatcher.Models;
using AnimeWatcher.Notifications;
using AnimeWatcher.Services;
using AnimeWatcher.ViewModels;
using AnimeWatcher.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using AnimeWatcher.Core.Flare;
using System.ComponentModel;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace AnimeWatcher;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }
    private readonly DispatcherQueue _dispatcherQueue;
    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar
    {
        get; set;
    }

    public App()
    {
        InitializeComponent();
         _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IWindowPresenterService, WindowPresenterService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<FavoritesViewModel>();
            services.AddTransient<FavoritesPage>();
            services.AddTransient<VideoPlayerViewModel>();
            services.AddTransient<VideoPlayerPage>();
            services.AddTransient<SearchDetailViewModel>();
            services.AddTransient<SearchDetailPage>();
            services.AddTransient<SearchViewModel>();
            services.AddTransient<SearchPage>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        //init database
        var db = new DatabaseHandler();
        var flareapp = new FlareSolverr();

        await db.InitDb();
        await App.GetService<IActivationService>().ActivateAsync(args);


        var bw = new BackgroundWorker();
        bw.DoWork += (sender, args) => _dispatcherQueue.TryEnqueue(async () =>
        {
            await flareapp.CheckFlareInstallation();
        });
        bw.RunWorkerAsync();


    }
}
