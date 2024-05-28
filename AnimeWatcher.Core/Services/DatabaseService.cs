using System.Diagnostics;
using System.Security.Cryptography;
using System.Xml.Linq;
using AnimeWatcher.Core.Database;
using AnimeWatcher.Core.Models;

namespace AnimeWatcher.Core.Services;
public class DatabaseService
{
    public readonly DatabaseHandler DB = new();

    public DatabaseService()
    {

    }
    public async Task<FavoriteList[]> GetFavoriteLists()
    {
        var FavLists = await DB._db.Table<FavoriteList>().ToListAsync();

        return FavLists.ToArray();

    }
    public async Task<bool> IsFavorite(int animeId)
    {
        var el = await DB._db.Table<AnimexFavorite>().Where(
                af => af.AnimeId == animeId).FirstOrDefaultAsync();

        return el != null ? true : false;

    }

    public async Task<string> AddToFavorites(Anime anime, string action, int favList = 1)
    {

        if (action == "add")
        {
            Debug.WriteLine(anime);
            var favxanime = new AnimexFavorite() { AnimeId = anime.Id, FavoriteListId = favList };
            await DB._db.InsertAsync(favxanime);
            return "added";
        }
        else
        {
            var el = await DB._db.Table<AnimexFavorite>().Where(
                af => af.AnimeId == anime.Id).ToListAsync();

            if (el.Count > 0)
            {
                foreach (var item in el)
                {
                    await DB._db.ExecuteAsync("delete from AnimexFavorite where Id=?", item.Id);
                }
                return "deleted";
            }
            return "deleted";
        }

    }
    public async Task<Anime> CreateMinimalAnime(Anime anime)
    {

        var aOnDb = await GetAnimeByProv(anime);
        if (aOnDb == null)
        {
            await DB._db.InsertAsync(anime);
            aOnDb = await GetAnimeByProv(anime);
        }
        anime.Chapters.ToList().ForEach(c => c.AnimeId = aOnDb.Id);

        var chapOnDb = await GetChaptersByAnime(aOnDb.Id);
        if (chapOnDb.Count == 0)
        {
            await DB._db.InsertAllAsync(anime.Chapters);
            chapOnDb = await GetChaptersByAnime(aOnDb.Id);
            aOnDb.Chapters = chapOnDb.ToArray();
        }
        else
        {
            aOnDb.Chapters = chapOnDb.ToArray();
        }
        // inthernet says to compare 2 list is necessary to use Except var list3=list1.Except(list2).ToList();
        // other way using linq var result = customlist.Where(p => !otherlist.Any(l => p.someproperty == l.someproperty));


        return aOnDb;
    }
    private async Task<List<Chapter>> GetChaptersByAnime(int animeId)
    {
        var chapOnDb = await DB._db.Table<Chapter>().Where(c => c.AnimeId == animeId).ToListAsync(); 
        foreach(var c in chapOnDb)
        {
            c.History = await GetHistoryByCap(c.Id);
        }

        return chapOnDb;
    }


    private async Task<Anime> GetAnimeByProv(Anime anime)
    {
        var exist = await DB._db.Table<Anime>().Where(a => a.RemoteID == anime.RemoteID && a.ProviderId == anime.ProviderId).ToListAsync();
        if (exist.Count == 0)
        {
            return null;
        }
        return exist[0];
    }

    public async Task<Anime[]> GetFavAnimeByList(int favId)
    {
        var data = await DB._db.QueryAsync<Anime>("select a.* from AnimexFavorite as af inner join Anime as a on af.AnimeId=a.Id  where af.FavoriteListId=?", favId);

        foreach (var item in data)
        {
            var prov = await DB._db.Table<Provider>().Where(p => p.Id == item.ProviderId).FirstOrDefaultAsync();
            item.Provider = prov;
        }
        return data.ToArray();
    }

    public async Task<List<FavoriteList>> GetFavoriteListByAnime(int animeId)
    {
        var data = await DB._db.QueryAsync<FavoriteList>("select fl.* from AnimexFavorite as af inner join FavoriteList as fl" +
            " on af.FavoriteListId=fl.Id  where af.AnimeId=?", animeId);
        if (data.Count > 0)
        {
            return data;
        }
        return null;
    }
    public async Task UpdateAnimeList(int animeId, List<int> lists)
    {

        await DB._db.ExecuteAsync("delete from AnimexFavorite " +
            "where AnimeId=?", animeId);
        foreach (var item in lists)
        {
            var favxanime = new AnimexFavorite() { AnimeId = animeId, FavoriteListId = item };
            await DB._db.InsertAsync(favxanime);
        }
    }

    private async Task<History> GetHistoryByCap(int chapterId)
    {
        var history = await DB._db.Table<History>()
                .Where(h => h.ChapterId == chapterId).FirstOrDefaultAsync();

        return history;
    }

    public async Task<History> GetOrCreateHistoryByCap(int chapterId)
    {
        var history = await GetHistoryByCap(chapterId);
        if (history != null)
        {
            await DB._db.ExecuteAsync("update History set WatchedDate=? where Id=?", DateTime.Now, history.Id);
            history = await GetHistoryByCap(chapterId);
            return history;
        }
        var hisCl = new History() { ChapterId = chapterId, WatchedDate = DateTime.Now, SecondsWatched = 0 };
        await DB._db.InsertAsync(hisCl);

        history = await GetHistoryByCap(chapterId);

        return history;
    }
    public async Task UpdateProgress(int historyId,long progress)
    {
        await DB._db.ExecuteAsync("update History set SecondsWatched=? where Id=?",progress,historyId);
    }
}
