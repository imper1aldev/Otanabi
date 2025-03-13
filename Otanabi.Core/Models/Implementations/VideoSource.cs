
namespace Otanabi.Core.Models;

public class VideoSource : IVideoSource
{
    public string Server
    {
        get; set;
    }
    public string Url
    {
        get; set;
    }
    public string Title
    {
        get; set;
    }
    public int Ads
    {
        get; set;
    }
    public bool Allow_mobile
    {
        get; set;
    }
    public string Code
    {
        get; set;
    }
    public string CheckedUrl => Url ?? Code;
    public List<Track> Subtitles
    {
        get; set;
    } = [];
    public List<Track> Audios
    {
        get; set;
    } = [];
}
