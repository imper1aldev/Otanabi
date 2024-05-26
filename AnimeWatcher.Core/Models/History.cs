using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace AnimeWatcher.Core.Models;
public class History
{
    [AutoIncrement,PrimaryKey]
    #nullable enable
    public int Id { get; set; }
    #nullable enable
    public DateTime WatchedDate {get;set; }
    #nullable enable
    public int? SecondsWatched {get;set; }

}
