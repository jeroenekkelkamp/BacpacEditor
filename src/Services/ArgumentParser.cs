using BacpacEditor.Models;

namespace BacpacEditor.Services;

public static class ArgumentParser
{
    public static (string filePath, string[] elementTypes) ParseArguments(string[] args)
    {
        if (args.Length < 2)
        {
            ShowUsage();
            Environment.Exit(1);
        }

        var filePath = args[0];
        var selectionOptions = args.Skip(1).ToArray();

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File '{filePath}' does not exist.");
            Environment.Exit(1);
        }

        var elementTypes = GetElementTypesFromSelections(selectionOptions);
        return (filePath, elementTypes);
    }

    private static string[] GetElementTypesFromSelections(string[] selectionOptions)
    {
        var allElementTypes = new List<string>();
        var validSelections = new List<string>();
        
        foreach (var selectionOption in selectionOptions)
        {
            if (ElementTypeGroups.Groups.TryGetValue(selectionOption, out var elementTypes))
            {
                allElementTypes.AddRange(elementTypes);
                validSelections.Add(selectionOption);
            }
            else
            {
                Console.WriteLine($"Error: Unknown selection option '{selectionOption}'");
                ShowUsage();
                Environment.Exit(1);
            }
        }

        var uniqueElementTypes = allElementTypes.Distinct().ToArray();
        Console.WriteLine($"Selections: {string.Join(", ", validSelections)}");
        Console.WriteLine($"Removing elements: {string.Join(", ", uniqueElementTypes)}");
        
        return uniqueElementTypes;
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Usage: BacpacEditor <file-path> <selection-option> [additional-options...]");
        Console.WriteLine();
        Console.WriteLine("Selection Options:");
        Console.WriteLine("  --Views              Remove all views");
        Console.WriteLine("  --StoredProcedures   Remove all stored procedures");
        Console.WriteLine("  --Functions          Remove all functions");
        Console.WriteLine("  --Tables             Remove all tables");
        Console.WriteLine("  --Indexes            Remove all indexes");
        Console.WriteLine("  --Constraints        Remove all constraints");
        Console.WriteLine("  --Triggers           Remove all triggers");
        Console.WriteLine("  --Schemas            Remove all schemas");
        Console.WriteLine("  --Users              Remove all users");
        Console.WriteLine("  --Roles              Remove all roles");
        Console.WriteLine("  --AllObjects         Remove all database objects");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  BacpacEditor database.bacpac --Views");
        Console.WriteLine("  BacpacEditor model.xml --StoredProcedures");
        Console.WriteLine("  BacpacEditor database.bacpac --Views --StoredProcedures");
        Console.WriteLine("  BacpacEditor database.bacpac --Functions --Triggers");
        Console.WriteLine("  BacpacEditor model.xml --Views --StoredProcedures --Functions");
    }
}
