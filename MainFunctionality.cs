using System.Reflection;
using System.Text.RegularExpressions;

public static class MainFunctionality
{
    private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    public static async Task<List<VnrControl>> GenerateControlsFromViewAndModel(string viewPath, string modelPath)
    {
        var matchers = typeof(Matchers).GetMethods().ToList();

        List<Task<List<VnrControl>>> tasks = new();
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
        List<VnrControl> results = (await Task.WhenAll(tasks)).SelectMany(resultFromMatcher => resultFromMatcher).ToList();
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
        List<Task> tasks = new();
        foreach ((string viewPath, string modelPath) in viewAndModel)
        {
            List<VnrControl> controls = await GenerateControlsFromViewAndModel(viewPath, modelPath);
            PageInfo pageInfo = await MatcherHelper.GetPageInfo(viewPath);
            tasks.Add(WriteToFile(controls,pageInfo));
        }
        await Task.WhenAll(tasks);
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
        //TODO: Optimize lại 3 vòng for,  time complexity tương đối lớn
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
    
    public static async Task WriteToFile(List<VnrControl> data, PageInfo pageInfo)
    {
        string controller = pageInfo.controller;
        string action = pageInfo.action;
        string category = pageInfo.category;
        if (data.Count > 0)
        {
            VnrControl r = data[0];
            string mainDirPath = $"./Results/{category}/{controller}";
            string objectPath = $"./Results/{category}/{controller}/{controller}_{action}Object.cs";
            string pagePath = $"./Results/{category}/{controller}/{controller}_{action}Page.cs";
            string mainPath = $"./Results/{category}/{controller}/{controller}_{action}.cs";
            string txtPath = $"./Results/{category}/{controller}/{controller}_{action}.txt";
            Directory.CreateDirectory(mainDirPath);



            await MainFunctionality.WriteObjectFile(objectPath, data,pageInfo);
            await MainFunctionality.WritePageFile(pagePath, data,pageInfo);
            await MainFunctionality.WriteMainFile(mainPath, data,pageInfo);
            await MainFunctionality.WriteTxtFile(txtPath, data,pageInfo);
        }
    }
    public static async Task WriteTxtFile(string txtPath, List<VnrControl> results, PageInfo pageInfo) {
        string xPaths = string.Empty;
        foreach (VnrControl result in results)
        {
            xPaths += (@$"[LanguageKeyMapping(""{result.languageKey}"")]
        public WebElement {result.controlTypeShortname}_{pageInfo.actionShortName}_{result.property} => FindWebElement(By.XPath(""//{result.htmlElement}[@id='{result.name}']""));");
            xPaths += Environment.NewLine;
        }
        using (StreamWriter writer = new StreamWriter(txtPath))
        {
            // Write the contents to the file
            await writer.WriteAsync(xPaths);
        }
    }
    public static async Task WriteObjectFile(string objectPath, List<VnrControl> results, PageInfo pageInfo)
    {
        await semaphore.WaitAsync();
        try
        {
            VnrControl r = results[0];

            string objectFile = await File.ReadAllTextAsync("./Template/ObjectTemplate.cs");
            string properties = string.Empty;
            foreach (VnrControl result in results)
            {
                if(string.IsNullOrEmpty(result.property )) continue;
                properties += $"public string {result.property} {{get;set;}}";
                properties += Environment.NewLine;
            }
            objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.CATEGORY_GOES_HERE)!, pageInfo.category);
            //Tên màn hình (Tiếng việt)
            objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.PAGENAME_GOES_HERE)!, pageInfo.className);
            objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.CLASSNAME_GOES_HERE)!, pageInfo.className);
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

    public static async Task WritePageFile(string pagePath, List<VnrControl> results, PageInfo pageInfo)
    {
        await semaphore.WaitAsync();
        try
        {
            VnrControl r = results[0];
            string xPaths = string.Empty;
            foreach (VnrControl control in results)
            {
                xPaths += (@$"[LanguageKeyMapping(""{control.languageKey}"")]
        public WebElement {control.controlTypeShortname}_{pageInfo.actionShortName}_{control.property} => FindWebElement(By.XPath(""//{control.htmlElement}[@id='{control.name}']""));");
                xPaths += Environment.NewLine;
            }
            string pageFile = await File.ReadAllTextAsync("./Template/PageTemplate.cs");
            pageFile = pageFile.Replace(Enum.GetName(TemplatePlaceholder.CATEGORY_GOES_HERE)!,pageInfo.category);
            pageFile = pageFile.Replace(Enum.GetName(TemplatePlaceholder.PAGENAME_GOES_HERE)!, pageInfo.pageNameVN);
            pageFile = pageFile.Replace(Enum.GetName(TemplatePlaceholder.CLASSNAME_GOES_HERE)!, pageInfo.className);
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
    public static async Task WriteMainFile(string mainPath, List<VnrControl> results, PageInfo pageInfo)
    {
        await semaphore.WaitAsync();
        try
        {
            string mainFile = await File.ReadAllTextAsync("./Template/MainTemplate.cs");
            mainFile = mainFile.Replace(Enum.GetName(TemplatePlaceholder.CATEGORY_GOES_HERE)!,pageInfo.category);
            mainFile = mainFile.Replace(Enum.GetName(TemplatePlaceholder.PAGENAME_GOES_HERE)!, pageInfo.pageNameVN);
            mainFile = mainFile.Replace(Enum.GetName(TemplatePlaceholder.CLASSNAME_GOES_HERE)!, pageInfo.className);

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