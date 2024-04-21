using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Core.WebApi.Team;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi.Legacy;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using GitItem = Microsoft.TeamFoundation.SourceControl.WebApi.GitItem;
public static class TFS
{
    public static async Task<string> GetFileFromDevelopMain(string path)
    {

        string projectID = "41abf2e0-a388-43de-9f5e-666da177bd5d";
        string repositoryID = "15002d83-9470-4b0e-9a8d-7a655d9af004";

        string branch = "develop-main";
        // Construct the URL for the TFS REST API
        string apiUrl = $@"http://172.21.35.3:8080/tfs/HRMCollection/{projectID}/_apis/git/repositories/{repositoryID}/Items?path={path}&recursionLevel=0&includeContentMetadata=true&latestProcessedChange=false&download=false&versionDescriptor%5BversionType%5D=branch&versionDescriptor%5Bversion%5D={branch}&includeContent=true";

        // Make an HTTP GET request to fetch the content of the file
        using (var httpClient = new HttpClient(new HttpClientHandler { UseDefaultCredentials = true }))
        {
            var response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;

            }
            else
            {
                throw new Exception($"Lỗi không get được file từ TFS. Status code: {response.StatusCode}");
            }
        }
    }
    public static async Task<StreamReader> GetFileFromDevelopMainUsingStreamReader(string path)
    {

        string projectID = "41abf2e0-a388-43de-9f5e-666da177bd5d";
        string repositoryID = "15002d83-9470-4b0e-9a8d-7a655d9af004";

        string branch = "develop-main";
        // Construct the URL for the TFS REST API
        string apiUrl = $@"http://172.21.35.3:8080/tfs/HRMCollection/{projectID}/_apis/git/repositories/{repositoryID}/Items?path={path}&recursionLevel=0&includeContentMetadata=true&latestProcessedChange=false&download=false&versionDescriptor%5BversionType%5D=branch&versionDescriptor%5Bversion%5D={branch}&includeContent=true";

        // Make an HTTP GET request to fetch the content of the file
        using (var httpClient = new HttpClient(new HttpClientHandler { UseDefaultCredentials = true }))
        {
            var response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                StreamReader streamReader = new StreamReader(stream);
                return streamReader;

            }
            else
            {
                throw new Exception($"Lỗi không get được file từ TFS. Status code: {response.StatusCode}");
            }
        }
    }
    public static async Task<List<string>> GetItemBatchFromDevelopMain(string path)
    {

        // Construct the URL for the TFS REST API
        const String domain = "http://172.21.35.3:8080/tfs/HRMCollection";
        const String c_projectName = "HRM9";
        const String c_repoName = "HRM9";

        Uri orgUrl = new Uri(domain);
        string username = "nghia.huynhhieu";
        string pwd = "4zkl.cmvn2k20.sv";

        NetworkCredential networkCredential = new NetworkCredential(username, pwd);
        //BasicAuthCredential basicAuthCredential = new BasicAuthCredential(networkCredential);
        WindowsCredential winCred = new WindowsCredential(networkCredential);
        VssCredentials vssCred = new VssClientCredentials(winCred);
        // Connect to Azure DevOps Services
        VssConnection connection = new VssConnection(orgUrl, vssCred);

        // Get a GitHttpClient to talk to the Git endpoints
        using (GitHttpClient gitClient = connection.GetClient<GitHttpClient>())
        {
            // Get data about a specific repository
            var repo = gitClient.GetRepositoryAsync(c_projectName, c_repoName).Result;
            List<GitItem> items = await gitClient.GetItemsAsync(
                repo.Id,
                recursionLevel: Microsoft.TeamFoundation.SourceControl.WebApi.VersionControlRecursionType.OneLevel,
                scopePath: path,
                versionDescriptor: new GitVersionDescriptor
                {
                    VersionType = GitVersionType.Branch,
                    Version = "develop-main",

                });

            items.RemoveAt(0);
            return items.Select(item => item.Path).ToList();
        }
    }
    public static async Task<Dictionary<string, string>> ToViewsAndModelsDictionary()
    {
        List<string> viewDirectories = await TFS.GetItemBatchFromDevelopMain(Global.TFSViewsPath);

        List<string> modelPaths = await TFS.GetItemBatchFromDevelopMain(Global.TFSPresentationPath);

        List<(string viewPath, string model)> modelsUsed = new();
        Dictionary<string, string> view = new();
        //Loop qua toàn bộ màn hình của phân hệ
        foreach (string viewDirectoryPath in viewDirectories)
        {
            //Với mỗi màn hình sẽ có các file Cshtml
            List<string> viewPaths = await TFS.GetItemBatchFromDevelopMain(viewDirectoryPath);
            foreach (string viewPath in viewPaths)
            {
                StreamReader reader = await TFS.GetFileFromDevelopMainUsingStreamReader(viewPath);
                await foreach (string line in ReadLinesAsync(reader))
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
    private static async IAsyncEnumerable<string> ReadLinesAsync(StreamReader reader)
    {
        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            yield return line;
        }
    }
}