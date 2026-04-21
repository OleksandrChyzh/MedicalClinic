using BLL.Interfaces;
using BLL.Models.Patient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Усі ендпоінти вимагають авторизації
public class PatientController(IPatientService patientService) : ControllerBase
{
    private int CurrentUserId => int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentUserRole => this.User.FindFirstValue(ClaimTypes.Role) ?? "User";

    // GET: api/patient/all (Тільки Адмін)
    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var patients = await patientService.GetAllPatientsAsync();
        return this.Ok(patients);
    }

    // GET: api/patient/my (Тільки Юзер)
    [Authorize(Roles = "User")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyPatients()
    {
        var patients = await patientService.GetMyPatientsAsync(this.CurrentUserId);
        return this.Ok(patients);
    }

    // GET: api/patient/5 (Всі авторизовані)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await patientService.GetPatientByIdAsync(id, this.CurrentUserId, this.CurrentUserRole);
        return this.Ok(patient);
    }

    // POST: api/patient (Юзер та Адмін)
    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddPatientDTO dto)
    {
        var createdPatient = await patientService.CreatePatientAsync(dto, this.CurrentUserId, this.CurrentUserRole);
        return this.CreatedAtAction(nameof(GetById), new { id = createdPatient.Id }, createdPatient);
    }

    // DELETE: api/patient/5 (Юзер та Адмін)
    [Authorize(Roles = "User,Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await patientService.DeletePatientAsync(id, this.CurrentUserId, this.CurrentUserRole);
        return this.NoContent();
    }
}
