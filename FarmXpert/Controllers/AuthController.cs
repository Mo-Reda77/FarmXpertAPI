using FarmXpert.Data;
using FarmXpert.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace FarmXpertLogin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FarmDbContext _FarmDb;
        private readonly IConfiguration _configuration;

        public AuthController(FarmDbContext FarmDb, IConfiguration configuration)
        {
            _FarmDb = FarmDb;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            // حاول نلاقي المستخدم في جدول المديرين
            var manager = await _FarmDb.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (manager != null)
            {
                if (BCrypt.Net.BCrypt.Verify(model.Password, manager.PasswordHash))
                {
                    var token = GenerateJwtToken(manager.Email, manager.Role, manager.FarmId);
                    return Ok(new { token });
                }
                else
                {
                    return Unauthorized(new { Message = "Password is incorrect." });
                }
            }

            // حاول نلاقي المستخدم في جدول العمال
            var worker = await _FarmDb.Workers.FirstOrDefaultAsync(w => w.Email == model.Email);
            if (worker != null)
            {
                if (BCrypt.Net.BCrypt.Verify(model.Password, worker.PasswordHash))
                {
                    var token = GenerateJwtToken(worker.Email, worker.Role, worker.FarmId);
                    return Ok(new { token });
                }
                else
                {
                    return Unauthorized(new { Message = "Password is incorrect." });
                }
            }

            // حاول نلاقي المستخدم في جدول الأطباء البيطريين
            var veterinar = await _FarmDb.Veterinarians.FirstOrDefaultAsync(v => v.Email == model.Email);
            if (veterinar != null)
            {
                if (BCrypt.Net.BCrypt.Verify(model.Password, veterinar.PasswordHash))
                {
                    var token = GenerateJwtToken(veterinar.Email, veterinar.Role, veterinar.FarmId);
                    return Ok(new { token });
                }
                else
                {
                    return Unauthorized(new { Message = "Password is incorrect." });
                }
            }


            var Admin = await _FarmDb.Admins.FirstOrDefaultAsync(v => v.Email == model.Email);
            if (Admin != null)
            {
                if (BCrypt.Net.BCrypt.Verify(model.Password, Admin.PasswordHash))
                {
                    var token = GenerateJwtToken(Admin.Email, Admin.Role);
                    return Ok(new { token });
                }
                else
                {
                    return Unauthorized(new { Message = "Password is incorrect." });
                }
            }

            // لو البريد مش موجود في أي جدول
            return Unauthorized(new { Message = "Email is incorrect." });
        }



         




        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Invalid token" });

            if (await _FarmDb.RevokedTokens.AnyAsync(t => t.Token == token))
                return BadRequest(new { message = "Token already revoked" });

            _FarmDb.RevokedTokens.Add(new RevokedToken
            {
                Token = token,
                RevokedAt = DateTime.UtcNow
            });

            await _FarmDb.SaveChangesAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var worker = await _FarmDb.Workers.FirstOrDefaultAsync(w => w.Email == request.Email);
            if (worker != null)
            {
                var resetToken = new Random().Next(100000, 999999).ToString();
                worker.ResetCode = resetToken;
                worker.ResetCodeExpires = DateTime.UtcNow.AddMinutes(10);
                await _FarmDb.SaveChangesAsync();
                return Ok(new { message = "Reset Code generated for Worker", email = worker.Email });
            }

            var manager = await _FarmDb.Users.FirstOrDefaultAsync(m => m.Email == request.Email);
            if (manager != null)
            {
                var resetToken = new Random().Next(100000, 999999).ToString();
                manager.ResetCode = resetToken;
                manager.ResetCodeExpires = DateTime.UtcNow.AddMinutes(10);
                await _FarmDb.SaveChangesAsync();
                return Ok(new { message = "Reset Code generated for Manager", email = manager.Email });
            }

            return NotFound(new { message = "Email not found in the system" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword model)
        {
            var worker = await _FarmDb.Workers.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (worker != null)
            {
                if (worker.ResetCode != model.Code || worker.ResetCodeExpires < DateTime.UtcNow)
                    return BadRequest("Invalid or expired Code");

                worker.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                worker.ResetCode = null;
                worker.ResetCodeExpires = null;
                await _FarmDb.SaveChangesAsync();
                return Ok("Worker password has been reset successfully");
            }

            var manager = await _FarmDb.Users.FirstOrDefaultAsync(m => m.Email == model.Email);
            if (manager != null)
            {
                if (manager.ResetCode != model.Code || manager.ResetCodeExpires < DateTime.UtcNow)
                    return BadRequest("Invalid or expired Code");

                manager.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                manager.ResetCode = null;
                manager.ResetCodeExpires = null;
                await _FarmDb.SaveChangesAsync();
                return Ok("Manager password has been reset successfully");
            }

            return NotFound("No user found with this email");
        }

        private string GenerateJwtToken(string email, string role, int? farmId = null)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Role, role)
    };

            // فقط لو FarmId موجود ضيفه
            if (farmId.HasValue)
            {
                claims.Add(new Claim("FarmId", farmId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
