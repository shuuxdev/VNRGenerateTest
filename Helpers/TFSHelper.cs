public static class TFS {
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
}