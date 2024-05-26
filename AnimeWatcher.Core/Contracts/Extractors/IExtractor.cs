using AnimeWatcher.Core.Models;

namespace AnimeWatcher.Core.Contracts.Extractors;
public interface IExtractor
{   
    public Task<Anime[]> SearchAnimeAsync(string searchTerm,int page);
    public Task<Anime> GetAnimeDetailsAsync(string requestUrl);
    public Task<VideoSource[]> GetVideoSources(string requestUrl);
}
