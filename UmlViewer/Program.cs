using IO;

bool showHelp = false;
bool running = true;

var fileLoader = new FileLoader();

while (running)
{
    Console.Clear();

    Console.WriteLine("=== Welcome to UML viewer ===");
    Console.WriteLine("Press 'l' to load a file 'h' to toggle Help, 'q' to quit.");

    // Render help menu if needed
    if (showHelp)
    {
        HelpMenu();
    }

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
            Console.WriteLine("Type in a valid path");
            var input = Console.ReadLine();

            if (input == null)
            {
                Console.WriteLine("Please type in a valid path");
            }

            break;

    }

}


static void HelpMenu()
{
    Console.WriteLine();
    Console.WriteLine("=== Help Menu ===");
    Console.WriteLine("h - Toggle this help menu");
    Console.WriteLine("q - Quit the application");
}

