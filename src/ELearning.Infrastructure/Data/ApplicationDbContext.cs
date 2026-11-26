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

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(course => course.Title).HasMaxLength(160).IsRequired();
            entity.Property(course => course.Description).HasMaxLength(2000).IsRequired();
            entity.Property(course => course.Instructor).HasMaxLength(120).IsRequired();
            entity.Property(course => course.Category).HasMaxLength(80).IsRequired();
            entity.Property(course => course.ImagePath).HasMaxLength(500);
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

            entity.HasOne(enrollment => enrollment.Student)
                .WithMany()
                .HasForeignKey(enrollment => enrollment.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
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

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(token => token.ReplacedByTokenHash).HasMaxLength(128);
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.Ignore(token => token.User);
        });
    }
}
