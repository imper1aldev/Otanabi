namespace AnimeWatcher.Core.Contracts.Extractors;
public interface IExtractor
{
     Task<string> GetStreamAsync(string url);
}
