using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController, Route("api/products")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await db.Products.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).ToListAsync(ct));

    [HasPermission(PermissionCodes.ProductsView), HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await db.Products.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).ToListAsync(ct));

    [HasPermission(PermissionCodes.ProductsCreate), HttpPost]
    public async Task<IActionResult> Create(ProductRequest request, CancellationToken ct)
    {
        var error = Validate(request);
        if (error is not null) return BadRequest(new { message = error });
        var product = new Product { Name = request.Name.Trim(), Category = request.Category.Trim(), Description = request.Description.Trim(), ImageUrl = request.ImageUrl?.Trim() ?? "", Price = request.Price, IsActive = request.IsActive, DisplayOrder = request.DisplayOrder };
        db.Products.Add(product); await db.SaveChangesAsync(ct); return Ok(product);
    }

    [HasPermission(PermissionCodes.ProductsEdit), HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductRequest request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct);
        if (product is null) return NotFound();
        var error = Validate(request);
        if (error is not null) return BadRequest(new { message = error });
        product.Name = request.Name.Trim(); product.Category = request.Category.Trim(); product.Description = request.Description.Trim(); product.ImageUrl = request.ImageUrl?.Trim() ?? ""; product.Price = request.Price; product.IsActive = request.IsActive; product.DisplayOrder = request.DisplayOrder;
        await db.SaveChangesAsync(ct); return Ok(product);
    }

    [HasPermission(PermissionCodes.ProductsDelete), HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct);
        if (product is null) return NotFound();
        db.Products.Remove(product); await db.SaveChangesAsync(ct); return NoContent();
    }

    private static string? Validate(ProductRequest request) =>
        string.IsNullOrWhiteSpace(request.Name) ? "Product name is required." :
        string.IsNullOrWhiteSpace(request.Category) ? "Category is required." :
        request.Price < 0 ? "Price cannot be negative." :
        request.DisplayOrder < 0 ? "Display order cannot be negative." : null;
}
