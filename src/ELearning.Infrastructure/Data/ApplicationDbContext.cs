using ELearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(course => course.Title).HasMaxLength(160).IsRequired();
            entity.Property(course => course.Description).HasMaxLength(2000).IsRequired();
            entity.Property(course => course.Instructor).HasMaxLength(120).IsRequired();
            entity.Property(course => course.Category).HasMaxLength(80).IsRequired();
            entity.HasMany(course => course.Lessons)
                .WithOne(lesson => lesson.Course)
                .HasForeignKey(lesson => lesson.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.Property(lesson => lesson.Title).HasMaxLength(160).IsRequired();
            entity.Property(lesson => lesson.Summary).HasMaxLength(1000).IsRequired();
            entity.HasIndex(lesson => new { lesson.CourseId, lesson.Order }).IsUnique();
        });
    }
}
