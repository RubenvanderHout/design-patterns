using System.Text;

namespace IO;

public sealed class FileLoader : ILoader
{
    public string Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is null/empty.", nameof(path));
        if (!File.Exists(path))
            throw new FileNotFoundException("FSM file not found.", path);

        return File.ReadAllText(path);
    }

    public sealed class FileLoaderFactory : ILoaderFactory
    {
        public ILoader CreateLoader() => new FileLoader();
    }
}
