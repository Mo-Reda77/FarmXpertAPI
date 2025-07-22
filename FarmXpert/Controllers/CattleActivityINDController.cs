using FarmXpert.Data;
using FarmXpert.Models.Cattle_Activity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmXpert.Controllers
{

    [Authorize(Roles = "manager,Worker")]
    [ApiController]
    [Route("api/[controller]")]
    public class CattleActivityINDController : Controller
    {
        private readonly FarmDbContext _farmDb;
        public CattleActivityINDController(FarmDbContext context)
        {
            _farmDb = context;
        }

        [HttpGet("EventTypes")]
        public IActionResult GetEventTypes()
        {
            var list = new[]
            {
             "Dry off", "Treated", "Weighted", "Inseminated","Gives Birth",
             "Vaccinated", "Pregnant", "Aborted Pregnancy", "Deworming",
             "Hoof Trimming","Other"
            };

            return Ok(list);
        }


        [HttpPost("AddEvent")]
        public async Task<IActionResult> AddCattleEvent([FromForm] CattleEventDto dto)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            if (dto.TagNumber == null)
                return BadRequest(new { message = "Tag number is required." });

            if (string.IsNullOrWhiteSpace(dto.Notes))
            {
                return BadRequest(new { message = "Note is required." });
            }

            var cattleExists = await _farmDb.Cattle
                .AnyAsync(c => c.CattleID == dto.TagNumber && c.FarmID == farmId);

            if (!cattleExists)
                return NotFound(new { message = $"Cattle with ID {dto.TagNumber} not found in your farm." });

            var newEvent = new CattleEvent
            {
                EventType = dto.EventType,
                TagNumber = dto.TagNumber,
                Date =DateTime.Now,
                Notes = dto.Notes,
                Medicine = dto.Medicine,
                Weight = dto.Weight,
                Dosage = dto.Dosage,
                WithdrawalTime = dto.WithdrawalTime,
                VaccineType = dto.VaccineType,
                CalfGender = dto.CalfGender,
                FarmID = farmId
            };

            _farmDb.CattleEvents.Add(newEvent);
            await _farmDb.SaveChangesAsync();

            return Ok(new
            {
                message = "Event added successfully.",
                eventData = new
                {
                    newEvent.Id,
                    newEvent.EventType,
                    newEvent.TagNumber,
                    newEvent.Medicine,
                    newEvent.Dosage,
                    newEvent.Weight,
                    newEvent.WithdrawalTime,
                    newEvent.VaccineType,
                    newEvent.CalfGender,
                    newEvent.Notes,
                    newEvent.Date

                }
            });
        }


                [HttpGet("AllEvents")]
        public async Task<IActionResult> GetAllEvents(
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sort = null,            // "asc" أو "desc"
        [FromQuery] string? eventType = null         // نوع الحدث
)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var query = _farmDb.CattleEvents
                .Where(e => e.FarmID == farmId);

            // فلترة حسب نوع الحدث
            if (!string.IsNullOrEmpty(eventType))
            {
                query = query.Where(e => e.EventType.ToLower() == eventType.ToLower());
            }

            // إجمالي النتائج
            var totalCount = await query.CountAsync();

            // ترتيب حسب التاريخ
            query = sort?.ToLower() == "desc"
                ? query.OrderByDescending(e => e.Date)
                : query.OrderBy(e => e.Date);  // الافتراضي: asc

            int currentPage = page ?? 1;
            int currentPageSize = pageSize ?? totalCount;

            int totalPages = (int)Math.Ceiling((double)totalCount / currentPageSize);

            if (page != null && pageSize != null && page > 0 && pageSize > 0)
            {
                int skip = (currentPage - 1) * currentPageSize;
                query = query.Skip(skip).Take(currentPageSize);
            }

            var events = await query
                .Select(e => new
                {
                    e.Id,
                    e.EventType,
                    e.TagNumber,
                    e.Medicine,
                    e.Dosage,
                    e.Weight,
                    e.WithdrawalTime,
                    e.VaccineType,
                    e.CalfGender,
                    e.Notes,
                    e.Date
                })
                .ToListAsync();

            return Ok(new
            {
                currentPage,
                pageSize = currentPageSize,
                totalPages,
                totalCount,
                data = events
            });
        }



        [HttpPut("EditEvent/{id}")]
        public async Task<IActionResult> EditEvent(int id, [FromForm] CattleEventUpdateDto dto)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var cattleEvent = await _farmDb.CattleEvents
                .FirstOrDefaultAsync(e => e.Id == id && e.FarmID == farmId);

            if (cattleEvent == null)
                return NotFound(new { message = "Event not found in your farm" });


            // تحديث الحقول فقط إذا تم إرسالها
            if (!string.IsNullOrEmpty(dto.EventType))
                cattleEvent.EventType = dto.EventType;

            if (dto.TagNumber.HasValue)
                cattleEvent.TagNumber = dto.TagNumber.Value;

            if (!string.IsNullOrEmpty(dto.Notes))
                cattleEvent.Notes = dto.Notes;

            if (!string.IsNullOrEmpty(dto.Medicine))
                cattleEvent.Medicine = dto.Medicine;

            if (dto.Weight.HasValue)
                cattleEvent.Weight = dto.Weight;

            if (!string.IsNullOrEmpty(dto.Dosage))
                cattleEvent.Dosage = dto.Dosage;

            if (!string.IsNullOrEmpty(dto.WithdrawalTime))
                cattleEvent.WithdrawalTime = dto.WithdrawalTime;

            if (!string.IsNullOrEmpty(dto.VaccineType))
                cattleEvent.VaccineType = dto.VaccineType;

            if (!string.IsNullOrEmpty(dto.CalfGender))
                cattleEvent.CalfGender = dto.CalfGender;

            if (dto.Date.HasValue)
                cattleEvent.Date = dto.Date.Value;


            await _farmDb.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully.",
                updatedEvent = new
                {
                    cattleEvent.Id,
                    cattleEvent.EventType,
                    cattleEvent.TagNumber,
                    cattleEvent.Medicine,
                    cattleEvent.Dosage,
                    cattleEvent.Weight,
                    cattleEvent.VaccineType,
                    cattleEvent.WithdrawalTime,
                    cattleEvent.CalfGender,
                    cattleEvent.Notes,
                    cattleEvent.Date
                }
            });
        }

        [HttpDelete("DeleteEvent/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var cattleEvent = await _farmDb.CattleEvents
                .FirstOrDefaultAsync(e => e.Id == id && e.FarmID == farmId);

            if (cattleEvent == null)
                return NotFound(new { message = "Event not found or not in your farm" });

            _farmDb.CattleEvents.Remove(cattleEvent);
            await _farmDb.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully" });
        }

        




    }
}
