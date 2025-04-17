using System.Net.Http.Headers;
using HtmlAgilityPack;
using JsUnpacker;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;

namespace Otanabi.Extensions.VideoExtractors;

public class FastreamExtractor : IVideoExtractor
{
    private readonly HttpClient _client = new();
    private const string FastreamUrl = "https://fastream.to";
    private readonly HttpRequestHeaders _headers = new HttpClient().DefaultRequestHeaders;

    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        try
        {
            // Step 1: Get the initial document
            var firstDoc = await _client.GetStringAsync(url);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(firstDoc);

            // Step 2: If there are form inputs, make a POST request
            HtmlNode scriptElement = null;
            if (htmlDoc.DocumentNode.SelectNodes("//input[@name]").Any())
            {
                var formData = new FormUrlEncodedContent(htmlDoc.DocumentNode
                    .SelectNodes("//input[@name]")
                    .Select(node => new KeyValuePair<string, string>(node.GetAttributeValue("name", ""), node.GetAttributeValue("value", "")))
                );

                var response = await _client.PostAsync(url, formData);
                var postResponse = await response.Content.ReadAsStringAsync();
                var postHtmlDoc = new HtmlDocument();
                postHtmlDoc.LoadHtml(postResponse);

                scriptElement = postHtmlDoc.DocumentNode.SelectSingleNode("//script[contains(text(),'jwplayer') and contains(text(),'vplayer')]");
            }
            else
            {
                scriptElement = htmlDoc.DocumentNode.SelectSingleNode("//script[contains(text(),'jwplayer') and contains(text(),'vplayer')]");
            }

            if (scriptElement == null)
                return new();

            // Step 3: Process script data
            var scriptData = scriptElement.InnerText;
            if (scriptData.Contains("eval(function("))
            {
                scriptData = Unpacker.UnpackAndCombine(scriptData) ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(scriptData))
                return new();

            // Step 4: Extract the video URL
            var videoUrl = scriptData.SubstringAfter("file:\"").SubstringBefore("\"").Trim();

            // Set headers for the request
            _headers.Add("Referer", $"{FastreamUrl}/");
            _headers.Referrer = new Uri($"{FastreamUrl}/");
            _headers.Add("Origin", FastreamUrl);

            return new(videoUrl, _headers);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FastreamExtractor] Error extracting video: {ex.Message}");
            return new();
        }
    }
}
