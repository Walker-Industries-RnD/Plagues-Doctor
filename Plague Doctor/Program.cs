using Plague_Doctor;
using System.Diagnostics;
using static Plague_Doctor.Functions;
using TextElements = Plague_Doctor.Functions.TextElements;
TextElements.TypeText("Click any key to start Plague Doctor.");
Console.ReadKey();
TextElements.TypeText(Art.Plagues, true, 0);
await Task.Delay(2000);
TextElements.TypeText(Art.PlaguesText, true, 0);
await Task.Delay(4000);
StartupMenu();
void StartupMenu()
{
    Console.Clear();
    TextElements.TypeText(">Home");
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    TextElements.TypeText("Welcome.");
    TextElements.Delay();
    TextElements.TypeText("Would you like to:");
    TextElements.TypeText("1 - Check for an existing Plague system");
    TextElements.TypeText("2 - Create a new Plague system");
    TextElements.TypeText("3 - Add a new Plague worker");


    TextElements.TypeText("(Please select the key for the cooresponding option, then click enter.)");
    var userEntry = Console.ReadLine();
    while (userEntry != "1" && userEntry != "2" && userEntry != "3")
    {
        TextElements.TypeText("Invalid Option; Please Try Again.");
        userEntry = Console.ReadLine().ToString();
    }
    if (userEntry == "1")
    {
        CheckForPlagueWorker();
    }
    if (userEntry == "2")
    {
        CreateNewCORE();
    }
    if (userEntry == "3")
    {
        AddNewPlagueWorker();
    }

}
void CheckForPlagueWorker()
{
    Console.Clear();
    TextElements.TypeText(">Home >Check For Plague Core");
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    TextElements.TypeText("Please enter the path of the Project with the Plague Core.");
    var userEntry = Console.ReadLine();
    while (userEntry == null)
    {
        TextElements.TypeText("Nothing entered, please try again.");
        userEntry = Console.ReadLine();
    }
    if (!Directory.Exists(userEntry))
    {
        TextElements.TypeText("Invalid path, please try again.");
        userEntry = Console.ReadLine();
    }
    CheckForCORE(userEntry);
}
void CheckForCORE(string plague)
{
    var isProperPath = Directory.Exists(plague);
    if (!isProperPath)
    {
        TextElements.TypeText("The path doesn't exist or this program may not have access to it, please try again or run on admin mode.");
        CheckForPlagueWorker();
    }
    var doesCoreExist = Functions.Core.CoreProgramExistsInDirectory(plague);
    if (!doesCoreExist)
    {
        TextElements.TypeText("No Plague Worker Exists Here; Would You Like To:");
        TextElements.TypeText("1 - Create A New Plague Worker");
        TextElements.TypeText("2 - Go Home");
        var userEntry = Console.ReadLine();
        while (userEntry != "1" && userEntry != "2")
        {
            TextElements.TypeText("Invalid Option; Please Try Again.");
            userEntry = Console.ReadLine().ToString();
        }
        if (userEntry == "1")
        {
            CreateNewCORE();
        }
        if (userEntry == "2")
        {
            StartupMenu();
        }
    }
    else
    {
        TextElements.TypeText("A Plague Exists Here; Would You Like To:");
        TextElements.TypeText("1 - Create A New Plague Worker");
        TextElements.TypeText("2 - Go Home");
        var userEntry = Console.ReadLine();
        while (userEntry != "1" && userEntry != "2")
        {
            TextElements.TypeText("Invalid Option; Please Try Again.");
            userEntry = Console.ReadLine().ToString();
        }
        if (userEntry == "1")
        {
            CreateNewCORE();
        }
        if (userEntry == "2")
        {
            StartupMenu();
        }
    }
}
void CreateNewCORE()
{
    Console.Clear();
    TextElements.TypeText(">Home >Create New Plague Core");
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    TextElements.TypeText("Please enter the path where you would like to create the new Plague Core.");

    var userEntry = Console.ReadLine();
    while (string.IsNullOrWhiteSpace(userEntry))
    {
        TextElements.TypeText("Nothing entered, please try again.");
        userEntry = Console.ReadLine();
    }
    TextElements.TypeText("Please enter the name for the new Plague Core.");
    var userEntry2 = Console.ReadLine();
    while (true)
    {
        if (string.IsNullOrWhiteSpace(userEntry2))
        {
            TextElements.TypeText("Project name cannot be empty. Please try again:");
            userEntry2 = Console.ReadLine();
            continue;
        }

        if (!IsValidName(userEntry2, out string error))
        {
            TextElements.TypeText($"Invalid project name: {error}");
            TextElements.TypeText("Please try again:");
            userEntry2 = Console.ReadLine();
            continue;
        }

        var safeProjectName = userEntry2.Trim();
        Core.CreatePlague(userEntry, safeProjectName);
        TextElements.TypeText(Art.PlagueLogo, true, 0);
        TextElements.TypeText("Plague created. Please remember to open the project and rebuild them for it to work properly. Let blood spill.");
        TextElements.TypeText("Enter anything to go home.");
        var userEntry3 = Console.ReadLine();
        StartupMenu();
    }
}
bool IsValidName(string name, out string errorMessage)
{
    errorMessage = string.Empty;
    if (string.IsNullOrWhiteSpace(name))
    {
        errorMessage = "Project name cannot be empty or whitespace.";
        return false;
    }
    var trimmed = name.Trim();
    if (trimmed != name)
    {
        errorMessage = "Project name cannot have leading or trailing spaces.";
        return false;
    }
    if (trimmed.EndsWith("."))
    {
        errorMessage = "Project name cannot end with a period (.).";
        return false;
    }
    // Forbidden characters
    char[] invalidChars = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
    if (trimmed.IndexOfAny(invalidChars) >= 0)
    {
        errorMessage = "Project name contains invalid characters: < > : \" / \\ | ? *";
        return false;
    }
    // Reserved Windows names (case-insensitive)
    string upper = trimmed.ToUpperInvariant();
    if (upper is "CON" or "PRN" or "AUX" or "NUL")
    {
        errorMessage = $"Project name '{trimmed}' is a reserved Windows device name.";
        return false;
    }
    if (upper.StartsWith("COM") || upper.StartsWith("LPT"))
    {
        if (upper.Length == 4 && char.IsDigit(upper[3]) && upper[3] >= '1' && upper[3] <= '9')
        {
            errorMessage = $"Project name '{trimmed}' is a reserved Windows device name.";
            return false;
        }
    }
    return true;
}
void AddNewPlagueWorker()
{
    while (true)
    {
        Console.Clear();
        TextElements.TypeText(">Home >Add Plague Worker");
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();

        TextElements.TypeText("Please enter the root directory containing your existing Plague projects:");
        string rootPath = Console.ReadLine()?.Trim();

        while (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
        {
            TextElements.TypeText("Invalid or empty path. Please try again:");
            rootPath = Console.ReadLine()?.Trim();
        }

        var validPlagueProjects = new List<(string ProjectName, string PlagueFullPath)>();

        try
        {
            foreach (var projectDir in Directory.GetDirectories(rootPath))
            {
                string projectName = Path.GetFileName(projectDir);
                string plaguePath = Path.Combine(projectDir, "Plague");
                if (!Directory.Exists(plaguePath))
                    continue;

                var expectedFolders = new[]
                {
                    Path.Combine(plaguePath, $"{projectName}.Core"),
                    Path.Combine(plaguePath, $"{projectName}.Interfaces"),
                    Path.Combine(plaguePath, $"{projectName}.Linux"),
                    Path.Combine(plaguePath, $"{projectName}.Windows")
                };

                if (expectedFolders.Any(Directory.Exists))
                {
                    validPlagueProjects.Add((projectName, plaguePath));
                }
            }
        }
        catch (Exception ex)
        {
            TextElements.TypeText("Error scanning directory: " + ex.Message);
            TextElements.TypeText("Please try again with a different path.");
            TextElements.Delay(2000);
            continue;
        }

        if (validPlagueProjects.Count == 0)
        {
            TextElements.TypeText("No existing Plague projects found in this directory.");
            TextElements.TypeText("Make sure you created one using option 2 first.");
            TextElements.TypeText("Enter a different root path or press Enter to try again...");
            Console.ReadLine();
            continue;
        }

        TextElements.TypeText("Found the following Plague projects:");
        Console.WriteLine();
        for (int i = 0; i < validPlagueProjects.Count; i++)
        {
            TextElements.TypeText($"{i + 1} - {validPlagueProjects[i].ProjectName}");
        }
        Console.WriteLine();

        TextElements.TypeText("Enter the number of the project to add a worker to (or press Enter to choose a different root):");
        string choice = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(choice))
        {
            continue;
        }

        if (!int.TryParse(choice, out int index) || index < 1 || index > validPlagueProjects.Count)
        {
            TextElements.TypeText("Invalid selection. Please enter a valid number.");
            TextElements.Delay(1500);
            continue;
        }

        var selected = validPlagueProjects[index - 1];
        string selectedProjectName = selected.ProjectName;
        string plagueRootPath = selected.PlagueFullPath;

        TextElements.TypeText($"Selected base project: {selectedProjectName}");

        TextElements.TypeText("Enter the name for the new Plague Worker (e.g., MyWorker, ServerNode, etc.):");
        string workerName = Console.ReadLine()?.Trim();

        while (true)
        {
            if (string.IsNullOrWhiteSpace(workerName))
            {
                TextElements.TypeText("Worker name cannot be empty. Please try again:");
                workerName = Console.ReadLine()?.Trim();
                continue;
            }

            if (!IsValidName(workerName, out string error))
            {
                TextElements.TypeText($"Invalid worker name: {error}");
                TextElements.TypeText("Please try again:");
                workerName = Console.ReadLine()?.Trim();
                continue;
            }

            workerName = workerName.Trim();
            break;
        }


        string linuxWorkerDir = Path.Combine(
            plagueRootPath,
            $"{selectedProjectName}.Linux.{workerName}"
        );

        string windowsWorkerDir = Path.Combine(
            plagueRootPath,
            $"{selectedProjectName}.Windows.{workerName}"
        );

        if (Directory.Exists(linuxWorkerDir) || Directory.Exists(windowsWorkerDir))
        {
            TextElements.TypeText(
                $"A Plague Worker named '{workerName}' already exists in this project."
            );
            TextElements.TypeText(
                "Please choose a different worker name."
            );
            TextElements.Delay(2000);
            continue;
        }


        TextElements.TypeText($"Creating Plague Worker '{workerName}'...");

        try
        {
            Functions.Core.CreatePlagueWorker(plagueRootPath, selectedProjectName, workerName);
            TextElements.TypeText($"Plague Worker '{workerName}' created successfully.");
            TextElements.TypeText("Let the infection spread.");
        }
        catch (Exception ex)
        {
            TextElements.TypeText($"Failed to create worker: {ex.Message}");
            TextElements.TypeText("Press any key to try again with different settings...");
            Console.ReadKey();
            continue;
        }

        TextElements.TypeText("Press any key to return home...");
        Console.ReadKey();
        StartupMenu();
        return;
    }
}


//This is like a signature ATP