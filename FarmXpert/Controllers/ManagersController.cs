using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmXpert.Data;
using FarmXpert.Models;
using BCrypt.Net;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;

namespace FarmXpert.Controllers
{
    //[Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ManagersController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public ManagersController(FarmDbContext context)
        {
            _context = context;
        }

        // إضافة مدير جديد وربطه بمزرعة
        [HttpPost("Add Manager")]
        public async Task<IActionResult> AddManager([FromBody] AddManagerRequest request)
        {
            // التحقق من البريد مسبقاً
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { Message = "Email is already in use." });

            // التحقق من وجود المزرعة
            var farm = await _context.Farms.FindAsync(request.FarmId);
            if (farm == null)
                return NotFound(new { Message = "The farm does not exist." });

            var manager = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = "manager",  // ثابت تلقائياً
                FarmId = request.FarmId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(manager);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Manager '{manager.Name}' added successfully.",
                ManagerId = manager.Id,
                Email = manager.Email,
                Password = request.Password
            });
        }




        // عرض كل المديرين مع بيانات المزرعة الخاصة بهم (بدون كلمات المرور)
        [HttpGet("All")]
        public async Task<IActionResult> GetAllManagers()
        {

            var managers = await _context.Users
                .Include(u => u.Farm)
                .Where(u => u.Role.ToLower() == "manager")


                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Email,
                    Farm = new
                    {
                        m.Farm.Id,
                        m.Farm.Name
                    }
                })
                .ToListAsync();

            return Ok(managers);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateManager(int id, [FromBody] Update_Manager request)
        {
            var manager = await _context.Users.FindAsync(id);
            if (manager == null || manager.Role.ToLower() != "manager")
                return NotFound(new { Message = "Manager not found." });

            if (!string.IsNullOrWhiteSpace(request.Name))
                manager.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Email))
                manager.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.Password))
                manager.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            if (request.FarmId.HasValue)
            {
                var farm = await _context.Farms.FindAsync(request.FarmId.Value);
                if (farm == null)
                    return NotFound(new { Message = "The specified farm does not exist." });

                manager.FarmId = request.FarmId.Value;
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Manager updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteManager(int id)
        {
            var manager = await _context.Users.FindAsync(id);
            if (manager == null || manager.Role.ToLower() != "manager")
                return NotFound(new { Message = "Manager not found." });

            _context.Users.Remove(manager);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Manager deleted successfully." });
        }



    }
}
