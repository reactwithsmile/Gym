using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController]
[Route("api/testimonials")]
public class TestimonialsController : ControllerBase
{
    private const int MaxImageUrlLength = 10_000_000;
    private readonly AppDbContext _db;
    public TestimonialsController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<TestimonialDto>>> GetPublic(CancellationToken cancellationToken)
    {
        var testimonials = await _db.Testimonials.Where(item => item.IsActive)
            .OrderBy(item => item.DisplayOrder).ThenBy(item => item.Id).ToListAsync(cancellationToken);
        return Ok(testimonials.Select(MapToDto).ToList());
    }

    [HasPermission(PermissionCodes.TestimonialsView)]
    [HttpGet("all")]
    public async Task<ActionResult<List<TestimonialDto>>> GetAll(CancellationToken cancellationToken)
    {
        var testimonials = await _db.Testimonials.OrderBy(item => item.DisplayOrder).ThenBy(item => item.Id).ToListAsync(cancellationToken);
        return Ok(testimonials.Select(MapToDto).ToList());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TestimonialDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var testimonial = await _db.Testimonials.FirstOrDefaultAsync(item => item.Id == id && item.IsActive, cancellationToken);
        return testimonial is null ? NotFound(new { message = $"Testimonial with id {id} was not found." }) : Ok(MapToDto(testimonial));
    }

    [HasPermission(PermissionCodes.TestimonialsCreate)]
    [HttpPost]
    public async Task<ActionResult<TestimonialDto>> Create([FromBody] CreateTestimonialRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var error = Validate(request.CustomerName, request.Review, request.Rating, request.ImageUrl, request.DisplayOrder);
        if (error is not null) return BadRequest(new { message = error });
        var testimonial = new Testimonial
        {
            CustomerName = request.CustomerName.Trim(),
            RoleOrDescription = request.RoleOrDescription?.Trim() ?? string.Empty,
            Review = request.Review.Trim(),
            Rating = request.Rating,
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Testimonials.Add(testimonial);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = testimonial.Id }, MapToDto(testimonial));
    }

    [HasPermission(PermissionCodes.TestimonialsEdit)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TestimonialDto>> Update(int id, [FromBody] UpdateTestimonialRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var testimonial = await _db.Testimonials.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (testimonial is null) return NotFound(new { message = $"Testimonial with id {id} was not found." });
        var customerName = request.CustomerName ?? testimonial.CustomerName;
        var review = request.Review ?? testimonial.Review;
        var rating = request.Rating ?? testimonial.Rating;
        var imageUrl = request.ImageUrl ?? testimonial.ImageUrl;
        var displayOrder = request.DisplayOrder ?? testimonial.DisplayOrder;
        var error = Validate(customerName, review, rating, imageUrl, displayOrder);
        if (error is not null) return BadRequest(new { message = error });
        testimonial.CustomerName = customerName.Trim();
        testimonial.RoleOrDescription = request.RoleOrDescription?.Trim() ?? testimonial.RoleOrDescription;
        testimonial.Review = review.Trim();
        testimonial.Rating = rating;
        testimonial.ImageUrl = imageUrl.Trim();
        testimonial.DisplayOrder = displayOrder;
        testimonial.IsActive = request.IsActive ?? testimonial.IsActive;
        testimonial.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(MapToDto(testimonial));
    }

    [HasPermission(PermissionCodes.TestimonialsDelete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var testimonial = await _db.Testimonials.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (testimonial is null) return NotFound(new { message = $"Testimonial with id {id} was not found." });
        _db.Testimonials.Remove(testimonial);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string? Validate(string? customerName, string? review, int rating, string? imageUrl, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(customerName)) return "Customer name is required.";
        if (string.IsNullOrWhiteSpace(review)) return "Review is required.";
        if (rating is < 1 or > 5) return "Rating must be between 1 and 5.";
        if (displayOrder < 0) return "Display order cannot be negative.";
        if ((imageUrl?.Length ?? 0) > MaxImageUrlLength) return $"ImageUrl cannot exceed {MaxImageUrlLength:N0} characters.";
        return null;
    }

    private static TestimonialDto MapToDto(Testimonial item) => new()
    {
        Id = item.Id, CustomerName = item.CustomerName, RoleOrDescription = item.RoleOrDescription,
        Review = item.Review, Rating = item.Rating, ImageUrl = item.ImageUrl,
        DisplayOrder = item.DisplayOrder, IsActive = item.IsActive,
        CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt
    };
}
