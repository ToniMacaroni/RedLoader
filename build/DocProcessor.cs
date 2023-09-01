using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Nuke.Common.IO;

namespace DefaultNamespace;

public class DocProcessor
{
    readonly List<AbsolutePath> XMLPaths;

    public DocProcessor(List<AbsolutePath> xmlPaths)
    {
        XMLPaths = xmlPaths;
    }

    public void ProcessReadme(AbsolutePath template, AbsolutePath target)
    {
        if(!template.FileExists())
            throw new FileNotFoundException("Template file not found", template);
        
        if(target.FileExists())
            target.DeleteFile();

        var doc = ExtractAllDocs();
        
        var templateContent = File.ReadAllText(template);
        templateContent = templateContent.Replace("{argtable}", ArgumentDocEntry.CreateDocTable(doc));
        templateContent = templateContent.Replace("{configtable}", ConfigDocEntry.CreateDocTable(doc));
        templateContent = templateContent.Replace("{commandtable}", CommandDocEntry.CreateDocTable(doc));

        File.WriteAllText(target, templateContent);
    }

    public List<DocEntry> ExtractAllDocs()
    {
        var entries = new List<DocEntry>();
        foreach (var xmlPath in XMLPaths)
        {
            if(!xmlPath.FileExists())
            {
                Serilog.Log.Warning($"XML file not found: {xmlPath}");
                continue;
            }

            entries.AddRange(ExtractDocumentation(xmlPath));
        }

        return entries;
    }

    public List<DocEntry> ExtractDocumentation(string xmlPath)
    {
        var xml = XDocument.Load(xmlPath);

        var entries = new List<DocEntry>();

        foreach (var member in xml.Root.Element("members").Elements("member"))
        {
            var name = member.Attribute("name").Value;
            var summary = member.Element("summary")?.Value.Trim();

            var arg = member.Element("arg")?.Value;
            var config = member.Element("config")?.Value;
            var command = member.Element("command")?.Value;

            if (!string.IsNullOrEmpty(arg) && !string.IsNullOrEmpty("summary"))
            {
                var example = member.Element("example")?.Value.Trim();
                var argEntry = new ArgumentDocEntry(arg, summary, example);
                entries.Add(argEntry);
            }
            
            if (!string.IsNullOrEmpty(config) && !string.IsNullOrEmpty("summary"))
            {
                var type = member.Element("type")?.Value.Trim();
                var argEntry = new ConfigDocEntry(config, summary, type);
                entries.Add(argEntry);
            }
            
            if (!string.IsNullOrEmpty(command) && !string.IsNullOrEmpty("summary"))
            {
                var example = member.Element("example")?.Value.Trim();
                var argEntry = new CommandDocEntry(command, summary, example);
                entries.Add(argEntry);
            }
        }

        return entries;
    }
}