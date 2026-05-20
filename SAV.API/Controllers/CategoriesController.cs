using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAV.Domain.Entities;
using SAV.Infrastructure.Repositories;

namespace SAV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var categories = await _unitOfWork.Categories
            .GetQueryable()
            .AsNoTracking()
            .Where(category => !category.IsDeleted)
            .ToListAsync();

        return Ok(categories.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var category = await _unitOfWork.Categories
            .GetQueryable()
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.Id == id)
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(category));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CategoryDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Name is required." });
        }

        var category = new Category
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        var createdCategory = await _unitOfWork.Categories
            .GetQueryable()
            .AsNoTracking()
            .FirstAsync(item => item.Id == category.Id);

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, MapToDto(createdCategory));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] CategoryDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Name is required." });
        }

        var category = await _unitOfWork.Categories
            .GetQueryable()
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        category.Name = dto.Name.Trim();
        category.Description = dto.Description?.Trim() ?? string.Empty;

        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        var updatedCategory = await _unitOfWork.Categories
            .GetQueryable()
            .AsNoTracking()
            .FirstAsync(item => item.Id == id);

        return Ok(MapToDto(updatedCategory));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var category = await _unitOfWork.Categories
            .GetQueryable()
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        category.IsDeleted = true;
        category.DeletedDate = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static bool IsValid(CategoryDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.Name);
    }

    private static object MapToDto(Category category)
    {
        return new
        {
            category.Id,
            category.Name,
            category.Description,
            category.CreatedDate,
            category.UpdatedDate
        };
    }
}

public class CategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
