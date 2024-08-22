using System.Collections.Specialized;

namespace AnimeWatcher.Contracts.Services;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);
    bool ShowByUpdate();


    NameValueCollection ParseArguments(string arguments);

    void Unregister();
}
