using System.Text;
using Newtonsoft.Json;
using Otanabi.Core.Contracts.Services;

namespace Otanabi.Core.Services;

public class FileService : IFileService
{
    public T Read<T>(string folderPath, string fileName)
    {
        var path = Path.Combine(folderPath, fileName);
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        return default;
    }

    public void Save<T>(string folderPath, string fileName, T content)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fullPath = Path.Combine(folderPath, fileName);
        var fileContent = JsonConvert.SerializeObject(content);

        using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(fileContent);
            }
        }
    }

    public void Delete(string folderPath, string fileName)
    {
        if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
        {
            File.Delete(Path.Combine(folderPath, fileName));
        }
    }
}
