using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public class History
{
    #nullable enable
    public int Id { get; set; }
    #nullable enable
    public DateTime WatchedDate {get;set; }
    #nullable enable
    public int? SecondsWatched {get;set; }

}
