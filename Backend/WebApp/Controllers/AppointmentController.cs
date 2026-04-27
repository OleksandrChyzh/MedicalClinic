using BLL.Interfaces;
using BLL.Models.Appointment;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Весь контролер потребує авторизації
public class AppointmentController(IAppointmentService appointmentService, IUnitOfWork unitOfWork) : ControllerBase
{
    private int CurrentUserId => int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll() => this.Ok(await appointmentService.GetAllAppointmentsAsync());

    [HttpGet("my")]
    public async Task<IActionResult> GetMyAppointments()
    {
        return this.Ok(await appointmentService.GetAppointmentsByUserIdAsync(this.CurrentUserId));
    }

    [Authorize(Roles = "Doctor")]
    [HttpGet("doctor-schedule")]
    public async Task<IActionResult> GetDoctorAppointments()
    {
        // Знаходимо DoctorId за UserId лікаря
        var doctor = await unitOfWork.DoctorRepository.GetFirstOrDefaultAsync(d => d.UserId == this.CurrentUserId);
        if (doctor == null)
        {
            return this.Forbid();
        }

        return this.Ok(await appointmentService.GetAppointmentsByDoctorIdAsync(doctor.Id));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddAppointmentDTO dto)
    {
        var result = await appointmentService.CreateAppointmentAsync(this.CurrentUserId, dto);
        return this.Ok(result);
    }

    [Authorize(Roles = "Doctor,Admin")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromQuery] AppointmentStatus status)
    {
        await appointmentService.ChangeStatusAsync(id, status);
        return this.NoContent();
    }

    [Authorize(Roles = "Doctor,Admin")]
    [HttpGet("doctor/{doctorId}/date/{date}")]
    public async Task<IActionResult> GetDoctorAppointmentsByDate(int doctorId, DateTime date)
    {
        // Перевірка безпеки для ролі "Лікар"
        if (this.User.IsInRole("Doctor"))
        {
            // Знаходимо DoctorId поточного користувача
            var currentDoctor = await unitOfWork.DoctorRepository.GetFirstOrDefaultAsync(d => d.UserId == this.CurrentUserId);

            // Якщо лікар намагається подивитися чужий розклад — блокуємо
            if (currentDoctor == null || currentDoctor.Id != doctorId)
            {
                return this.Forbid();
            }
        }
        // Адмін проходить цю перевірку безперешкодно

        var appointments = await appointmentService.GetDoctorAppointmentsByDateAsync(doctorId, date);
        return this.Ok(appointments);
    }

    [HttpGet("doctor/{doctorId}/available-slots")]
    public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateTime date)
    {
        // Оскільки ми просто дивимось вільні вікна (не персональні дані пацієнтів),
        // цей метод може викликати будь-який авторизований користувач.
        var slots = await appointmentService.GetAvailableSlotsAsync(doctorId, date);
        return this.Ok(slots);
    }
}
