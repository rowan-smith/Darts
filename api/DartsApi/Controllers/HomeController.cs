using DartsApi.Data;
using DartsApi.DTOs;
using DartsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DartsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController(DartsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HomeFeedDto>> GetHomeFeed()
    {
        var recentScores = await db.Matches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Winner)
            .OrderByDescending(m => m.UpdatedAt)
            .Take(10)
            .ToListAsync();

        var articles = await db.Articles
            .OrderByDescending(a => a.PublishedAt)
            .Take(10)
            .ToListAsync();

        var featuredArticles = await db.Articles
            .Where(a => a.IsFeatured)
            .OrderByDescending(a => a.PublishedAt)
            .Take(5)
            .ToListAsync();

        var suggestions = await db.Suggestions
            .Where(s => s.IsActive)
            .OrderBy(s => s.Priority)
            .Take(8)
            .ToListAsync();

        var featured = await db.FeaturedHighlights
            .Where(f => f.IsActive)
            .OrderBy(f => f.SortOrder)
            .ToListAsync();

        return Ok(new HomeFeedDto(
            recentScores.Select(m => m.ToSummaryDto()).ToList(),
            articles.Select(a => a.ToDto()).ToList(),
            featuredArticles.Select(a => a.ToDto()).ToList(),
            suggestions.Select(s => s.ToDto()).ToList(),
            featured.Select(f => f.ToDto()).ToList()
        ));
    }
}
