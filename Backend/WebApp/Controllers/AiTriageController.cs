using BLL.Interfaces;
using BLL.Models.AiTriage;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AiTriageController(IAiTriageService aiTriageService) : ControllerBase
{
    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AiTriageRequestDto dto)
    {
        var result = await aiTriageService.AnalyzeAsync(dto);
        return this.Ok(result);
    }
}
