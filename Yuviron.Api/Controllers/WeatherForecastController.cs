using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yuviron.Api.Data;
using Yuviron.Api.Dtos;
using Yuviron.Api.Models;

namespace Yuviron.Api.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] CreateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
            return BadRequest("firstName and lastName are required");

        var user = new User
        {
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim()
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (user is null) return NotFound();
        return Ok(user);
    }
}