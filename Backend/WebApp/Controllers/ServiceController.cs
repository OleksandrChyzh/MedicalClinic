using BLL.Interfaces;
using BLL.Models.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceController(IServiceManagementService service) : ControllerBase
{
    // ==========================================
    // ПУБЛІЧНА ЧАСТИНА (Доступно всім)
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> GetAllServices([FromQuery] int? directionId, [FromQuery] int? typeId)
    {
        // Якщо параметри передані - фільтруємо, якщо ні - віддаємо всі
        var services = await service.GetServicesAsync(directionId, typeId);
        return this.Ok(services);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetServiceById(int id)
    {
        var result = await service.GetServiceByIdAsync(id);
        return this.Ok(result);
    }

    // ==========================================
    // АДМІН ЧАСТИНА (Тільки для персоналу)
    // ==========================================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateService([FromBody] AddServiceDTO dto)
    {
        var result = await service.CreateServiceAsync(dto);
        return this.CreatedAtAction(nameof(GetServiceById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] UpdateServiceDTO dto)
    {
        await service.UpdateServiceAsync(id, dto);
        return this.NoContent();
    }
}
