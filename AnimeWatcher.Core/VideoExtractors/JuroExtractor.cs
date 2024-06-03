using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnimeWatcher.Core.Contracts.VideoExtractors;

namespace AnimeWatcher.Core.VideoExtractors;
internal class JuroExtractor : IVideoExtractor
{
    public async Task<string> GetStreamAsync(string url)
    {
        await Task.CompletedTask;
        return url;
    }
}
