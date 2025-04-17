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

    public async Task<SelectedSource> SelectSourceAsync(VideoSource[] videoSources, string byDefault = "")
    {
        var headers = new HttpClient().DefaultRequestHeaders;
        var (streamUrl, serverName, useVlc, subtitles) = (string.Empty, string.Empty, false, new List<Track>());

        try
        {
            var preferredSource = videoSources.FirstOrDefault(e => e.Server == byDefault) ?? videoSources[0];
            var orderedSources = MoveToFirst([.. videoSources], preferredSource);

            foreach (var source in orderedSources)
            {
                var selected = new SelectedSource();
                if (source.IsLocalSource)
                {
                    selected = new(source.Url ?? source.Code, source.Subtitles, null)
                    {
                        Server = source.Server
                    };
                }
                else
                {
                    var (method, instance) = _classReflectionHelper.GetMethodFromVideoSource(source);
                    selected = await (Task<SelectedSource>)method.Invoke(instance, [source.CheckedUrl]);
                }

                if (string.IsNullOrEmpty(selected.StreamUrl))
                {
                    continue;
                }

                serverName = source.Server;
                selected.Subtitles ??= [];
                selected.Subtitles.AddRange(source.Subtitles);
                (streamUrl, subtitles, useVlc) = (selected.StreamUrl, selected.Subtitles, selected.UseVlcProxy);
                headers = selected.Headers ?? headers;
                break;
            }
        }
        catch (Exception e)
        {
            logger.LogFatal("Failed on load video extension {0}", e.Message);
            throw;
        }

        return new SelectedSource(streamUrl, subtitles, headers)
        {
            Server = serverName,
            UseVlcProxy = useVlc
        };
    }

}
