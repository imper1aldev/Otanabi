using HtmlAgilityPack;
using JsUnpacker;
using Newtonsoft.Json;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;
using ScrapySharp.Extensions;

namespace Otanabi.Extensions.VideoExtractors;

public class FilemoonExtractor : IVideoExtractor
{
    private readonly HttpClient _client = new();

    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        try
        {
            var uri = new Uri(url);
            var host = uri.Host;

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Referer", url);
            request.Headers.Add("Origin", $"https://{host}");

            var response = await _client.SendAsync(request);
            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var needsRedirect = doc.DocumentNode.CssSelect("#iframe-holder iframe");
            var redirectUrl = string.Empty;
            if (needsRedirect.Any())
            {
                redirectUrl = needsRedirect.FirstOrDefault().GetAttributeValue("src")
                                .SubstringAfter("window.location.href = '").SubstringBefore("';");
                html = await _client.GetStringAsync(redirectUrl);
                doc.LoadHtml(html);
            }

            var scriptNode = doc.DocumentNode
                .SelectSingleNode("//script[contains(text(),'eval') and contains(text(),'m3u8')]");

            if (scriptNode == null)
            {
                return new();
            }

            var unpacked = Unpacker.UnpackAndCombine(scriptNode.InnerText) ?? string.Empty;

            var masterUrl = unpacked
                .SubstringAfter("{file:\"")
                .SubstringBefore("\"}")
                .Trim();

            if (string.IsNullOrWhiteSpace(masterUrl))
            {
                return new();
            }

            // --- Subtitles ---
            var subtitleList = new List<Track>();
            var subUrl = uri.Query.Contains("sub.info")
                ? System.Web.HttpUtility.ParseQueryString(uri.Query).Get("sub.info")
                : unpacked.SubstringAfter("fetch('").SubstringBefore("').");

            if (!string.IsNullOrWhiteSpace(subUrl))
            {
                try
                {
                    using var subRequest = new HttpRequestMessage(HttpMethod.Get, subUrl);
                    subRequest.Headers.Add("Referer", url);
                    subRequest.Headers.Add("Origin", $"https://{host}");

                    var subResponse = await _client.SendAsync(subRequest);
                    var subJson = await subResponse.Content.ReadAsStringAsync();

                    var subtitles = JsonConvert.DeserializeObject<List<Track>>(subJson);
                    if (subtitles != null)
                    {
                        subtitleList = subtitles;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FilemoonExtractor] Failed to load subtitles: {ex.Message}");
                }
            }

            return new(masterUrl, subtitleList, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FilemoonExtractor] Error extracting stream: {ex.Message}");
        }

        return new();
    }
}
