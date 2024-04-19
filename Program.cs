using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;


if (Config.modelParsingMode == ModelParsingMode.DynamicCompilation)
    Helper.LoadAllNecessaryDll();

string[] views = [@"C:\Users\shuu\Workspace\DataTest\Hre_Profile\Index.cshtml"];
string modelPath = @"C:\Users\shuu\Workspace\DataTest\Hre_Profile\Hre_ProfileModel.cs";

var builder = WebHost.CreateDefaultBuilder();

var matchers = typeof(PatternMatchers).GetMethods().ToList();

async Task<List<ResultModel>> Call()
{
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


    foreach (var path in views)
    {
        List<(string controlType, int startIndex, string pattern)> controlTypesAndItsIndex = new();

        listMatchers.ForEach(matcher =>
        {
            tasks.Add(matcher.Method.Invoke(path, modelPath, matcher.Pattern));
        });
    }
    List<ResultModel> results = (await Task.WhenAll(tasks)).SelectMany(resultFromMatcher => resultFromMatcher).ToList();
    return results;
}
// Config.modelParsingMode = ModelParsingMode.DynamicCompilation;
// List<ResultModel> results1 = await Call();
// Config.modelParsingMode = ModelParsingMode.StringProcessing;
List<ResultModel> results = await Call();





ResultModel r = results[0];
string mainDirPath = $"./Results/{r.category}";
string objectPath = $"./Results/{r.category}/{r.followingName}Object.cs";
string pagePath = $"./Results/{r.category}/{r.followingName}Page.cs";
string mainPath = $"./Results/{r.category}/{r.followingName}.cs";
Directory.CreateDirectory(mainDirPath);

async Task WriteObjectFile()
{

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

    await File.WriteAllTextAsync(objectPath, objectFile);

}

async Task WritePageFile()
{
    string xPaths = string.Empty;
    foreach (ResultModel result in results)
    {
        xPaths += (@$"[LanguageKeyMapping(""{result.languageKey}"")]
        public WebElement {result.fieldType}_{result.pageType}_{result.fieldName} => FindWebElement(By.XPath(""//{result.htmlElement}[@id='{result.name}']""));");
        xPaths += Environment.NewLine;
    }
    string objectFile = await File.ReadAllTextAsync("./Template/PageTemplate.cs");
    objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.CATEGORY_GOES_HERE)!, r.category);
    objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.PAGENAME_GOES_HERE)!, r.className);
    objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.CLASSNAME_GOES_HERE)!, r.className);
    objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.XPATH_GOES_HERE)!, xPaths);

    await File.WriteAllTextAsync(pagePath, objectFile);

}
async Task WriteMainFile()
{
    string objectFile = await File.ReadAllTextAsync("./Template/MainTemplate.cs");
    objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.CATEGORY_GOES_HERE)!, r.category);
    objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.PAGENAME_GOES_HERE)!, r.className);
    objectFile = objectFile.Replace(Enum.GetName(TemplatePlaceholder.CLASSNAME_GOES_HERE)!, r.className);

    await File.WriteAllTextAsync(mainPath, objectFile);
}

await WriteObjectFile();
await WritePageFile();
await WriteMainFile();