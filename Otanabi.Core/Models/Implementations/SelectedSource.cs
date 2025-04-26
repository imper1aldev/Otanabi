using System.Net.Http.Headers;

namespace Otanabi.Core.Models;
public class SelectedSource
{
    public SelectedSource()
    {
    }

    public SelectedSource(string streamUrl, HttpRequestHeaders headers)
    {
        StreamUrl = streamUrl;
        Headers = headers;
    }

    public SelectedSource(string streamUrl, List<Track> subtitles, HttpRequestHeaders headers)
    {
        StreamUrl = streamUrl;
        Subtitles = subtitles;
        Headers = headers;
    }

    public SelectedSource(string streamUrl, List<Track> subtitles, List<Track> audios, HttpRequestHeaders headers)
    {
        StreamUrl = streamUrl;
        Subtitles = subtitles;
        Audios = audios;
        Headers = headers;
    }
    public string Id
    {
        get;
        set;
    }
    public string Server
    {
        get; set;
    }

    public string StreamUrl
    {
        get; set;
    }

    public List<Track> Subtitles
    {
        get; set;
    } = [];
    public List<Track> Audios
    {
        get; set;
    } = [];
    public HttpRequestHeaders Headers
    {
        get;
        set;
    }
    public string ContentType
    {
        get; set;
    }

    private bool? _useVlcProxy;

    public bool UseVlcProxy
    {
        get => _useVlcProxy ?? (Headers != null && Headers.Any(h => !IsDefaultHeader(h.Key)));
        set => _useVlcProxy = value;
    }

    private static bool IsDefaultHeader(string key) => key is "Accept" or "Accept-Encoding" or "User-Agent" or "Connection";
}
