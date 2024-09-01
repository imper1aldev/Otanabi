using Otanabi.Extensions.Contracts.VideoExtractors;

namespace Otanabi.Extensions.VideoExtractors;
internal class JuroExtractor : IVideoExtractor
{
    public async Task<string> GetStreamAsync(string url)
    {
        await Task.CompletedTask;
        return url;
    }
}
