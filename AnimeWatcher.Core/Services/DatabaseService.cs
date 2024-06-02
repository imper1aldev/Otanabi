using System.Diagnostics;
using System.Security.Cryptography;
using System.Xml.Linq;
using AnimeWatcher.Core.Database;
using AnimeWatcher.Core.Models;

namespace AnimeWatcher.Core.Services;
public class DatabaseService
{
    public readonly DatabaseHandler DB = DatabaseHandler.GetInstance();
    private readonly SearchAnimeService _searchAnimeService = new();

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

    public async Task<Anime> GetAnimeOnDB(Anime request)
    {
        if (request.Url == null)
            return null;
        var animeDB = await GetAnimeByProv(request.Url, request.ProviderId);
        var chapters = new List<Chapter>();
        if (animeDB != null)
            chapters = await GetChaptersByAnime(animeDB.Id);
        if (chapters.Count > 0)
            animeDB.Chapters = chapters.ToArray();
        return animeDB;
    }
    public async Task<Anime> SaveAnime(Anime request)
    {
        request.LastUpdate = DateTime.Now;
        await DB._db.InsertAsync(request);
        var anime = await GetAnimeByProv(request.Url, request.ProviderId);
        return anime;
    }
    public async Task<Anime> UpsertAnime(Anime request, bool forceUpdate = false)
    {
        //
        var animeDB = await GetAnimeByProv(request.Url, request.ProviderId);
        if (animeDB != null && forceUpdate == false)
        {
            var lastUpdate = animeDB.LastUpdate;

            var diffOfDates = DateTime.Now - lastUpdate;

            if (diffOfDates.Days < 2)
            {
                return null;
            }

        }
        var animeSource = await _searchAnimeService.GetAnimeDetailsAsync(request);
        if (animeDB == null)
        {
            animeDB = await SaveAnime(animeSource);
            animeSource.Id = animeDB.Id;
        }
        // update animedata
        else
        {
            animeSource.Id = animeDB.Id;
            animeSource.LastUpdate = DateTime.Now;
            await DB._db.UpdateAsync(animeSource);
            animeDB = await GetAnimeByProv(animeSource.Url, animeSource.ProviderId);
        }


        var chapsSource = new List<Chapter>();
        foreach (var chap in animeSource.Chapters.ToList())
        {
            chap.AnimeId = animeDB.Id;
            chapsSource.Add(chap);
        }

        var chapsDB = await GetChaptersByAnime(animeDB.Id);
        if (chapsDB.Count == 0)
        {
            await DB._db.InsertAllAsync(chapsSource);
            chapsDB = await GetChaptersByAnime(animeDB.Id);
        }
        else
        {
            var chapstoadd = chapsSource.Where(c1 => !chapsDB.Any(c2 => c1.ChapterNumber == c2.ChapterNumber));
            await DB._db.InsertAllAsync(chapstoadd);
            chapsDB = await GetChaptersByAnime(animeDB.Id);
        }
        /**/
        animeDB.Chapters = chapsDB.ToArray();

        return animeDB;
    }

    private async Task<List<Chapter>> GetChaptersByAnime(int animeId)
    {
        var chapOnDb = await DB._db.Table<Chapter>().Where(c => c.AnimeId == animeId).ToListAsync();
        foreach (var c in chapOnDb)
        {
            c.History = await GetHistoryByCap(c.Id);
        }

        return chapOnDb;
    }
    private async Task<Anime> GetAnimeByProv(string Url, int ProviderId)
    {
        if (Url == null)
            return null;

        var exist = await DB._db.Table<Anime>().Where(a => a.Url == Url
        && a.ProviderId == ProviderId).FirstOrDefaultAsync();

        if (exist != null)
        {
            exist.Provider = await DB._db.Table<Provider>().Where(p => p.Id == ProviderId).FirstOrDefaultAsync();
            exist.ProviderId = exist.Provider.Id;
        }
        return exist;
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
    public async Task UpdateProgress(int historyId, long progress)
    {
        await DB._db.ExecuteAsync("update History set SecondsWatched=? where Id=?", progress, historyId);
    }
}
