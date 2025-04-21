using Newtonsoft.Json;

namespace Otanabi.Extensions.Models.Cuevanaeu;

public class PopularAnimeList
{
    [JsonProperty("props")]
    public Props Props { get; set; } = new Props();

    [JsonProperty("page")]
    public string Page
    {
        get; set;
    }

    [JsonProperty("query")]
    public Query Query { get; set; } = new Query();

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

    [JsonProperty("gsp")]
    public bool? Gsp
    {
        get; set;
    }

    [JsonProperty("locale")]
    public string Locale
    {
        get; set;
    }

    [JsonProperty("locales")]
    public List<string> Locales { get; set; } = new List<string>();

    [JsonProperty("defaultLocale")]
    public string DefaultLocale
    {
        get; set;
    }

    [JsonProperty("scriptLoader")]
    public List<string> ScriptLoader { get; set; } = new List<string>();
}

public class Titles
{
    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }

    [JsonProperty("original")]
    public Original Original { get; set; } = new Original();
}

public class Images
{
    [JsonProperty("poster")]
    public string Poster
    {
        get; set;
    }

    [JsonProperty("backdrop")]
    public string Backdrop
    {
        get; set;
    }
}

public class Rate
{
    [JsonProperty("average")]
    public double? Average
    {
        get; set;
    }

    [JsonProperty("votes")]
    public int? Votes
    {
        get; set;
    }
}

public class Genres
{
    [JsonProperty("id")]
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
}

public class Acting
{
    [JsonProperty("id")]
    public string Id
    {
        get; set;
    }

    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }
}

public class Cast
{
    [JsonProperty("acting")]
    public List<Acting> Acting { get; set; } = new List<Acting>();

    [JsonProperty("directing")]
    public List<Directing> Directing { get; set; } = new List<Directing>();
}

public class Url
{
    [JsonProperty("slug")]
    public string Slug
    {
        get; set;
    }
}

public class Slug
{
    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }

    [JsonProperty("season")]
    public string Season
    {
        get; set;
    }

    [JsonProperty("episode")]
    public string Episode
    {
        get; set;
    }
}

public class Movies
{
    [JsonProperty("titles")]
    public Titles Titles { get; set; } = new Titles();

    [JsonProperty("images")]
    public Images Images { get; set; } = new Images();

    [JsonProperty("rate")]
    public Rate Rate { get; set; } = new Rate();

    [JsonProperty("overview")]
    public string Overview
    {
        get; set;
    }

    [JsonProperty("TMDbId")]
    public string TMDbId
    {
        get; set;
    }

    [JsonProperty("genres")]
    public List<Genres> Genres { get; set; } = new List<Genres>();

    [JsonProperty("cast")]
    public Cast Cast { get; set; } = new Cast();

    [JsonProperty("runtime")]
    public int? Runtime
    {
        get; set;
    }

    [JsonProperty("releaseDate")]
    public string ReleaseDate
    {
        get; set;
    }

    [JsonProperty("url")]
    public Url Url { get; set; } = new Url();

    [JsonProperty("slug")]
    public Slug Slug { get; set; } = new Slug();
}

public class PageProps
{
    [JsonProperty("thisSerie")]
    public ThisSerie ThisSerie { get; set; } = new ThisSerie();

    [JsonProperty("thisMovie")]
    public ThisMovie ThisMovie { get; set; } = new ThisMovie();

    [JsonProperty("movies")]
    public List<Movies> Movies { get; set; } = new List<Movies>();

    [JsonProperty("pages")]
    public int? Pages
    {
        get; set;
    }

    [JsonProperty("season")]
    public Season Season { get; set; } = new Season();

    [JsonProperty("episode")]
    public Episode Episode { get; set; } = new Episode();
}

public class Props
{
    [JsonProperty("pageProps")]
    public PageProps PageProps { get; set; } = new PageProps();

    [JsonProperty("__N_SSG")]
    public bool? NSSG
    {
        get; set;
    }
}

