using FarmXpert.Data;
using FarmXpert.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BCrypt.Net;  // تأكد من استدعاء هذه المكتبة

namespace FarmXpertLogin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VeterinariansController : ControllerBase
    {
        private readonly FarmDbContext _FarmDb;

        public VeterinariansController(FarmDbContext FarmDb)
        {
            _FarmDb = FarmDb;
        }

        [Authorize(Roles = "manager")]
        [HttpPost("AddVeterinar")]
        public async Task<IActionResult> RegisterVeterinar([FromForm] AddWorker model)
        {
            string imagePath = null;

            if (model.Image != null)
            {
                var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "veterinarians");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fullPath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                imagePath = $"/images/veterinarians/{fileName}";
            }

            var managerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var manager = await _FarmDb.Users.FirstOrDefaultAsync(u => u.Email == managerEmail && u.Role == "Manager");
            if (manager == null)
                return Unauthorized("Invalid manager");

            if (await _FarmDb.Veterinarians.AnyAsync(w => w.Email == model.Email) ||
                await _FarmDb.Workers.AnyAsync(v => v.Email == model.Email) ||
                await _FarmDb.Users.AnyAsync(v => v.Email == model.Email))
                return BadRequest("Email already exists in another user.");

            if (await _FarmDb.Veterinarians.AnyAsync(w => w.NationalID == model.NationalID) ||
                await _FarmDb.Workers.AnyAsync(v => v.NationalID == model.NationalID))
                return BadRequest("National ID already exists.");

            string uniqueCode;
            do
            {
                uniqueCode = Guid.NewGuid().ToString("N").Substring(0, 4);
            } while (await _FarmDb.Veterinarians.AnyAsync(w => w.Code == uniqueCode));

            var Veterinar = new Veterinarians
            {
                ImagePath = imagePath,
                Name = model.Name,
                NationalID = model.NationalID,
                Specialty = model.Specialty,
                Phone = model.Phone.ToString(),
                Salary = model.Salary,
                Age = model.Age,
                Experience = model.Experience,
                Code = uniqueCode,
                CreatedAt = DateTime.Now,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password), // تشفير بكريبت هنا
                Role = "Veterinar",
                FarmId = manager.FarmId
            };

            _FarmDb.Veterinarians.Add(Veterinar);
            await _FarmDb.SaveChangesAsync();

            return Ok(new
            {
                message = "Veterinar registered successfully",
                veterinarId = Veterinar.Id,
                code = Veterinar.Code,
                createdAt = Veterinar.CreatedAt,
                imageUrl = Veterinar.ImagePath
            });
        }

        [Authorize(Roles = "manager")]
        [HttpGet("all")]
        public async Task<IActionResult> GetVeterinarians()
        {
            var managerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var manager = await _FarmDb.Users.FirstOrDefaultAsync(m => m.Email == managerEmail && m.Role == "Manager");
            if (manager == null)
                return Unauthorized("Manager not found");

            var veterinarians = await _FarmDb.Veterinarians
                .Where(v => v.FarmId == manager.FarmId)
                .Select(v => new
                {
                    Id = v.Id,
                    Name = v.Name,
                    NationalID = v.NationalID,
                    Age = v.Age,
                    Experience = v.Experience,
                    Specialty = v.Specialty,
                    Phone = v.Phone,
                    Salary = v.Salary,
                    Code = v.Code,
                    v.CreatedAt,
                    Email = v.Email,
                    ImagePath = v.ImagePath ?? ""
                })
                .ToListAsync();

            return Ok(veterinarians);
        }

        [Authorize(Roles = "manager")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVeterinarById(int id)
        {
            var managerFarmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var veterinar = await _FarmDb.Veterinarians
                .Where(v => v.Id == id && v.FarmId == managerFarmId)
                .Select(v => new
                {
                    Id = v.Id,
                    Name = v.Name,
                    NationalID = v.NationalID,
                    Age = v.Age + " Year",
                    Experience = v.Experience + " Year",
                    Specialty = v.Specialty,
                    Salary = v.Salary,
                    Phone = v.Phone,
                    Code = v.Code,
                    v.CreatedAt,
                    Email = v.Email,
                    ImagePath = v.ImagePath ?? ""
                })
                .FirstOrDefaultAsync();

            if (veterinar == null)
                return NotFound(new { message = "Veterinar not found or does not belong to your farm" });

            return Ok(veterinar);
        }

        [Authorize(Roles = "manager")]
        [HttpPut("UpdateVeterinar/{id}")]
        public async Task<IActionResult> UpdateVeterinar(int id, [FromForm] UpdateWorker model)
        {
            var managerFarmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var veterinar = await _FarmDb.Veterinarians
                .Where(v => v.Id == id && v.FarmId == managerFarmId)
                .FirstOrDefaultAsync();

            if (veterinar == null)
                return NotFound("Veterinar not found or does not belong to your farm");

            if (!string.IsNullOrEmpty(model.Name))
                veterinar.Name = model.Name;

            if (!string.IsNullOrEmpty(model.Specialty))
                veterinar.Specialty = model.Specialty;

            if (!string.IsNullOrEmpty(model.Phone))
                veterinar.Phone = model.Phone;

            if (!string.IsNullOrEmpty(model.Age))
                veterinar.Age = model.Age;

            if (model.Salary.HasValue)
                veterinar.Salary = model.Salary.Value;

            if (!string.IsNullOrEmpty(model.Experience))
                veterinar.Experience = model.Experience;

            if (!string.IsNullOrEmpty(model.NationalID))
                veterinar.NationalID = model.NationalID;

            // ✅ تحديث الصورة إن وُجدت
            if (model.ImagePath != null)
            {
                var fileName = $"{Guid.NewGuid()}_{model.ImagePath.FileName}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "veterinar");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fullPath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                 await model.ImagePath.CopyToAsync(stream);
                }

                // حذف الصورة القديمة إن وُجدت
                if (!string.IsNullOrEmpty(veterinar.ImagePath))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", veterinar.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                veterinar.ImagePath = $"/images/veterinar/{fileName}";
            }

            await _FarmDb.SaveChangesAsync();

            return Ok(new

            {
                veterinar.Id,
                    veterinar.Name,
                    veterinar.Specialty,
                    veterinar.Salary,
                    veterinar.NationalID,
                    veterinar.Phone,
                    veterinar.Age,
                    veterinar.Experience,
                    veterinar.ImagePath
                
            });
        }

        [Authorize(Roles = "manager")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteVeterinar(int id)
        {
            var managerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var manager = await _FarmDb.Users.FirstOrDefaultAsync(m => m.Email == managerEmail && m.Role == "Manager");
            if (manager == null)
                return Unauthorized("Manager not found.");

            var veterinar = await _FarmDb.Veterinarians.FirstOrDefaultAsync(v => v.Id == id && v.FarmId == manager.FarmId);
            if (veterinar == null)
                return NotFound(new { message = "Veterinar not found or doesn't belong to your farm." });

            if (veterinar.Role != "Veterinar")
                return BadRequest(new { message = "Cannot delete this record. Not a Veterinar." });

            if (!string.IsNullOrEmpty(veterinar.ImagePath))
            {
                try
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", veterinar.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to delete image file: " + ex.Message);
                }
            }

            _FarmDb.Veterinarians.Remove(veterinar);
            await _FarmDb.SaveChangesAsync();

            return Ok(new { message = "Veterinar deleted successfully." });
        }
    }
}
