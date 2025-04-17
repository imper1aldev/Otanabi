using System.Net.Http.Headers;

namespace Otanabi.Contracts.Services;
public interface IVlcProxyService : IDisposable
{
    Task<string> StartProxyAsync(string streamUrl, HttpRequestHeaders headers);
    void StopProxy();
}
