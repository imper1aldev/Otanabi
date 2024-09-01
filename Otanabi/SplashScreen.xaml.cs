using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Otanabi.Core.Database;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUIEx;

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
