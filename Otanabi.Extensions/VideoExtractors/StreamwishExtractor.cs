using System.Text.RegularExpressions;
using HtmlAgilityPack;
using JsUnpacker;
using Newtonsoft.Json;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;
namespace Otanabi.Extensions.VideoExtractors;
public class StreamwishExtractor : IVideoExtractor
{
    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        try
        {
            HtmlWeb oWeb = new();
            var doc = await oWeb.LoadFromWebAsync(url);
            var packedScript = doc.DocumentNode.Descendants()
                .FirstOrDefault(x => x.Name == "script" && x.InnerText?.Contains("eval") == true);

            if (packedScript != null && Unpacker.IsPacked(packedScript.InnerText))
            {
                var unpacked = Unpacker.UnpackAndCombine(packedScript.InnerText);
                var streamUrl = Regex.Matches(unpacked, @":\s*""([^""]*?m3u8[^""]*?)""")
                                .Cast<Match>()
                                .Select(m => m.Groups[1].Value)
                                .Distinct(StringComparer.OrdinalIgnoreCase) // elimina duplicados si hay
                                .OrderBy(url => url.StartsWith("/") ? 1 : 0) // prioriza las absolutas
                                .FirstOrDefault();

                var subtitles = ExtractSubtitles(unpacked);
                return new(streamUrl, subtitles, null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting video stream: {ex.Message}");
        }
        return new();
    }

    public static List<Track> ExtractSubtitles(string script)
    {
        try
        {
            var subtitleStr = script.SubstringAfter("tracks").SubstringAfter("[").SubstringBefore("]");
            return JsonConvert.DeserializeObject<List<Track>>($"[{subtitleStr}]").Where(x => x.Kind != "thumbnails").ToList();
        }
        catch (Exception)
        {
            return [];
        }
    }
}
