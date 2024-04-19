using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

public static class PatternMatcherHelper
{
    
    public static Assembly? compiledModel = null;
    

   
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
            "TextBox" => "txt",
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
    public static string? GetLanguageKeyFromLangVN(string property)
    {
        if(Regex.Match(Global.LANG_VN, $@"\<Language Name=""{property}""").Success){
            return property;
        }
        return null;
    }
    public static string? GetLanguageKeyFromModelFile(string modelPath, string className, string property)
    {
        string content = File.ReadAllText(modelPath);
        string nameSpace = Regex.Match(content, @"namespace\s*([a-zA-Z.]*)", RegexOptions.Multiline).Groups[1].Value;
        string? languageKeyUsed = null;

        //BETA, hiện tại tính năng này chưa được hoàn thiện cho lắm
        //Tính năng này dùng để kiểm tra thuật toán xử lý chuỗi có chạy đúng hay không
        //Không đem dùng chính thức được vì không tìm ra được giải pháp update DLL mỗi khi source code thay đổi trên TFS
        if (Config.modelParsingMode == ModelParsingMode.DynamicCompilation)
        {
            if (compiledModel == null)
                compiledModel = CustomAssemblies.LoadModelIntoAssembly(content, nameSpace, className);
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
        //Xử lý chuỗi like a f*cking moron
        
        else if (Config.modelParsingMode == ModelParsingMode.StringProcessing)
        {
            int classStartIndex = Regex.Match(content, @$"class\s+{className}", RegexOptions.Multiline).Index;
            int classEndIndex = Regex.Match(content.Substring(classStartIndex), @"class\s+\w", RegexOptions.Multiline).Index;

            //Trường hợp class đang tìm nằm ở cuối file
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

            if (Config.loggingMode == LoggingMode.All || Config.loggingMode == LoggingMode.Model)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("========================================================");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Property: {0}", property);
                Console.WriteLine(propertyScope);
            }


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