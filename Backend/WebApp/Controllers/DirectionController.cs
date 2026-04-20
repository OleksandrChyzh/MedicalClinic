using BLL.Interfaces;
using BLL.Models.Direction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DirectionController(IDirectionService directionService) : ControllerBase
{
    // ==========================================
    // ПУБЛІЧНА ЧАСТИНА (Доступно всім)
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> GetAllDirections()
    {
        var directions = await directionService.GetAllDirectionsAsync();
        return this.Ok(directions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDirectionById(int id)
    {
        var result = await directionService.GetDirectionByIdAsync(id);
        return this.Ok(result);
    }

    // ==========================================
    // АДМІН ЧАСТИНА (Тільки для персоналу)
    // ==========================================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateDirection([FromBody] AddDirectionDTO dto)
    {
        var result = await directionService.CreateDirectionAsync(dto);

        // Повертаємо 201 Created і посилання на новостворений ресурс
        return this.CreatedAtAction(nameof(GetDirectionById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDirection(int id, [FromBody] AddDirectionDTO dto)
    {
        await directionService.UpdateDirectionAsync(id, dto);
        return this.NoContent(); // 204 No Content - стандарт для успішного PUT
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDirection(int id)
    {
        await directionService.DeleteDirectionAsync(id);
        return this.NoContent(); // 204 No Content - стандарт для успішного DELETE
    }
}
