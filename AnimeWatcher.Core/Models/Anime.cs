using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public enum AnimeType { OVA, TV, MOVIE ,SPECIAL }
public class Anime
{
    public int id
    {
        get; set;
    }
    public string remoteID
    {
        get; set;
    }
    public string cover
    {
        get; set;
    }
    public string title
    {
        get; set;
    }
    public string url
    {
        get; set;
    }
    public AnimeType type
    {
        get; set;
    }
    public string description
    {
        get; set;
    }
    public int providerId
    {
        get; set;
    }
    public Provider provider
    {
        get; set;
    }
    public string status
    {
        get; set;
    }
    public ICollection<Chapter> Chapters
    {
        get; set;
    }
}
