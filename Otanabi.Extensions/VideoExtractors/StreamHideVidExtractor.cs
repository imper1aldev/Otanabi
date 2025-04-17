using HtmlAgilityPack;
using JsUnpacker;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;

namespace Otanabi.Extensions.VideoExtractors;

public class StreamHideVidExtractor : IVideoExtractor
{
    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        try
        {
            using var httpClient = new HttpClient();
            var embedUrl = GetEmbedUrl(url);
            var html = await httpClient.GetStringAsync(embedUrl);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(),'m3u8')]");

            if (scriptNode != null)
            {
                var script = scriptNode.InnerText;
                var masterUrl = "";
                if (script.Contains("eval(function(p,a,c"))
                {
                    script = Unpacker.UnpackAndCombine(script);
                }

                if (script != null && script.Contains("var links=", StringComparison.OrdinalIgnoreCase))
                {
                    masterUrl = script.SubstringAfter("hls2\":\"").Split(["\"}"], StringSplitOptions.None)[0];
                }
                else
                {
                    masterUrl = script.SubstringAfter("source").SubstringAfter("file:\"").SubstringBefore("\"");
                }

                if (!string.IsNullOrWhiteSpace(masterUrl))
                {
                    return new(masterUrl, null, null);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting master URL: {ex.Message}");
        }

        return new();
    }

    private static string GetEmbedUrl(string url)
    {
        if (url.Contains("/d/")) return url.Replace("/d/", "/v/");
        if (url.Contains("/download/")) return url.Replace("/download/", "/v/");
        if (url.Contains("/file/")) return url.Replace("/file/", "/v/");
        return url.Replace("/f/", "/v/");
    }
}
