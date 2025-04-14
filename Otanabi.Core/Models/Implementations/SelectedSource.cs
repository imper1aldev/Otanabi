using System.Net.Http.Headers;

namespace Otanabi.Core.Models;
public class SelectedSource
{
    public SelectedSource()
    {
    }

    public SelectedSource(string streamUrl, HttpHeaders headers)
    {
        StreamUrl = streamUrl;
        Headers = headers;
    }

    public SelectedSource(string streamUrl, List<Track> subtitles, HttpHeaders headers)
    {
        StreamUrl = streamUrl;
        Subtitles = subtitles;
        Headers = headers;
    }

    public SelectedSource(string streamUrl, List<Track> subtitles, List<Track> audios, HttpHeaders headers)
    {
        StreamUrl = streamUrl;
        Subtitles = subtitles;
        Audios = audios;
        Headers = headers;
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
    public HttpHeaders Headers
    {
        get;
        set;
    }
    public string ContentType
    {
        get; set;
    }
}
