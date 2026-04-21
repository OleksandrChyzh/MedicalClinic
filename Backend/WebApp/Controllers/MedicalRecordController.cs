using BLL.Interfaces;
using BLL.Models.MedicalRecord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MedicalRecordController(IMedicalRecordService medicalRecordService) : ControllerBase
{
    private int CurrentUserId => int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentUserRole => this.User.FindFirstValue(ClaimTypes.Role) ?? "User";

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var records = await medicalRecordService.GetAllRecordsAsync();
        return this.Ok(records);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var record = await medicalRecordService.GetRecordByIdAsync(id, this.CurrentUserId, this.CurrentUserRole);
        return this.Ok(record);
    }

    [HttpGet("card/{patientId}")]
    public async Task<IActionResult> GetMedicalCard(int patientId)
    {
        var card = await medicalRecordService.GetMedicalCardAsync(patientId, this.CurrentUserId, this.CurrentUserRole);
        return this.Ok(card);
    }

    [Authorize(Roles = "Admin,Doctor")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddMedicalRecordDTO dto)
    {
        var result = await medicalRecordService.CreateMedicalRecordAsync(this.CurrentUserId, this.CurrentUserRole, dto);
        return this.CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
