using DartsApi.Data;
using DartsApi.DTOs;
using DartsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DartsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController(DartsDbContext db) : ControllerBase
{
    private static readonly Guid DefaultProfileId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.Id == DefaultProfileId);
        if (profile is null) return NotFound();
        return Ok(profile.ToDto());
    }

    [HttpPut]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.Id == DefaultProfileId);
        if (profile is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Name)) profile.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Email)) profile.Email = request.Email;
        if (request.Theme.HasValue) profile.Theme = request.Theme.Value;
        if (request.AvatarUrl is not null) profile.AvatarUrl = request.AvatarUrl;
        profile.UpdatedAt = DateTime.UtcNow;

        var localPlayer = await db.Players.FirstOrDefaultAsync(p => p.IsLocal);
        if (localPlayer is not null && !string.IsNullOrWhiteSpace(request.Name))
            localPlayer.Name = request.Name;

        await db.SaveChangesAsync();
        return Ok(profile.ToDto());
    }
}
