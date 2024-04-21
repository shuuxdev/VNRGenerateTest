using System.Reflection;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;


if (Config.modelParsingMode == ModelParsingMode.DynamicCompilation)
    CustomAssemblies.LoadAllNecessaryDll();

string viewPath = @"D:\Code\Main\Source\Presentation\HRM.Presentation.Main\Views";
string modelPath = @"D:\Code\Main\Source\Presentation";

string viewPathSingle = @"D:\Code\Main\Source\wt-1\Main\Source\Presentation\HRM.Presentation.Main\Views\Hre_Passport\Index.cshtml";
string modelPathSingle = @"D:\Code\Main\Source\wt-1\Main\Source\Presentation\HRM.Presentation.Hr.Models\Hre_PassportModel.cs";

Global.LANG_VN = await TFS.GetFileFromDevelopMain(@"/Main/Source/Presentation/HRM.Presentation.Main/Settings/LANG_VN.XML");
Global.MVCSitemap = await TFS.GetFileFromDevelopMain(@"/Main/Source/Presentation/HRM.Presentation.Main/Mvc.sitemap");
// List<VnrControl> res = await MainFunctionality.GenerateControlsFromViewAndModel( viewPathSingle, modelPathSingle);
// PageInfo pageInfo = await MatcherHelper.GetPageInfo(viewPathSingle);
// await MainFunctionality.WriteToFile(res, pageInfo);

string views = await TFS.GetFileFromDevelopMain("/Main/Source/Presentation/HRM.Presentation.Main/Views");


// var builder = WebApplication.CreateBuilder();
// builder.Services.AddControllersWithViews();
// var app = builder.Build();


// var customPath = Path.Combine(builder.Environment.ContentRootPath, "Assets");
// var fileProvider = new PhysicalFileProvider(customPath);

// app.UseRouting();
// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = fileProvider
// });
// // await MainFunctionality.GenerateTests(Category.Hre, viewPath, modelPath);
// app.MapControllers();
// app.UseEndpoints(endpoints =>
// {
//     endpoints.MapControllerRoute(
//         name: "default",
//         pattern: "{controller=Home}/{action=Index}/{id?}");
// });
// app.Run();

