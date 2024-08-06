using AnimeWatcher.Core.Models;



namespace AnimeWatcher.Extensions.Contracts.Extractors;

public interface IExtractor
{
    public Task<IAnime[]> MainPageAsync(int page = 1);
    public Task<IAnime[]> SearchAnimeAsync(string searchTerm, int page);
    public Task<IAnime> GetAnimeDetailsAsync(string requestUrl);
    public Task<IVideoSource[]> GetVideoSources(string requestUrl);
    public IProvider GenProvider(); 
}
