using System.Text.RegularExpressions;

public static class LoggingHelper {
     public static async Task<IEnumerable<IGrouping<string, string>>> FindAllKindOfCodingStyles(string[] paths, string pattern, RegexOptions options = RegexOptions.None)
    {
        List<Task<List<string>>> tasks = paths.Select(item =>
        {
            return Task.Run(async () =>
            {
                string content = await File.ReadAllTextAsync(item);
                MatchCollection matchCollection = Regex.Matches(content, pattern, options);
                return matchCollection.Select(item => item.Groups[1].Value).ToList();
            });
        }).ToList();

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(item => item).Order().GroupBy(item => item);
    }

    public static async Task FindAllAssembliesUsedInModels()
    {
        string pathModels = @"D:\Code\Main\Source\Presentation";
        string[] modelDirectories = Directory.GetDirectories(pathModels, "HRM.Presentation.*.Models", SearchOption.AllDirectories);

        List<string[]> ls = new List<string[]>();
        int totalFile = 0;
        // Iterate over each matched directory
        foreach (string modelDirectory in modelDirectories)
        {
            // Get all C# files (*.cs) in the current directory
            string[] csFiles = Directory.GetFiles(modelDirectory, "*.cs");

            ls.Add(csFiles);

        }
        string[] paths = ls.SelectMany(i => i).ToArray();
        var result = await FindAllKindOfCodingStyles(paths, @"^using\s*([a-zA-Z.]*)", RegexOptions.Multiline);
        foreach (IGrouping<string, string> item in result)
        {
            Console.WriteLine("{0} - {1}", item.Key, item.Count());
        }
        Console.WriteLine("Total files: {0}", paths.Length);
    }
}