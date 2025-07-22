using FarmXpert.Data;
using FarmXpert.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;





[Authorize(Roles = "manager,Worker")]
[Route("api/[controller]")]
[ApiController]

public class CattleController : ControllerBase
{
    private readonly FarmDbContext _FarmDb;

    public CattleController(FarmDbContext farmDb)
    {
        _FarmDb = farmDb;
    }
    [HttpPost("AddCattle")]
    public async Task<IActionResult> AddCattle([FromForm] CattleDto model)
    {
        var managerFarmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");
        if (string.IsNullOrWhiteSpace(model.Type))
            return BadRequest("Type is required");

        var cattle = new Cattle
        {
            Type = model.Type,
            Weight = model.Weight??0,
            Gender = model.Gender??"Unknown",
            Age = model.Age??0,
            FarmID = managerFarmId
        };

        _FarmDb.Cattle.Add(cattle);
        await _FarmDb.SaveChangesAsync();

        return Ok(new { message = "Cattle added successfully", CattleID = cattle.CattleID });
    }
    [HttpPatch("UpdateCattle/{id}")]
    public async Task<IActionResult> UpdateCattlePartial(int id, [FromBody] UpdateCattle model)
    {
        var managerFarmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

        var cattle = await _FarmDb.Cattle
            .Where(c => c.CattleID == id && c.FarmID == managerFarmId)
            .FirstOrDefaultAsync();

        if (cattle == null)
            return NotFound("Cattle not found or does not belong to your farm");

       
        if (!string.IsNullOrEmpty(model.Type))
            cattle.Type = model.Type;

        if (model.Weight.HasValue)
            cattle.Weight = model.Weight.Value;

        if (!string.IsNullOrEmpty(model.Gender))
            cattle.Gender = model.Gender;

        if (model.Age.HasValue)
            cattle.Age = model.Age.Value;

        await _FarmDb.SaveChangesAsync();

        return Ok(new { message = "Cattle updated successfully" });
    }
    [HttpDelete("DeleteCattle/{id}")]
    public async Task<IActionResult> DeleteCattle(int id)
    {
        var managerFarmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

        var cattle = await _FarmDb.Cattle
            .Where(c => c.CattleID == id && c.FarmID == managerFarmId)
            .FirstOrDefaultAsync();

        if (cattle == null)
            return NotFound("Cattle not found or does not belong to your farm");

        _FarmDb.Cattle.Remove(cattle);
        await _FarmDb.SaveChangesAsync();

        return Ok(new { message = "Cattle deleted successfully" });
    }
    [HttpGet("GetCattlesByType/{type}")]
    public async Task<IActionResult> GetCattlesByType(
    string type,
    [FromQuery] int? page = null,
    [FromQuery] int? pageSize = null,
    [FromQuery] string? sort = null,            // asc أو desc
    [FromQuery] string? sortBy = null        // weight أو age
)
    {
        var managerFarmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

        var query = _FarmDb.Cattle
            .Where(c => c.Type == type && c.FarmID == managerFarmId);

        // Sort by Weight
        query = sort?.ToLower() == "desc"
            ? query.OrderByDescending(c => c.Weight)
            : query.OrderBy(c => c.Weight);

        var totalCount = await query.CountAsync();

        int currentPage = page ?? 1;
        int currentPageSize = pageSize ?? totalCount;
        int totalPages = (int)Math.Ceiling((double)totalCount / currentPageSize);

        if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
        {
            int skip = (currentPage - 1) * currentPageSize;
            query = query.Skip(skip).Take(currentPageSize);
        }

        var cattles = await query
            .Select(c => new
            {
                c.CattleID,
                c.Type,
                c.Weight,
                c.Gender,
                c.Age
            })
            .ToListAsync();

        return Ok(new
        {
            currentPage,
            pageSize = currentPageSize,
            totalPages,
            totalCount,
            data = cattles
        });
    }

    [HttpGet("GetCattleByTypeAndId/{type}/{id}")]
    public async Task<IActionResult> GetCattleByTypeAndId(string type, int id)
    {
        var managerFarmId = int.Parse(User.FindFirst("FarmId")?.Value ?? "0");

        var cattle = await _FarmDb.Cattle
            .Where(c => c.Type == type && c.CattleID == id && c.FarmID == managerFarmId)
            .Select(c => new
            {
                c.CattleID,
                c.Type,
                c.Weight,
                c.Gender,
                c.Age,
                
            })
            .FirstOrDefaultAsync();

        if (cattle == null)
            return NotFound(new { message = $"{type} with ID {id} not found in your farm." });

        return Ok(cattle);
    }
}
