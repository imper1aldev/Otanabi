using AnimeWatcher.Core.Contracts.Extractors;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace AnimeWatcher.Core.Extractors;
public class YourUploadExtractor:IExtractor
{
    public async Task<string> GetStreamAsync(string url)
    {
        var streaminUrl = "";
        try
        {
            HtmlWeb oWeb = new HtmlWeb();
            HtmlDocument doc = await oWeb.LoadFromWebAsync(url);
            var body = doc.DocumentNode.SelectSingleNode("/html");


            Debug.WriteLine(body.InnerText);
            var pattern = @"file: '(https?://[^']+)'";
            var match = Regex.Match(body.InnerText, pattern);
            if (match.Success)
            {
                //it's working, but it needs cookies, and there's no way to add them
                //streaminUrl = match.Groups[1].Value.Replace("{", "").Replace("}", "");
                streaminUrl ="";

            }

        } catch (Exception e)
        {
            throw e;
        }
        return streaminUrl;
    }
}
