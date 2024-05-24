using AnimeWatcher.Core.Models;
using System.Reflection;

namespace AnimeWatcher.Core.Helpers;
public class ClassReflectionHelper
{
    public (string, string) GetProviderClassName(string className)
    {
        var classType = Type.GetType(className);
        var classInstance = Activator.CreateInstance(classType);
        var methodName = classType.GetMethod("GetSourceName");
        var methodUrl = classType.GetMethod("GetUrl");

        var sourceName = (string)methodName.Invoke(classInstance, new object[] { });
        var sourceUrl = (string)methodUrl.Invoke(classInstance, new object[] { });
        return (sourceName, sourceUrl);
    }

    public Type[] ExtractAssembliesOnlyClass(string namesp)
    {
        var types = new List<Type>();

        var q = Assembly.GetExecutingAssembly()
                           .GetTypes()
                           .Where(t => t.Namespace == namesp)
                           .Select(t => t);

        var data = q.ToList();
        foreach (var cls in data)
        {
            if (!cls.Name.Contains("<"))
            {
                types.Add(cls);

            }
        }
        return types.ToArray();
    }

    public (MethodInfo, object) GetMethodFromProvider(string methodName,Provider provider)
    {
        var provCl = provider.Name.Substring(0, 1).ToUpper() + provider.Name.Substring(1).ToLower();
        var extractorType = Type.GetType($"AnimeWatcher.Core.Extractors.{provCl}Extractor");
        var extractorInstance = Activator.CreateInstance(extractorType);
        var method = extractorType.GetMethod(methodName);
        return (method, extractorInstance);
    }


}
