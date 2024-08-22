using System.Diagnostics; 
using System.Reflection;
using AnimeWatcher.Core.Helpers;
using Newtonsoft.Json.Linq;

namespace AnimeWatcher.Core.Services;

public class AppUpdateService
{
    private readonly HttpService _http = new();
    private readonly ClassReflectionHelper reflectionHelper = new();
    private readonly string gitUrl =
        "https://raw.githubusercontent.com/havsalazar/AnimeWatcher/master/AnimeWatcher/version.v";
    private readonly string gitRelease =
        "https://api.github.com/repos/havsalazar/AnimeWatcher/releases";
    internal string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:126.0) Gecko/20100101 Firefox/126.0";

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
    public async Task UpdateApp()
    { 
        var tag = await GetLastReleaseTag();
        var updateUrl = $"https://github.com/havsalazar/AnimeWatcher/releases/download/{tag}/AnimeWatcher-{tag}-x64.zip";
        var currDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        await DownloadAndInstallUpdate(updateUrl, currDir);
    }

    public Version GetExtensionVer()
    {
        var assemblyPath = reflectionHelper.GetAssemblyPath();
        var fvi = FileVersionInfo.GetVersionInfo(assemblyPath);
        return new Version(fvi.FileVersion);
    }

    public void RestartApp()
    {
        Process.Start(AppDomain.CurrentDomain.FriendlyName);
        Environment.Exit(0);
    }

    public static async Task DownloadAndInstallUpdate(string url, string destinationFolder)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "animeupdate.zip");

        await DownloadFileAsync(url, tempFile);
        var ps1File = Path.Combine(destinationFolder, "update.ps1");
        var startInfo = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"{ps1File}\"",
            UseShellExecute = false
        };
        Process.Start(startInfo);
    }

    internal static async Task DownloadFileAsync(string url, string outputPath)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromMinutes(3);
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        using Stream contentStream = await response.Content.ReadAsStreamAsync(),
            fileStream = new FileStream(
                outputPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                8192,
                true
            );
        await contentStream.CopyToAsync(fileStream);
    }

    internal async Task<string> GetLastReleaseTag()
    {
        var url = $"{gitRelease}/latest";
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

        try
        {
            var responseBody = await client.GetStringAsync(url);
            var release = JObject.Parse(responseBody);

            var tagName = release["tag_name"].ToString();
            var releaseName = release["name"].ToString();
            var releaseDate = release["published_at"].ToString();
            return tagName;
        } catch (HttpRequestException e)
        {
            Debug.WriteLine("\nException Caught!");
            Debug.WriteLine("Message :{0} ", e.Message);
        }
        return "";
    }
}
