using ELearning.Application.Common;
using ELearning.Application.DTOs;
using ELearning.Application.Interfaces.Repositories;
using ELearning.Application.Interfaces.Services;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using FluentValidation;

namespace ELearning.Application.Services;

public sealed class AttendanceService(
    IAttendanceRepository attendanceRepository,
    IValidator<SaveAttendanceRosterDto> saveValidator) : IAttendanceService
{
    public async Task<ServiceResult<AttendanceSessionResponseDto>> GetRosterAsync(
        int courseId,
        DateOnly sessionDate,
        string batch,
        string section,
        CancellationToken cancellationToken = default)
    {
        if (courseId <= 0 || string.IsNullOrWhiteSpace(batch) || string.IsNullOrWhiteSpace(section))
        {
            return ServiceResult<AttendanceSessionResponseDto>.Fail(
                "Course, batch, and section are required.",
                400);
        }

        var course = await attendanceRepository.GetCourseByIdAsync(courseId, cancellationToken);
        if (course is null)
        {
            return ServiceResult<AttendanceSessionResponseDto>.Fail("Course not found.", 404);
        }

        var normalizedBatch = batch.Trim();
        var normalizedSection = section.Trim().ToUpperInvariant();

        var students = await attendanceRepository.GetStudentsByBatchAndSectionAsync(
            normalizedBatch,
            normalizedSection,
            cancellationToken);

        var existingSession = await attendanceRepository.GetSessionAsync(
            courseId,
            sessionDate,
            normalizedBatch,
            normalizedSection,
            cancellationToken);

        var existingByStudentId = existingSession?.Records
            .ToDictionary(r => r.StudentId) ?? new Dictionary<int, AttendanceRecord>();

        var rows = students.Select(student =>
        {
            existingByStudentId.TryGetValue(student.Id, out var existing);
            return new AttendanceRosterRowDto
            {
                StudentId = student.Id,
                StudentCode = student.StudentId,
                FullName = student.FullName,
                Email = student.Email,
                Batch = student.Batch,
                Section = student.Section,
                Status = (existing?.Status ?? AttendanceStatus.Present).ToString(),
                Remarks = existing?.Remarks
            };
        }).ToList();

        return ServiceResult<AttendanceSessionResponseDto>.Ok(new AttendanceSessionResponseDto
        {
            SessionId = existingSession?.Id ?? 0,
            CourseId = course.Id,
            CourseTitle = course.Title,
            TeacherId = existingSession?.TeacherId ?? 0,
            TeacherName = existingSession?.Teacher?.FullName ?? course.Instructor,
            SessionDate = sessionDate,
            Batch = normalizedBatch,
            Section = normalizedSection,
            Topic = existingSession?.Topic,
            PresentCount = rows.Count(r => r.Status == AttendanceStatus.Present.ToString()),
            AbsentCount = rows.Count(r => r.Status == AttendanceStatus.Absent.ToString()),
            LateCount = rows.Count(r => r.Status == AttendanceStatus.Late.ToString()),
            ExcusedCount = rows.Count(r => r.Status == AttendanceStatus.Excused.ToString()),
            Records = rows
        });
    }

    public async Task<ServiceResult<AttendanceSessionResponseDto>> SaveRosterAsync(
        int teacherUserId,
        SaveAttendanceRosterDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await saveValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return ServiceResult<AttendanceSessionResponseDto>.Fail("Validation failed.", 400, errors);
        }

        var teacher = await attendanceRepository.GetTeacherByUserIdAsync(teacherUserId, cancellationToken);
        if (teacher is null)
        {
            return ServiceResult<AttendanceSessionResponseDto>.Fail(
                "Only registered Teachers can record attendance.",
                403);
        }

        var course = await attendanceRepository.GetCourseByIdAsync(dto.CourseId, cancellationToken);
        if (course is null)
        {
            return ServiceResult<AttendanceSessionResponseDto>.Fail("Course not found.", 404);
        }

        var normalizedBatch = dto.Batch.Trim();
        var normalizedSection = dto.Section.Trim().ToUpperInvariant();

        var validStudents = await attendanceRepository.GetStudentsByBatchAndSectionAsync(
            normalizedBatch,
            normalizedSection,
            cancellationToken);
        var validStudentIds = validStudents.Select(s => s.Id).ToHashSet();

        var session = await attendanceRepository.GetSessionAsync(
            dto.CourseId,
            dto.SessionDate,
            normalizedBatch,
            normalizedSection,
            cancellationToken);

        var now = DateTime.UtcNow;

        if (session is null)
        {
            session = new AttendanceSession
            {
                CourseId = course.Id,
                TeacherId = teacher.Id,
                SessionDate = dto.SessionDate,
                Batch = normalizedBatch,
                Section = normalizedSection,
                Topic = string.IsNullOrWhiteSpace(dto.Topic) ? null : dto.Topic.Trim(),
                CreatedAtUtc = now
            };

            foreach (var input in dto.Records.Where(r => validStudentIds.Contains(r.StudentId)))
            {
                session.Records.Add(new AttendanceRecord
                {
                    StudentId = input.StudentId,
                    Status = input.Status,
                    Remarks = string.IsNullOrWhiteSpace(input.Remarks) ? null : input.Remarks.Trim(),
                    MarkedAtUtc = now
                });
            }

            await attendanceRepository.AddSessionAsync(session, cancellationToken);
        }
        else
        {
            if (session.TeacherId != teacher.Id)
            {
                return ServiceResult<AttendanceSessionResponseDto>.Fail(
                    "You can only update attendance sessions that you created.",
                    403);
            }

            session.Topic = string.IsNullOrWhiteSpace(dto.Topic) ? null : dto.Topic.Trim();
            session.UpdatedAtUtc = now;

            var existingRecords = session.Records.ToDictionary(r => r.StudentId);
            foreach (var input in dto.Records.Where(r => validStudentIds.Contains(r.StudentId)))
            {
                if (existingRecords.TryGetValue(input.StudentId, out var existing))
                {
                    existing.Status = input.Status;
                    existing.Remarks = string.IsNullOrWhiteSpace(input.Remarks) ? null : input.Remarks.Trim();
                    existing.MarkedAtUtc = now;
                }
                else
                {
                    session.Records.Add(new AttendanceRecord
                    {
                        AttendanceSessionId = session.Id,
                        StudentId = input.StudentId,
                        Status = input.Status,
                        Remarks = string.IsNullOrWhiteSpace(input.Remarks) ? null : input.Remarks.Trim(),
                        MarkedAtUtc = now
                    });
                }
            }
        }

        await attendanceRepository.SaveChangesAsync(cancellationToken);

        return await GetRosterAsync(course.Id, dto.SessionDate, normalizedBatch, normalizedSection, cancellationToken);
    }

    public async Task<ServiceResult<IReadOnlyList<AttendanceSessionResponseDto>>> GetTeacherSessionsAsync(
        int teacherUserId,
        CancellationToken cancellationToken = default)
    {
        var teacher = await attendanceRepository.GetTeacherByUserIdAsync(teacherUserId, cancellationToken);
        if (teacher is null)
        {
            return ServiceResult<IReadOnlyList<AttendanceSessionResponseDto>>.Fail("Teacher profile not found.", 404);
        }

        var sessions = await attendanceRepository.GetTeacherSessionsAsync(teacher.Id, cancellationToken);
        var result = sessions.Select(s => new AttendanceSessionResponseDto
        {
            SessionId = s.Id,
            CourseId = s.CourseId,
            CourseTitle = s.Course?.Title ?? string.Empty,
            TeacherId = s.TeacherId,
            TeacherName = s.Teacher?.FullName ?? teacher.FullName,
            SessionDate = s.SessionDate,
            Batch = s.Batch,
            Section = s.Section,
            Topic = s.Topic,
            PresentCount = s.Records.Count(r => r.Status == AttendanceStatus.Present),
            AbsentCount = s.Records.Count(r => r.Status == AttendanceStatus.Absent),
            LateCount = s.Records.Count(r => r.Status == AttendanceStatus.Late),
            ExcusedCount = s.Records.Count(r => r.Status == AttendanceStatus.Excused)
        }).ToList();

        return ServiceResult<IReadOnlyList<AttendanceSessionResponseDto>>.Ok(result);
    }

    public async Task<ServiceResult<IReadOnlyList<StudentAttendanceSummaryDto>>> GetStudentAttendanceAsync(
        int studentUserId,
        CancellationToken cancellationToken = default)
    {
        var student = await attendanceRepository.GetStudentByUserIdAsync(studentUserId, cancellationToken);
        if (student is null)
        {
            return ServiceResult<IReadOnlyList<StudentAttendanceSummaryDto>>.Fail("Student profile not found.", 404);
        }

        var records = await attendanceRepository.GetStudentRecordsAsync(student.Id, cancellationToken);
        var result = records.Select(r => new StudentAttendanceSummaryDto
        {
            SessionId = r.AttendanceSessionId,
            CourseTitle = r.Session?.Course?.Title ?? string.Empty,
            TeacherName = r.Session?.Teacher?.FullName ?? string.Empty,
            SessionDate = r.Session?.SessionDate ?? default,
            Batch = r.Session?.Batch ?? string.Empty,
            Section = r.Session?.Section ?? string.Empty,
            Topic = r.Session?.Topic,
            Status = r.Status.ToString(),
            Remarks = r.Remarks
        }).ToList();

        return ServiceResult<IReadOnlyList<StudentAttendanceSummaryDto>>.Ok(result);
    }
}

