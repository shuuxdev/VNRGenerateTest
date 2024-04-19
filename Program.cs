using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;


if (Config.modelParsingMode == ModelParsingMode.DynamicCompilation)
    Helper.LoadAllNecessaryDll();

string viewPath = @"C:\Users\shuu\Workspace\DataTest\Views";
string modelPath = @"C:\Users\shuu\Workspace\DataTest\HRM.Presentation.Hr.Models";

var builder = WebHost.CreateDefaultBuilder();



await MainFunctionality.GenerateTests(Category.Hre, viewPath, modelPath);


// Config.modelParsingMode = ModelParsingMode.DynamicCompilation;
// List<ResultModel> results1 = await Call();
// Config.modelParsingMode = ModelParsingMode.StringProcessing;
// List<ResultModel> data = (await MainFunctionality
//                                 .GenerateDataForTestingFromViewAndModel(viewPath, modelPath))
//                                 .FilterBadElement().ToList();
// ResultModel r = data[0];

// string mainDirPath = $"./Results/{r.category}";
// string objectPath = $"./Results/{r.category}/{r.followingName}Object.cs";
// string pagePath = $"./Results/{r.category}/{r.followingName}Page.cs";
// string mainPath = $"./Results/{r.category}/{r.followingName}.cs";

// Directory.CreateDirectory(mainDirPath);



// await MainFunctionality.WriteObjectFile(objectPath, data);
// await MainFunctionality.WritePageFile(pagePath, data);
// await MainFunctionality.WriteMainFile(mainPath, data);