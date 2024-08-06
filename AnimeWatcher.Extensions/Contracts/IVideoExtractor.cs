namespace AnimeWatcher.Extensions.Contracts;
public interface IVideoExtractor
{
     Task<string> GetStreamAsync(string url);
}
