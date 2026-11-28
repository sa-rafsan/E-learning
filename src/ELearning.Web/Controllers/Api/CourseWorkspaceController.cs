using System.Security.Claims;
using System.Text.Json;
using ELearning.Application.DTOs;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Web.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/workspace")]
public sealed class CourseWorkspaceController(
    ApplicationDbContext dbContext,
    IWebHostEnvironment environment) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".txt", ".zip", ".jpg", ".jpeg", ".png", ".webp"
    };

    [HttpGet("my-courses")]
    public async Task<IActionResult> GetMyCourses(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        if (User.IsInRole(nameof(UserRole.Admin)))
        {
            var all = await dbContext.Courses
                .AsNoTracking()
                .Include(c => c.Batches)
                .OrderByDescending(c => c.Id)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.CourseCode,
                    c.Category,
                    c.DurationWeeks,
                    c.ImagePath,
                    status = c.Status.ToString(),
                    batches = c.Batches.Select(b => new { b.Id, b.BatchName, b.BatchCode })
                })
                .ToListAsync(cancellationToken);
            return Ok(all);
        }

        if (User.IsInRole(nameof(UserRole.Teacher)))
        {
            var teacher = await dbContext.Teachers.AsNoTracking()
                .SingleOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);
            if (teacher is null) return Ok(Array.Empty<object>());

            var assignedBatchIds = await dbContext.BatchTeachers
                .AsNoTracking()
                .Where(bt => bt.TeacherId == teacher.Id)
                .Select(bt => bt.CourseBatchId)
                .ToListAsync(cancellationToken);

            var courses = await dbContext.Courses
                .AsNoTracking()
                .Include(c => c.Batches)
                .Where(c => c.TeacherId == teacher.Id || c.Batches.Any(b => assignedBatchIds.Contains(b.Id)))
                .OrderBy(c => c.Title)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.CourseCode,
                    c.Category,
                    c.DurationWeeks,
                    c.ImagePath,
                    status = c.Status.ToString(),
                    batches = c.Batches
                        .Where(b => assignedBatchIds.Contains(b.Id) || c.TeacherId == teacher.Id)
                        .Select(b => new { b.Id, b.BatchName, b.BatchCode })
                })
                .ToListAsync(cancellationToken);

            return Ok(courses);
        }

        var student = await dbContext.Students.AsNoTracking()
            .SingleOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);
        if (student is null) return Ok(Array.Empty<object>());

        var enrollments = await dbContext.CourseEnrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Include(e => e.Batch)
            .Where(e => e.StudentId == student.Id)
            .ToListAsync(cancellationToken);

        if (enrollments.Count == 0)
        {
            var published = await dbContext.Courses.AsNoTracking()
                .Where(c => c.IsPublished)
                .OrderBy(c => c.Title)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.CourseCode,
                    c.Category,
                    c.DurationWeeks,
                    c.ImagePath,
                    status = c.Status.ToString(),
                    batches = Array.Empty<object>()
                })
                .ToListAsync(cancellationToken);
            return Ok(published);
        }

        var enrolledCourses = enrollments
            .Where(e => e.Course != null)
            .Select(e => new
            {
                e.Course!.Id,
                e.Course.Title,
                e.Course.CourseCode,
                e.Course.Category,
                e.Course.DurationWeeks,
                e.Course.ImagePath,
                status = e.Course.Status.ToString(),
                batches = e.Batch != null
                    ? new[] { new { e.Batch.Id, e.Batch.BatchName, e.Batch.BatchCode } }
                    : Array.Empty<object>()
            });

        return Ok(enrolledCourses);
    }

    [HttpGet("courses/{courseId:int}")]
    public async Task<IActionResult> GetCourseDetails(int courseId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var course = await dbContext.Courses
            .AsNoTracking()
            .Include(c => c.Teacher)
                .ThenInclude(t => t!.Qualifications)
            .Include(c => c.Batches)
                .ThenInclude(b => b.BatchTeachers)
                    .ThenInclude(bt => bt.Teacher)
                        .ThenInclude(t => t!.Qualifications)
            .Include(c => c.Batches)
                .ThenInclude(b => b.Enrollments)
                    .ThenInclude(e => e.Student)
            .Include(c => c.Weeks)
                .ThenInclude(w => w.AssignedTeacher)
            .Include(c => c.Weeks)
                .ThenInclude(w => w.Topics)
            .Include(c => c.Weeks)
                .ThenInclude(w => w.Materials)
            .Include(c => c.Weeks)
                .ThenInclude(w => w.Activities)
                    .ThenInclude(a => a.Submissions)
            .SingleOrDefaultAsync(c => c.Id == courseId, cancellationToken);

        if (course is null) return NotFound(new { message = "Course not found." });

        // Ensure weeks exist dynamically based on DurationWeeks
        if (course.Weeks.Count == 0 && course.DurationWeeks > 0)
        {
            for (var w = 1; w <= course.DurationWeeks; w++)
            {
                await dbContext.CourseWeeks.AddAsync(new CourseWeek
                {
                    CourseId = course.Id,
                    WeekNumber = w,
                    Title = $"Week {w:D2}",
                    AssignedTeacherId = course.TeacherId
                }, cancellationToken);
            }
            await dbContext.SaveChangesAsync(cancellationToken);
            return await GetCourseDetails(courseId, cancellationToken);
        }

        int? currentStudentId = null;
        int? currentStudentBatchId = null;
        int? currentTeacherId = null;

        if (User.IsInRole(nameof(UserRole.Student)))
        {
            var student = await dbContext.Students.AsNoTracking()
                .SingleOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);
            currentStudentId = student?.Id;
            if (student is not null)
            {
                var enrollment = await dbContext.CourseEnrollments.AsNoTracking()
                    .SingleOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == student.Id, cancellationToken);
                currentStudentBatchId = enrollment?.CourseBatchId;
            }
        }
        else if (User.IsInRole(nameof(UserRole.Teacher)))
        {
            var teacher = await dbContext.Teachers.AsNoTracking()
                .SingleOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);
            currentTeacherId = teacher?.Id;
        }

        var teachers = new Dictionary<int, Teacher>();
        if (course.Teacher is not null) teachers[course.Teacher.Id] = course.Teacher;
        foreach (var bt in course.Batches.SelectMany(b => b.BatchTeachers))
        {
            if (bt.Teacher is not null) teachers[bt.Teacher.Id] = bt.Teacher;
        }

        var now = DateTime.UtcNow;

        return Ok(new
        {
            general = new
            {
                course.Id,
                course.Title,
                course.CourseCode,
                course.Description,
                course.Category,
                course.DurationHours,
                course.DurationWeeks,
                course.ImagePath,
                status = course.Status.ToString(),
                teachers = teachers.Values.Select(t => new
                {
                    t.Id,
                    t.FullName,
                    t.ImagePath,
                    t.Email,
                    t.PhoneNumber,
                    t.Address,
                    t.Nationality,
                    t.Country,
                    t.Specialization,
                    t.ProfessionalExperience,
                    t.ShortBiography,
                    qualifications = t.Qualifications.Select(q => new
                    {
                        q.DegreeName,
                        q.InstitutionName,
                        q.PassingYear
                    })
                }),
                batches = course.Batches.Select(b => new
                {
                    b.Id,
                    b.BatchName,
                    b.BatchCode,
                    b.StartDate,
                    b.EndDate,
                    b.Schedule,
                    status = b.Status.ToString(),
                    teachers = b.BatchTeachers.Select(bt => new
                    {
                        id = bt.TeacherId,
                        fullName = bt.Teacher?.FullName ?? string.Empty
                    }),
                    enrolledStudents = b.Enrollments.Select(e => new
                    {
                        id = e.StudentId,
                        studentId = e.Student?.StudentId ?? string.Empty,
                        fullName = e.Student?.FullName ?? string.Empty,
                        email = e.Student?.Email ?? string.Empty
                    })
                })
            },
            weeks = course.Weeks
                .OrderBy(w => w.WeekNumber)
                .Select(w => new
                {
                    w.Id,
                    w.WeekNumber,
                    w.Title,
                    w.Description,
                    w.LearningObjectives,
                    w.StartDate,
                    w.EndDate,
                    assignedTeacherName = w.AssignedTeacher?.FullName ?? course.Instructor,
                    topics = w.Topics.OrderBy(t => t.Order).Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Description,
                        t.Order
                    }),
                    materials = w.Materials.Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.MaterialType,
                        m.FilePath,
                        m.ExternalUrl,
                        m.UploadedAtUtc
                    }),
                    activities = w.Activities
                        .Where(a => currentStudentId == null ||
                                    (a.Status != ActivityStatus.Draft &&
                                     (a.CourseBatchId == null || currentStudentBatchId == null || a.CourseBatchId == currentStudentBatchId)))
                        .OrderBy(a => a.StartAtUtc)
                        .Select(a =>
                        {
                            var mySub = currentStudentId.HasValue
                                ? a.Submissions.FirstOrDefault(s => s.StudentId == currentStudentId.Value)
                                : null;

                            return new
                            {
                                a.Id,
                                type = a.Type.ToString(),
                                a.Title,
                                a.CourseBatchId,
                                a.MaxMarks,
                                a.PassingMarks,
                                a.StartAtUtc,
                                a.EndAtUtc,
                                a.SubmissionDeadlineUtc,
                                a.TimeLimitMinutes,
                                a.AllowLateSubmission,
                                status = a.Status.ToString(),
                                isUnlocked = currentStudentId == null || now >= a.StartAtUtc,
                                submissionsCount = a.Submissions.Count,
                                mySubmission = mySub == null ? null : new
                                {
                                    mySub.Id,
                                    mySub.SubmittedAtUtc,
                                    mySub.ObtainedMarks,
                                    mySub.Feedback,
                                    mySub.IsEvaluated,
                                    mySub.IsPublished
                                }
                            };
                        })
                })
        });
    }

    [Authorize(Roles = $"{nameof(UserRole.Teacher)},{nameof(UserRole.Admin)}")]
    [HttpPut("weeks/{weekId:int}")]
    public async Task<IActionResult> UpdateWeek(int weekId, [FromBody] UpdateWeekDto dto, CancellationToken cancellationToken)
    {
        var week = await dbContext.CourseWeeks.SingleOrDefaultAsync(w => w.Id == weekId, cancellationToken);
        if (week is null) return NotFound();

        if (!await CanManageCourseAsync(week.CourseId, cancellationToken)) return Forbid();

        week.Title = string.IsNullOrWhiteSpace(dto.Title) ? week.Title : dto.Title.Trim();
        week.Description = dto.Description?.Trim();
        week.LearningObjectives = dto.LearningObjectives?.Trim();
        week.AssignedTeacherId = dto.AssignedTeacherId;
        week.StartDate = dto.StartDate;
        week.EndDate = dto.EndDate;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Week updated." });
    }

    [Authorize(Roles = $"{nameof(UserRole.Teacher)},{nameof(UserRole.Admin)}")]
    [HttpPost("weeks/{weekId:int}/topics")]
    public async Task<IActionResult> AddTopic(int weekId, [FromBody] CreateWeekTopicDto dto, CancellationToken cancellationToken)
    {
        var week = await dbContext.CourseWeeks.SingleOrDefaultAsync(w => w.Id == weekId, cancellationToken);
        if (week is null) return NotFound();
        if (!await CanManageCourseAsync(week.CourseId, cancellationToken)) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest(new { message = "Topic title is required." });

        var topic = new WeekTopic
        {
            CourseWeekId = weekId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Order = dto.Order
        };

        await dbContext.WeekTopics.AddAsync(topic, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return StatusCode(201, new { topic.Id, topic.CourseWeekId, topic.Title, topic.Description, topic.Order });
    }

    [Authorize(Roles = $"{nameof(UserRole.Teacher)},{nameof(UserRole.Admin)}")]
    [HttpDelete("topics/{topicId:int}")]
    public async Task<IActionResult> DeleteTopic(int topicId, CancellationToken cancellationToken)
    {
        var topic = await dbContext.WeekTopics.Include(t => t.Week).SingleOrDefaultAsync(t => t.Id == topicId, cancellationToken);
        if (topic is null) return NotFound();
        if (!await CanManageCourseAsync(topic.Week!.CourseId, cancellationToken)) return Forbid();

        dbContext.WeekTopics.Remove(topic);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [Authorize(Roles = $"{nameof(UserRole.Teacher)},{nameof(UserRole.Admin)}")]
    [HttpPost("weeks/{weekId:int}/materials")]
    public async Task<IActionResult> AddMaterial(int weekId, [FromBody] CreateWeekMaterialDto dto, CancellationToken cancellationToken)
    {
        var week = await dbContext.CourseWeeks.SingleOrDefaultAsync(w => w.Id == weekId, cancellationToken);
        if (week is null) return NotFound();
        if (!await CanManageCourseAsync(week.CourseId, cancellationToken)) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest(new { message = "Material title is required." });

        var material = new WeekMaterial
        {
            CourseWeekId = weekId,
            Title = dto.Title.Trim(),
            MaterialType = dto.MaterialType,
            FilePath = dto.FilePath?.Trim(),
            ExternalUrl = dto.ExternalUrl?.Trim(),
            UploadedAtUtc = DateTime.UtcNow
        };

        await dbContext.WeekMaterials.AddAsync(material, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return StatusCode(201, new { material.Id, material.CourseWeekId, material.Title, material.MaterialType, material.FilePath, material.ExternalUrl, material.UploadedAtUtc });
    }

    [Authorize(Roles = $"{nameof(UserRole.Teacher)},{nameof(UserRole.Admin)}")]
    [HttpPost("weeks/{weekId:int}/activities")]
    public async Task<IActionResult> CreateActivity(
        int weekId,
        [FromBody] CreateWeekActivityDto dto,
        CancellationToken cancellationToken)
    {
        var week = await dbContext.CourseWeeks.SingleOrDefaultAsync(w => w.Id == weekId, cancellationToken);
        if (week is null) return NotFound();
        if (!await CanManageCourseAsync(week.CourseId, cancellationToken)) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest(new { message = "Activity title is required." });

        var teacherId = await ResolveTeacherIdAsync(week.CourseId, cancellationToken);
        if (teacherId <= 0) return BadRequest(new { message = "No teacher profile found to own this activity." });

        var activity = new WeekActivity
        {
            CourseWeekId = weekId,
            CourseBatchId = dto.CourseBatchId,
            TeacherId = teacherId,
            Type = dto.Type,
            Title = dto.Title.Trim(),
            Instructions = dto.Instructions?.Trim(),
            RequiredTasks = dto.RequiredTasks?.Trim(),
            AttachmentPath = dto.AttachmentPath?.Trim(),
            MaxMarks = dto.MaxMarks,
            PassingMarks = dto.PassingMarks,
            StartAtUtc = dto.StartAtUtc,
            EndAtUtc = dto.EndAtUtc,
            SubmissionDeadlineUtc = dto.SubmissionDeadlineUtc ?? dto.EndAtUtc,
            TimeLimitMinutes = dto.TimeLimitMinutes,
            AllowLateSubmission = dto.AllowLateSubmission,
            Status = dto.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        var order = 1;
        foreach (var q in dto.Questions)
        {
            if (string.IsNullOrWhiteSpace(q.QuestionText)) continue;
            activity.Questions.Add(new ActivityQuestion
            {
                QuestionText = q.QuestionText.Trim(),
                Type = q.Type,
                OptionA = q.OptionA?.Trim(),
                OptionB = q.OptionB?.Trim(),
                OptionC = q.OptionC?.Trim(),
                OptionD = q.OptionD?.Trim(),
                CorrectOption = q.CorrectOption?.Trim(),
                Marks = q.Marks > 0 ? q.Marks : 1,
                Order = order++
            });
        }

        await dbContext.WeekActivities.AddAsync(activity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return StatusCode(201, new { activity.Id, activity.Title, type = activity.Type.ToString() });
    }

    [HttpGet("activities/{activityId:int}")]
    public async Task<IActionResult> GetActivity(int activityId, CancellationToken cancellationToken)
    {
        var activity = await dbContext.WeekActivities
            .AsNoTracking()
            .Include(a => a.Questions)
            .Include(a => a.ForumPosts)
            .SingleOrDefaultAsync(a => a.Id == activityId, cancellationToken);

        if (activity is null) return NotFound();

        var isStudent = User.IsInRole(nameof(UserRole.Student));
        var now = DateTime.UtcNow;

        if (isStudent && now < activity.StartAtUtc)
        {
            return StatusCode(403, new { message = $"This activity opens at {activity.StartAtUtc:u}." });
        }

        object? mySubmission = null;
        if (isStudent)
        {
            var userId = GetCurrentUserId();
            var student = userId.HasValue
                ? await dbContext.Students.AsNoTracking().SingleOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken)
                : null;
            if (student is not null)
            {
                var sub = await dbContext.ActivitySubmissions.AsNoTracking()
                    .SingleOrDefaultAsync(s => s.WeekActivityId == activityId && s.StudentId == student.Id, cancellationToken);
                if (sub is not null)
                {
                    mySubmission = new
                    {
                        sub.Id,
                        sub.SubmissionText,
                        sub.AttachmentPath,
                        sub.ObtainedMarks,
                        sub.Feedback,
                        sub.IsAutoEvaluated,
                        sub.IsEvaluated,
                        sub.IsPublished,
                        sub.SubmittedAtUtc
                    };
                }
            }
        }

        return Ok(new
        {
            activity.Id,
            type = activity.Type.ToString(),
            activity.Title,
            activity.Instructions,
            activity.RequiredTasks,
            activity.AttachmentPath,
            activity.MaxMarks,
            activity.PassingMarks,
            activity.StartAtUtc,
            activity.EndAtUtc,
            activity.SubmissionDeadlineUtc,
            activity.TimeLimitMinutes,
            activity.AllowLateSubmission,
            status = activity.Status.ToString(),
            isExpired = now > (activity.SubmissionDeadlineUtc ?? activity.EndAtUtc),
            questions = activity.Questions.OrderBy(q => q.Order).Select(q => new
            {
                q.Id,
                q.QuestionText,
                type = q.Type.ToString(),
                q.OptionA,
                q.OptionB,
                q.OptionC,
                q.OptionD,
                correctOption = isStudent ? null : q.CorrectOption,
                q.Marks,
                q.Order
            }),
            forumPosts = activity.ForumPosts.OrderBy(p => p.CreatedAtUtc).Select(p => new
            {
                p.Id,
                p.ParentPostId,
                p.AuthorName,
                p.AuthorRole,
                p.Content,
                p.CreatedAtUtc
            }),
            mySubmission
        });
    }

    [Authorize(Roles = nameof(UserRole.Student))]
    [HttpPost("activities/{activityId:int}/submit")]
    public async Task<IActionResult> SubmitActivity(
        int activityId,
        [FromBody] SubmitActivityDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var student = await dbContext.Students.AsNoTracking()
            .SingleOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);
        if (student is null) return NotFound(new { message = "Student profile not found." });

        var activity = await dbContext.WeekActivities
            .Include(a => a.Questions)
            .SingleOrDefaultAsync(a => a.Id == activityId, cancellationToken);
        if (activity is null) return NotFound();

        var now = DateTime.UtcNow;
        if (now < activity.StartAtUtc)
        {
            return BadRequest(new { message = "Activity has not started yet." });
        }

        var deadline = activity.SubmissionDeadlineUtc ?? activity.EndAtUtc;
        if (now > deadline && !activity.AllowLateSubmission)
        {
            return BadRequest(new { message = "Submission deadline has passed." });
        }

        var existing = await dbContext.ActivitySubmissions
            .SingleOrDefaultAsync(s => s.WeekActivityId == activityId && s.StudentId == student.Id, cancellationToken);

        decimal autoMarks = 0;
        var hasObjectiveQuestions = activity.Questions.Any(q => q.Type != QuestionType.ShortAnswer);
        var hasShortAnswer = activity.Questions.Any(q => q.Type == QuestionType.ShortAnswer);

        if (dto.Answers is not null && activity.Questions.Count > 0)
        {
            foreach (var q in activity.Questions.Where(q => q.Type != QuestionType.ShortAnswer))
            {
                if (dto.Answers.TryGetValue(q.Id, out var chosen) &&
                    !string.IsNullOrWhiteSpace(q.CorrectOption) &&
                    string.Equals(chosen.Trim(), q.CorrectOption.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    autoMarks += q.Marks;
                }
            }
        }

        var answersJson = dto.Answers is null ? null : JsonSerializer.Serialize(dto.Answers);
        var isFullyAutoEvaluated = hasObjectiveQuestions && !hasShortAnswer;

        if (existing is null)
        {
            existing = new ActivitySubmission
            {
                WeekActivityId = activityId,
                StudentId = student.Id,
                SubmissionText = dto.SubmissionText?.Trim(),
                AttachmentPath = dto.AttachmentPath?.Trim(),
                AnswersJson = answersJson,
                ObtainedMarks = isFullyAutoEvaluated ? autoMarks : (hasObjectiveQuestions ? autoMarks : null),
                IsAutoEvaluated = hasObjectiveQuestions,
                IsEvaluated = isFullyAutoEvaluated,
                IsPublished = isFullyAutoEvaluated,
                StartedAtUtc = dto.StartedAtUtc,
                SubmittedAtUtc = now,
                EvaluatedAtUtc = isFullyAutoEvaluated ? now : null
            };
            await dbContext.ActivitySubmissions.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.SubmissionText = dto.SubmissionText?.Trim();
            existing.AttachmentPath = dto.AttachmentPath?.Trim() ?? existing.AttachmentPath;
            existing.AnswersJson = answersJson ?? existing.AnswersJson;
            existing.SubmittedAtUtc = now;
            if (hasObjectiveQuestions)
            {
                existing.ObtainedMarks = autoMarks;
                existing.IsAutoEvaluated = true;
                existing.IsEvaluated = isFullyAutoEvaluated;
                existing.IsPublished = isFullyAutoEvaluated;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new
        {
            message = "Submitted successfully.",
            obtainedMarks = existing.ObtainedMarks,
            isAutoEvaluated = existing.IsAutoEvaluated
        });
    }

    [Authorize(Roles = $"{nameof(UserRole.Teacher)},{nameof(UserRole.Admin)}")]
    [HttpGet("activities/{activityId:int}/submissions")]
    public async Task<IActionResult> GetSubmissions(int activityId, CancellationToken cancellationToken)
    {
        var subs = await dbContext.ActivitySubmissions
            .AsNoTracking()
            .Include(s => s.Student)
            .Where(s => s.WeekActivityId == activityId)
            .OrderByDescending(s => s.SubmittedAtUtc)
            .Select(s => new
            {
                s.Id,
                s.StudentId,
                studentCode = s.Student != null ? s.Student.StudentId : string.Empty,
                studentName = s.Student != null ? s.Student.FullName : string.Empty,
                s.SubmissionText,
                s.AttachmentPath,
                s.AnswersJson,
                s.ObtainedMarks,
                s.Feedback,
                s.IsEvaluated,
                s.IsPublished,
                s.SubmittedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(subs);
    }

    [Authorize(Roles = $"{nameof(UserRole.Teacher)},{nameof(UserRole.Admin)}")]
    [HttpPost("submissions/{submissionId:int}/evaluate")]
    public async Task<IActionResult> EvaluateSubmission(
        int submissionId,
        [FromBody] EvaluateSubmissionDto dto,
        CancellationToken cancellationToken)
    {
        var sub = await dbContext.ActivitySubmissions.SingleOrDefaultAsync(s => s.Id == submissionId, cancellationToken);
        if (sub is null) return NotFound();

        sub.ObtainedMarks = dto.ObtainedMarks;
        sub.Feedback = dto.Feedback?.Trim();
        sub.IsEvaluated = true;
        sub.IsPublished = dto.IsPublished;
        sub.EvaluatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Evaluation saved." });
    }

    [HttpPost("activities/{activityId:int}/forum")]
    public async Task<IActionResult> PostForumMessage(
        int activityId,
        [FromBody] CreateForumPostDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        if (string.IsNullOrWhiteSpace(dto.Content)) return BadRequest(new { message = "Content is required." });

        var activity = await dbContext.WeekActivities.AsNoTracking().SingleOrDefaultAsync(a => a.Id == activityId, cancellationToken);
        if (activity is null || activity.Type != ActivityType.Forum) return NotFound();

        var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        var name = User.Identity?.Name ?? "User";

        var post = new ForumPost
        {
            WeekActivityId = activityId,
            UserId = userId.Value,
            AuthorName = name,
            AuthorRole = role,
            ParentPostId = dto.ParentPostId,
            Content = dto.Content.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        await dbContext.ForumPosts.AddAsync(post, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return StatusCode(201, new { post.Id, post.WeekActivityId, post.ParentPostId, post.AuthorName, post.AuthorRole, post.Content, post.CreatedAtUtc });
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadAttachment(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0 || file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { message = "Please upload a valid file up to 10 MB." });
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            return BadRequest(new { message = "Unsupported file type." });
        }

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var uploadDir = Path.Combine(webRoot, "uploads", "materials");
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(uploadDir, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return Ok(new { filePath = $"/uploads/materials/{fileName}" });
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }

    private async Task<bool> CanManageCourseAsync(int courseId, CancellationToken cancellationToken)
    {
        if (User.IsInRole(nameof(UserRole.Admin))) return true;
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return false;

        var teacher = await dbContext.Teachers.AsNoTracking()
            .SingleOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);
        if (teacher is null) return false;

        var ownsCourse = await dbContext.Courses.AsNoTracking()
            .AnyAsync(c => c.Id == courseId && c.TeacherId == teacher.Id, cancellationToken);
        if (ownsCourse) return true;

        return await dbContext.BatchTeachers.AsNoTracking()
            .AnyAsync(bt => bt.TeacherId == teacher.Id && bt.Batch!.CourseId == courseId, cancellationToken);
    }

    private async Task<int> ResolveTeacherIdAsync(int courseId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            var teacher = await dbContext.Teachers.AsNoTracking()
                .SingleOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);
            if (teacher is not null) return teacher.Id;
        }

        var course = await dbContext.Courses.AsNoTracking().SingleOrDefaultAsync(c => c.Id == courseId, cancellationToken);
        return course?.TeacherId ?? 0;
    }
}

