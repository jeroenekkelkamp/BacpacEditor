using BacpacEditor.Services;

namespace BacpacEditor;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            var (filePath, elementTypes) = ArgumentParser.ParseArguments(args);
            
            Console.WriteLine($"Processing file: {filePath}");

            if (filePath.EndsWith(".bacpac", StringComparison.OrdinalIgnoreCase))
            {
                BacpacProcessor.ProcessBacpacFile(filePath, elementTypes);
            }
            else
            {
                XmlProcessor.ProcessXmlFile(filePath, elementTypes);
            }

            Console.WriteLine("Processing completed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}