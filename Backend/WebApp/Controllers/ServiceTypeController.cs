using BLL.Interfaces;
using BLL.Models.ServiceType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceTypeController(IServiceTypeService serviceTypeService) : ControllerBase
{
    // Гість/Пацієнт може бачити типи послуг
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await serviceTypeService.GetAllServiceTypesAsync();
        return this.Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await serviceTypeService.GetServiceTypeByIdAsync(id);
        return this.Ok(result);
    }

    // Тільки адмін може змінювати структуру послуг
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddServiceTypeDTO dto)
    {
        var result = await serviceTypeService.CreateServiceTypeAsync(dto);
        return this.CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceTypeDTO dto)
    {
        await serviceTypeService.UpdateServiceTypeAsync(id, dto);
        return this.NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await serviceTypeService.DeleteServiceTypeAsync(id);
        return this.NoContent();
    }
}
