using System.Security.Cryptography;
using System.Text;

namespace Otanabi.Core.Models;

public class VideoSource : IVideoSource
{
    public string Id => new Guid([.. SHA1.HashData(Encoding.UTF8.GetBytes(Title ?? Url)).Take(16)]).ToString("N");

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
    public bool IsLocalSource
    {
        get; set;
    }
}
