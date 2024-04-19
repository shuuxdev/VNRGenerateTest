using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

public static class Helper
{
    public static Assembly? compiledModel = null;
    public static void LoadAllNecessaryDll()
    {
        string rootDirectory = System.IO.Directory.GetCurrentDirectory();
        string[] DLLs = Directory.GetFiles($"{rootDirectory}\\Dll");
        foreach (string dll in DLLs)
        {
            var assembly = Assembly.LoadFrom(dll);
            if (assembly == null)
            {
                throw new Exception("Cannot load assemby: " + dll);
            }
            LoadedAssemblies.assemblies.Add(assembly);
        }
    }

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
        var result = await Helper.FindAllKindOfCodingStyles(paths, @"^using\s*([a-zA-Z.]*)", RegexOptions.Multiline);
        foreach (IGrouping<string, string> item in result)
        {
            Console.WriteLine("{0} - {1}", item.Key, item.Count());
        }
        Console.WriteLine("Total files: {0}", paths.Length);
    }
    public static string GetControlType(string originalControlType)
    {
        string result = originalControlType switch
        {
            "ComboBox" => "cbx",
            "MultiSelect" => "sml",
            "TreeViewDropdDown" => "org",
            "DropDownList" => "ddl",
            "DateTimePicker" => "dtp",
            "DatePicker" => "dtp",
            "CheckBox" => "ckb",
            "NumericTextBox" => "num",
            _ => originalControlType
        };
        return result;
    }
    public static string GetHtmlElement(string originalControlType)
    {
        string result = originalControlType switch
        {
            "ComboBox" => "input",
            "MultiSelect" => "span",
            "DropDownList" => "input",
            "NumericTextBox" => "input",
            "TextBox" => "input",
            "TextArea" => "textarea",
            "TreeViewDropDownBuilderInfo" => "input",
            "DateTimePicker" => "input",
            "DatePicker" => "input",
            _ => "input"
        };
        return result;
    }
    /// <summary>
    /// Hàm này dùng để biên dịch động 1 file bất kì, từ đó ta có thể truy cập trực tiếp đến DisplayName attribute của mỗi property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="classContent"></param>
    /// <param name="nameSpace"></param>
    /// <param name="className"></param>
    /// <returns></returns>
    public static Assembly LoadModelIntoAssembly(string classContent, string nameSpace, string className)
    {
        // define source code, then parse it (to the type used for compilation)

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(classContent);
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);


        // define other necessary objects for compilation
        string assemblyName = Path.GetRandomFileName();
        List<MetadataReference> references =
        [
                MetadataReference.CreateFromFile(LoadedAssemblies.Get("HRM.Presentation.Service").Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DisplayNameAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.RangeAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Xml.Serialization.XmlSerializer).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                .. LoadedAssemblies.assemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)),

        ];


        // analyse and generate IL code from syntax tree
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using (var ms = new MemoryStream())
        {
            // write IL code into memory
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                // handle exceptions
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (Diagnostic diagnostic in failures)
                {
                    Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                }
            }
            // load this 'virtual' DLL so that we can use
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());
            return assembly;
        }
    }
    public static string? GetLanguageKeyFromModelFile(string modelPath, string className, string property)
    {
        string content = File.ReadAllText(modelPath);
        string nameSpace = Regex.Match(content, @"namespace\s*([a-zA-Z.]*)", RegexOptions.Multiline).Groups[1].Value;
        string? languageKeyUsed = null;

        //BETA, hiện tại tính năng này chưa được hoàn thiện cho lắm
        if (Config.modelParsingMode == ModelParsingMode.DynamicCompilation)
        {
            if (compiledModel == null)
                compiledModel = LoadModelIntoAssembly(content, nameSpace, className);
            if (compiledModel == null)
            {
                throw new Exception("Không load được file model");
            }
            Type type = compiledModel.GetType($"{nameSpace}.{className}")!;
            PropertyInfo prop = type.GetProperty(property)!;
            var displayAttribute = prop.GetCustomAttribute<DisplayNameAttribute>();
            if (displayAttribute == null) return null;
            return displayAttribute.DisplayName;
        }
        else if (Config.modelParsingMode == ModelParsingMode.StringProcessing)
        {
            int classStartIndex = Regex.Match(content, @$"class\s+{className}", RegexOptions.Multiline).Index;
            int classEndIndex = Regex.Match(content.Substring(classStartIndex), @"class\s+\w", RegexOptions.Multiline).Index;

            //Trường hợp class này nằm ở cuối file
            //Khúc này có thể quăng exception
            if (classEndIndex == 0)
            {
                classEndIndex = content.Length - 1;
            }
            string classScope = content.Substring(classStartIndex, classEndIndex - classStartIndex + 1);
            var linesDictionary = DictionaryHelper.ToLines(classScope);

            int classStartLineIndex = DictionaryHelper.GetLineIndex(0, linesDictionary);
            int classEndLineIndex = DictionaryHelper.GetLineIndex(classEndIndex - classStartIndex, linesDictionary);

            //Console.Write(classScope);
            string[] classContentSplittedIntoLines = classScope.Split(Environment.NewLine);
            //Search bắt đầu từ vị trí của tên class cho đến cuối file
            //Không check null property bởi vì phải đảm bảo là property chắc chắn sẽ có trong file, có trong class đang tìm
            int matchedPropertyIndex = Regex.Match(classScope, @$"\s+{property}\s*\{{", RegexOptions.Multiline).Index;
            //Lấy index của dòng hiện tại
            int matchedPropertyLineIndex = DictionaryHelper.GetLineIndex(matchedPropertyIndex, linesDictionary);

            int previousPropertyIndex = 0;
            int previousPropertyLineIndex = 0;
            int nextPropertyIndex = 0;
            int nextPropertyLineIndex = 0;

            int currentLineIndex = matchedPropertyLineIndex;
            while (previousPropertyIndex == 0 && currentLineIndex > classStartLineIndex)
            {
                --currentLineIndex;
                previousPropertyIndex = Regex.Match(classContentSplittedIntoLines[currentLineIndex], "}", RegexOptions.Multiline).Index;
            }
            previousPropertyLineIndex = currentLineIndex;
            currentLineIndex = matchedPropertyLineIndex;
            while (nextPropertyIndex == 0 && currentLineIndex < classEndLineIndex)
            {
                ++currentLineIndex;
                nextPropertyIndex = Regex.Match(classContentSplittedIntoLines[currentLineIndex], "{", RegexOptions.Multiline).Index;
            }
            nextPropertyLineIndex = currentLineIndex;
            string propertyScope = string.Join(Environment.NewLine, classContentSplittedIntoLines.Skip(previousPropertyLineIndex).Take(nextPropertyLineIndex - previousPropertyLineIndex + 1));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("========================================================");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Property: {0}", property);
            Console.WriteLine(propertyScope);

            for (int i = previousPropertyLineIndex; i <= matchedPropertyLineIndex; ++i)
            {
                Match matchedDisplayAttribute = Regex.Match(classContentSplittedIntoLines[i], @"\[DisplayName\((.*?)\)");
                if (matchedDisplayAttribute.Success)
                {
                    languageKeyUsed = matchedDisplayAttribute.Groups[1].Value;
                    if (languageKeyUsed.StartsWith("ConstantDisplay"))
                    {
                        return languageKeyUsed.Split(".")[1];
                    }
                    return languageKeyUsed.Trim('\"');
                }
            }
        }
        //Trường hợp không tìm thấy thì return null
        return null;
    }

}