using System.Collections.Generic;
using System.Diagnostics;
using Otanabi.Core.Anilist.Models;
using Otanabi.Core.Database;
using Otanabi.Core.Models;

namespace Otanabi.Core.Services;

public class DatabaseService
{
    public readonly DatabaseHandler DB = DatabaseHandler.GetInstance();
    private readonly SearchAnimeService _searchAnimeService = new();

    public DatabaseService() { }

    public async Task Vacuum()
    {
        await DB._db.ExecuteAsync("vacuum");
    }

    public async Task<Anime> GetOrAddAnimeByMedia(Media media, Provider provider)
    {
        var id = media.Id;
        var anime = await DB._db.Table<Anime>().Where(a => a.IdAnilist == id && a.ProviderId == provider.Id).FirstOrDefaultAsync();
        if (anime == null)
        {
            anime = new Anime { IdAnilist = media.Id, ProviderId = provider.Id };
            await DB._db.InsertAsync(anime);
        }
        return anime;
    }

    public async Task<Anime> GetAnimeById(int mediaId, Provider provider)
    {
        var anime = await DB._db.Table<Anime>().Where(a => a.IdAnilist == mediaId && a.ProviderId == provider.Id).FirstOrDefaultAsync();
        return anime;
    }

    public async Task<Chapter> GetOrAddChapter(int animeId, int chapterNumber)
    {
        var chapter = await DB._db.Table<Chapter>().Where(a => a.AnimeId == animeId && a.ChapterNumber == chapterNumber).FirstOrDefaultAsync();
        if (chapter == null)
        {
            chapter = new Chapter { AnimeId = animeId, ChapterNumber = chapterNumber };
            await DB._db.InsertAsync(chapter);
            chapter = await DB._db.Table<Chapter>().Where(a => a.AnimeId == animeId && a.ChapterNumber == chapterNumber).FirstOrDefaultAsync();
        }
        return chapter;
    }

    public async Task<FavoriteList[]> GetFavoriteLists()
    {
        var FavLists = await DB._db.Table<FavoriteList>().ToListAsync();

        return FavLists.ToArray();
    }

    public async Task<Anime[]> GetFavAnimeByList(int favId)
    {
        var data = await DB._db.QueryAsync<Anime>(
            "select a.* from AnimexFavorite as af inner join Anime as a on af.AnimeId=a.Id  where af.FavoriteListId=?",
            favId
        );

        foreach (var item in data)
        {
            var prov = await DB._db.Table<Provider>().Where(p => p.Id == item.ProviderId).FirstOrDefaultAsync();
            item.Provider = prov;
        }
        return data.ToArray();
    }

    public async Task<bool> IsFavorite(int animeId)
    {
        var el = await DB._db.Table<AnimexFavorite>().Where(af => af.AnimeId == animeId).FirstOrDefaultAsync();

        return el != null;
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
        var hisCl = new History()
        {
            ChapterId = chapterId,
            WatchedDate = DateTime.Now,
            SecondsWatched = 0,
        };
        await DB._db.InsertAsync(hisCl);

        history = await GetHistoryByCap(chapterId);

        return history;
    }

    private async Task<History> GetHistoryByCap(int chapterId)
    {
        var history = await DB._db.Table<History>().Where(h => h.ChapterId == chapterId).FirstOrDefaultAsync();
        return history;
    }

    public async Task<List<FavoriteList>> GetFavoriteListByUrl(string url, int providerId)
    {
        var data = await DB._db.QueryAsync<FavoriteList>(
            """
            select fl.* from AnimexFavorite as af inner join FavoriteList as fl
            on af.FavoriteListId=fl.Id inner join Anime a on af.AnimeId=a.Id
             where a.Url=? and a.ProviderId=?
            """,
            url,
            providerId
        );
        if (data.Count > 0)
        {
            return data;
        }
        return null;
    }

    public async Task CreateFavorite(string fav)
    {
        var favorite = new FavoriteList { Name = fav };

        await DB._db.InsertAsync(favorite);
    }

    public async Task UpdateFavorite(FavoriteList favorite)
    {
        await DB._db.UpdateAsync(favorite);
    }

    public async Task DeleteFavorite(FavoriteList favorite)
    {
        await DB._db.ExecuteAsync("delete from AnimexFavorite where FavoriteListId=? and Id>1", favorite.Id);
        await DB._db.DeleteAsync(favorite);
    }
}
