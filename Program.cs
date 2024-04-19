using System.Reflection;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;


if (Config.modelParsingMode == ModelParsingMode.DynamicCompilation)
    CustomAssemblies.LoadAllNecessaryDll();

string viewPath = @"D:\Code\Main\Source\Presentation\HRM.Presentation.Main\Views";
string modelPath = @"D:\Code\Main\Source\Presentation";

string viewPathSingle = @"D:\Code\Main\Source\wt-1\Main\Source\Presentation\HRM.Presentation.Main\Views\Hre_Passport\Index.cshtml";
string modelPathSingle = @"D:\Code\Main\Source\wt-1\Main\Source\Presentation\HRM.Presentation.Hr.Models\Hre_PassportModel.cs";

Global.LANG_VN = await TFS.GetFileFromDevelopMain(@"/Main/Source/Presentation/HRM.Presentation.Main/Settings/LANG_VN.XML");


//var builder = WebHost.CreateDefaultBuilder();



// await MainFunctionality.GenerateTests(Category.Hre, viewPath, modelPath);
List<ResultModel> res = await MainFunctionality.GenerateDataForTestingFromViewAndModel( viewPathSingle, modelPathSingle);

await MainFunctionality.WriteToFile(res);


    


// Config.modelParsingMode = ModelParsingMode.DynamicCompilation;
// List<ResultModel> results1 = await Call();
// Config.modelParsingMode = ModelParsingMode.StringProcessing;
// List<ResultModel> data = (await MainFunctionality
//                                 .GenerateDataForTestingFromViewAndModel(viewPath, modelPath))
//                                 .FilterBadElement().ToList();