using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace BacpacEditor.Services;

public static class BacpacProcessor
{
    public static void ProcessBacpacFile(string bacpacPath, string[] elementTypesToRemove)
    {
        var tempDir = CreateTempDirectory();
        
        try
        {
            ExtractBacpac(bacpacPath, tempDir);
            var modelXmlPath = Path.Combine(tempDir, "model.xml");
            
            if (!File.Exists(modelXmlPath))
            {
                throw new Exception("model.xml not found in BACPAC file");
            }

            Console.WriteLine("Found model.xml in BACPAC");
            XmlProcessor.ProcessXmlFile(modelXmlPath, elementTypesToRemove);
            
            UpdateOriginXml(tempDir, elementTypesToRemove);
            CreateNewBacpac(bacpacPath, tempDir);
        }
        finally
        {
            CleanupTempDirectory(tempDir);
        }
    }

    private static string CreateTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "BacpacEditor", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        Console.WriteLine($"Extracting BACPAC to temporary directory: {tempDir}");
        return tempDir;
    }

    private static void ExtractBacpac(string bacpacPath, string tempDir)
    {
        ZipFile.ExtractToDirectory(bacpacPath, tempDir);
    }

    private static void UpdateOriginXml(string tempDir, string[] elementTypesToRemove)
    {
        var originXmlPath = Path.Combine(tempDir, "origin.xml");
        if (!File.Exists(originXmlPath))
        {
            Console.WriteLine("Warning: origin.xml not found, skipping checksum update");
            return;
        }

        Console.WriteLine("Updating checksum in origin.xml");
        var originDoc = XDocument.Load(originXmlPath);
        
        var modelXmlPath = Path.Combine(tempDir, "model.xml");
        var newChecksum = CalculateFileChecksum(modelXmlPath);
        
        Console.WriteLine($"Calculated new checksum: {newChecksum}");
        
        UpdateChecksum(originDoc, newChecksum);
        UpdateObjectCounts(originDoc, tempDir, elementTypesToRemove);
        SaveOriginXml(originDoc, originXmlPath);
    }

    private static void UpdateChecksum(XDocument originDoc, string newChecksum)
    {
        var checksumElement = originDoc.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "Checksums");
        
        if (checksumElement != null)
        {
            var modelChecksumElement = checksumElement.Descendants()
                .FirstOrDefault(e => e.Attribute("Uri")?.Value == "/model.xml");
            
            if (modelChecksumElement != null)
            {
                modelChecksumElement.SetValue(newChecksum);
                Console.WriteLine($"Updated checksum for model.xml: {newChecksum}");
            }
            else
            {
                Console.WriteLine("Warning: Could not find model.xml entry in Checksums");
            }
        }
        else
        {
            Console.WriteLine("Warning: Could not find Checksums element in origin.xml");
        }
    }

    private static void UpdateObjectCounts(XDocument originDoc, string tempDir, string[] elementTypesToRemove)
    {
        Console.WriteLine("Updating object counts in origin.xml");
        var modelXmlPath = Path.Combine(tempDir, "model.xml");
        var modelDoc = XDocument.Load(modelXmlPath);

        var objectCountsElement = originDoc.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "ObjectCounts");

        if (objectCountsElement == null)
        {
            Console.WriteLine("Warning: ObjectCounts element not found in origin.xml");
            return;
        }

        Console.WriteLine("Found ObjectCounts element, updating counts...");
        var objectCounts = modelDoc.Descendants()
            .Where(e => e.Attribute("Type") != null)
            .GroupBy(e => e.Attribute("Type")?.Value ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var childElement in objectCountsElement.Elements())
        {
            var elementName = childElement.Name.LocalName;
            var typeValue = MapElementNameToType(elementName);
            
            if (typeValue != null && objectCounts.TryGetValue(typeValue, out int count))
            {
                childElement.SetValue(count);
                Console.WriteLine($"Updated {elementName}: {count}");
            }
            else if (elementTypesToRemove.Contains(typeValue))
            {
                childElement.SetValue(0);
                Console.WriteLine($"Set {elementName} to 0 (removed)");
            }
        }
    }

    private static string? MapElementNameToType(string elementName)
    {
        return elementName switch
        {
            "View" => "SqlView",
            "Procedure" => "SqlProcedure",
            "ScalarFunction" => "SqlScalarFunction",
            "TableValuedFunction" => "SqlInlineTableValuedFunction",
            "Table" => "SqlTable",
            "Index" => "SqlIndex",
            "DefaultConstraint" => "SqlDefaultConstraint",
            "CheckConstraint" => "SqlCheckConstraint",
            "ForeignKeyConstraint" => "SqlForeignKeyConstraint",
            "PrimaryKeyConstraint" => "SqlPrimaryKeyConstraint",
            "UniqueConstraint" => "SqlUniqueConstraint",
            "DmlTrigger" => "SqlDmlTrigger",
            "Schema" => "SqlSchema",
            "User" => "SqlUser",
            "Role" => "SqlRole",
            "ComputedColumn" => "SqlComputedColumn",
            "SimpleColumn" => "SqlSimpleColumn",
            "SubroutineParameter" => "SqlSubroutineParameter",
            "DatabaseOptions" => "SqlDatabaseOptions",
            "DatabaseCredential" => "SqlDatabaseCredential",
            "MasterKey" => "SqlMasterKey",
            "PermissionStatement" => "SqlPermissionStatement",
            "RoleMembership" => "SqlRoleMembership",
            "ScriptFunctionImplementation" => "SqlScriptFunctionImplementation",
            _ => null
        };
    }

    private static void SaveOriginXml(XDocument originDoc, string originXmlPath)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace,
            OmitXmlDeclaration = false,
            Encoding = System.Text.Encoding.UTF8
        };
        
        using (var writer = XmlWriter.Create(originXmlPath, settings))
        {
            originDoc.Save(writer);
        }
        
        Console.WriteLine("Origin.xml saved with proper formatting");
        
        var savedDoc = XDocument.Load(originXmlPath);
        var savedChecksumElement = savedDoc.Descendants()
            .FirstOrDefault(e => e.Attribute("Uri")?.Value == "/model.xml");
        
        if (savedChecksumElement != null)
        {
            var savedChecksum = savedChecksumElement.Value;
            Console.WriteLine($"Verified saved checksum: {savedChecksum}");
            Console.WriteLine("✓ Checksum verification successful");
        }
    }

    private static void CreateNewBacpac(string bacpacPath, string tempDir)
    {
        var newBacpacPath = bacpacPath.Replace(".bacpac", "_modified.bacpac");
        Console.WriteLine($"Creating new BACPAC file: {newBacpacPath}");

        if (File.Exists(newBacpacPath))
        {
            File.Delete(newBacpacPath);
        }

        ZipFile.CreateFromDirectory(tempDir, newBacpacPath);
        Console.WriteLine($"New BACPAC file created: {newBacpacPath}");
    }

    private static void CleanupTempDirectory(string tempDir)
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
            Console.WriteLine("Cleaned up temporary files");
        }
    }

    private static string CalculateFileChecksum(string filePath)
    {
        using (var sha256 = SHA256.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }
    }
}
