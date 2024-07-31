namespace AnimeWatcher.Core.Services;
public class AppUpdateService
{
    private readonly HttpService _http = new();
    private readonly string githubUrl = "https://raw.githubusercontent.com/havsalazar/AnimeWatcher/master/AnimeWatcher/version.v";

    public async Task<String> CheckGitHubVersion()
    {
        var response = await _http.GetAsync(githubUrl);
        return response;
    }

}
