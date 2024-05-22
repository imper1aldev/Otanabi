using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeWatcher.Core.Models;
public class History
{
    public int id { get; set; }
    public DateTime watched_date {get;set; }
    public int seconds_watched {get;set; }

}
