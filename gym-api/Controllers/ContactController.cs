using System.Net.Mail;
using GymApi.Auth;
using GymApi.Data;
using GymApi.DTOs;
using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controllers;

[ApiController]
[Route("api/contact")]
public class ContactController : ControllerBase
{
    private readonly AppDbContext _db;
    public ContactController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ContactDto>> Get(CancellationToken cancellationToken)
    {
        var contact = await _db.Contacts.OrderByDescending(item => item.UpdatedAt).ThenByDescending(item => item.Id).FirstOrDefaultAsync(cancellationToken);
        return contact is null ? NotFound(new { message = "No contact content found." }) : Ok(MapToDto(contact));
    }

    [HasPermission(PermissionCodes.ContactEdit)]
    [HttpPost]
    public async Task<ActionResult<ContactDto>> Create([FromBody] CreateContactRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        if (await _db.Contacts.AnyAsync(cancellationToken)) return Conflict(new { message = "Contact content already exists. Update the existing record instead." });
        var error = Validate(request);
        if (error is not null) return BadRequest(new { message = error });
        var contact = MapToEntity(request);
        _db.Contacts.Add(contact);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = contact.Id }, MapToDto(contact));
    }

    [HasPermission(PermissionCodes.ContactEdit)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ContactDto>> Update(int id, [FromBody] UpdateContactRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { message = "Request body is required." });
        var contact = await _db.Contacts.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (contact is null) return NotFound(new { message = $"Contact with id {id} was not found." });
        var error = Validate(request);
        if (error is not null) return BadRequest(new { message = error });
        contact.GymName = request.GymName.Trim();
        contact.Address = request.Address?.Trim() ?? string.Empty;
        contact.Phone = request.Phone?.Trim() ?? string.Empty;
        contact.Email = request.Email?.Trim() ?? string.Empty;
        contact.OpeningHours = request.OpeningHours?.Trim() ?? string.Empty;
        contact.GoogleMapsUrl = request.GoogleMapsUrl?.Trim() ?? string.Empty;
        contact.InstagramUrl = request.InstagramUrl?.Trim() ?? string.Empty;
        contact.FacebookUrl = request.FacebookUrl?.Trim() ?? string.Empty;
        contact.WhatsAppNumber = request.WhatsAppNumber?.Trim() ?? string.Empty;
        contact.Description = request.Description?.Trim() ?? string.Empty;
        contact.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(MapToDto(contact));
    }

    private static string? Validate(CreateContactRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.GymName)) return "Gym name is required.";
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            try { _ = new MailAddress(request.Email); }
            catch (FormatException) { return "Email format is invalid."; }
        }
        return null;
    }

    private static Contact MapToEntity(CreateContactRequest request) => new()
    {
        GymName = request.GymName.Trim(), Address = request.Address?.Trim() ?? string.Empty,
        Phone = request.Phone?.Trim() ?? string.Empty, Email = request.Email?.Trim() ?? string.Empty,
        OpeningHours = request.OpeningHours?.Trim() ?? string.Empty, GoogleMapsUrl = request.GoogleMapsUrl?.Trim() ?? string.Empty,
        InstagramUrl = request.InstagramUrl?.Trim() ?? string.Empty, FacebookUrl = request.FacebookUrl?.Trim() ?? string.Empty,
        WhatsAppNumber = request.WhatsAppNumber?.Trim() ?? string.Empty, Description = request.Description?.Trim() ?? string.Empty,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    private static ContactDto MapToDto(Contact contact) => new()
    {
        Id = contact.Id, GymName = contact.GymName, Address = contact.Address, Phone = contact.Phone,
        Email = contact.Email, OpeningHours = contact.OpeningHours, GoogleMapsUrl = contact.GoogleMapsUrl,
        InstagramUrl = contact.InstagramUrl, FacebookUrl = contact.FacebookUrl, WhatsAppNumber = contact.WhatsAppNumber,
        Description = contact.Description, CreatedAt = contact.CreatedAt, UpdatedAt = contact.UpdatedAt
    };
}
