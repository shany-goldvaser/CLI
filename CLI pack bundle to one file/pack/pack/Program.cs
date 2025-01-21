using System;
using System.CommandLine;
using System.Xml.Linq;
static string[] languageFiles(string[] paths, string[] languages)
{
    Dictionary<string, string> languageExtensions = new()
            {
                { "REACT", ".tsx" },
                { "PYTHON", ".py" },
                { "JAVA", ".java" },
                { "JS", ".js" },
                { "C#", ".cs" },
                { "C++", ".cpp" },
                { "C", ".c" },
                { "TS", ".ts" },
                { "HTML", ".html" },
                { "CSS", ".css" },
                { "SQL", ".sql" },

            };
    var matchingPaths = new List<string>();
    if (languages[0].Equals("All", StringComparison.OrdinalIgnoreCase))
    {
        foreach (var extension in languageExtensions.Values)
        {
            foreach (var path in paths)
            {
                if (path.EndsWith(extension))
                {
                    matchingPaths.Add(path);
                }
            }
        }
    }
    else
    {
        foreach (var language in languages)
        {
            if (languageExtensions.TryGetValue(language, out var extension))
            {

                foreach (var path in paths)
                {
                    if (path.EndsWith(extension))
                    {
                        matchingPaths.Add(path);
                    }
                }
            }
        }
    }

    return matchingPaths.ToArray();

}
static string removeEmptyLines(string filePath)
{
    string[] lines = File.ReadAllLines(filePath);
    string[] nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
    return string.Join(Environment.NewLine, nonEmptyLines); 
}
static string currentPath() => Directory.GetCurrentDirectory();
var bundleOptionOutput = new Option<FileInfo>(new[] { "--output", "-o","-O" }, "file path and name") { IsRequired=true};
var bundleOptionLanguage = new Option<string[]>(new string[] { "--language", "-l","-L" }, "language to include in the bundle required")
{ IsRequired = true, AllowMultipleArgumentsPerToken = true };
var bundleOptionSort = new Option<string>(new string[] { "--sort", "-s","-S" }, "how to bundle the files abc or kind");
bundleOptionSort.SetDefaultValue("abc");
var bundleOptionNote = new Option<bool>(new string[] { "--note", "-n","-N" }, "to write a note in the file");
var bundleOptionAuthor = new Option<string>(new string[] { "--author", "-a","-A" }, "to write the author");
var bundleOptionRemoveEmptyLines = new Option<bool>(new string[]{"--remove-empty-lines","-r","-R"}, "remove empty lines in the files");
var bundlecommand = new Command("bundle", "bundle the files to one file")
{
    bundleOptionOutput,
    bundleOptionLanguage,
    bundleOptionSort,
    bundleOptionNote,
    bundleOptionAuthor,
    bundleOptionRemoveEmptyLines
};
//
bundlecommand.SetHandler((output, lang, sort, Isnote, author, IsRemove) =>
{
    try
    {  
        if(output==null||lang==null)
        {
            Console.WriteLine("you dont gave required option!");
        }
        string[] allFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories)
         .Where(file => !file.Contains(@"\bin\") && !file.Contains(@"\debug\") && !file.Contains(@"\obj\"))
         .ToArray();
        string[] filesLang = languageFiles(allFiles, lang);
        if (filesLang.Length==0)
        {
            Console.WriteLine("it is not support in this language!");
            return;
        }
        if (sort.Equals("kind", StringComparison.OrdinalIgnoreCase))
        {
            filesLang = filesLang.OrderBy(path => Path.GetExtension(path)).ToArray();
        }
        else 
            filesLang = filesLang.OrderBy(path => Path.GetFileName(path)).ToArray();

        using (FileStream fs = new(output.FullName, FileMode.Create))
        {
            using (StreamWriter writer = new(fs))
            {
                if (author != null)
                    writer.WriteLine(author);
                foreach (string filePath in filesLang)
                {
                    writer.WriteLine(Path.GetFileNameWithoutExtension(filePath));
                    if (IsRemove)
                    {
                        writer.WriteLine(removeEmptyLines(filePath));
                    }
                    else
                    {
                        writer.WriteLine(File.ReadAllText(filePath));
                    }
                };
                if (Isnote)
                {
                    writer.WriteLine(currentPath()); 
                }
            };
        }
        File.Delete("resp.rsp");
    }
    catch (DirectoryNotFoundException d)
    {
        Console.WriteLine("you dont enter a correct path");
    }


}, bundleOptionOutput,
    bundleOptionLanguage,
    bundleOptionSort,
    bundleOptionNote,
    bundleOptionAuthor,
    bundleOptionRemoveEmptyLines);
var creareRspCommand = new Command("create-rsp", "create a response file");
creareRspCommand.SetHandler(() =>
{
    try
    {
        string[] parameters = new string[7];
        int count = 0;
        parameters[count++] = "bundle";
        Console.WriteLine("Enter your languages separated by spaces (or 'all' for all languages):");
        parameters[count++] = " -l " + Console.ReadLine();
        Console.WriteLine("Enter the path where you want to save the file");
        parameters[count++] = " -o \"" + Console.ReadLine() + "\"";
        Console.WriteLine("do tou want to sort yes/no");
        string answer = Console.ReadLine();
        if (answer == "yes")
        {
            parameters[count] = " -s ";
            Console.WriteLine("Do you want to sort by 'abc' (defualt) or 'kind' ?");
            parameters[count++] += Console.ReadLine();
        }
        Console.WriteLine("Do tou want to note yes/no");
        answer = Console.ReadLine();
        if (answer == "yes")
        {
            parameters[count++] = " -n ";
        }
        Console.WriteLine("Do tou want to remove empty lines yes/no");
        answer = Console.ReadLine();
        if (answer == "yes")
        {
            parameters[count++] = " -r ";
        }
        Console.WriteLine("Do tou want to write a author yes/no");
        answer = Console.ReadLine();
        if (answer == "yes")
        {
            parameters[count++] = " -a \"" + Console.ReadLine() + "\"";
        }
        File.WriteAllLines("resp.rsp", parameters);
        Console.WriteLine("now run the command pack @resp.rsp");
    }
    catch(Exception e)
    {
        Console.WriteLine("sorry ther is a error try again:>");
    }
});

var rootCommand = new RootCommand("root command for pack files cli")
{
    bundlecommand,creareRspCommand
};
await rootCommand.InvokeAsync(args);

