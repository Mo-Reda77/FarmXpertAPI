using FarmXpert.Data;
using FarmXpert.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BCrypt.Net;
namespace FarmXpertLogin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkerController : ControllerBase
    {
        private readonly FarmDbContext _FarmDb;

        public WorkerController(FarmDbContext FarmDb)
        {
            _FarmDb = FarmDb;
        }

        [Authorize(Roles = "manager")]
        [HttpPost("AddWorker")]
        public async Task<IActionResult> RegisterWorker([FromForm] AddWorker model)
        {
            string imagePath = null;

            if (model.Image != null)
            {
                var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "workers");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fullPath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                imagePath = $"/images/workers/{fileName}";
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
            } while (await _FarmDb.Workers.AnyAsync(w => w.Code == uniqueCode));

            var worker = new Worker
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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password), // 🔒 تشفير بكريبت
                Role = "Worker",
                FarmId = manager.FarmId
            };

            _FarmDb.Workers.Add(worker);
            await _FarmDb.SaveChangesAsync();

            return Ok(new
            {
                message = "Worker registered successfully",
                workerId = worker.Id,
                code = worker.Code,
                createdAt = worker.CreatedAt,
                imageUrl = worker.ImagePath
            });
        }

        [Authorize(Roles = "manager")]
        [HttpGet("all")]
        public async Task<IActionResult> GetWorkers()
        {
            var managerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var manager = await _FarmDb.Users.FirstOrDefaultAsync(m => m.Email == managerEmail && m.Role == "Manager");
            if (manager == null)
                return Unauthorized("Manager not found");

            var workers = await _FarmDb.Workers
               .Where(w => w.FarmId == manager.FarmId)
               .Select(w => new
               {
                   Id = w.Id,
                   Name = w.Name,
                   NationalID = w.NationalID,
                   Age = w.Age,
                   Experience = w.Experience,
                   Specialty = w.Specialty,
                   Phone = w.Phone,
                   Salary = w.Salary,
                   Code = w.Code,
                   w.CreatedAt,
                   Email = w.Email,
                   ImagePath = w.ImagePath ?? ""
               })
               .ToListAsync();

            return Ok(workers);
        }

        [Authorize(Roles = "manager")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkerById(int id)
        {
            var managerFarmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var worker = await _FarmDb.Workers
                .Where(w => w.Id == id && w.FarmId == managerFarmId)
                .Select(w => new
                {
                    Id = w.Id,
                    Name = w.Name,
                    NationalID = w.NationalID,
                    Age = w.Age + " Year",
                    Experience = w.Experience + " Year",
                    Specialty = w.Specialty,
                    Salary = w.Salary,
                    Phone = w.Phone,
                    Code = w.Code,
                    w.CreatedAt,
                    Email = w.Email,
                    ImagePath = w.ImagePath ?? ""
                })
                .FirstOrDefaultAsync();

            if (worker == null)
                return NotFound(new { message = "Worker not found or does not belong to your farm" });

            return Ok(worker);
        }

        [Authorize(Roles = "manager")]
        [HttpPut("UpdateWorker/{id}")]
        public async Task<IActionResult> UpdateWorker(int id, [FromForm] UpdateWorker model)
        {
            var managerFarmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var worker = await _FarmDb.Workers
                .Where(w => w.Id == id && w.FarmId == managerFarmId)
                .FirstOrDefaultAsync();

            if (worker == null)
                return NotFound("Worker not found or does not belong to your farm");

            if (!string.IsNullOrEmpty(model.Name))
                worker.Name = model.Name;

            if (!string.IsNullOrEmpty(model.Specialty))
                worker.Specialty = model.Specialty;

            if (!string.IsNullOrEmpty(model.Phone))
                worker.Phone = model.Phone;

            if (!string.IsNullOrEmpty(model.Age))
                worker.Age = model.Age;

            if (model.Salary.HasValue)
                worker.Salary = model.Salary.Value;

            if (!string.IsNullOrEmpty(model.Experience))
                worker.Experience = model.Experience;

            if (!string.IsNullOrEmpty(model.NationalID))
                worker.NationalID = model.NationalID;

            // ✅ تحديث الصورة إن وُجدت
            if (model.ImagePath != null)
            {
                var fileName = $"{Guid.NewGuid()}_{model.ImagePath.FileName}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "workers");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fullPath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ImagePath.CopyToAsync(stream);
                }

                // حذف الصورة القديمة إن وُجدت
                if (!string.IsNullOrEmpty(worker.ImagePath))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", worker.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                worker.ImagePath = $"/images/workers/{fileName}";
            }

            await _FarmDb.SaveChangesAsync();

            return Ok(new
            {
                message = "Worker updated successfully",
                worker = new
                {
                    worker.Id,
                    worker.Name,
                    worker.Specialty,
                    worker.Salary,
                    worker.NationalID,
                    worker.Phone,
                    worker.Age,
                    worker.Experience,
                    worker.ImagePath
                }
            });
        }

        [Authorize(Roles = "manager")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            var managerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var manager = await _FarmDb.Users.FirstOrDefaultAsync(m => m.Email == managerEmail && m.Role == "Manager");
            if (manager == null)
                return Unauthorized("Manager not found.");

            var worker = await _FarmDb.Workers.FirstOrDefaultAsync(w => w.Id == id && w.FarmId == manager.FarmId);
            if (worker == null)
                return NotFound(new { message = "Worker not found or doesn't belong to your farm." });

            if (worker.Role != "Worker")
                return BadRequest(new { message = "Cannot delete this record. Not a Worker." });

            if (!string.IsNullOrEmpty(worker.ImagePath))
            {
                try
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", worker.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to delete image file: " + ex.Message);
                }
            }

            _FarmDb.Workers.Remove(worker);
            await _FarmDb.SaveChangesAsync();

            return Ok(new { message = "Worker deleted successfully." });
        }
    }
}
