using System.Text.RegularExpressions;

internal class Program
{
    // Dates are formated as [[2021-08-01]] in LogSeq - create a regex to match them
    static Regex dateRegex = new Regex(@"\[\[(?<date>\d{4}-\d{2}-\d{2})\]\]");
    // Match links in the format [[link]] in LogSeq
    static Regex linkRegex = new Regex(@"\[\[(?<link>.*)\]\]");
    // Match tags in the format tag:: value in LogSeq
    static Regex tagRegex = new Regex(@"^(?<tag>.*):: (?<value>.*)$");

    // Source and target directories
    const string sourceDirectory = "c:\\Users\\mj\\Google Drive\\Logseq\\";
    const string targetDirecotry = "c:\\Users\\mj\\Google Drive\\Obsidian\\Test\\";


    private static void Main(string[] args)
    {        
        ConvertPages();
        ConvertJournal();
    }

    /// <summary>
    /// Convert the journal files from LogSeq to Obsidian. Use the date in the file name to create the target file name and directory structure.
    /// </summary>
    static void ConvertJournal()
    {
        var journalSourceDirectory = Path.Combine(sourceDirectory, "journals");
        var journalTargetDirectory = Path.Combine(targetDirecotry, "Journal");
        var files = Directory.GetFiles(journalSourceDirectory, "*.md", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            Console.WriteLine($"Converting {file}");
            // The file name has the format 2021_08_01.md - extract the date as DateOnly
            var dateOnly = DateOnly.Parse(file.Substring(file.LastIndexOf('\\') + 1).Replace("_", "-").Replace(".md", ""), System.Globalization.CultureInfo.InvariantCulture);

            var content = File.ReadAllLines(file);
            if (content.Length == 0)
            {
                Console.WriteLine("Empty file");
                continue;
            }
            string[] newContent = ConvertFileContent(content);
            var directory = Path.Combine(journalTargetDirectory, $"{dateOnly.Year}\\{dateOnly.Month:D2}");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var targetFile = Path.Combine(journalTargetDirectory, $"{dateOnly.Year}\\{dateOnly.Month:D2}\\{dateOnly.Year}-{dateOnly.Month:D2}-{dateOnly.Day:D2}-{dateOnly.DayOfWeek}.md");
            File.WriteAllLines(targetFile, newContent);
        }
    }

    /// <summary>
    /// Convert the pages from LogSeq to Obsidian. 
    /// </summary>
    static void ConvertPages()
    {
        var pagesSourceDirectory = Path.Combine(sourceDirectory, "pages");
        var pagesTargetDirectory = Path.Combine(targetDirecotry, "pages");
        var files = Directory.GetFiles(pagesSourceDirectory, "*.md", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            Console.WriteLine($"Converting {file}");
            var content = File.ReadAllLines(file);
            if (content.Length == 0)
            {
                Console.WriteLine("Empty file");
                continue;
            }

            string[] newContent = ConvertFileContent(content);

            var targetFile = file.Replace(pagesSourceDirectory, pagesTargetDirectory);
            File.WriteAllLines(targetFile, newContent);
        }
    }

    /// <summary>
    /// Convert the content of the file from LogSeq to Obsidian format. 
    /// 
    /// Handles:
    /// 
    /// <list type="bullet"> 
    /// <item>Converts from LogSeq tags to Obsidian frontmatter</item>
    /// <item>Converts DONE to [x] and TODO to [ ]</item>
    /// </list>
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    static string[] ConvertFileContent(string[] content)
    {
        var tags = new List<Tuple<string, string>>();
        int index = 0;
        while (index < content.Length && tagRegex.IsMatch(content[index]))
        {
            var match = tagRegex.Match(content[index]);
            var tag = match.Groups["tag"].Value;
            var value = match.Groups["value"].Value;
            tags.Add(new Tuple<string, string>(tag, value));
            index++;
        }

        var newContent = content.Skip(index).ToArray();
        for (int contentLineIndex = 0; contentLineIndex < newContent.Length; contentLineIndex++)
        {
            newContent[contentLineIndex] = newContent[contentLineIndex].Replace("DONE", "[x]");
            newContent[contentLineIndex] = newContent[contentLineIndex].Replace("TODO", "[ ]");
        }

        var newTags = new List<string>();
        if (tags.Count > 0)
        {
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
        }

        newContent = newTags.Concat(newContent).ToArray();
        return newContent;
    }
}