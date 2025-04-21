using Newtonsoft.Json;

namespace Otanabi.Extensions.Models.Cuevanaeu;
public class AnimeEpisodesList
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

public class Episodes
{
    [JsonProperty("title")]
    public string Title
    {
        get; set;
    }

    [JsonProperty("TMDbId")]
    public string TMDbId
    {
        get; set;
    }

    [JsonProperty("number")]
    public int? Number
    {
        get; set;
    }

    [JsonProperty("releaseDate")]
    public string ReleaseDate
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
}

public class Seasons
{
    [JsonProperty("number")]
    public int? Number
    {
        get; set;
    }

    [JsonProperty("episodes")]
    public List<Episodes> Episodes { get; set; } = new List<Episodes>();
}

public class Original
{
    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }
}

public class ThisSerie
{
    [JsonProperty("TMDbId")]
    public string TMDbId
    {
        get; set;
    }

    [JsonProperty("seasons")]
    public List<Seasons> Seasons { get; set; } = new List<Seasons>();

    [JsonProperty("titles")]
    public Titles Titles { get; set; } = new Titles();

    [JsonProperty("images")]
    public Images Images { get; set; } = new Images();

    [JsonProperty("overview")]
    public string Overview
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
}