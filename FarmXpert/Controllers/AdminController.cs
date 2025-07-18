using FarmXpert.Data;
using FarmXpert.Models;
using FarmXpert.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmXpert.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private readonly FarmDbContext _context;
        public AdminController(FarmDbContext context)
        {
            _context = context;
        }
        [HttpPost("AddAdmiinn")]
        public async Task<IActionResult> AddAdmin([FromBody] Add_Admin request)
        {
            // التحقق من البريد مسبقاً
            if (await _context.Admins.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { Message = "Email is already in use." });

        

            var Admin = new Admin
            {
                Name = request.Name,
                Email = request.Email,
                Role = "admin",  // ثابت تلقائياً
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Admins.Add(Admin);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"admin '{Admin.Name}' added successfully.",
                AdminId = Admin.Id,
                Email = Admin.Email,
                Password = request.Password
            });
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllAdmins()
        {

            var Admin = await _context.Admins
                
                .Where(u => u.Role.ToLower() == "admin")


                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Email,
                    
                })
                .ToListAsync();

            return Ok(Admin);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null || admin.Role.ToLower() != "admin")
                return NotFound(new { Message = "Admin not found." });

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Admin deleted successfully." });
        }
    }
}
