using ELearning.Domain.Entities;
using ELearning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options)
{
    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();

    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();

    public DbSet<TeacherQualification> TeacherQualifications => Set<TeacherQualification>();

    public DbSet<CourseBatch> CourseBatches => Set<CourseBatch>();

    public DbSet<BatchTeacher> BatchTeachers => Set<BatchTeacher>();

    public DbSet<CourseWeek> CourseWeeks => Set<CourseWeek>();

    public DbSet<WeekTopic> WeekTopics => Set<WeekTopic>();

    public DbSet<WeekMaterial> WeekMaterials => Set<WeekMaterial>();

    public DbSet<WeekActivity> WeekActivities => Set<WeekActivity>();

    public DbSet<ActivityQuestion> ActivityQuestions => Set<ActivityQuestion>();

    public DbSet<ActivitySubmission> ActivitySubmissions => Set<ActivitySubmission>();

    public DbSet<ForumPost> ForumPosts => Set<ForumPost>();

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<MasterBatch> MasterBatches => Set<MasterBatch>();
    public DbSet<MasterSection> MasterSections => Set<MasterSection>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<CourseBatchSection> CourseBatchSections => Set<CourseBatchSection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(course => course.Title).HasMaxLength(160).IsRequired();
            entity.Property(course => course.CourseCode).HasMaxLength(40).IsRequired();
            entity.Property(course => course.Description).HasMaxLength(2000).IsRequired();
            entity.Property(course => course.Instructor).HasMaxLength(120).IsRequired();
            entity.Property(course => course.Category).HasMaxLength(80).IsRequired();
            entity.Property(course => course.ImagePath).HasMaxLength(500);
            entity.Property(course => course.DurationWeeks).HasDefaultValue(12);
            entity.HasMany(course => course.Lessons)
                .WithOne(lesson => lesson.Course)
                .HasForeignKey(lesson => lesson.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(course => course.Teacher)
                .WithMany()
                .HasForeignKey(course => course.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CourseEnrollment>(entity =>
        {
            entity.HasIndex(enrollment => new { enrollment.CourseId, enrollment.StudentId })
                .IsUnique();

            entity.HasOne(enrollment => enrollment.Course)
                .WithMany(course => course.Enrollments)
                .HasForeignKey(enrollment => enrollment.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(enrollment => enrollment.Batch)
                .WithMany(batch => batch.Enrollments)
                .HasForeignKey(enrollment => enrollment.CourseBatchId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(enrollment => enrollment.Student)
                .WithMany(student => student.Enrollments)
                .HasForeignKey(enrollment => enrollment.StudentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasIndex(lp => new { lp.StudentId, lp.LessonId })
                .IsUnique();

            entity.HasOne(lp => lp.Student)
                .WithMany()
                .HasForeignKey(lp => lp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lp => lp.Lesson)
                .WithMany()
                .HasForeignKey(lp => lp.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.Property(lesson => lesson.Title).HasMaxLength(160).IsRequired();
            entity.Property(lesson => lesson.Summary).HasMaxLength(1000).IsRequired();
            entity.HasIndex(lesson => new { lesson.CourseId, lesson.Order }).IsUnique();
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.StudentId).HasMaxLength(64);
            entity.Property(user => user.Department).HasMaxLength(120);
            entity.Property(user => user.Role).IsRequired();
            entity.Property(user => user.Status).IsRequired();

            entity.HasIndex(user => user.NormalizedEmail).IsUnique();
            entity.HasIndex(user => user.StudentId)
                .IsUnique()
                .HasFilter("[StudentId] IS NOT NULL");

            entity.HasOne(user => user.TeacherProfile)
                .WithOne()
                .HasForeignKey<Teacher>(teacher => teacher.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(user => user.StudentProfile)
                .WithOne()
                .HasForeignKey<Student>(student => student.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(user => user.RefreshTokens)
                .WithOne()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.Property(teacher => teacher.FullName).HasMaxLength(120).IsRequired();
            entity.Property(teacher => teacher.Email).HasMaxLength(256).IsRequired();
            entity.Property(teacher => teacher.PhoneNumber).HasMaxLength(30);
            entity.Property(teacher => teacher.Department).HasMaxLength(120);
            entity.Property(teacher => teacher.Designation).HasMaxLength(120);
            entity.Property(teacher => teacher.ImagePath).HasMaxLength(500);
            entity.HasIndex(teacher => teacher.UserId).IsUnique();
            entity.HasIndex(teacher => teacher.Email).IsUnique();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(student => student.StudentId).HasMaxLength(64).IsRequired();
            entity.Property(student => student.FullName).HasMaxLength(120).IsRequired();
            entity.Property(student => student.Email).HasMaxLength(256).IsRequired();
            entity.Property(student => student.PhoneNumber).HasMaxLength(30);
            entity.Property(student => student.Batch).HasMaxLength(32);
            entity.Property(student => student.Section).HasMaxLength(16);
            entity.Property(student => student.Department).HasMaxLength(120);
            entity.Property(student => student.ImagePath).HasMaxLength(500);
            entity.HasIndex(student => student.UserId).IsUnique();
            entity.HasIndex(student => student.StudentId).IsUnique();
            entity.HasIndex(student => student.Email).IsUnique();
        });

        modelBuilder.Entity<AttendanceSession>(entity =>
        {
            entity.Property(session => session.Batch).HasMaxLength(32).IsRequired();
            entity.Property(session => session.Section).HasMaxLength(16).IsRequired();
            entity.Property(session => session.Topic).HasMaxLength(200);
            entity.HasIndex(session => new { session.CourseId, session.SessionDate, session.Batch, session.Section })
                .IsUnique();

            entity.HasOne(session => session.Course)
                .WithMany()
                .HasForeignKey(session => session.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(session => session.Teacher)
                .WithMany()
                .HasForeignKey(session => session.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(session => session.Records)
                .WithOne(record => record.Session)
                .HasForeignKey(record => record.AttendanceSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.Property(record => record.Status).IsRequired();
            entity.Property(record => record.Remarks).HasMaxLength(300);
            entity.HasIndex(record => new { record.AttendanceSessionId, record.StudentId })
                .IsUnique();

            entity.HasOne(record => record.Student)
                .WithMany()
                .HasForeignKey(record => record.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TeacherQualification>(entity =>
        {
            entity.Property(q => q.DegreeName).HasMaxLength(160).IsRequired();
            entity.Property(q => q.InstitutionName).HasMaxLength(200).IsRequired();
            entity.HasOne(q => q.Teacher)
                .WithMany(t => t.Qualifications)
                .HasForeignKey(q => q.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CourseBatch>(entity =>
        {
            entity.Property(b => b.BatchName).HasMaxLength(120).IsRequired();
            entity.Property(b => b.BatchCode).HasMaxLength(40).IsRequired();
            entity.Property(b => b.Schedule).HasMaxLength(200);
            entity.HasIndex(b => new { b.CourseId, b.BatchCode }).IsUnique();
            entity.HasOne(b => b.Course)
                .WithMany(c => c.Batches)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BatchTeacher>(entity =>
        {
            entity.HasIndex(bt => new { bt.CourseBatchId, bt.TeacherId }).IsUnique();
            entity.HasOne(bt => bt.Batch)
                .WithMany(b => b.BatchTeachers)
                .HasForeignKey(bt => bt.CourseBatchId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(bt => bt.Teacher)
                .WithMany(t => t.BatchAssignments)
                .HasForeignKey(bt => bt.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CourseWeek>(entity =>
        {
            entity.Property(w => w.Title).HasMaxLength(200).IsRequired();
            entity.Property(w => w.Description).HasMaxLength(2000);
            entity.Property(w => w.LearningObjectives).HasMaxLength(2000);
            entity.HasIndex(w => new { w.CourseId, w.WeekNumber }).IsUnique();
            entity.HasOne(w => w.Course)
                .WithMany(c => c.Weeks)
                .HasForeignKey(w => w.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(w => w.AssignedTeacher)
                .WithMany()
                .HasForeignKey(w => w.AssignedTeacherId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<WeekTopic>(entity =>
        {
            entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(1000);
            entity.HasOne(t => t.Week)
                .WithMany(w => w.Topics)
                .HasForeignKey(t => t.CourseWeekId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WeekMaterial>(entity =>
        {
            entity.Property(m => m.Title).HasMaxLength(200).IsRequired();
            entity.Property(m => m.MaterialType).HasMaxLength(40).IsRequired();
            entity.Property(m => m.FilePath).HasMaxLength(500);
            entity.Property(m => m.ExternalUrl).HasMaxLength(500);
            entity.HasOne(m => m.Week)
                .WithMany(w => w.Materials)
                .HasForeignKey(m => m.CourseWeekId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WeekActivity>(entity =>
        {
            entity.Property(a => a.Title).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Instructions).HasMaxLength(4000);
            entity.Property(a => a.RequiredTasks).HasMaxLength(2000);
            entity.Property(a => a.AttachmentPath).HasMaxLength(500);
            entity.Property(a => a.MaxMarks).HasPrecision(8, 2);
            entity.Property(a => a.PassingMarks).HasPrecision(8, 2);
            entity.HasOne(a => a.Week)
                .WithMany(w => w.Activities)
                .HasForeignKey(a => a.CourseWeekId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.Batch)
                .WithMany()
                .HasForeignKey(a => a.CourseBatchId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(a => a.Teacher)
                .WithMany()
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ActivityQuestion>(entity =>
        {
            entity.Property(q => q.QuestionText).HasMaxLength(1000).IsRequired();
            entity.Property(q => q.OptionA).HasMaxLength(400);
            entity.Property(q => q.OptionB).HasMaxLength(400);
            entity.Property(q => q.OptionC).HasMaxLength(400);
            entity.Property(q => q.OptionD).HasMaxLength(400);
            entity.Property(q => q.CorrectOption).HasMaxLength(200);
            entity.Property(q => q.Marks).HasPrecision(8, 2);
            entity.HasOne(q => q.Activity)
                .WithMany(a => a.Questions)
                .HasForeignKey(q => q.WeekActivityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActivitySubmission>(entity =>
        {
            entity.Property(s => s.AttachmentPath).HasMaxLength(500);
            entity.Property(s => s.Feedback).HasMaxLength(2000);
            entity.Property(s => s.ObtainedMarks).HasPrecision(8, 2);
            entity.HasIndex(s => new { s.WeekActivityId, s.StudentId }).IsUnique();
            entity.HasOne(s => s.Activity)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.WeekActivityId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ForumPost>(entity =>
        {
            entity.Property(p => p.AuthorName).HasMaxLength(120).IsRequired();
            entity.Property(p => p.AuthorRole).HasMaxLength(40).IsRequired();
            entity.Property(p => p.Content).HasMaxLength(2000).IsRequired();
            entity.HasOne(p => p.Activity)
                .WithMany(a => a.ForumPosts)
                .HasForeignKey(p => p.WeekActivityId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(p => p.ParentPost)
                .WithMany()
                .HasForeignKey(p => p.ParentPostId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(token => token.ReplacedByTokenHash).HasMaxLength(128);
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.Ignore(token => token.User);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasOne(s => s.DepartmentEntity)
                .WithMany()
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(s => s.MasterBatch)
                .WithMany()
                .HasForeignKey(s => s.MasterBatchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CourseBatch>(entity =>
        {
            entity.HasOne(cb => cb.MasterBatch)
                .WithMany()
                .HasForeignKey(cb => cb.MasterBatchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CourseBatchSection>(entity =>
        {
            entity.HasIndex(x => new { x.CourseBatchId, x.MasterSectionId }).IsUnique();
            entity.HasOne(x => x.CourseBatch)
                .WithMany(cb => cb.BatchSections)
                .HasForeignKey(x => x.CourseBatchId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.MasterSection)
                .WithMany()
                .HasForeignKey(x => x.MasterSectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Computer Science & Engineering", Code = "CSE", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 2, Name = "Business Administration", Code = "BBA", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 3, Name = "Design & Media", Code = "DES", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<MasterBatch>().HasData(
            new MasterBatch { Id = 1, Name = "Batch 70", Code = "B70", Sections = "A,B,C,D", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new MasterBatch { Id = 2, Name = "Batch 71", Code = "B71", Sections = "A,B,C,D", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<MasterSection>().HasData(
            new MasterSection { Id = 1, Name = "A", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new MasterSection { Id = 2, Name = "B", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new MasterSection { Id = 3, Name = "C", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new MasterSection { Id = 4, Name = "D", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Semester>().HasData(
            new Semester { Id = 1, Name = "1st Semester", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Semester { Id = 2, Name = "2nd Semester", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Semester { Id = 3, Name = "3rd Semester", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Semester { Id = 4, Name = "4th Semester", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Semester { Id = 5, Name = "5th Semester", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Semester { Id = 6, Name = "6th Semester", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Semester { Id = 7, Name = "7th Semester", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Semester { Id = 8, Name = "8th Semester", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Semester { Id = 9, Name = "9th Semester", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Semester { Id = 10, Name = "10th Semester", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
