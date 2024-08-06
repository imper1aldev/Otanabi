using AnimeWatcher.Extensions.Contracts.VideoExtractors;
using HtmlAgilityPack; 
using JsUnpacker;
namespace AnimeWatcher.Extensions.VideoExtractors;
public class StreamwishExtractor : IVideoExtractor
{
    public async Task<string> GetStreamAsync(string url)
    {
        var streaminUrl = "";
        try
        {
            HtmlWeb oWeb = new HtmlWeb();
            HtmlDocument doc = await oWeb.LoadFromWebAsync(url);

            var packed = doc.DocumentNode.Descendants()
                         .FirstOrDefault(x => x.Name == "script" && x.InnerText?.Contains("eval") == true);
            var unpacked = "";
            if (Unpacker.IsPacked(packed?.InnerText))
            {
                unpacked = Unpacker.UnpackAndCombine(packed?.InnerText);
            }
            else
            {
                //not valid pack
                return streaminUrl="";
            }

            streaminUrl = unpacked.SubstringAfter("sources:[{file:\"").Split(new[] { "\"}" }, StringSplitOptions.None)[0];

        } catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        return streaminUrl;
    }
}
