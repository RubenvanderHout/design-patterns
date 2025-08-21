using IO;                     
using Validation;             
using UmlViewer.UI;           

bool showHelp = false;
bool running = true;

var fileLoader = new FileLoader();

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
            LoadAndRenderFlow(fileLoader);
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

static void LoadAndRenderFlow(FileLoader fileLoader)
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
    
        var raw = fileLoader.Load(path);
        var parser = new FsmFileParser();
        var dto = parser.Parse(raw);

        
        var repo = new FsmRepository(dto);
        if (repo.RootState is null)
        {
            ShowError("Validation produced no RootState (INITIAL). Check the FSM file.");
            return;
        }

        
        var builder = new RepositoryFsmViewBuilder(repo);
        var title = Path.GetFileNameWithoutExtension(path);
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

static void ShowError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine();
    Console.WriteLine($"Error: {message}");
    Console.ResetColor();
    Console.WriteLine("Press any key to return to the menu…");
    Console.ReadKey(intercept: true);
}
