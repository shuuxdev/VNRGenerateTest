using Microsoft.Extensions.Caching.Memory;

public class TfsApiCallerService : BackgroundService
{
    private readonly ILogger<TfsApiCallerService> _logger;
    private readonly IMemoryCache cache;

    public TfsApiCallerService(ILogger<TfsApiCallerService> logger, IMemoryCache cache)
    {
        _logger = logger;
        this.cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Call the TFS API here
                List<string> paths = await TFS.GetItemBatchFromDevelopMain(Global.TFSViewsPath);
                if (paths == null)
                    throw new Exception("Không lấy được danh sách views từ TFS");

                bool viewsIsCached = cache.TryGetValue(Global.viewsFolderCacheKey, out List<string> cachedViews);
                if (!viewsIsCached || (viewsIsCached && cachedViews.Count < paths.Count))
                {
                    Dictionary<string, string> viewsAndModels = await TFS.ToViewsAndModelsDictionary();
                    var cacheEntryOption = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove);
                    cache.Set(Global.viewsFolderCacheKey, cachedViews, cacheEntryOption);
                    cache.Set(Global.viewsAndModelsDictionaryCacheKey, viewsAndModels);
                }


                // Wait for 5 minutes before the next call
                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calling TFS API");
            }
        }
    }
}