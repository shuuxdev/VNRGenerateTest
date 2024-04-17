using System.Text.RegularExpressions;
using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.ComponentModel;
using Microsoft.CodeAnalysis.Host;
using System.Text;
public static class PatternMatchers
{


    /// <summary>
    /// Lấy toàn bộ control có dạng @Html.VnrControlFor(_ => _.Property, controlInstance);
    /// </summary>
    /// <param name="cshtmlPath"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    [Pattern(@"Html\.Vnr(?!Label|Window|Button)(\w*\s*)\(\s*")]
    public static async Task<List<ResultModel>> GetAllStandardControlsMatcherAsync(string cshtmlPath, string modelPath, string pattern)
    {
        string content = await File.ReadAllTextAsync(cshtmlPath);
        Dictionary<(int startIndex, int endIndex), string> lines = DictionaryHelper.ToLines(content);
        MatchCollection matchCollection = Regex.Matches(content, pattern);
        List<ResultModel> result = new();
        foreach (Match match in matchCollection)
        {

            string fullControlName = match.Groups[1].Value;
            string viewName = cshtmlPath.Substring(cshtmlPath.LastIndexOf("\\") + 1).Replace(".cshtml", string.Empty);
            string[] classNameWithDotsSeperated = Regex.Match(content, @"^\@model\s+([a-zA-Z_.]*)", RegexOptions.Multiline).Groups[1].Value.Split(".");
            string className = classNameWithDotsSeperated[classNameWithDotsSeperated.Length - 1];

            //Fields cần trả về
            string controlType = Helper.GetControlType(fullControlName.Replace("For", string.Empty)); //fieldType
            string pageType = viewName == "Index" ? "TK" : "TM"; //pageType
            string property = null; //fieldName
            string languageKey = null; //languageKey
            string controlId = null;
            string htmlElement = Helper.GetHtmlElement(fullControlName.Replace("For", string.Empty));
            //Trường hợp có dùng VnrControlFor, thì bên trong sẽ có truyền 1 delegate, delegate đó sẽ trả về property tương ứng trong model
            //Từ đó ta có thể tìm thấy key dịch bằng cách tìm property đó trong model

            string scope = DictionaryHelper.GetCurrentVnrControlScope(match.Index, content.Split(Environment.NewLine), lines);
            if (fullControlName.EndsWith("For"))
            {
                string fromThisControlNameLetsFindPropertyNamePattern = @"Vnr(?!Label|Window)(\w*\s*)\(\s*\w*\s*=>\s\w*\.(\w*)";

                var regex = Regex.Match(content.Substring(match.Index), fromThisControlNameLetsFindPropertyNamePattern);
                if (!regex.Success)
                {
                    Console.WriteLine("Strange case, control: {0}, position: {1}, file: {2}", fullControlName, match.Index, cshtmlPath);
                    continue;
                }
                property = regex.Groups[2].Value;
                //Thử tìm key dịch dựa trên property được sử dụng trong control
                //Giả sử có control @Html.VnrComboBoxFor(model => model.ProfileID) 
                // => ProfileID chính là property cần tìm trong file model
                languageKey = Helper.GetLanguageKeyFromModelFile(modelPath, className, property);


            }
            if (languageKey == null)
            {
                Match languageKeyMatch = null;
                if ((languageKeyMatch = Regex.Match(scope, @"\@Html.VnrLabelFor\(\s*\w*\s*=>\s\w*\.(\w*)")).Success)
                {
                    if (string.IsNullOrEmpty(property))
                    {
                        property = languageKeyMatch.Groups[1].Value;
                    }
                    languageKey = Helper.GetLanguageKeyFromModelFile(modelPath, className, property);
                }
                else if ((languageKeyMatch = Regex.Match(scope, @"\@Html\.(?:VnrLabel|Raw)\(ConstantDisplay\.(\w*)")).Success)
                {
                    languageKey = languageKeyMatch.Groups[1].Value;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            //Trường hợp tìm không thấy, ta sẽ thử tìm VnrLabel trong FieldTitle
            Console.WriteLine("Language key: {0} property {1} control {2}", languageKey, property, controlType);
            int newKeywordIndex = content.Substring(0, match.Index).LastIndexOf("new");
            if (newKeywordIndex == -1)
            {
                Console.WriteLine("Cannot find the new keyword index for control: {0} at index: {1}", fullControlName, match.Index);
                continue;
            }

            scope = content.Substring(newKeywordIndex, match.Index - newKeywordIndex + 1);

            //Tìm Name và IsShowCheckBox
            string patternForLookingUpBuilderInfoProperty = @"\s*new\s*[\w]*(?:.*?Name\s*\=\s*\""(\w*)\"")?(?:.*?IsShowCheckBox\s*\=\s*(\w*))?";
            string patternForLookingUpBuilderInfoPropertyReverse = @"\s*new\s*[\w]*(?:.*?Name\s*\=\s*\""(\w*)\"")?(?:.*?IsShowCheckBox\s*\=\s*(\w*))?";
            Match builderInfoPropertyMatched = Regex.Match(
                scope,
                patternForLookingUpBuilderInfoProperty,
                RegexOptions.Singleline
            );
            Match builderInfoPropertyMatchesReverseMatched = Regex.Match(
                scope,
                patternForLookingUpBuilderInfoPropertyReverse,
                RegexOptions.Singleline
            );
            bool validBooleanValue = false;
            if (builderInfoPropertyMatched.Success)
            {

                controlId = controlId ?? builderInfoPropertyMatched.Groups[1].Value;
                if (validBooleanValue == false)
                {
                    Boolean.TryParse(builderInfoPropertyMatched.Groups[2].Value, out validBooleanValue);
                    controlType = validBooleanValue == true ? "orgNoCkb" : controlType;
                }
            }
            if (builderInfoPropertyMatchesReverseMatched.Success)
            {
                controlId = controlId ?? builderInfoPropertyMatched.Groups[2].Value;
                if (validBooleanValue == false)
                {
                    Boolean.TryParse(builderInfoPropertyMatched.Groups[1].Value, out validBooleanValue);
                    controlType = validBooleanValue == true ? "orgNoCkb" : controlType;
                }

            }

            result.Add(new ResultModel()
            {
                fieldType = controlType,
                pageType = pageType,
                languageKey = languageKey,
                fieldName = property,
                name = string.IsNullOrEmpty(controlId) ? property : controlId,
                htmlElement = htmlElement
            });
        }
        return result;
    }

