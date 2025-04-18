﻿using HtmlAgilityPack;
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
                var streamUrl = "";
                if (unpacked != null && unpacked.Contains("var links=", StringComparison.OrdinalIgnoreCase))
                {
                    streamUrl = unpacked.SubstringAfter("hls2\":\"").Split(["\"}"], StringSplitOptions.None)[0];
                }
                else
                {
                    streamUrl = unpacked.SubstringAfter("sources:[{file:\"").Split(["\"}"], StringSplitOptions.None)[0];
                }

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
            return JsonConvert.DeserializeObject<List<Track>>($"[{subtitleStr}]");
        }
        catch (Exception)
        {
            return [];
        }
    }
}
