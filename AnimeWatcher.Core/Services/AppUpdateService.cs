using System.Diagnostics;
using System.Reflection;
using AnimeWatcher.Core.Helpers;
namespace AnimeWatcher.Core.Services;
public class AppUpdateService
{
    private readonly HttpService _http = new();
    private readonly ClassReflectionHelper reflectionHelper = new();
    private readonly string gitUrl = "https://raw.githubusercontent.com/havsalazar/AnimeWatcher/master/AnimeWatcher/version.v";
    private readonly string gitRelease = "https://api.github.com/repos/havsalazar/AnimeWatcher/releases";

    public async Task<String> CheckGitHubVersion()
    {
        var response = await _http.GetAsync(gitUrl);
        return response;
    }
    public async Task<string> GetReleaseNotes()
    {
        var response = await _http.GetAsync($"{gitRelease}/latest");
        return response;
    }
    public async Task<string> GetReleaseNotes(string version)
    {
        var response = await _http.GetAsync($"{gitRelease}/tags/v{version}");
        return response;
    }

    public async Task<(int, Version)> CheckMainUpdates()
    {
        var gitResponse = await CheckGitHubVersion();
        var gitVersion = new Version(gitResponse);
        var currVersion = Assembly.GetExecutingAssembly().GetName().Version;

        var result = currVersion.CompareTo(gitVersion);
        return (result, gitVersion);
    }
    public Version GetExtensionVer()
    {
        var assemblyPath = reflectionHelper.GetAssemblyPath();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assemblyPath);
        return new Version(fvi.FileVersion);
    }
    public void RestartApp()
    {
        Process.Start(AppDomain.CurrentDomain.FriendlyName);
        Environment.Exit(0);
    }
}
