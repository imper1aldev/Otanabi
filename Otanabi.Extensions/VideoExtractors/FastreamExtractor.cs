using HtmlAgilityPack;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using JsUnpacker;

namespace Otanabi.Extensions.VideoExtractors;

public class FastreamExtractor : IVideoExtractor
{
    private readonly HttpClient _client = new();
    private readonly HttpRequestHeaders _headers;
    private const string FastreamUrl = "https://fastream.to";

    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        try
        {
            //// Set headers for the request
            //var videoHeaders = _headers
            //    .Clone()
            //    .Add("Referer", $"{FastreamUrl}/")
            //    .Add("Origin", FastreamUrl);

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

            if (videoUrl.Contains(".m3u8"))
            {
                return new SelectedSource(videoUrl, null);
            }
            else
            {
                return new SelectedSource(videoUrl, null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FastreamExtractor] Error extracting video: {ex.Message}");
            return new();
        }
    }
}
