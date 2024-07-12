using AnimeWatcher.Core.Contracts.VideoExtractors;

namespace AnimeWatcher.Core.VideoExtractors;
internal class JuroExtractor : IVideoExtractor
{
    public async Task<string> GetStreamAsync(string url)
    {
        await Task.CompletedTask;
        return url;
    }
}
