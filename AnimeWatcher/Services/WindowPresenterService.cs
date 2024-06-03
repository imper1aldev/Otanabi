using System.Diagnostics;
using AnimeWatcher.Contracts.Services;

using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;


namespace AnimeWatcher.Services;
public class WindowPresenterService : IWindowPresenterService
{
    private readonly AppWindow _appWindow;
    private readonly WindowEx _window;

    public WindowPresenterService()
    {
        _window = App.MainWindow;

        var windowHandle = WindowNative.GetWindowHandle(_window);
        var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        _appWindow.Changed += AppWindow_Changed;
    }

    public event EventHandler? WindowPresenterChanged;

    public bool IsFullScreen => _appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen;

    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidPresenterChange)
        {
            //Debug.WriteLine("Presenter changed to {0}", sender.Presenter.Kind);
            WindowPresenterChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ToggleFullScreen()
    {
        if (IsFullScreen)
        {
            //Debug.WriteLine("Exiting fullscreen");
            _appWindow.SetPresenter(AppWindowPresenterKind.Default);
        }
        else
        {
            //Debug.WriteLine("Entering fullscreen");
            _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }
    }
}
