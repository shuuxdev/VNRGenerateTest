using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;



Helper.LoadAllNecessaryDll();

string[] views = [@"D:\Code\Main\Source\Presentation\HRM.Presentation.Main\Views\Hre_Profile\Index.cshtml"];
string modelPath = @"D:\Code\Main\Source\Presentation\HRM.Presentation.Hr.Models\Hre_ProfileModel.cs";

var builder = WebHost.CreateDefaultBuilder();

var app = builder.Build();
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


foreach (var path in views)
{
    List<(string controlType, int startIndex, string pattern)> controlTypesAndItsIndex = new();

    listMatchers.ForEach(matcher =>
    {
        tasks.Add(matcher.Method.Invoke(path, modelPath, matcher.Pattern));
    });
}



List<ResultModel> results = (await Task.WhenAll(tasks)).SelectMany(resultFromMatcher => resultFromMatcher).ToList();



using (StreamWriter writer = new StreamWriter("~/../Result.txt"))
{
    foreach (ResultModel result in results)
    {
        writer.WriteLine(@$"[LanguageKeyMapping(""{result.languageKey}"")]
    public WebElement {result.fieldType}_{result.pageType}_{result.fieldName} => FindWebElement(By.XPath(""//{result.htmlElement}[@id='{result.name}']\""));");
    }
}
