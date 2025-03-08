// See https://aka.ms/new-console-template for more information
using System.Text.RegularExpressions;

Console.WriteLine("Hello, World!");

var sourceDirectory = "c:\\Users\\mj\\Google Drive\\Logseq\\pages\\";
var targetDirecotry = "c:\\Users\\mj\\Google Drive\\Obsidian\\Test\\Pages\\";

var files = Directory.GetFiles(sourceDirectory, "*.md", SearchOption.AllDirectories);
foreach (var file in files)
{
    Console.WriteLine($"Converting {file}");
    var content = File.ReadAllLines(file);

    if (content.Length == 0)
    {
        Console.WriteLine("Empty file");
        continue;
    }
    var tags = new List<Tuple<string, string>>();
    // Regeg to match LogSeq tags
    var tagRegex = new Regex(@"^(?<tag>.*):: (?<value>.*)$");
    // Regex to match LogSeq links until lines do not match
    int index = 0;
    while (index < content.Length && tagRegex.IsMatch(content[index]))
    {
        var match = tagRegex.Match(content[index]);
        var tag = match.Groups["tag"].Value;
        var value = match.Groups["value"].Value;
        // Console.WriteLine($"{tag} -> {value}");

        tags.Add(new Tuple<string, string>(tag, value));
        index++;
    }

    var newContent = content.Skip(index).ToArray();
    for (int contentLineIndex = 0; contentLineIndex < newContent.Length; contentLineIndex++)
    {
        // line starts with '-' then strip it by removing two characters
        if (newContent[contentLineIndex].Length > 1 && newContent[contentLineIndex][0] == '-')
        {
            newContent[contentLineIndex] = newContent[contentLineIndex].Substring(2);
        }
        else if (newContent[contentLineIndex].Length > 0)
        {
            // Strip first character - a tab
            newContent[contentLineIndex] = newContent[contentLineIndex].Substring(1);
        }
    }

    // Dates are formated as [[2021-08-01]] in LogSeq - create a regex to match them
    var dateRegex = new Regex(@"\[\[(?<date>\d{4}-\d{2}-\d{2})\]\]");
    var linkRegex = new Regex(@"\[\[(?<link>.*)\]\]");
    var newTags = new List<string>();
    newTags.Add("---");
    foreach (var tag in tags)
    {
        var value = tag.Item2;
        var dateMatch = dateRegex.Match(value);
        var linkMatch = linkRegex.Match(value);
        if (dateMatch.Success)
        {
            value = dateMatch.Groups["date"].Value;
        }
        else if (linkMatch.Success)
        {
            value = $"\"[[{linkMatch.Groups["link"].Value}]]\"";
        }

        newTags.Add($"{tag.Item1.Replace(" ", "")}: {value}");
    }
    newTags.Add("---");

    newContent = newTags.Concat(newContent).ToArray();

    foreach (var line in newContent)
    {
        Console.WriteLine(line);
    }

    var targetFile = file.Replace(sourceDirectory, targetDirecotry);
    File.WriteAllLines(targetFile, newContent);
}