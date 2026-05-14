using BLL.Interfaces;
using BLL.Models.Schedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Загальний доступ тільки для авторизованих користувачів
public class ScheduleController(IScheduleService scheduleService) : ControllerBase
{
    private int CurrentUserId => int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [Authorize(Roles = "Doctor")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMySchedule()
    {
        var schedules = await scheduleService.GetMyScheduleAsync(this.CurrentUserId);
        return this.Ok(schedules);
    }

    // GET: api/schedule/doctor/5
    // Доступно всім авторизованим (Пацієнтам, Лікарям, Адмінам)
    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetDoctorSchedule(int doctorId)
    {
        var schedules = await scheduleService.GetDoctorScheduleAsync(doctorId);
        return this.Ok(schedules);
    }

    // GET: api/schedule/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var schedule = await scheduleService.GetScheduleByIdAsync(id);
        return this.Ok(schedule);
    }

    // POST: api/schedule
    // Тільки для Адміністратора
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddScheduleDTO dto)
    {
        var createdSchedule = await scheduleService.CreateScheduleAsync(dto);
        return this.Ok(createdSchedule); // Або CreatedAtAction
    }

    // PUT: api/schedule
    // Тільки для Адміністратора
    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateScheduleDTO dto)
    {
        await scheduleService.UpdateScheduleAsync(dto);
        return this.NoContent(); // 204 статус при успішному оновленні
    }

    // DELETE: api/schedule/5
    // Тільки для Адміністратора
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await scheduleService.DeleteScheduleAsync(id);
        return this.NoContent(); // 204 статус при успішному видаленні
    }
}
