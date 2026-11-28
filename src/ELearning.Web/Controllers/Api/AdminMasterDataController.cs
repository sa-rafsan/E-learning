using ELearning.Domain.Entities;
using ELearning.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Web.Controllers.Api;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/master-data")]
public sealed class AdminMasterDataController(ApplicationDbContext db) : ControllerBase
{
    public sealed record DepartmentRequest(string Name, string Code);
    public sealed record MasterBatchRequest(string Name, string Code, List<string>? Sections);
    public sealed record MasterSectionRequest(string Name);
    public sealed record SemesterRequest(string Name);

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
    {
        var items = await db.Departments
            .OrderBy(d => d.Name)
            .Select(d => new { d.Id, d.Name, d.Code })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] DepartmentRequest req, CancellationToken cancellationToken)
    {
        var name = (req.Name ?? string.Empty).Trim();
        var code = (req.Code ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code))
            return BadRequest(new { message = "Department Name and Code are required." });

        var entity = new Department { Name = name, Code = code };
        db.Departments.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { entity.Id, entity.Name, entity.Code });
    }

    [HttpPut("departments/{id:int}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentRequest req, CancellationToken cancellationToken)
    {
        var entity = await db.Departments.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound(new { message = "Department not found." });

        var name = (req.Name ?? string.Empty).Trim();
        var code = (req.Code ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code))
            return BadRequest(new { message = "Department Name and Code are required." });

        entity.Name = name;
        entity.Code = code;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { entity.Id, entity.Name, entity.Code });
    }

    [HttpDelete("departments/{id:int}")]
    public async Task<IActionResult> DeleteDepartment(int id, CancellationToken cancellationToken)
    {
        var entity = await db.Departments.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound(new { message = "Department not found." });

        var nameLower = entity.Name.ToLower();
        var codeLower = entity.Code.ToLower();
        var inUseByStudent = await db.Students.AnyAsync(
            s => s.Department != null && (s.Department.ToLower().Contains(nameLower) || s.Department.ToLower().Contains(codeLower)),
            cancellationToken);
        var inUseByCourse = await db.Courses.AnyAsync(
            c => c.Category.ToLower() == nameLower || c.Category.ToLower() == codeLower,
            cancellationToken);

        if (inUseByStudent || inUseByCourse)
            return BadRequest(new { message = $"Cannot remove department '{entity.Name}' because it is already assigned to students or courses." });

        db.Departments.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"Department '{entity.Name}' removed." });
    }

    [HttpGet("batches")]
    public async Task<IActionResult> GetBatches(CancellationToken cancellationToken)
    {
        var items = await db.MasterBatches
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
        var result = items.Select(b => new
        {
            b.Id,
            b.Name,
            b.Code,
            Sections = (b.Sections ?? "A,B,C,D").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        });
        return Ok(result);
    }

    [HttpPost("batches")]
    public async Task<IActionResult> CreateBatch([FromBody] MasterBatchRequest req, CancellationToken cancellationToken)
    {
        var name = (req.Name ?? string.Empty).Trim();
        var code = (req.Code ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code))
            return BadRequest(new { message = "Batch Name and Code are required." });

        var sections = req.Sections is { Count: > 0 } ? string.Join(",", req.Sections) : "A,B,C,D";
        var entity = new MasterBatch { Name = name, Code = code, Sections = sections };
        db.MasterBatches.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { entity.Id, entity.Name, entity.Code, Sections = sections.Split(',') });
    }

    [HttpPut("batches/{id:int}")]
    public async Task<IActionResult> UpdateBatch(int id, [FromBody] MasterBatchRequest req, CancellationToken cancellationToken)
    {
        var entity = await db.MasterBatches.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound(new { message = "Batch not found." });

        var name = (req.Name ?? string.Empty).Trim();
        var code = (req.Code ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code))
            return BadRequest(new { message = "Batch Name and Code are required." });

        var sections = req.Sections is { Count: > 0 } ? string.Join(",", req.Sections) : "A,B,C,D";
        entity.Name = name;
        entity.Code = code;
        entity.Sections = sections;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { entity.Id, entity.Name, entity.Code, Sections = sections.Split(',') });
    }

    [HttpDelete("batches/{id:int}")]
    public async Task<IActionResult> DeleteBatch(int id, CancellationToken cancellationToken)
    {
        var entity = await db.MasterBatches.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound(new { message = "Batch not found." });

        var nameLower = entity.Name.ToLower();
        var codeLower = entity.Code.ToLower();
        var inUseByStudent = await db.Students.AnyAsync(
            s => (s.Batch != null && (s.Batch.ToLower() == nameLower || s.Batch.ToLower() == codeLower))
              || (s.Department != null && (s.Department.ToLower().Contains(nameLower) || s.Department.ToLower().Contains(codeLower))),
            cancellationToken);
        var inUseByCourseBatch = await db.CourseBatches.AnyAsync(
            cb => cb.BatchName.ToLower() == nameLower || cb.BatchCode.ToLower() == codeLower,
            cancellationToken);

        if (inUseByStudent || inUseByCourseBatch)
            return BadRequest(new { message = $"Cannot remove batch '{entity.Name}' because it is already in use." });

        db.MasterBatches.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"Batch '{entity.Name}' removed." });
    }

    [HttpGet("sections")]
    public async Task<IActionResult> GetSections(CancellationToken cancellationToken)
    {
        var items = await db.MasterSections
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost("sections")]
    public async Task<IActionResult> CreateSection([FromBody] MasterSectionRequest req, CancellationToken cancellationToken)
    {
        var name = (req.Name ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(name))
            return BadRequest(new { message = "Section Name is required." });

        var entity = new MasterSection { Name = name };
        db.MasterSections.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { entity.Id, entity.Name });
    }

    [HttpPut("sections/{id:int}")]
    public async Task<IActionResult> UpdateSection(int id, [FromBody] MasterSectionRequest req, CancellationToken cancellationToken)
    {
        var entity = await db.MasterSections.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound(new { message = "Section not found." });

        var name = (req.Name ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(name))
            return BadRequest(new { message = "Section Name is required." });

        entity.Name = name;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { entity.Id, entity.Name });
    }

    [HttpDelete("sections/{id:int}")]
    public async Task<IActionResult> DeleteSection(int id, CancellationToken cancellationToken)
    {
        var entity = await db.MasterSections.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound(new { message = "Section not found." });

        var nameLower = entity.Name.ToLower();
        var inUseByStudent = await db.Students.AnyAsync(
            s => s.Section != null && s.Section.ToLower() == nameLower,
            cancellationToken);
        if (inUseByStudent)
            return BadRequest(new { message = $"Cannot remove section '{entity.Name}' because it is assigned to students." });

        db.MasterSections.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"Section '{entity.Name}' removed." });
    }

    [HttpGet("semesters")]
    public async Task<IActionResult> GetSemesters(CancellationToken cancellationToken)
    {
        var items = await db.Semesters
            .OrderBy(s => s.Id)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost("semesters")]
    public async Task<IActionResult> CreateSemester([FromBody] SemesterRequest req, CancellationToken cancellationToken)
    {
        var name = (req.Name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name))
            return BadRequest(new { message = "Semester Name is required." });

        var entity = new Semester { Name = name };
        db.Semesters.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { entity.Id, entity.Name });
    }

    [HttpPut("semesters/{id:int}")]
    public async Task<IActionResult> UpdateSemester(int id, [FromBody] SemesterRequest req, CancellationToken cancellationToken)
    {
        var entity = await db.Semesters.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound(new { message = "Semester not found." });

        var name = (req.Name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name))
            return BadRequest(new { message = "Semester Name is required." });

        entity.Name = name;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { entity.Id, entity.Name });
    }

    [HttpDelete("semesters/{id:int}")]
    public async Task<IActionResult> DeleteSemester(int id, CancellationToken cancellationToken)
    {
        var entity = await db.Semesters.FindAsync([id], cancellationToken);
        if (entity is null) return NotFound(new { message = "Semester not found." });

        db.Semesters.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"Semester '{entity.Name}' removed." });
    }
}

