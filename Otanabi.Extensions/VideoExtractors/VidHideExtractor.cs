using System.Text.RegularExpressions;
using Acornima.Ast;
using HtmlAgilityPack;
using JsUnpacker;
using Newtonsoft.Json;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;
using Windows.Devices.Power;

namespace Otanabi.Extensions.VideoExtractors;

public class VidHideExtractor : IVideoExtractor
{
    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        try
        {
            using var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var scriptNode = doc.DocumentNode
                .SelectNodes("//script")
                ?.FirstOrDefault(n => n.InnerText.Contains("eval(function(p,a,c,k,e,d)"));

            if (scriptNode != null)
            {
                var unpacked = Unpacker.UnpackAndCombine(scriptNode.InnerText);
                var videoUrl = Regex.Matches(unpacked, @":\s*""([^""]*?m3u8[^""]*?)""")
                                .Cast<Match>()
                                .Select(m => m.Groups[1].Value)
                                .Distinct(StringComparer.OrdinalIgnoreCase) // elimina duplicados si hay
                                .OrderBy(url => url.StartsWith("/") ? 1 : 0) // prioriza las absolutas
                                .FirstOrDefault();

                return new(videoUrl, null, null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting video URL: {ex.Message}");
        }

        return new();
    }


    public static List<Track> ExtractSubtitles(string script)
    {
        try
        {
            var subtitleStr = script
                .SubstringAfter("tracks")
                .SubstringAfter("[")
                .SubstringBefore("]");

            var json = $"[{subtitleStr}]";

            var trackDtos = JsonConvert.DeserializeObject<List<Track>>(json);

            return trackDtos?
                .Where(t => string.Equals(t.Kind, "captions", StringComparison.OrdinalIgnoreCase))
                .Select(t => new Track(t.File, t.Label ?? ""))
                .ToList()
                ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

}
