using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAV.Domain.Entities;
using SAV.Infrastructure.Repositories;
using System.IO;

namespace SAV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArticlesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ArticlesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var articles = await _unitOfWork.Articles
            .GetQueryable()
            .AsNoTracking()
            .Where(article => !article.IsDeleted)
            .Include(article => article.Category)
            .ToListAsync();

        return Ok(articles.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var article = await _unitOfWork.Articles
            .GetQueryable()
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.Id == id)
            .Include(item => item.Category)
            .FirstOrDefaultAsync();

        if (article == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(article));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ArticleDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Name, price, stock quantity, and category are required." });
        }

        // Verify category exists
        var category = await _unitOfWork.Categories
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && !c.IsDeleted);

        if (category == null)
        {
            return BadRequest(new { message = "Category not found." });
        }

        var article = new Article
        {
            Name = dto.Name.Trim(),
            SerialNumber = dto.SerialNumber.Trim(),
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            Image = NormalizeImageName(dto.Image),
            CategoryId = dto.CategoryId,
        };

        await _unitOfWork.Articles.AddAsync(article);
        await _unitOfWork.SaveChangesAsync();

        var createdArticle = await _unitOfWork.Articles
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == article.Id)
            .Include(item => item.Category)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = article.Id }, MapToDto(createdArticle));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] ArticleDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Name, price, stock quantity, and category are required." });
        }

        var article = await _unitOfWork.Articles
            .GetQueryable()
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (article == null)
        {
            return NotFound();
        }

        // Verify category exists
        var category = await _unitOfWork.Categories
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && !c.IsDeleted);

        if (category == null)
        {
            return BadRequest(new { message = "Category not found." });
        }

        article.Name = dto.Name.Trim();
        article.SerialNumber = dto.SerialNumber.Trim();
        article.Price = dto.Price;
        article.StockQuantity = dto.StockQuantity;
        article.Image = NormalizeImageName(dto.Image);
        article.CategoryId = dto.CategoryId;

        await _unitOfWork.Articles.UpdateAsync(article);
        await _unitOfWork.SaveChangesAsync();

        var updatedArticle = await _unitOfWork.Articles
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Include(item => item.Category)
            .FirstAsync();

        return Ok(MapToDto(updatedArticle));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var article = await _unitOfWork.Articles
            .GetQueryable()
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (article == null)
        {
            return NotFound();
        }

        article.IsDeleted = true;
        article.DeletedDate = DateTime.UtcNow;

        await _unitOfWork.Articles.UpdateAsync(article);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static bool IsValid(ArticleDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.Name) &&
               dto.Price >= 0 &&
               dto.StockQuantity >= 0 &&
               dto.CategoryId > 0;
    }

    private static object MapToDto(Article article)
    {
        return new
        {
            article.Id,
            article.Name,
            article.SerialNumber,
            article.Price,
            article.StockQuantity,
            article.CategoryId,
            category = article.Category?.Name,
            article.Image,
            article.CreatedDate,
            article.UpdatedDate
        };
    }

    private static string? NormalizeImageName(string? image)
    {
        if (string.IsNullOrWhiteSpace(image))
        {
            return null;
        }

        return Path.GetFileName(image.Trim());
    }
}

public class ArticleDto
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string? Image { get; set; }
}
