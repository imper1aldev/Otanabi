﻿using System.Collections.Generic;
using System.Diagnostics;
using Otanabi.Core.Anilist.Models;
using Otanabi.Core.Database;
using Otanabi.Core.Models;

namespace Otanabi.Core.Services;

public class DatabaseService
{
    public readonly DatabaseHandler DB = DatabaseHandler.GetInstance();
    private readonly SearchAnimeService _searchAnimeService = new();
    private AnilistService _anilistService = new();

    public DatabaseService() { }

    #region Anime
    public async Task<Anime> GetOrAddAnimeByMedia(Media media, Provider provider, Anime providerAnime)
    {
        var id = media.Id;
        var anime = await DB._db.Table<Anime>().Where(a => a.IdAnilist == id && a.ProviderId == provider.Id).FirstOrDefaultAsync();
        if (anime == null)
        {
            anime = new Anime
            {
                IdAnilist = media.Id,
                ProviderId = provider.Id,
                Url = providerAnime.Url,
                RemoteID = providerAnime.RemoteID,
                Cover = media.CoverImage.ExtraLarge,
                Title = media.Title.Romaji,
            };
            await DB._db.InsertAsync(anime);
            anime = await DB
                ._db.Table<Anime>()
                .Where(a =>
                    a.IdAnilist == media.Id && a.ProviderId == provider.Id && a.Url == providerAnime.Url && a.RemoteID == providerAnime.RemoteID
                )
                .FirstOrDefaultAsync();
        }
        return anime;
    }

    public async Task<Anime> GetOrCreateAnime(Provider provider, Anime providerAnime)
    {
        var anime = await DB._db.Table<Anime>().Where(a => a.ProviderId == provider.Id && a.RemoteID == providerAnime.RemoteID).FirstOrDefaultAsync();
        if (anime == null)
        {
            var anilistData = await _anilistService.SearchByName(providerAnime.Title, provider.IsAdult);
            if (anilistData != null)
            {
                string coverImage = anilistData.CoverImage.ExtraLarge ?? anilistData.CoverImage.Large ?? anilistData.CoverImage.Medium;
                anime = new Anime
                {
                    IdAnilist = anilistData.Id,
                    ProviderId = provider.Id,
                    Url = providerAnime.Url,
                    RemoteID = providerAnime.RemoteID,
                    Cover = coverImage,
                    Title = anilistData.Title.Romaji,
                };
                await DB._db.InsertAsync(anime);
                anime = await DB
                    ._db.Table<Anime>()
                    .Where(a =>
                        a.IdAnilist == anilistData.Id
                        && a.ProviderId == provider.Id
                        && a.Url == providerAnime.Url
                        && a.RemoteID == providerAnime.RemoteID
                    )
                    .FirstOrDefaultAsync();
            }
            else
            {
                anime = new Anime
                {
                    IdAnilist = 0,
                    ProviderId = provider.Id,
                    Url = providerAnime.Url,
                    RemoteID = providerAnime.RemoteID,
                    Cover = providerAnime.Cover,
                    Title = providerAnime.Title,
                };
                await DB._db.InsertAsync(anime);
                anime = await DB
                    ._db.Table<Anime>()
                    .Where(a => a.IdAnilist == 0 && a.ProviderId == provider.Id && a.Url == providerAnime.Url && a.RemoteID == providerAnime.RemoteID)
                    .FirstOrDefaultAsync();
            }
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
    #endregion

    #region Favorites
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
    #endregion

    #region History

    public async Task<History> GetOrCreateHistoryByCap(Anime anime, int chapterNumber)
    {
        var history = await GetHistoryByCap(anime, chapterNumber);
        if (history != null)
        {
            await DB._db.ExecuteAsync("update History set WatchedDate=? where Id=?", DateTime.Now, history.Id);
            history = await GetHistoryByCap(anime, chapterNumber);
            return history;
        }
        var hisCl = new History()
        {
            ChapterNumber = chapterNumber,
            WatchedDate = DateTime.Now,
            AnimeId = anime.Id,
            SecondsWatched = 0,
        };
        await DB._db.InsertAsync(hisCl);

        history = await GetHistoryByCap(anime, chapterNumber);

        return history;
    }

    private async Task<History> GetHistoryByCap(Anime anime, int chapterNumber)
    {
        var history = await DB._db.Table<History>().Where(h => h.ChapterNumber == chapterNumber && h.AnimeId == anime.Id).FirstOrDefaultAsync();
        return history;
    }

    public async Task<History[]> GetAllHistories()
    {
        var histories = await DB._db.Table<History>().ToListAsync();
        var providers = await DB._db.Table<Provider>().ToListAsync();
        foreach (var item in histories)
        {
            var anime = await DB._db.Table<Anime>().Where(a => a.Id == item.AnimeId).FirstOrDefaultAsync();
            anime.Provider = providers.FirstOrDefault(p => p.Id == anime.ProviderId);
            item.Anime = anime;
        }
        return histories.ToArray();
    }

    public async Task UpdateProgress(int historyId, long progress, long totalMedia)
    {
        await DB._db.ExecuteAsync(
            "update History set SecondsWatched=? ,TotalMedia=? , WatchedDate=?  where Id=?",
            progress,
            totalMedia,
            DateTime.Now,
            historyId
        );
    }

    public async Task DeleteFromHistory(int Id)
    {
        var history = await DB._db.Table<History>().Where(h => h.Id == Id).FirstOrDefaultAsync();

        await DB._db.DeleteAsync(history);
    }

    public async Task DeleteAllHistory()
    {
        await DB._db.ExecuteAsync("delete from History");
    }
    #endregion

    #region Maintenance
    public async Task Vacuum()
    {
        await DB._db.ExecuteAsync("vacuum");
    }
    #endregion
}
