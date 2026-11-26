using ELearning.Application.Common;
using ELearning.Application.DTOs;

namespace ELearning.Application.Interfaces.Services;

public interface IAttendanceService
{
    Task<ServiceResult<AttendanceSessionResponseDto>> GetRosterAsync(
        int courseId,
        DateOnly sessionDate,
        string batch,
        string section,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AttendanceSessionResponseDto>> SaveRosterAsync(
        int teacherUserId,
        SaveAttendanceRosterDto dto,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<AttendanceSessionResponseDto>>> GetTeacherSessionsAsync(
        int teacherUserId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<StudentAttendanceSummaryDto>>> GetStudentAttendanceAsync(
        int studentUserId,
        CancellationToken cancellationToken = default);
}

