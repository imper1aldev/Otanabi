using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;
using ScrapySharp.Extensions;

namespace Otanabi.Extensions.VideoExtractors;
public class OkruExtractor : IVideoExtractor
{
    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        var streaminUrl = "";
        try
        {
            HtmlWeb oWeb = new();
            var doc = await oWeb.LoadFromWebAsync(url);
            var values = doc.DocumentNode.SelectSingleNode("/html/body/div[2]/div").GetAttributeValue("data-options").Replace("&quot;", "\"");
            dynamic contourManifest = JObject.Parse(values);
            var metadata = (string)contourManifest.flashvars["metadata"];
            var meta2 = JObject.Parse(metadata);
            var videos = meta2["videos"];
            foreach (var video in videos)
            {
                if ((string)video["name"] == "hd")
                {
                    streaminUrl = (string)video["url"];
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting master URL: {ex.Message}");
        }
        return new(streaminUrl, null);
    }
}
