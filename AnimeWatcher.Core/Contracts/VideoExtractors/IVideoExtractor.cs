namespace AnimeWatcher.Core.Contracts.VideoExtractors;
public interface IVideoExtractor
{
     Task<string> GetStreamAsync(string url);
}
