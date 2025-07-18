using FarmXpert.Data;
using FarmXpert.Models.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmXpert.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientRequestController : ControllerBase
    {
        private readonly FarmDbContext _context;

        public ClientRequestController(FarmDbContext context)
        {
            _context = context;
        }

        [HttpPost("Submit")]
        public async Task<IActionResult> SubmitRequest([FromBody] CreateClientRequestDto dto)
        {
            var request = new ClientRequest
            {
               
                Email = dto.Email,
                FarmName = dto.FarmName,
                
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            _context.ClientRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request submitted successfully." });
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await _context.ClientRequests
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, [FromBody] Update_Client dto)
        {
            var request = await _context.ClientRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound(new { message = "Request not found." });
            }

            // تحديث فقط القيم المرسلة (غير null)
          
            if (!string.IsNullOrWhiteSpace(dto.FarmName))
                request.FarmName = dto.FarmName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                request.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                request.PhoneNumber = dto.PhoneNumber;

       

            _context.ClientRequests.Update(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request updated successfully." });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.ClientRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound(new { message = "Request not found." });
            }

            _context.ClientRequests.Remove(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request deleted successfully." });
        }
    }
}
