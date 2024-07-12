using System.Text.RegularExpressions;
using AnimeWatcher.Core.Contracts.VideoExtractors;
using HtmlAgilityPack;

namespace AnimeWatcher.Core.VideoExtractors;
public class StreamwishExtractor : IVideoExtractor
{
    public async Task<string> GetStreamAsync(string url)
    {
        var streaminUrl = "";
        try
        {
            HtmlWeb oWeb = new HtmlWeb();
            HtmlDocument doc = await oWeb.LoadFromWebAsync(url);
            var body = doc.DocumentNode.SelectSingleNode("/html"); 
            var pattern = @"file:""(https?://[^""]+)""";
            var match = Regex.Match(body.InnerHtml, pattern);
            if (match.Success)
            {
                streaminUrl = match.Groups[1].Value.Replace("{", "").Replace("}", "");
            }

        } catch (Exception e)
        {
            
        }
        return streaminUrl;
    }
}
