using System.Net.Http.Headers;
using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
namespace Otanabi.Core.Services;
public class SelectSourceService
{
    private readonly ClassReflectionHelper _classReflectionHelper = new();
    private readonly LoggerService logger = new();

    internal static List<T> MoveToFirst<T>(List<T> list, T item)
    {
        var newList = new List<T>(list);
        if (newList.Remove(item))
        {
            newList.Insert(0, item);
        }
        return newList;
    }

    //public async Task<(string, string, HttpHeaders)> SelectSourceAsync(VideoSource[] videoSources, string byDefault = "")
    //{
    //    var streamUrl = "";
    //    var subUrl = "";
    //    HttpHeaders headers = new HttpClient().DefaultRequestHeaders;
    //    try
    //    {
    //        var item = videoSources.FirstOrDefault(e => e.Server == byDefault) ?? videoSources[0];
    //        var orderedSources = MoveToFirst([.. videoSources], item);
    //        subUrl = item.Subtitle ?? "";
    //        foreach (var source in orderedSources)
    //        {
    //            (string, HttpHeaders) tempUrl;
    //            var reflex = _classReflectionHelper.GetMethodFromVideoSource(source);
    //            var method = reflex.Item1;
    //            var instance = reflex.Item2;
    //            tempUrl = await (Task<(string, HttpHeaders)>)method.Invoke(instance, [source.CheckedUrl]);
    //            if (!string.IsNullOrEmpty(tempUrl.Item1))
    //            {
    //                streamUrl = tempUrl.Item1;
    //                if (tempUrl.Item2 != null)
    //                {
    //                    headers = tempUrl.Item2;
    //                }
    //                break;
    //            }

    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        logger.LogFatal("Failed on load video extension {0}", e.Message);
    //        streamUrl = "";
    //        throw;
    //    }
    //    return (streamUrl, subUrl, headers);
    //}


    public async Task<SelectedSource> SelectSourceAsync(VideoSource[] videoSources, string byDefault = "")
    {
        HttpHeaders headers = new HttpClient().DefaultRequestHeaders;
        var streamUrl = "";
        var subtitles = new List<Track>();
        try
        {
            var item = videoSources.FirstOrDefault(e => e.Server == byDefault) ?? videoSources[0];
            var orderedSources = MoveToFirst([.. videoSources], item);
            //subUrl = item.Subtitle ?? "";
            foreach (var source in orderedSources)
            {
                SelectedSource tempUrl = new();
                var reflex = _classReflectionHelper.GetMethodFromVideoSource(source);
                var method = reflex.Item1;
                var instance = reflex.Item2;
                tempUrl = await (Task<SelectedSource>)method.Invoke(instance, [source.CheckedUrl]);
                if (!string.IsNullOrEmpty(tempUrl.StreamUrl))
                {
                    tempUrl.Subtitles.AddRange(source.Subtitles);

                    streamUrl = tempUrl.StreamUrl;
                    subtitles = tempUrl.Subtitles;
                    if (tempUrl.Headers != null)
                    {
                        headers = tempUrl.Headers;
                    }
                    break;
                }
            }
        }
        catch (Exception e)
        {
            logger.LogFatal("Failed on load video extension {0}", e.Message);
            streamUrl = "";
            throw;
        }
        //return (streamUrl, subUrl, headers);
        return new(streamUrl, subtitles, headers);
    }
}
