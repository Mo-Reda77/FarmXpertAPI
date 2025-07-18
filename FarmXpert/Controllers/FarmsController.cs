using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmXpert.Data;
using FarmXpert.Models;
using Microsoft.AspNetCore.Authorization;

namespace FarmXpert.Controllers
{

    //[Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class FarmsController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public FarmsController(FarmDbContext context)
        {
            _context = context;
        }

        

        [HttpPost("add")]
        public async Task<IActionResult> AddFarm([FromBody] CreateFarmDto farmDto)
        {
            if (string.IsNullOrWhiteSpace(farmDto.Name))
                return BadRequest(new { Message = "Farm name required." });

            if (await _context.Farms.AnyAsync(f => f.Name == farmDto.Name))
                return BadRequest(new { Message = "The farm already exists." });

            var farm = new Farm
            {
                Name = farmDto.Name
            };

            _context.Farms.Add(farm);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $" The farm has been added successfully: {farm.Name}",
                FarmId = farm.Id
            });
        }




        [HttpGet("All")]
        public async Task<IActionResult> GetFarmsWithManager()
        {
            var farms = await _context.Farms
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    Manager = _context.Users
                        .Where(u => u.FarmId == f.Id && u.Role.ToLower() == "manager")
                        .Select(m => new
                        {
                            m.Id,
                            m.Name,
                            m.Email
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(farms);
        }
        // تحديث مزرعة
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateFarm(int id, [FromBody] CreateFarmDto farmDto)
        {
            var farm = await _context.Farms.FindAsync(id);
            if (farm == null)
                return NotFound(new { Message = "Farm not found." });

            if (string.IsNullOrWhiteSpace(farmDto.Name))
                return BadRequest(new { Message = "Farm name required." });

            farm.Name = farmDto.Name;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Farm updated successfully." });
        }

        // حذف مزرعة
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFarm(int id)
        {
            var farm = await _context.Farms.FindAsync(id);
            if (farm == null)
                return NotFound(new { Message = "Farm not found." });

            _context.Farms.Remove(farm);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Farm deleted successfully." });
        }



    }
}
