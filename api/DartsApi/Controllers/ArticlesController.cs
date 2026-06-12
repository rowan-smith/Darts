using DartsApi.Data;
using DartsApi.DTOs;
using DartsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DartsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController(DartsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ArticleDto>>> GetArticles([FromQuery] bool? featured = null)
    {
        var query = db.Articles.AsQueryable();
        if (featured == true) query = query.Where(a => a.IsFeatured);

        var articles = await query.OrderByDescending(a => a.PublishedAt).ToListAsync();
        return Ok(articles.Select(a => a.ToDto()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticleDto>> GetArticle(Guid id)
    {
        var article = await db.Articles.FindAsync(id);
        if (article is null) return NotFound();
        return Ok(article.ToDto());
    }
}
