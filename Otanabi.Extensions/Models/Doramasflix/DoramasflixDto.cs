using Newtonsoft.Json;

namespace Otanabi.Extensions.Models.Doramasflix;

public class SeasonModel
{
    [JsonProperty("data")]
    public DataSeason Data { get; set; } = new();
}

public class DataSeason
{
    [JsonProperty("listSeasons")]
    public List<ListSeason> ListSeasons { get; set; } = new();
}

public class ListSeason
{
    [JsonProperty("slug")]
    public string Slug
    {
        get; set;
    }

    [JsonProperty("season_number")]
    public long SeasonNumber
    {
        get; set;
    }

    [JsonProperty("poster_path")]
    public string PosterPath
    {
        get; set;
    }

    [JsonProperty("air_date")]
    public string AirDate
    {
        get; set;
    }

    [JsonProperty("serie_name")]
    public string SerieName
    {
        get; set;
    }

    [JsonProperty("poster")]
    public string Poster
    {
        get; set;
    }

    [JsonProperty("__typename")]
    public string Typename
    {
        get; set;
    }
}

public class EpisodeModel
{
    [JsonProperty("data")]
    public DataEpisode Data { get; set; } = new();
}

public class DataEpisode
{
    [JsonProperty("listEpisodes")]
    public List<ListEpisode> ListEpisodes { get; set; } = new();
}

public class ListEpisode
{
    [JsonProperty("_id")]
    public string Id
    {
        get; set;
    }

    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }

    [JsonProperty("slug")]
    public string Slug
    {
        get; set;
    }

    [JsonProperty("serie_name")]
    public string SerieName
    {
        get; set;
    }

    [JsonProperty("serie_name_es")]
    public string SerieNameEs
    {
        get; set;
    }

    [JsonProperty("air_date")]
    public string AirDate
    {
        get; set;
    }

    [JsonProperty("season_number")]
    public long? SeasonNumber
    {
        get; set;
    }

    [JsonProperty("episode_number")]
    public long? EpisodeNumber
    {
        get; set;
    }

    [JsonProperty("poster")]
    public string Poster
    {
        get; set;
    }

    [JsonProperty("__typename")]
    public string Typename
    {
        get; set;
    }
}

public class PaginationModel
{
    [JsonProperty("data")]
    public DataPagination Data { get; set; } = new();
}

public class DataPagination
{
    [JsonProperty("paginationDorama")]
    public PaginationDorama PaginationDorama
    {
        get; set;
    }

    [JsonProperty("paginationMovie")]
    public PaginationDorama PaginationMovie
    {
        get; set;
    }
}

public class PaginationDorama
{
    [JsonProperty("count")]
    public long Count
    {
        get; set;
    }

    [JsonProperty("pageInfo")]
    public PageInfo PageInfo
    {
        get; set;
    }

    [JsonProperty("items")]
    public List<Item> Items { get; set; } = new();

    [JsonProperty("__typename")]
    public string Typename
    {
        get; set;
    }
}

public class PageInfo
{
    [JsonProperty("currentPage")]
    public long CurrentPage
    {
        get; set;
    }

    [JsonProperty("hasNextPage")]
    public bool HasNextPage
    {
        get; set;
    }

    [JsonProperty("hasPreviousPage")]
    public bool HasPreviousPage
    {
        get; set;
    }

    [JsonProperty("__typename")]
    public string Typename
    {
        get; set;
    }
}

public class Item
{
    [JsonProperty("_id")]
    public string Id
    {
        get; set;
    }

    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }

    [JsonProperty("name_es")]
    public string NameEs
    {
        get; set;
    }

    [JsonProperty("slug")]
    public string Slug
    {
        get; set;
    }

    [JsonProperty("names")]
    public string Names
    {
        get; set;
    }

    [JsonProperty("overview")]
    public string Overview
    {
        get; set;
    }

    [JsonProperty("poster_path")]
    public string PosterPath
    {
        get; set;
    }

    [JsonProperty("poster")]
    public string Poster
    {
        get; set;
    }

    [JsonProperty("genres")]
    public List<Genre> Genres { get; set; } = new();

    [JsonProperty("__typename")]
    public string Typename
    {
        get; set;
    }
}

