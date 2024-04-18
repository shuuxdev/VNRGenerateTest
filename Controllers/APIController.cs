

using Microsoft.AspNetCore.Mvc;
[ApiController]
public class APIController : ControllerBase
{

    public APIController()
    {

    }
    public async Task<IActionResult> Process()
    {
        return Ok();
    }
}