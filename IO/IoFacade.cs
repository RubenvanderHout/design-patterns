using IO.DTO;

namespace IO;

/// <summary>
/// Facade over loader + parser: `LoadAndParse(path)`
/// </summary>
public sealed class IoFacade
{
    private readonly ILoaderFactory _loaderFactory;
    private readonly IParserFactory _parserFactory;

    public IoFacade()
        : this(new FileLoader.FileLoaderFactory(), new FsmFileParserFactory()) { }

    public IoFacade(ILoaderFactory loaderFactory, IParserFactory parserFactory)
    {
        _loaderFactory = loaderFactory ?? throw new ArgumentNullException(nameof(loaderFactory));
        _parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
    }

    public string Load(string path)
    {
        var loader = _loaderFactory.CreateLoader();
        return loader.Load(path);
    }

    public FsmDto Parse(string raw)
    {
        var parser = _parserFactory.CreateParser();
        return parser.Parse(raw);
    }

    public FsmDto LoadAndParse(string path)
    {
        var raw = Load(path);
        return Parse(raw);
    }
}
