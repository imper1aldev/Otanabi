﻿using Otanabi.Core.Database;
using Otanabi.Core.Models;

namespace Otanabi.Core.Services;

public class DatabaseService
{
    public readonly DatabaseHandler DB = DatabaseHandler.GetInstance();
    private readonly SearchAnimeService _searchAnimeService = new();

    public DatabaseService()
    {
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

    public async Task<FavoriteList[]> GetFavoriteLists()
    {
        var FavLists = await DB._db.Table<FavoriteList>().ToListAsync();

        return FavLists.ToArray();
    }

    public async Task<bool> IsFavorite(int animeId)
    {
        var el = await DB._db.Table<AnimexFavorite>().Where(af => af.AnimeId == animeId).FirstOrDefaultAsync();

        return el != null;
    }

    public async Task<string> AddToFavorites(Anime anime, string action, int favList = 1)
    {
        if (action == "add")
        {
            var favxanime = new AnimexFavorite() { AnimeId = anime.Id, FavoriteListId = favList };
            await DB._db.InsertAsync(favxanime);
            return "added";
        }
        else
        {
            var el = await DB._db.Table<AnimexFavorite>().Where(af => af.AnimeId == anime.Id).ToListAsync();

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

    public async Task<string> UpsertAnimeFavorite(Anime anime, int favId)
    {
        var el = await DB._db.Table<AnimexFavorite>().Where(af => af.AnimeId == anime.Id && af.FavoriteListId == favId).FirstOrDefaultAsync();
        if (el == null)
        {
            var favxanime = new AnimexFavorite() { AnimeId = anime.Id, FavoriteListId = favId };
            await DB._db.InsertAsync(favxanime);
            return "added";
        }
        else
        {
            await DB._db.DeleteAsync(el);
            return "deleted";
        }
    }

    public async Task<Anime> GetAnimeOnDB(Anime request)
    {
        if (request.Url == null)
        {
            return null;
        }

        var animeDB = await GetAnimeByProv(request.Url, request.ProviderId);
        var chapters = new List<Chapter>();
        if (animeDB != null)
        {
            chapters = await GetChaptersByAnime(animeDB.Id);
        }

        if (chapters.Count > 0)
        {
            animeDB.Chapters = chapters.ToArray();
        }

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
        var persistency = request.Provider.Persistent;

        if (persistency)
        {
            var animeDB = await GetAnimeByProv(request.Url, request.ProviderId);
            if (animeDB != null && forceUpdate == false && persistency)
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

                if (forceUpdate)
                {
                    // update all chapters url 
                    foreach (var chap in chapsDB)
                    {
                        var updatechapData = chapsSource.FirstOrDefault(c => c.ChapterNumber == chap.ChapterNumber);
                        if (updatechapData != null)
                        {
                            chap.Url = updatechapData.Url;
                            await DB._db.UpdateAsync(chap);
                        }

                    }

                }

                chapsDB = await GetChaptersByAnime(animeDB.Id);
            }
            /**/
            animeDB.Chapters = chapsDB.ToArray();
            return animeDB;
        }
        else
        {
            var animeDB = await GetAnimeByProv(request.Url, request.ProviderId);
            if (animeDB != null && forceUpdate == false)
            {
                var lastUpdate = animeDB.LastUpdate;
                var diffOfDates = DateTime.Now - lastUpdate;
                if (diffOfDates.Days < 1)
                {
                    return null;
                }
            }
            var animeSourceList = await _searchAnimeService.SearchAnimeAsync(request.Title, 1, request.Provider);
            var animeSource = animeSourceList.Where(a => a.Title == request.Title).FirstOrDefault();

            if (animeSource != null)
            {
                if (animeDB == null)
                {
                    animeDB = await SaveAnime(animeSource);
                    animeDB = await GetAnimeByProv(request.Url, request.ProviderId);
                }
                var animeSourceDet = await _searchAnimeService.GetAnimeDetailsAsync(animeSource);

                animeSource.Id = animeDB.Id;
                animeSource.LastUpdate = DateTime.Now;
                animeSourceDet.Id = animeDB.Id;
                animeSourceDet.LastUpdate = DateTime.Now;

                await DB._db.UpdateAsync(animeSourceDet);
                animeDB = await GetAnimeByProv(animeSource.Url, animeSource.ProviderId);

                animeSource.Chapters = animeSourceDet.Chapters;
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
                var toInsert = new List<Chapter>();
                var toUpdate = new List<Chapter>();
                foreach (var chap in chapsSource)
                {
                    var chpDB = chapsDB.Where(c => c.Name == chap.Name).FirstOrDefault();
                    if (chpDB == null)
                    {
                        toInsert.Add(chap);
                    }
                    else
                    {
                        chap.Id = chpDB.Id;
                        toUpdate.Add(chap);
                    }
                }
                if (toInsert.Count > 0)
                {
                    await DB._db.InsertAllAsync(toInsert);
                }
                if (toUpdate.Count > 0)
                {
                    await DB._db.UpdateAllAsync(toUpdate);
                }
                chapsDB = await GetChaptersByAnime(animeDB.Id);
            }
            animeDB.Chapters = chapsDB.ToArray();

            return animeDB;
        }
    }

    public async Task<List<Chapter>> GetChaptersByAnime(int animeId)
    {
        var chapOnDb = await DB._db.Table<Chapter>().Where(c => c.AnimeId == animeId).ToListAsync();
        var chapIds = chapOnDb.Select(c => c.Id).Distinct().ToList();
        var histories = await GetHistoriesByCapIds(chapIds);
        foreach (var c in chapOnDb)
        {
            var history = histories.FirstOrDefault(h => h.ChapterId == c.Id);
            if (history != null)
            {
                c.History = history;
            }
        }

        return chapOnDb;
    }

    private async Task<Anime> GetAnimeByProv(string Url, int ProviderId)
    {
        if (Url == null)
        {
            return null;
        }

        var exist = await DB._db.Table<Anime>().Where(a => a.Url == Url && a.ProviderId == ProviderId).FirstOrDefaultAsync();

        if (exist != null)
        {
            exist.Provider = await DB._db.Table<Provider>().Where(p => p.Id == ProviderId).FirstOrDefaultAsync();
            exist.ProviderId = exist.Provider.Id;
        }
        return exist;
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

    public async Task<List<FavoriteList>> GetFavoriteListByAnime(int animeId = 0)
    {
        if (animeId == 0)
        {
            return null;
        }
        var data = await DB._db.QueryAsync<FavoriteList>(
            "select fl.* from AnimexFavorite as af inner join FavoriteList as fl" + " on af.FavoriteListId=fl.Id  where af.AnimeId=?",
            animeId
        );
        if (data.Count > 0)
        {
            return data;
        }
        return null;
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

    public async Task UpdateAnimeList(int animeId, List<int> lists)
    {
        await DB._db.ExecuteAsync("delete from AnimexFavorite " + "where AnimeId=?", animeId);
        foreach (var item in lists)
        {
            var favxanime = new AnimexFavorite() { AnimeId = animeId, FavoriteListId = item };
            await DB._db.InsertAsync(favxanime);
        }
    }

    private async Task<List<History>> GetHistoriesByCapIds(List<int> chapterIds)
    {
        var query = $"select * from History where ChapterId in ({string.Join(",", chapterIds.ToArray())})";
        var histories = await DB._db.QueryAsync<History>(query);

        //var history = await DB._db.Table<History>()
        //        .Where(h => h.ChapterId == chapterId).FirstOrDefaultAsync();
        return histories;
    }

    private async Task<History> GetHistoryByCap(int chapterId)
    {
        var history = await DB._db.Table<History>().Where(h => h.ChapterId == chapterId).FirstOrDefaultAsync();
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

    public async Task UpdateProgress(int historyId, long progress)
    {
        await DB._db.ExecuteAsync("update History set SecondsWatched=? where Id=?", progress, historyId);
    }

    public async Task<List<History>> GetAllHistoriesAsync()
    {
        var history = await DB._db.Table<History>().OrderByDescending(h => h.WatchedDate).ToListAsync();

        var chaptersId = history.Select(h => h.ChapterId).ToList().Distinct();
        var queryC = $"select Chapter.* from Chapter where Chapter.Id in ({string.Join(",", chaptersId.ToArray())})";

        var chapters = await DB._db.QueryAsync<Chapter>(queryC);
        var animeIds = chapters.Select(c => c.AnimeId).ToList().Distinct();
        var queryA = $"select Anime.* from Anime where Anime.Id in ({string.Join(",", animeIds.ToArray())})";

        var animes = await DB._db.QueryAsync<Anime>(queryA);
        var providersId = animes.Select(a => a.ProviderId).ToList().Distinct();
        var queryProv = $"select * from Provider where Id in ({string.Join(",", providersId.ToArray())})";

        var providers = await DB._db.QueryAsync<Provider>(queryProv);

        foreach (var h in history)
        {
            var chSel = chapters.FirstOrDefault(c => c.Id == h.ChapterId);
            chSel.Anime = animes.FirstOrDefault(a => a.Id == chSel.AnimeId);
            chSel.Anime.Provider = providers.FirstOrDefault(p => p.Id == chSel.Anime.ProviderId);
            h.Chapter = chSel;
        }
        return history;
    }

    public async Task<List<History>> GetHistoriesAsync(int page, int limit = 20)
    {
        var offset = (page - 1) * limit;
        var history = await DB._db.Table<History>().OrderByDescending(h => h.WatchedDate).Take(limit).Skip(offset).ToListAsync();

        var chaptersId = history.Select(h => h.ChapterId).ToList().Distinct();
        var queryC = $"select Chapter.* from Chapter where Chapter.Id in ({string.Join(",", chaptersId.ToArray())})";

        var chapters = await DB._db.QueryAsync<Chapter>(queryC);
        var animeIds = chapters.Select(c => c.AnimeId).ToList().Distinct();
        var queryA = $"select Anime.* from Anime where Anime.Id in ({string.Join(",", animeIds.ToArray())})";

        var animes = await DB._db.QueryAsync<Anime>(queryA);
        var providersId = animes.Select(a => a.ProviderId).ToList().Distinct();
        var queryProv = $"select * from Provider where Id in ({string.Join(",", providersId.ToArray())})";

        var providers = await DB._db.QueryAsync<Provider>(queryProv);

        foreach (var h in history)
        {
            var chSel = chapters.FirstOrDefault(c => c.Id == h.ChapterId);
            chSel.Anime = animes.FirstOrDefault(a => a.Id == chSel.AnimeId);
            chSel.Anime.Provider = providers.FirstOrDefault(p => p.Id == chSel.Anime.ProviderId);
            h.Chapter = chSel;
        }
        return history;
    }

    public async Task Vacuum()
    {
        await DB._db.ExecuteAsync("vacuum");
    }

    public async Task DeleteFromHistory(int Id)
    {
        var history = await DB._db.Table<History>().Where(h => h.Id == Id).FirstOrDefaultAsync();

        await DB._db.DeleteAsync(history);
    }
}
