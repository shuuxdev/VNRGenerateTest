using System.Reflection;
using System.Text.RegularExpressions;

public static class MainFunctionality
{
    private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    public static async Task<List<ResultModel>> GenerateDataForTestingFromViewAndModel(string viewPath, string modelPath)
    {
        var matchers = typeof(PatternMatchers).GetMethods().ToList();

        List<Task<List<ResultModel>>> tasks = new();
        List<Matcher> listMatchers = new List<Matcher>();
        matchers.ForEach(matcher =>
        {
            var patternAttribute = matcher.GetCustomAttribute<PatternAttribute>();
            if (patternAttribute == null)
            {
                return;
            };
            listMatchers.Add(new Matcher()
            {
                Pattern = patternAttribute.pattern,
                Method = matcher.CreateDelegate<Matcher.PatternMatcher>()
            });
        });



        List<(string controlType, int startIndex, string pattern)> controlTypesAndItsIndex = new();

        listMatchers.ForEach(matcher =>
        {
            tasks.Add(matcher.Method.Invoke(viewPath, modelPath, matcher.Pattern));
        });
        List<ResultModel> results = (await Task.WhenAll(tasks)).SelectMany(resultFromMatcher => resultFromMatcher).ToList();
        return results;
    }

    /// <summary>
    /// Sinh toàn bộ test cho phân hệ được chọn
    /// </summary>
    /// <param name="category"></param>
    /// <param name="viewsPath"></param>
    /// <param name="modelsPath"></param>
    /// <returns></returns>
    public static async Task GenerateTests(Category category, string viewsPath, string modelsPath)
    {
        Dictionary<string, string> viewAndModel = await ToViewsAndModelsDictionary(category, viewsPath, modelsPath);
        List<Task<List<ResultModel>>> tasks = new();
        foreach ((string viewPath, string modelPath) in viewAndModel)
        {
            tasks.Add(GenerateDataForTestingFromViewAndModel(viewPath, modelPath));
        }
        List<List<ResultModel>> results = (await Task.WhenAll(tasks)).ToList();

        await Task.WhenAll(results.Select(WriteToFile));
    }
    public static async Task<Dictionary<string, string>> ToViewsAndModelsDictionary(Category category, string viewsPath, string modelsPath)
    {
        string[] viewDirectories = Directory.GetDirectories(viewsPath, $"*{Enum.GetName(category)}_*");
        string[] modelPaths = null;
        if (modelsPath.EndsWith("Presentation"))
        {
            string[] modelDirectories = Directory.GetDirectories(modelsPath, "HRM.Presentation.*.Models");
            List<string> tempModelFilePaths = new();
            foreach (string modelDirectory in modelDirectories)
            {
                tempModelFilePaths.AddRange(Directory.GetFiles(modelDirectory, $"*{Enum.GetName(category)}_*"));
            }
            modelPaths = tempModelFilePaths.ToArray();
        }
        else
        {
            modelPaths = Directory.GetFiles(modelsPath, $"*{Enum.GetName(category)}_*");
        }
        //viewDirectories = viewDirectories.Take(5).ToArray();
        List<(string viewPath, string model)> modelsUsed = new();
        Dictionary<string, string> view = new();
        //Loop qua toàn bộ màn hình của phân hệ
        foreach (string viewDirectoryPath in viewDirectories)
        {
            //Với mỗi màn hình sẽ có các file Cshtml
            string[] viewPaths = Directory.GetFiles(viewDirectoryPath);
            foreach (string viewPath in viewPaths)
            {
                await foreach (string line in File.ReadLinesAsync(viewPath))
                {
                    //Tìm Model được sử dụng trong file cshtml này;
                    Match match = Regex.Match(line, @"^\@model\s+([a-zA-Z_.]*)", RegexOptions.Multiline);
                    if (match.Success)
                    {
                        string[] classNameWithDotsSeperated = match.Groups[1].Value.Split(".");
                        string className = classNameWithDotsSeperated[classNameWithDotsSeperated.Length - 1];
                        modelsUsed.Add((viewPath, className));
                        break;
                    }
                }
            }
        }

        foreach (string modelPath in modelPaths)
        {
            await foreach (string line in File.ReadLinesAsync(modelPath))
            {
                //Tìm Model được sử dụng trong file cshtml này;
                foreach ((string viewPath, string model) in modelsUsed)
                {
                    Match match = Regex.Match(line, @$"class\s+{model}", RegexOptions.Multiline);
                    if (match.Success)
                    {
                        view[viewPath] = modelPath;
                        break;
                    }
                }
            }
        }
        return view;
    }
    public static async Task WriteToFile(List<ResultModel> data)
    {
        if (data.Count > 0)
        {
            ResultModel r = data[0];
            string mainDirPath = $"./Results/{r.category}/{r.followingName}";
            string objectPath = $"./Results/{r.category}/{r.followingName}/{r.followingName}_{r.viewName}Object.cs";
            string pagePath = $"./Results/{r.category}/{r.followingName}/{r.followingName}_{r.viewName}Page.cs";
            string mainPath = $"./Results/{r.category}/{r.followingName}/{r.followingName}_{r.viewName}.cs";

            Directory.CreateDirectory(mainDirPath);



            await MainFunctionality.WriteObjectFile(objectPath, data);
            await MainFunctionality.WritePageFile(pagePath, data);
            await MainFunctionality.WriteMainFile(mainPath, data);

        }
    }
    public static async Task WriteObjectFile(string objectPath, List<ResultModel> results)
    {
        await semaphore.WaitAsync();
        try
        {
            ResultModel r = results[0];

            string objectFile = await File.ReadAllTextAsync("./Template/ObjectTemplate.cs");
            string properties = string.Empty;
            foreach (ResultModel result in results)
            {
                properties += $"public static string {result.fieldName} {{get;set;}}";
                properties += Environment.NewLine;
            }
            objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.CATEGORY_GOES_HERE)!, r.category);
            //Tên màn hình (Tiếng việt)
            objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.PAGENAME_GOES_HERE)!, r.className);
            objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.CLASSNAME_GOES_HERE)!, r.className);
            objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.PROPERTIES_GOES_HERE)!, properties);

            using (StreamWriter writer = new StreamWriter(objectPath))
            {
                // Write the contents to the file
                await writer.WriteAsync(objectFile);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    public static async Task WritePageFile(string pagePath, List<ResultModel> results)
    {
        await semaphore.WaitAsync();
        try
        {
            ResultModel r = results[0];
            string xPaths = string.Empty;
            foreach (ResultModel result in results)
            {
                xPaths += (@$"[LanguageKeyMapping(""{result.languageKey}"")]
        public WebElement {result.fieldType}_{result.pageType}_{result.fieldName} => FindWebElement(By.XPath(""//{result.htmlElement}[@id='{result.name}']""));");
                xPaths += Environment.NewLine;
            }
            string pageFile = await File.ReadAllTextAsync("./Template/PageTemplate.cs");
            pageFile = pageFile.Replace(Enum.GetName(TemplatePlaceholder.CATEGORY_GOES_HERE)!, r.category);
            pageFile = pageFile.Replace(Enum.GetName(TemplatePlaceholder.PAGENAME_GOES_HERE)!, r.className);
            pageFile = pageFile.Replace(Enum.GetName(TemplatePlaceholder.CLASSNAME_GOES_HERE)!, r.className);
            pageFile = pageFile.Replace(Enum.GetName(TemplatePlaceholder.XPATH_GOES_HERE)!, xPaths);

            using (StreamWriter writer = new StreamWriter(pagePath))
            {
                // Write the contents to the file
                await writer.WriteAsync(pageFile);
            }
        }
        finally
        {
            semaphore.Release();
        }

    }
    public static async Task WriteMainFile(string mainPath, List<ResultModel> results)
    {
        await semaphore.WaitAsync();
        try
        {
            ResultModel r = results[0];
            string mainFile = await File.ReadAllTextAsync("./Template/MainTemplate.cs");
            mainFile = mainFile.Replace(Enum.GetName(TemplatePlaceholder.CATEGORY_GOES_HERE)!, r.category);
            mainFile = mainFile.Replace(Enum.GetName(TemplatePlaceholder.PAGENAME_GOES_HERE)!, r.className);
            mainFile = mainFile.Replace(Enum.GetName(TemplatePlaceholder.CLASSNAME_GOES_HERE)!, r.className);

            using (StreamWriter writer = new StreamWriter(mainPath))
            {
                // Write the contents to the file
                await writer.WriteAsync(mainFile);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
}