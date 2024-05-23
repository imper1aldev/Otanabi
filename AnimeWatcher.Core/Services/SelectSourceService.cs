using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Extractors;
namespace AnimeWatcher.Core.Services;
public class SelectSourceService
{
    internal OkruExtractor okruExtractor = new();
    internal StreamWishExtractor streamWishExtractor = new();
    internal YourUploadExtractor yourUploadExtractor = new();
    internal static List<T> MoveToFirst<T>(List<T> list, T item)
    {
        List<T> newList = new List<T>(list);
        if (newList.Remove(item))
        {
            newList.Insert(0, item);
        }
        return newList;
    }

    public async Task<string> SelectSourceAsync(VideoSource[] videoSources,string byDefault="")
    {
        /*logic to get the default source here*/
        /**/
        var streamUrl="";

        var item = videoSources.FirstOrDefault(e=>e.server == byDefault) ?? videoSources[0];
        var orderedSources =MoveToFirst(videoSources.ToList(),item);

        foreach (var source in orderedSources)
        {
            var tempUrl="";
            switch (source.server)
            {
                case "Okru":
                    tempUrl= await okruExtractor.GetStreamAsync(source.checkedUrl);
                    break;
                case "Streamwish":
                    tempUrl= await streamWishExtractor.GetStreamAsync(source.checkedUrl);
                    break;
                case "YourUpload":
                    tempUrl= await yourUploadExtractor.GetStreamAsync(source.checkedUrl);
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(tempUrl) )
            {
                streamUrl=tempUrl;
                break;
            }
        }


        return streamUrl;
    }


}
