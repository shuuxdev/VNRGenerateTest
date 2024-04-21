

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
[ApiController]
[Route("/api")]
public class APIController : ControllerBase
{
    private static readonly object objectLock = new();
    private static readonly string cacheKey = "views";

    public IMemoryCache cache { get; }

    public APIController(IMemoryCache cache)
    {
        this.cache = cache;
    }
    public async Task<IActionResult> Process()
    {
        return Ok();
    }
    public async Task<IActionResult> GetAllViews()
    {

        if (cache.TryGetValue(cacheKey, out List<string> cachedViews))
        {
            return Ok(cachedViews);
        }
        else
        {
            string views = await TFS.GetFileFromDevelopMain("/Main/Source/Presentation/HRM.Presentation.Main/Views");
            var cacheEntryOption = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15)).SetPriority(CacheItemPriority.NeverRemove);
            cache.Set(cacheKey, cachedViews, cacheEntryOption);


        }
        return Ok();

    }
}