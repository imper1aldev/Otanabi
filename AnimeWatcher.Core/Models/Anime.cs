using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public enum AnimeType { OVA, TV, MOVIE ,SPECIAL,OTHER }
public class Anime
{
    public int Id
    {
        get; set;
    }
    public string RemoteID
    {
        get; set;
    }
    public string Cover
    {
        get; set;
    }
    public string Title
    {
        get; set;
    }
    public string Url
    {
        get; set;
    }
    public AnimeType Type
    {
        get; set;
    }
    public string Description
    {
        get; set;
    }
    public int ProviderId
    {
        get; set;
    }
    public Provider Provider
    {
        get; set;
    }
    public string Status
    {
        get; set;
    }
    public ICollection<Chapter> Chapters
    {
        get; set;
    }
    public string TypeStr=>Type.ToString();
     
}
