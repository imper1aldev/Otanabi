namespace AnimeWatcher.Extensions.Contracts.VideoExtractors;
public interface IVideoExtractor
{
    Task<string> GetStreamAsync(string url);
}
