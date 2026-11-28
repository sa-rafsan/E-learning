using ELearning.Application.DTOs;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Web.Controllers.Api;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/admin/batches")]
public sealed class AdminBatchesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBatches([FromQuery] int? courseId, CancellationToken cancellationToken)
    {
        var query = dbContext.CourseBatches
            .AsNoTracking()
            .Include(b => b.Course)
            .Include(b => b.BatchTeachers)
                .ThenInclude(bt => bt.Teacher)
            .Include(b => b.Enrollments)
            .AsQueryable();

        if (courseId.HasValue)
        {
            query = query.Where(b => b.CourseId == courseId.Value);
        }

        var batches = await query
            .OrderByDescending(b => b.Id)
            .Select(b => new
            {
                b.Id,
                b.CourseId,
                courseTitle = b.Course != null ? b.Course.Title : string.Empty,
                courseCode = b.Course != null ? b.Course.CourseCode : string.Empty,
                b.BatchName,
                b.BatchCode,
                b.StartDate,
                b.EndDate,
                b.Schedule,
                status = b.Status.ToString(),
                teachers = b.BatchTeachers.Select(bt => new
                {
                    id = bt.TeacherId,
                    fullName = bt.Teacher != null ? bt.Teacher.FullName : string.Empty,
                    email = bt.Teacher != null ? bt.Teacher.Email : string.Empty
                }),
                enrolledCount = b.Enrollments.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(batches);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBatch(
        [FromBody] SaveBatchDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.CourseId <= 0 || string.IsNullOrWhiteSpace(dto.BatchName) || string.IsNullOrWhiteSpace(dto.BatchCode))
        {
            return BadRequest(new { message = "Course, Batch Name, and Batch Code are required." });
        }

        var courseExists = await dbContext.Courses.AnyAsync(c => c.Id == dto.CourseId, cancellationToken);
        if (!courseExists)
        {
            return NotFound(new { message = "Course not found." });
        }

        var code = dto.BatchCode.Trim().ToUpperInvariant();
        var duplicate = await dbContext.CourseBatches
            .AnyAsync(b => b.CourseId == dto.CourseId && b.BatchCode == code, cancellationToken);
        if (duplicate)
        {
            return Conflict(new { message = $"Batch code '{code}' already exists for this course." });
        }

        var batch = new CourseBatch
        {
            CourseId = dto.CourseId,
            BatchName = dto.BatchName.Trim(),
            BatchCode = code,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Schedule = string.IsNullOrWhiteSpace(dto.Schedule) ? null : dto.Schedule.Trim(),
            Status = dto.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        var distinctTeacherIds = dto.TeacherIds.Where(id => id > 0).Distinct().ToList();
        if (distinctTeacherIds.Count > 0)
        {
            var validTeacherIds = await dbContext.Teachers
                .Where(t => distinctTeacherIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            foreach (var tId in validTeacherIds)
            {
                batch.BatchTeachers.Add(new BatchTeacher
                {
                    TeacherId = tId,
                    AssignedAtUtc = DateTime.UtcNow
                });
            }
        }

        await dbContext.CourseBatches.AddAsync(batch, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return StatusCode(201, new { batch.Id, batch.BatchName, batch.BatchCode, batch.CourseId });
    }

    [HttpPut("{id:int}/teachers")]
    public async Task<IActionResult> AssignTeachers(
        int id,
        [FromBody] AssignBatchTeachersDto dto,
        CancellationToken cancellationToken)
    {
        var batch = await dbContext.CourseBatches
            .Include(b => b.BatchTeachers)
            .SingleOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (batch is null)
        {
            return NotFound(new { message = "Batch not found." });
        }

        var distinctIds = dto.TeacherIds.Where(t => t > 0).Distinct().ToList();
        var validTeacherIds = await dbContext.Teachers
            .Where(t => distinctIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        batch.BatchTeachers.Clear();
        foreach (var tId in validTeacherIds)
        {
            batch.BatchTeachers.Add(new BatchTeacher
            {
                CourseBatchId = batch.Id,
                TeacherId = tId,
                AssignedAtUtc = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Batch teachers updated.", teacherCount = validTeacherIds.Count });
    }

    [HttpPost("{id:int}/students")]
    public async Task<IActionResult> EnrollStudentsInBatch(
        int id,
        [FromBody] AssignCourseStudentsDto dto,
        CancellationToken cancellationToken)
    {
        var batch = await dbContext.CourseBatches
            .SingleOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (batch is null)
        {
            return NotFound(new { message = "Batch not found." });
        }

        var targetIds = new HashSet<int>(dto.StudentIds.Where(s => s > 0));
        if (!string.IsNullOrWhiteSpace(dto.Batch))
        {
            var q = dbContext.Students.AsNoTracking().Where(s => s.Batch == dto.Batch.Trim());
            if (!string.IsNullOrWhiteSpace(dto.Section))
            {
                q = q.Where(s => s.Section == dto.Section.Trim().ToUpperInvariant());
            }

            foreach (var sid in await q.Select(s => s.Id).ToListAsync(cancellationToken))
            {
                targetIds.Add(sid);
            }
        }

        if (targetIds.Count == 0)
        {
            return BadRequest(new { message = "Select at least one student or batch/section." });
        }

        var existingEnrollments = await dbContext.CourseEnrollments
            .Where(e => e.CourseId == batch.CourseId && targetIds.Contains(e.StudentId))
            .ToListAsync(cancellationToken);

        var existingMap = existingEnrollments.ToDictionary(e => e.StudentId);
        var now = DateTime.UtcNow;

        foreach (var sid in targetIds)
        {
            if (existingMap.TryGetValue(sid, out var existing))
            {
                existing.CourseBatchId = batch.Id;
            }
            else
            {
                await dbContext.CourseEnrollments.AddAsync(new CourseEnrollment
                {
                    CourseId = batch.CourseId,
                    CourseBatchId = batch.Id,
                    StudentId = sid,
                    EnrolledAtUtc = now
                }, cancellationToken);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"Enrolled {targetIds.Count} students into {batch.BatchName}." });
    }
}

