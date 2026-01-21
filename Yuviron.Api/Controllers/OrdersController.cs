using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yuviron.Api.Data;
using Yuviron.Api.Dtos;
using Yuviron.Api.Models;

namespace Yuviron.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] CreateOrderRequest req) {
        if (req is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(req.ProductName))
            return BadRequest("ProductName is required.");

        if (req.Quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");

        if (req.Price < 0m)
            return BadRequest("Price must be non-negative.");

        var order = new Order
        {
            ProductName = req.ProductName.Trim(),
            Quantity = req.Quantity,
            Price = req.Price,
            Description = req.Description?.Trim()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetById(int id)
    {
        var order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();
        return Ok(order);
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAllOrders()
    {
        var orders = await _db.Orders.AsNoTracking().ToListAsync();
        return Ok(orders);
    }
}
