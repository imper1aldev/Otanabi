using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Otanabi.Core.Models;
using Otanabi.Extensions.Contracts.VideoExtractors;

namespace Otanabi.Extensions.VideoExtractors;

public class VoeExtractor : IVideoExtractor
{
    private static readonly Regex LinkRegex = new(@"(http|https):\/\/([\w_-]+(?:\.[\w_-]+)+)([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])", RegexOptions.Compiled);
    private static readonly Regex Base64Regex = new(@"'.*?'", RegexOptions.Compiled);
    private static readonly Regex ScriptBase64Regex = new(@"(let|var)\s+\w+\s*=\s*'(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)';", RegexOptions.Compiled);

    private readonly HttpClient _client;

    public VoeExtractor(HttpClient client)
    {
        _client = client;
    }

    public async Task<SelectedSource> GetStreamAsync(string url)
    {
        try
        {
            var html = await _client.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var needsRedirect = doc.DocumentNode.SelectSingleNode("//script[contains(text(),'localStorage')]")?.InnerText;

            if (!string.IsNullOrEmpty(needsRedirect))
            {
                var redirectUrl = needsRedirect.SubstringAfter("window.location.href = '").SubstringBefore("';");
                html = await _client.GetStringAsync(redirectUrl);
                doc.LoadHtml(html);
            }

            var scripts = doc.DocumentNode.SelectNodes("//script") ?? new HtmlNodeCollection(null);

            var altScript = scripts
                .Select(x => x.InnerText)
                .FirstOrDefault(s => ScriptBase64Regex.IsMatch(s));

            var script = scripts
                .Select(x => x.InnerText)
                .FirstOrDefault(s => s.Contains("const sources") || s.Contains("var sources") || s.Contains("wc0"))
                ?? altScript;

            if (string.IsNullOrEmpty(script))
            {
                return new();
            }

            string playlistUrl = null;

            if (script.Contains("sources"))
            {
                var link = script.SubstringAfter("hls': '").SubstringBefore("'");
                playlistUrl = LinkRegex.IsMatch(link)
                    ? link
                    : Encoding.UTF8.GetString(Convert.FromBase64String(link));
            }
            else if (script.Contains("wc0") || altScript != null)
            {
                var base64Match = Base64Regex.Match(script);
                if (base64Match.Success)
                {
                    var encoded = base64Match.Value.Trim('\'');
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));

                    if (altScript != null)
                        decoded = new string(decoded.Reverse().ToArray());

                    var dto = JsonConvert.DeserializeObject<VideoLinkDto>(decoded);
                    playlistUrl = dto?.File;
                }
            }

            if (!string.IsNullOrWhiteSpace(playlistUrl))
            {
                Console.WriteLine($"[VoeExtractor] Playlist URL: {playlistUrl}");
                return new(playlistUrl, null, null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VoeExtractor] Error extracting video URL: {ex.Message}");
        }

        return new();
    }

    private class VideoLinkDto
    {
        [JsonProperty("file")]
        public string File
        {
            get; set;
        }
    }
}
