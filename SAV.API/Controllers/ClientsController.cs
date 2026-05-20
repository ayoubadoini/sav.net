using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAV.Domain.Entities;
using SAV.Infrastructure.Repositories;
using SAV.Shared.Constants;

namespace SAV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ClientsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var clients = await _unitOfWork.Clients
            .GetQueryable()
            .AsNoTracking()
            .Where(client => !client.IsDeleted)
            .Include(client => client.User)
            .ToListAsync();

        return Ok(clients.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var client = await _unitOfWork.Clients
            .GetQueryable()
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.Id == id)
            .Include(item => item.User)
            .FirstOrDefaultAsync();

        if (client == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(client));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ClientDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Full name, email, phone, and address are required." });
        }

        var existingUser = await _unitOfWork.Users
            .GetQueryable()
            .Include(user => user.Client)
            .FirstOrDefaultAsync(user => user.Email == dto.Email || user.UserName == dto.Email);

        if (existingUser?.Client != null)
        {
            return BadRequest(new { message = "A client already exists for this user." });
        }

        var clientRoleId = await _unitOfWork.Roles
            .GetQueryable()
            .Where(role => role.Name == Roles.Client)
            .Select(role => role.Id)
            .FirstOrDefaultAsync();

        if (clientRoleId == 0)
        {
            return BadRequest(new { message = "Client role is not configured." });
        }

        var user = existingUser ?? new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim(),
            UserName = dto.Email.Trim(),
            IsActive = true,
            RoleId = clientRoleId
        };

        if (existingUser == null)
        {
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        else
        {
            existingUser.FullName = dto.FullName.Trim();
            existingUser.Email = dto.Email.Trim();
            existingUser.UserName = dto.Email.Trim();
            existingUser.IsActive = true;
            existingUser.RoleId = clientRoleId;
            await _unitOfWork.Users.UpdateAsync(existingUser);
            user = existingUser;
        }

        var client = new Client
        {
            UserId = user.Id,
            Phone = dto.Phone.Trim(),
            Address = dto.Address.Trim()
        };

        await _unitOfWork.Clients.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();

        var createdClient = await _unitOfWork.Clients
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == client.Id)
            .Include(item => item.User)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = client.Id }, MapToDto(createdClient));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] ClientDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Full name, email, phone, and address are required." });
        }

        var client = await _unitOfWork.Clients
            .GetQueryable()
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (client == null)
        {
            return NotFound();
        }

        client.Phone = dto.Phone.Trim();
        client.Address = dto.Address.Trim();

        if (client.User != null)
        {
            client.User.FullName = dto.FullName.Trim();
            client.User.Email = dto.Email.Trim();
            client.User.UserName = dto.Email.Trim();
            client.User.IsActive = true;
            await _unitOfWork.Users.UpdateAsync(client.User);
        }

        await _unitOfWork.Clients.UpdateAsync(client);
        await _unitOfWork.SaveChangesAsync();

        var updatedClient = await _unitOfWork.Clients
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Include(item => item.User)
            .FirstAsync();

        return Ok(MapToDto(updatedClient));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var client = await _unitOfWork.Clients
            .GetQueryable()
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (client == null)
        {
            return NotFound();
        }

        await _unitOfWork.Clients.DeleteAsync(client);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static bool IsValid(ClientDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.FullName)
            && !string.IsNullOrWhiteSpace(dto.Email)
            && !string.IsNullOrWhiteSpace(dto.Phone)
            && !string.IsNullOrWhiteSpace(dto.Address);
    }

    private static ClientDto MapToDto(Client client)
    {
        return new ClientDto
        {
            Id = client.Id,
            FullName = client.User?.FullName ?? string.Empty,
            Email = client.User?.Email ?? string.Empty,
            Phone = client.Phone,
            Address = client.Address,
            Status = client.User?.IsActive == false ? "Inactive" : "Active"
        };
    }

    public sealed class ClientDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}