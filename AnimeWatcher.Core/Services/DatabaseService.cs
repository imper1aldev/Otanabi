using AnimeWatcher.Core.Database;
using AnimeWatcher.Core.Models;

namespace AnimeWatcher.Core.Services;
public class DatabaseService
{
    public readonly DatabaseHandler Database = new();

    public DatabaseService()
    {
        Database.InitDb();
    }

    public async Task<bool> AddToFavorites(Anime anime)
    {
        var fav = new Favorite();

        var provider = await GetOrCreateProvider(anime.Provider);

        anime.ProviderId = provider.Id;

        await Database._db.InsertAsync(anime).ContinueWith((a) =>
        {
            fav.AnimeId = a.Id;
        }); 
        anime.Chapters.ToList().ForEach(c => c.AnimeId = anime.Id);

        await Database._db.InsertAllAsync(anime.Chapters);
        return true;
    }
    private async Task<Provider> GetOrCreateProvider(Provider provi)
    {
        var data = await Database._db.Table<Provider>().Where((p) => p.Id == provi.Id).FirstOrDefaultAsync();
        if (data != null)
        {
            return data;
        }
        var tmp = await Database._db.InsertAsync(provi).ContinueWith<Provider>((a) => provi);

        return tmp;


    }

}
