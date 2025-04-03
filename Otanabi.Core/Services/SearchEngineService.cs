using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otanabi.Core.Anilist.Models;
using Otanabi.Core.Models;

namespace Otanabi.Core.Services;

//this class will connect anilist with the providers
public sealed class SearchEngineService
{
    //search the anime title
    //will return a list of possibile anime/s

    public static async Task<(Anime, List<Anime>)> SearchByName(MediaTitle searchTerm, Provider provider)
    {
        var animeService = new SearchAnimeService();
        var data = await animeService.SearchAnimeAsync(searchTerm.Romaji, 1, provider);
        var searchTerms = new[] { searchTerm.Romaji, searchTerm.English, searchTerm.Native };

        var result = data.Where(x => searchTerms.Contains(x.Title)).ToList().FirstOrDefault();
        var fullResult = data.ToList();
        return (result, fullResult);
    }

    public static async Task<Anime> GetAnimeDetail(Anime anime)
    {
        var animeService = new SearchAnimeService();
        var data = await animeService.GetAnimeDetailsAsync(anime);

        return data;
    }

    //using the provider url
    //retrive the provider anime data : Url chapters , AnimeUrl
}
