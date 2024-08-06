using AnimeWatcher.Extensions.Contracts.VideoExtractors;

namespace AnimeWatcher.Extensions.VideoExtractors;
internal class JuroExtractor : IVideoExtractor
{
    public async Task<string> GetStreamAsync(string url)
    {
        await Task.CompletedTask;
        return url;
    }
}
