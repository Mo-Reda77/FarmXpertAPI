using FarmXpert.Data;
using FarmXpert.Models.Cattle_Activity;
using FarmXpert.Models.Cattle_Activity_Mass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmXpert.Controllers
{
    [Authorize(Roles = "manager,Worker")]
    [ApiController]
    [Route("api/[controller]")]
    public class CattleEventMassController : Controller
    {
        private readonly FarmDbContext _farmDb;
        public CattleEventMassController(FarmDbContext context)
        {
            _farmDb = context;
        }

        [HttpGet("EventTypes")]
        public IActionResult GetEventTypes()
        {
            var list = new[]
            {
              "Vaccination", "Heed Spraying", "Treatment",
               "Tagging", "Deworming","Hoof Trimming","Other"
            };

            return Ok(list);
        }


        [HttpPost("AddMassEvent")]
        public async Task<IActionResult> AddMassEvent([FromForm] CattleEventMassAdd dto)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            if (dto.EventType == null)
                return BadRequest(new { message = "EventType is required." });

            if (string.IsNullOrWhiteSpace(dto.Notes))
            {
                return BadRequest(new { message = "Note is required." });
            }



            var newEvent = new CattleEventMass
            {
                EventType = dto.EventType,
                Notes = dto.Notes,
                Medicine = dto.Medicine,
                Dosage = dto.Dosage,
                Date = DateTime.Now,
                FarmID = farmId
            };

            _farmDb.CattleEventMasses.Add(newEvent);
            await _farmDb.SaveChangesAsync();

            return Ok(new
            {
                message = "Mass event added successfully.",
                eventData = new
                {
                    newEvent.Id,
                    newEvent.EventType,
                    newEvent.Notes,
                    newEvent.Medicine,
                    newEvent.Dosage,
                    newEvent.Date
                }
            });
        }

        [HttpGet("AllMassEvents")]
        public async Task<IActionResult> GetAllMassEvents()
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var events = await _farmDb.CattleEventMasses
                .Where(e => e.FarmID == farmId)
                .OrderByDescending(e => e.Date)
                .Select(e => new
                {
                    e.Id,
                    e.EventType,  
                    e.Medicine,
                    e.Dosage,
                    e.Notes,
                    e.Date
                })
                .ToListAsync();

            return Ok(events);
        }


        [HttpPut("EditMassEvent/{id}")]
        public async Task<IActionResult> EditMassEvent(int id, [FromForm] CattleEventMassUpdate dto)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var existingEvent = await _farmDb.CattleEventMasses
                .FirstOrDefaultAsync(e => e.Id == id && e.FarmID == farmId);

            if (existingEvent == null)
                return NotFound(new { message = "Mass event not found in your farm." });

            if (dto.EventType != null) existingEvent.EventType = dto.EventType;
            if (dto.Notes != null) existingEvent.Notes = dto.Notes;
            if (dto.Medicine != null) existingEvent.Medicine = dto.Medicine;
            if (dto.Dosage != null) existingEvent.Dosage = dto.Dosage;
            if (dto.Date != null) existingEvent.Date = dto.Date.Value;

            await _farmDb.SaveChangesAsync();

            return Ok(new
            {
                message = "Mass event updated successfully.",
                updatedEvent = new
                {
                    existingEvent.Id,
                    existingEvent.EventType,
                    existingEvent.Notes,
                    existingEvent.Medicine,
                    existingEvent.Dosage,
                    existingEvent.Date
                }
            });
        }


        [HttpDelete("DeleteMassEvent/{id}")]
        public async Task<IActionResult> DeleteMassEvent(int id)
        {
            var farmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

            var existingEvent = await _farmDb.CattleEventMasses
                .FirstOrDefaultAsync(e => e.Id == id && e.FarmID == farmId);

            if (existingEvent == null)
                return NotFound(new { message = "Mass event not found or not in your farm." });

            _farmDb.CattleEventMasses.Remove(existingEvent);
            await _farmDb.SaveChangesAsync();

            return Ok(new { message = "Mass event deleted successfully." });
        }
    }
}
