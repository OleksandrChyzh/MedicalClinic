using System.Security.Claims;
using BLL.Interfaces;
using BLL.Models.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DoctorController(IDoctorService doctorService) : ControllerBase
{
    private int CurrentUserId => int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Доступно всім (гостям і авторизованим)
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? directionId)
    {
        // Передаємо параметр фільтрації в сервіс
        var doctors = await doctorService.GetAllDoctorsAsync(directionId);
        return this.Ok(doctors);
    }

    /// <summary>Поточний лікар переглядає свій профіль (лікарські + користувацькі дані). UserId з JWT.</summary>
    [Authorize(Roles = "Doctor")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var profile = await doctorService.GetMyDoctorProfileAsync(this.CurrentUserId);
        return this.Ok(profile);
    }

    // Доступно всім
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await doctorService.GetDoctorByIdAsync(id);
        return this.Ok(doctor);
    }

    // Тільки адмін
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddDoctorDTO dto)
    {
        var createdDoctor = await doctorService.CreateDoctorAsync(dto);
        return this.CreatedAtAction(nameof(GetById), new { id = createdDoctor.Id }, createdDoctor);
    }

    // Тільки адмін
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorDTO dto)
    {
        await doctorService.UpdateDoctorAsync(id, dto);
        return this.NoContent();
    }

    // Тільки адмін
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await doctorService.DeleteDoctorAsync(id);
        return this.NoContent();
    }
}
