using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Helpers;

public static class WinSparkle
{
    [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_init();

    [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_cleanup();

    [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_check_update_with_ui();

    [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_set_appcast_url(string url);
}