public class Genre
{
    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }

    [JsonProperty("slug")]
    public string Slug
    {
        get; set;
    }

    [JsonProperty("__typename")]
    public string Typename
    {
        get; set;
    }
}

public class SearchModel
{
    [JsonProperty("data")]
    public Data Data { get; set; } = new();
}

public class Data
{
    [JsonProperty("searchDorama")]
    public List<SearchDorama> SearchDorama { get; set; } = new();

    [JsonProperty("searchMovie")]
    public List<SearchDorama> SearchMovie { get; set; } = new();
}

public class SearchDorama
{
    [JsonProperty("_id")]
    public string Id
    {
        get; set;
    }

    [JsonProperty("slug")]
    public string Slug
    {
        get; set;
    }

    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }

    [JsonProperty("name_es")]
    public string NameEs
    {
        get; set;
    }

    [JsonProperty("poster_path")]
    public string PosterPath
    {
        get; set;
    }

    [JsonProperty("poster")]
    public string Poster
    {
        get; set;
    }

    [JsonProperty("__typename")]
    public string Typename
    {
        get; set;
    }
}

public class VideoToken
{
    [JsonProperty("link")]
    public string Link
    {
        get; set;
    }

    [JsonProperty("server")]
    public string Server
    {
        get; set;
    }

    [JsonProperty("app")]
    public string App
    {
        get; set;
    }

    [JsonProperty("iat")]
    public long? Iat
    {
        get; set;
    }

    [JsonProperty("exp")]
    public long? Exp
    {
        get; set;
    }
}

public class TokenModel
{
    [JsonProperty("props")]
    public PropsToken Props { get; set; } = new();

    [JsonProperty("page")]
    public string Page
    {
        get; set;
    }

    [JsonProperty("query")]
    public QueryToken Query { get; set; } = new();

    [JsonProperty("buildId")]
    public string BuildId
    {
        get; set;
    }

    [JsonProperty("isFallback")]
    public bool? IsFallback
    {
        get; set;
    }

    [JsonProperty("isExperimentalCompile")]
    public bool? IsExperimentalCompile
    {
        get; set;
    }

    [JsonProperty("gssp")]
    public bool? Gssp
    {
        get; set;
    }
}

public class PropsToken
{
    [JsonProperty("pageProps")]
    public PagePropsToken PageProps { get; set; } = new();

    [JsonProperty("__N_SSP")]
    public bool? NSsp
    {
        get; set;
    }
}

public class PagePropsToken
{
    [JsonProperty("token")]
    public string Token
    {
        get; set;
    }

    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }

    [JsonProperty("app")]
    public string App
    {
        get; set;
    }

    [JsonProperty("server")]
    public string Server
    {
        get; set;
    }

    [JsonProperty("iosapp")]
    public string IosApp
    {
        get; set;
    }

    [JsonProperty("externalLink")]
    public string ExternalLink
    {
        get; set;
    }
}

public class QueryToken
{
    [JsonProperty("token")]
    public string Token
    {
        get; set;
    }
}

public class DataDoramas
{
    [JsonProperty("listDoramas")]
    public List<ListDorama> ListDoramas
    {
        get; set;
    }
}

public class ListDorama
{
    [JsonProperty("_id")]
    public string Id
    {
        get; set;
    }

    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }

    [JsonProperty("name_es")]
    public string NameEs
    {
        get; set;
    }

    [JsonProperty("slug")]
    public string Slug
    {
        get; set;
    }

    [JsonProperty("names")]
    public string Names
    {
        get; set;
    }

    [JsonProperty("overview")]
    public string Overview
    {
        get; set;
    }

    [JsonProperty("poster_path")]
    public string PosterPath
    {
        get; set;
    }

    [JsonProperty("poster")]
    public string Poster
    {
        get; set;
    }

    [JsonProperty("__typename")]
    public string Typename
    {
        get; set;
    }
}

public class ListDoramasModel
{
    [JsonProperty("data")]
    public DataDoramas Data
    {
        get; set;
    }
}
