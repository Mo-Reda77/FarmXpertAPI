using FarmXpert.Data;
using FarmXpert.Models.MilkProduction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmXpert.Controllers
{
    [Authorize(Roles = "manager,Worker")]
    [ApiController]
    [Route("api/[controller]")]
    public class MilkProductionController : Controller
    {
        private readonly FarmDbContext _farmDb;
        public MilkProductionController(FarmDbContext context)
        {
            _farmDb = context;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddMilkProduction([FromBody] AddMilk dto)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            if (string.IsNullOrEmpty(dto.TagNumber))
                return BadRequest(new { message = "Tag number is required." });

            if (!int.TryParse(dto.TagNumber, out int cattleId))
                return BadRequest(new { message = "Tag number must be a valid integer representing the CattleID." });

            var cattleExists = await _farmDb.Cattle.AnyAsync(c => c.CattleID == cattleId && c.FarmID == farmId);

            if (!cattleExists)
                return NotFound(new { message = $"No cattle found with ID {cattleId} in your farm." });

            // تحقق من أن أحد الحقول AM أو Noon أو PM غير صفر أو null
            if ((dto.AM == null || dto.AM == 0) && (dto.Noon == null || dto.Noon == 0) && (dto.PM == null || dto.PM == 0))
            {
                return BadRequest(new { message = "At least one of AM, Noon or PM milk quantity is required." });
            }

            // تحقق من أن Notes ليست فارغة (إذا كنت تريد أن تكون مطلوبة)
            if (string.IsNullOrWhiteSpace(dto.Notes))
            {
                return BadRequest(new { message = "Note is required." });
            }

            var today = DateTime.Now;
            var milk = new MilkProduction
            {
                Date = today,
                TagNumber = dto.TagNumber,
                CountNumber = "1",
                AM = dto.AM ?? 0,
                Noon = dto.Noon ?? 0,
                PM = dto.PM ?? 0,
                Notes = dto.Notes,
                Total = (dto.AM ?? 0) + (dto.Noon ?? 0) + (dto.PM ?? 0),
                FarmID = farmId
            };

            _farmDb.MilkProductions.Add(milk);
            await _farmDb.SaveChangesAsync();

            return Ok(new
            {
                message = "Milk production record added successfully.",
                milk = new
                {
                    milk.Id,
                    milk.TagNumber,
                    milk.CountNumber,
                    milk.AM,
                    milk.Noon,
                    milk.PM,
                    milk.Total,
                    milk.Notes,
                    milk.Date
                }
            });
        }



        [HttpPost("AddMultiple")]
        public async Task<IActionResult> AddMilkProductionForMultiple([FromBody] AddMilkMultiple dto)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            if (string.IsNullOrEmpty(dto.CountNumber))
                return BadRequest(new { message = "Count number is required." });

            // تحقق من أن أحد الحقول AM أو Noon أو PM غير صفر أو null
            if ((dto.AM == null || dto.AM == 0) && (dto.Noon == null || dto.Noon == 0) && (dto.PM == null || dto.PM == 0))
            {
                return BadRequest(new { message = "At least one of AM, Noon or PM milk quantity is required." });
            }

            // تحقق من أن Notes ليست فارغة (إذا كنت تريد أن تكون مطلوبة)
            if (string.IsNullOrWhiteSpace(dto.Notes))
            {
                return BadRequest(new { message = "Note is required." });
            }

            var milk = new MilkProduction
            {
                Date = DateTime.Now,
                TagNumber = "multiple",             // ✅ قيمة ثابتة
                CountNumber = dto.CountNumber,      // ✅ يدخلها المستخدم (مثلاً "10")
                AM = dto.AM ?? 0,
                Noon = dto.Noon ?? 0,
                PM = dto.PM ?? 0,
                Notes = dto.Notes,
                Total = (dto.AM ?? 0) + (dto.Noon ?? 0) + (dto.PM ?? 0),
                FarmID = farmId
            };

            _farmDb.MilkProductions.Add(milk);
            await _farmDb.SaveChangesAsync();

            return Ok(new
            {
                message = "Milk production record for multiple cattles added successfully.",
                milk = new
                {
                    milk.Id,
                    milk.TagNumber,
                    milk.CountNumber,
                    milk.AM,
                    milk.Noon,
                    milk.PM,
                    milk.Total,
                    milk.Notes,
                    milk.Date
                }
            });
        }


        [HttpGet("All")]
        public async Task<IActionResult> GetAllMilkProduction()
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var data = await _farmDb.MilkProductions
                .Where(m => m.FarmID == farmId)
                .OrderByDescending(m => m.Date)
                .Select(m => new
                {
                    m.Id,
                    m.TagNumber,
                    m.CountNumber,
                    m.AM,
                    m.Noon,
                    m.PM,
                    m.Total,
                    m.Notes,
                    m.Date
                })
                .ToListAsync();

            return Ok(data);
        }


        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> EditMilkProduction(int id, [FromBody] AddMilk dto)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var milk = await _farmDb.MilkProductions
                .Where(m => m.Id == id && m.FarmID == farmId)
                .FirstOrDefaultAsync();

            if (milk == null)
                return NotFound(new { message = "Milk production record not found or doesn't belong to your farm." });

            
            milk.TagNumber = dto.TagNumber;
            milk.AM = dto.AM ?? 0;
            milk.Noon = dto.Noon ?? 0;
            milk.PM = dto.PM ?? 0;
            milk.Notes = dto.Notes;
            milk.Total = (dto.AM ?? 0) + (dto.Noon ?? 0) + (dto.PM ?? 0);

            await _farmDb.SaveChangesAsync();

            return Ok(new
            {
                message = "Milk production record updated successfully.",
                milk = new
                {
                    milk.Id,
                    milk.TagNumber,
                    milk.CountNumber,
                    milk.AM,
                    milk.Noon,
                    milk.PM,
                    milk.Total,
                    milk.Notes,
                    milk.Date
                }
            });
        }


        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteMilkProduction(int id)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var milk = await _farmDb.MilkProductions
                .Where(m => m.Id == id && m.FarmID == farmId)
                .FirstOrDefaultAsync();

            if (milk == null)
                return NotFound(new { message = "Milk production record not found or doesn't belong to your farm." });

            _farmDb.MilkProductions.Remove(milk);
            await _farmDb.SaveChangesAsync();

            return Ok(new { message = "Milk production record deleted successfully." });
        }

        [HttpGet("GetCattlesByTypeAndGender")]
        public async Task<IActionResult> GetCattlesByTypeAndGender(string type, string gender)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var cattles = await _farmDb.Cattle
                .Where(c => c.Type == type && c.FarmID == farmId && c.Gender.ToLower() == gender.ToLower())
                .Select(c => new
                {
                    c.CattleID,
                    c.Type,
                    c.Gender,
                    c.Weight,
                    c.Age
                })
                .ToListAsync();
            
            return Ok(cattles);
        }


    }
}