public class Query
{
    [JsonProperty("page")]
    public string Page
    {
        get; set;
    }

    [JsonProperty("serie")]
    public string Serie
    {
        get; set;
    }

    [JsonProperty("movie")]
    public string Movie
    {
        get; set;
    }

    [JsonProperty("episode")]
    public string Episode
    {
        get; set;
    }

    [JsonProperty("q")]
    public string Q
    {
        get; set;
    }
}

public class Directing
{
    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }
}

public class Server
{
    [JsonProperty("cyberlocker")]
    public string Cyberlocker
    {
        get; set;
    }

    [JsonProperty("result")]
    public string Result
    {
        get; set;
    }

    [JsonProperty("quality")]
    public string Quality
    {
        get; set;
    }
}

public class Videos
{
    [JsonProperty("latino")]
    public List<Server> Latino { get; set; } = new List<Server>();

    [JsonProperty("spanish")]
    public List<Server> Spanish { get; set; } = new List<Server>();

    [JsonProperty("english")]
    public List<Server> English { get; set; } = new List<Server>();

    [JsonProperty("japanese")]
    public List<Server> Japanese { get; set; } = new List<Server>();
}

public class Downloads
{
    [JsonProperty("cyberlocker")]
    public string Cyberlocker
    {
        get; set;
    }

    [JsonProperty("result")]
    public string Result
    {
        get; set;
    }

    [JsonProperty("quality")]
    public string Quality
    {
        get; set;
    }

    [JsonProperty("language")]
    public string Language
    {
        get; set;
    }
}

public class ThisMovie
{
    [JsonProperty("TMDbId")]
    public string TMDbId
    {
        get; set;
    }

    [JsonProperty("titles")]
    public Titles Titles { get; set; } = new Titles();

    [JsonProperty("images")]
    public Images Images { get; set; } = new Images();

    [JsonProperty("overview")]
    public string Overview
    {
        get; set;
    }

    [JsonProperty("runtime")]
    public int? Runtime
    {
        get; set;
    }

    [JsonProperty("genres")]
    public List<Genres> Genres { get; set; } = new List<Genres>();

    [JsonProperty("cast")]
    public Cast Cast { get; set; } = new Cast();

    [JsonProperty("rate")]
    public Rate Rate { get; set; } = new Rate();

    [JsonProperty("url")]
    public Url Url { get; set; } = new Url();

    [JsonProperty("slug")]
    public Slug Slug { get; set; } = new Slug();

    [JsonProperty("releaseDate")]
    public string ReleaseDate
    {
        get; set;
    }

    [JsonProperty("videos")]
    public Videos Videos { get; set; } = new Videos();

    [JsonProperty("downloads")]
    public List<Downloads> Downloads { get; set; } = new List<Downloads>();
}

public class Season
{
    [JsonProperty("number")]
    public int? Number
    {
        get; set;
    }
}

public class NextEpisode
{
    [JsonProperty("title")]
    public string Title
    {
        get; set;
    }

    [JsonProperty("slug")]
    public string Slug
    {
        get; set;
    }
}

public class Episode
{
    [JsonProperty("TMDbId")]
    public string TMDbId
    {
        get; set;
    }

    [JsonProperty("title")]
    public string Title
    {
        get; set;
    }

    [JsonProperty("number")]
    public int? Number
    {
        get; set;
    }

    [JsonProperty("image")]
    public string Image
    {
        get; set;
    }

    [JsonProperty("url")]
    public Url Url { get; set; } = new Url();

    [JsonProperty("slug")]
    public Slug Slug { get; set; } = new Slug();

    [JsonProperty("nextEpisode")]
    public NextEpisode NextEpisode { get; set; } = new NextEpisode();

    [JsonProperty("previousEpisode")]
    public string PreviousEpisode
    {
        get; set;
    }

    [JsonProperty("videos")]
    public Videos Videos { get; set; } = new Videos();

    [JsonProperty("downloads")]
    public List<Downloads> Downloads { get; set; } = new List<Downloads>();
}