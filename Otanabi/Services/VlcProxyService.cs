using System.Net.Http.Headers;
using LibVLCSharp.Shared;
using Otanabi.Contracts.Services;

namespace Otanabi.Services;
public class VlcProxyService : IVlcProxyService
{
    private static LibVLC _libVLC;
    private static MediaPlayer _mediaPlayer;

    public Task<string> StartProxyAsync(string streamUrl, HttpRequestHeaders headers)
    {
        _libVLC ??= new LibVLC("--no-video", "--intf", "dummy", "--sout-keep");
        var httpProxyUrl = "http://127.0.0.1:8080";
        var headerArgs = string.Join(" ", headers.Select(h => $"http-header={h.Key}:{string.Join(",", h.Value)}"));
        var media = new Media(_libVLC, streamUrl, FromType.FromLocation);
        var referer = headers?.TryGetValues("Referer", out var values) == true ? values.FirstOrDefault() : null;
        if (!string.IsNullOrEmpty(referer))
        {
            media.AddOption($":http-referrer={referer}");
        }
        media.AddOption($":sout=#duplicate{{dst=standard{{access=http,mux=ts,dst=127.0.0.1:8080}}}}");

        _mediaPlayer = new MediaPlayer(media);
        _mediaPlayer.Play();

        return Task.FromResult(httpProxyUrl);
    }

    public void StopProxy()
    {
        _mediaPlayer?.Stop();
        _mediaPlayer?.Dispose();
        _mediaPlayer = null;
        _libVLC?.Dispose();
        _libVLC = null;
    }

    public void Dispose()
    {
        StopProxy();
        _mediaPlayer?.Dispose();
        _libVLC?.Dispose();
    }
}
