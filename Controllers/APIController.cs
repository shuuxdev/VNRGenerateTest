

using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("/api")]
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