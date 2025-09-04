using System.Xml;
using System.Xml.Linq;

namespace BacpacEditor.Services;

public static class XmlProcessor
{
    public static void ProcessXmlFile(string filePath, string[] elementTypesToRemove)
    {
        var xmlDoc = LoadXmlDocument(filePath);
        var elementsToRemove = FindElementsToRemove(xmlDoc, elementTypesToRemove);
        
        if (elementsToRemove.Count == 0)
        {
            Console.WriteLine($"No elements found with specified types");
            return;
        }

        RemoveElements(xmlDoc, elementsToRemove);
        SaveXmlDocument(xmlDoc, filePath);
        ReportRemovedElements(elementsToRemove);
    }

    private static XDocument LoadXmlDocument(string filePath)
    {
        try
        {
            return XDocument.Load(filePath);
        }
        catch (XmlException ex)
        {
            throw new Exception($"Invalid XML file: {ex.Message}");
        }
    }

    private static List<XElement> FindElementsToRemove(XDocument xmlDoc, string[] elementTypesToRemove)
    {
        return xmlDoc.Descendants()
            .Where(e => e.Attribute("Type") != null && 
                       elementTypesToRemove.Contains(e.Attribute("Type")?.Value))
            .ToList();
    }

    private static void RemoveElements(XDocument xmlDoc, List<XElement> elementsToRemove)
    {
        foreach (var element in elementsToRemove)
        {
            element.Remove();
        }
    }

    private static void SaveXmlDocument(XDocument xmlDoc, string filePath)
    {
        try
        {
            xmlDoc.Save(filePath);
            Console.WriteLine($"File saved: {filePath}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save file: {ex.Message}");
        }
    }

    private static void ReportRemovedElements(List<XElement> elementsToRemove)
    {
        var removedByType = elementsToRemove
            .GroupBy(e => e.Attribute("Type")?.Value ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        Console.WriteLine($"Removed {elementsToRemove.Count} total element(s):");
        foreach (var kvp in removedByType.OrderBy(x => x.Key))
        {
            Console.WriteLine($"  - {kvp.Key}: {kvp.Value} element(s)");
        }
    }
}