    [Pattern(@"(VnrUpload)\(\w*\)")]
    public static async Task<List<ResultModel>> GetVnrUploadControl(string cshtmlPath, string modelPath, string pattern)
    {
        string content = await File.ReadAllTextAsync(cshtmlPath);
        Dictionary<(int startIndex, int endIndex), string> lines = DictionaryHelper.ToLines(content);
        MatchCollection matchCollection = Regex.Matches(content, pattern, RegexOptions.Multiline);
        List<ResultModel> result = new();
        foreach (Match match in matchCollection)
        {
            string fullControlName = match.Groups[1].Value;
            string viewName = cshtmlPath.Substring(cshtmlPath.LastIndexOf("\\") + 1).Replace(".cshtml", string.Empty);
            string[] classNameWithDotsSeperated = Regex.Match(content, @"^\@model\s+([a-zA-Z_.]*)", RegexOptions.Multiline).Groups[1].Value.Split(".");
            string className = classNameWithDotsSeperated[classNameWithDotsSeperated.Length - 1];

            //Fields cần trả về
            string controlType = Helper.GetControlType(fullControlName); //fieldType
            string pageType = viewName == "Index" ? "TK" : "TM"; //pageType
            string property = null; //fieldName
            string languageKey = null; //languageKey
            string controlId = null;
            string htmlElement = Helper.GetHtmlElement(fullControlName);
            //Trường hợp có dùng VnrControlFor, thì bên trong sẽ có truyền 1 delegate, delegate đó sẽ trả về property tương ứng trong model
            //Từ đó ta có thể tìm thấy key dịch bằng cách tìm property đó trong model

            string scope = DictionaryHelper.GetCurrentVnrControlScope(match.Index, content.Split(Environment.NewLine), lines, maxLevel: 2);
            //Trường hợp có dùng VnrControlFor, thì bên trong sẽ có truyền 1 delegate, delegate đó sẽ trả về property tương ứng trong model
            //Từ đó ta có thể tìm thấy key dịch bằng cách tìm property đó trong model

            if (languageKey == null)
            {
                Match languageKeyMatch = null;
                if ((languageKeyMatch = Regex.Match(scope, @"\@Html.VnrLabelFor\(\s*\w*\s*=>\s\w*\.(\w*)")).Success)
                {
                    if (string.IsNullOrEmpty(property))
                    {
                        property = languageKeyMatch.Groups[1].Value;
                    }
                    languageKey = Helper.GetLanguageKeyFromModelFile(modelPath, className, property);
                }
                else if ((languageKeyMatch = Regex.Match(scope, @"\@Html\.(?:VnrLabel|Raw)\(ConstantDisplay\.(\w*)")).Success)
                {
                    languageKey = languageKeyMatch.Groups[1].Value;
                }
            }

            controlId = Regex.Match(scope, @"\w*\.Id\s*=\s*\""(\w*)\""").Groups[1].Value;





            result.Add(new ResultModel()
            {
                fieldType = controlType,
                pageType = pageType,
                languageKey = languageKey,
                fieldName = property,
                name = string.IsNullOrEmpty(controlId) ? property : controlId,
                htmlElement = htmlElement
            });
        }
        return result;
    }
    [Pattern(@"(TreeViewDropdDownBuilderInfo)\(\w*\)")]
    public static async Task<List<ResultModel>> GetTreeViewDropdown(string cshtmlPath, string modelPath, string pattern)
    {
        string content = await File.ReadAllTextAsync(cshtmlPath);
        Dictionary<(int startIndex, int endIndex), string> lines = DictionaryHelper.ToLines(content);
        MatchCollection matchCollection = Regex.Matches(content, pattern, RegexOptions.Multiline);
        List<ResultModel> result = new();
        foreach (Match match in matchCollection)
        {
            string fullControlName = match.Groups[1].Value;
            string viewName = cshtmlPath.Substring(cshtmlPath.LastIndexOf("\\") + 1).Replace(".cshtml", string.Empty);
            string[] classNameWithDotsSeperated = Regex.Match(content, @"^\@model\s+([a-zA-Z_.]*)", RegexOptions.Multiline).Groups[1].Value.Split(".");
            string className = classNameWithDotsSeperated[classNameWithDotsSeperated.Length - 1];

            //Fields cần trả về
            string controlType = Helper.GetControlType(fullControlName); //fieldType
            string pageType = viewName == "Index" ? "TK" : "TM"; //pageType
            string property = null; //fieldName
            string languageKey = null; //languageKey
            string controlId = null;
            string htmlElement = Helper.GetHtmlElement(fullControlName);
            //Trường hợp có dùng VnrControlFor, thì bên trong sẽ có truyền 1 delegate, delegate đó sẽ trả về property tương ứng trong model
            //Từ đó ta có thể tìm thấy key dịch bằng cách tìm property đó trong model

            string scope = DictionaryHelper.GetCurrentVnrControlScope(match.Index, content.Split(Environment.NewLine), lines, maxLevel: 2);
            //Trường hợp có dùng VnrControlFor, thì bên trong sẽ có truyền 1 delegate, delegate đó sẽ trả về property tương ứng trong model
            //Từ đó ta có thể tìm thấy key dịch bằng cách tìm property đó trong model


            if (languageKey == null)
            {
                Match languageKeyMatch = null;
                if ((languageKeyMatch = Regex.Match(scope, @"\@Html.VnrLabelFor\(\s*\w*\s*=>\s\w*\.(\w*)")).Success)
                {
                    if (string.IsNullOrEmpty(property))
                    {
                        property = languageKeyMatch.Groups[1].Value;
                    }
                    languageKey = Helper.GetLanguageKeyFromModelFile(modelPath, className, property);
                }
                else if ((languageKeyMatch = Regex.Match(scope, @"\@Html\.(?:VnrLabel|Raw)\(ConstantDisplay\.(\w*)")).Success)
                {
                    languageKey = languageKeyMatch.Groups[1].Value;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            //Trường hợp tìm không thấy, ta sẽ thử tìm VnrLabel trong FieldTitle
            Console.WriteLine("Language key: {0} property {1} control {2}", languageKey, property, controlType);
            int newKeywordIndex = content.Substring(0, match.Index).LastIndexOf("new");
            if (newKeywordIndex == -1)
            {
                Console.WriteLine("Cannot find the new keyword index for control: {0} at index: {1}", fullControlName, match.Index);
                continue;
            }

            scope = content.Substring(newKeywordIndex, match.Index - newKeywordIndex + 1);

            //Tìm Name và IsShowCheckBox
            string patternForLookingUpBuilderInfoProperty = @"\s*new\s*[\w]*(?:.*?Name\s*\=\s*\""(\w*)\"")?(?:.*?IsShowCheckBox\s*\=\s*(\w*))?";
            string patternForLookingUpBuilderInfoPropertyReverse = @"\s*new\s*[\w]*(?:.*?Name\s*\=\s*\""(\w*)\"")?(?:.*?IsShowCheckBox\s*\=\s*(\w*))?";
            Match builderInfoPropertyMatched = Regex.Match(
                scope,
                patternForLookingUpBuilderInfoProperty,
                RegexOptions.Singleline
            );
            Match builderInfoPropertyMatchesReverseMatched = Regex.Match(
                scope,
                patternForLookingUpBuilderInfoPropertyReverse,
                RegexOptions.Singleline
            );
            bool validBooleanValue = false;
            if (builderInfoPropertyMatched.Success)
            {

                controlId = controlId ?? builderInfoPropertyMatched.Groups[1].Value;
                if (validBooleanValue == false)
                {
                    Boolean.TryParse(builderInfoPropertyMatched.Groups[2].Value, out validBooleanValue);
                    controlType = validBooleanValue == true ? "orgNoCkb" : controlType;
                }
            }
            if (builderInfoPropertyMatchesReverseMatched.Success)
            {
                controlId = controlId ?? builderInfoPropertyMatched.Groups[2].Value;
                if (validBooleanValue == false)
                {
                    Boolean.TryParse(builderInfoPropertyMatched.Groups[1].Value, out validBooleanValue);
                    controlType = validBooleanValue == true ? "orgNoCkb" : controlType;
                }

            }


            result.Add(new ResultModel()
            {
                fieldType = controlType,
                pageType = pageType,
                languageKey = languageKey,
                fieldName = property,
                name = string.IsNullOrEmpty(controlId) ? property : controlId,
                htmlElement = htmlElement
            });
        }
        return result;
    }
}