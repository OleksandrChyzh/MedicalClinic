using BLL.Interfaces;
using BLL.Models.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DoctorController(IDoctorService doctorService) : ControllerBase
{
    // Доступно всім (гостям і авторизованим)
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await doctorService.GetAllDoctorsAsync();
        return this.Ok(doctors);
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
