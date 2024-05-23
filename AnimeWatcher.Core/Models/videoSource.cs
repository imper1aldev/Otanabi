using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public class VideoSource
{
    public string server
    {
        get; set;
    }
    public string url
    {
        get; set;
    }
    public string title
    {
        get; set;
    }
    public int ads
    {
        get; set;
    }
    public bool allow_mobile
    {
        get; set;
    }
    public string code
    {
        get; set;
    }

    public string checkedUrl => url != null ? url : code;


}
