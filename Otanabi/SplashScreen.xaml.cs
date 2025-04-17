using Microsoft.UI.Xaml;
using Otanabi.Core.Database;

namespace Otanabi;

public sealed partial class SplashScreen : WinUIEx.SplashScreen
{
    public SplashScreen(Window window)
        : base(window)
    {
        InitializeComponent();
        this.SystemBackdrop = new TransparentTintBackdrop();
    }

    protected override async Task OnLoading()
    {
        //maybe ill move the sync methods here
        var db = new DatabaseHandler();
        await db.InitDb();
        await Task.Delay(1000);
    }
}
