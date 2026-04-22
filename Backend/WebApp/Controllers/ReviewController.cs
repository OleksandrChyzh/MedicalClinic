using BLL.Interfaces;
using BLL.Models.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewController(IReviewService reviewService) : ControllerBase
{
    private int CurrentUserId => int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    private string CurrentUserRole => this.User.FindFirstValue(ClaimTypes.Role) ?? "Guest";

    // --- ПУБЛІЧНІ МЕТОДИ ---

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctor(int doctorId)
    {
        var reviews = await reviewService.GetReviewsByDoctorAsync(doctorId);
        return this.Ok(reviews);
    }

    [HttpGet("service/{serviceId}")]
    public async Task<IActionResult> GetByService(int serviceId)
    {
        var reviews = await reviewService.GetReviewsByServiceAsync(serviceId);
        return this.Ok(reviews);
    }

    // --- ПРИВАТНІ МЕТОДИ (Авторизація) ---

    [Authorize(Roles = "User")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddReviewDTO dto)
    {
        var result = await reviewService.AddReviewAsync(dto, this.CurrentUserId);
        return this.Ok(result);
    }

    [Authorize(Roles = "User")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDTO dto)
    {
        if (id != dto.Id)
        {
            return this.BadRequest("ID не збігаються.");
        }

        await reviewService.UpdateReviewAsync(dto, this.CurrentUserId);
        return this.NoContent();
    }

    [Authorize(Roles = "Admin,User")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await reviewService.DeleteReviewAsync(id, this.CurrentUserId, this.CurrentUserRole);
        return this.NoContent();
    }
}
