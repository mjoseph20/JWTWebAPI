using JWTAuthServer.Data;
using JWTAuthServer.DTOs;
using JWTAuthServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());

            if (existingUser != null)
            {
                return Conflict(new { message = "Email is already registered." });
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var newUser = new User
            { 
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                Password = hashedPassword
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

            if (userRole != null)
            {
                var newUserRole = new UserRole
                { 
                    UserId = newUser.Id,
                    RoleId = userRole.Id
                };
                _context.UserRoles.Add(newUserRole);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetProfile), new { id = newUser.Id }, new { message = "User registered successfully!" });
        }

        [HttpGet("GetProfile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);

            if (emailClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: Email claim missing." });
            }

            string userEmail = emailClaim.Value;
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail.ToLower());
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var profile = new ProfileDTO
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            };

            return Ok(profile);
        }

        [HttpPut("UpdateProfile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);
            if (emailClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: Email claim missing." });
            }

            string userEmail = emailClaim.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail.ToLower());
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // update fields if provided
            if (!string.IsNullOrEmpty(updateDto.FirstName))
            { 
                user.FirstName = updateDto.FirstName;
            }

            if (!string.IsNullOrEmpty(updateDto.LastName))
            { 
                user.LastName = updateDto.LastName;
            }

            if (!string.IsNullOrEmpty(updateDto.Email))
            {
                // check if new email is already taken by another user
                var emailExists = await _context.Users.AnyAsync(
                    u => 
                    u.Email.ToLower() == updateDto.Email.ToLower() 
                    &&
                    u.Id != user.Id);
                if (emailExists)
                {
                    return Conflict(new { message = "Email is already in use by another account." });
                }
                user.Email = updateDto.Email;
            }

            if (!string.IsNullOrEmpty(updateDto.Password))
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(updateDto.Password);
                user.Password = hashedPassword;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully!" });
        }
    }
}
