using IO;                     
using Validation;             
using UmlViewer.UI;
using static IO.FileLoader;
using Validation.ValidationRules;

bool showHelp = false;
bool running = true;

ILoaderFactory loaderFactory = new FileLoaderFactory();

while (running)
{
    Console.Clear();

    Console.WriteLine("=== Welcome to UML viewer ===");
    Console.WriteLine("Press 'l' to load a file, 'h' to toggle Help, 'q' to quit.");

    if (showHelp) HelpMenu();

    var key = Console.ReadKey(intercept: true).Key;
    switch (key)
    {
        case ConsoleKey.H:
            showHelp = !showHelp;
            break;

        case ConsoleKey.Q:
            running = false;
            break;

        case ConsoleKey.L:
            LoadAndRenderFlow(loaderFactory);
            break;
    }
}

// ===== helpers =====

static void HelpMenu()
{
    Console.WriteLine();
    Console.WriteLine("=== Help Menu ===");
    Console.WriteLine("l - Load and render a .txt FSM file");
    Console.WriteLine("h - Toggle this help menu");
    Console.WriteLine("q - Quit the application");
}

static void LoadAndRenderFlow(ILoaderFactory loaderFactory)
{
    Console.WriteLine();
    Console.Write("Type a valid path to an FSM .txt file: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        ShowError("No path provided.");
        return;
    }

    var path = input.Trim('"', ' '); // allow pasted paths with quotes

    if (!File.Exists(path))
    {
        ShowError($"File not found: {path}");
        return;
    }

    try
    {
        var fileLoader = loaderFactory.CreateLoader();
        
        var raw = fileLoader.Load(path);
        var parser = new FsmFileParser();
        var dto = parser.Parse(raw);

        var rules = Array.Empty<IValidationRule>();
        var ruleParser = new FsmRuleParser(rules, dto);
        var repo = ruleParser.Repo;   

        
        var builder = new RepositoryFsmViewBuilder(repo);
         var title = TryExtractTitle(raw) ?? Path.GetFileNameWithoutExtension(path);
        var view = builder.BuildFromRepository(title);

        var renderer = new TextRenderer();
        var output = renderer.Render(view);

        
        Console.Clear();
        Console.WriteLine(output);

        Console.WriteLine();
        Console.WriteLine("Press any key to return to the menu…");
        Console.ReadKey(intercept: true);
    }
    catch (ParseException ex)
    {
        ShowError($"Parse error: {ex.Message}");
    }
    catch (UnauthorizedAccessException)
    {
        ShowError("Access denied. Run from a location you can read, or choose another file.");
    }
    catch (IOException io)
    {
        ShowError($"I/O error: {io.Message}");
    }
    catch (Exception ex)
    {
        ShowError($"Unexpected error: {ex.Message}");
    }
}

static string? TryExtractTitle(string raw)
{
    var lines = raw.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    foreach (var line in lines)
    {
        var t = line.Trim();
        if (t.Length == 0) continue;
        if (t.StartsWith("#"))
        {
            var candidate = t.TrimStart('#').Trim();
            if (candidate.Length > 0) return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(candidate.ToLower());
            continue;
        }
        break; // real content reached
    }
    return null;
}

static void ShowError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine();
    Console.WriteLine($"Error: {message}");
    Console.ResetColor();
    Console.WriteLine("Press any key to return to the menu…");
    Console.ReadKey(intercept: true);
}
