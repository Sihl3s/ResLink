using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResLink.Api.Auth;
using ResLink.Api.Contracts;
using ResLink.Api.Data;
using ResLink.Api.Domain;
using ResLink.Api.Services;

namespace ResLink.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AppDbContext db,
    JwtTokenService tokens,
    IConfiguration configuration,
    CurrentUser currentUser) : ControllerBase
{
    private readonly PasswordHasher<AppUser> _hasher = new();

    [HttpGet("residences")]
    public async Task<ActionResult<IReadOnlyList<ResidenceDto>>> Residences()
    {
        var items = await db.Residences
            .OrderBy(r => r.Name)
            .Select(r => new ResidenceDto(r.Id, r.Name, r.Campus, r.Address))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("register/student")]
    public async Task<ActionResult<AuthResponse>> RegisterStudent(StudentRegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and password are required.");
        }

        if (!await db.Residences.AnyAsync(r => r.Id == request.ResidenceId))
        {
            return BadRequest("Residence was not found.");
        }

        if (await db.Users.AnyAsync(u => u.Email == request.Email.Trim().ToLower()))
        {
            return Conflict("An account with that email already exists.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            Role = Roles.Student,
            StudentNumber = request.StudentNumber.Trim(),
            Room = request.Room.Trim(),
            ResidenceId = request.ResidenceId,
            Points = 10
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await db.Entry(user).Reference(u => u.Residence).LoadAsync();

        return Ok(DtoMapper.ToAuth(user, tokens.CreateToken(user)));
    }

    [HttpPost("register/staff")]
    public async Task<ActionResult<AuthResponse>> RegisterStaff(StaffRegisterRequest request)
    {
        var expectedCode = configuration["Staff:AccessCode"];
        if (!string.Equals(request.AccessCode, expectedCode, StringComparison.Ordinal))
        {
            return Unauthorized("Invalid staff access code.");
        }

        if (!Roles.Staff.Contains(request.Role))
        {
            return BadRequest("Staff role must be Admin, Security, or Maintenance.");
        }

        if (!await db.Residences.AnyAsync(r => r.Id == request.ResidenceId))
        {
            return BadRequest("Residence was not found.");
        }

        if (await db.Users.AnyAsync(u => u.Email == request.Email.Trim().ToLower()))
        {
            return Conflict("An account with that email already exists.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            Role = request.Role,
            StaffId = request.StaffId.Trim(),
            ResidenceId = request.ResidenceId
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await db.Entry(user).Reference(u => u.Residence).LoadAsync();

        return Ok(DtoMapper.ToAuth(user, tokens.CreateToken(user)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await db.Users
            .Include(u => u.Residence)
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLower());

        if (user is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return Unauthorized("This account has been deactivated.");
        }

        return Ok(DtoMapper.ToAuth(user, tokens.CreateToken(user)));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthResponse>> Me()
    {
        var user = await db.Users.Include(u => u.Residence).FirstOrDefaultAsync(u => u.Id == currentUser.UserId);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(DtoMapper.ToAuth(user, tokens.CreateToken(user)));
    }
}
