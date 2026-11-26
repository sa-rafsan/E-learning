using ELearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Data;

public static class CourseDataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        if (await dbContext.Courses.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.Courses.AddRange(
            new Course
            {
                Title = "Web Development Foundations",
                Description = "Build a strong foundation in the web: how browsers work, how pages are structured, and how to bring ideas to life with code.",
                Instructor = "Ayesha Rahman",
                Category = "Development",
                DurationHours = 8,
                IsPublished = true,
                Lessons =
                [
                    new Lesson { Title = "How the web works", Summary = "Explore browsers, servers, and the request-response cycle.", DurationMinutes = 18, Order = 1 },
                    new Lesson { Title = "Your first page", Summary = "Structure a clear, accessible page with HTML.", DurationMinutes = 24, Order = 2 },
                    new Lesson { Title = "Style with confidence", Summary = "Use CSS foundations to make a page feel like your own.", DurationMinutes = 30, Order = 3 }
                ]
            },
            new Course
            {
                Title = "Product Design Essentials",
                Description = "Learn a practical design process, from understanding people and mapping a problem to creating and testing a useful solution.",
                Instructor = "Marcus Chen",
                Category = "Design",
                DurationHours = 6,
                IsPublished = true,
                Lessons =
                [
                    new Lesson { Title = "Start with people", Summary = "Turn observations and interviews into useful design insights.", DurationMinutes = 20, Order = 1 },
                    new Lesson { Title = "Map the experience", Summary = "Make journeys and flows that keep the whole experience in view.", DurationMinutes = 26, Order = 2 },
                    new Lesson { Title = "Prototype and learn", Summary = "Test an early idea and use feedback to improve it.", DurationMinutes = 28, Order = 3 }
                ]
            },
            new Course
            {
                Title = "Data Literacy for Everyone",
                Description = "Read charts with a critical eye, ask better questions of data, and share findings in a way that supports good decisions.",
                Instructor = "Nadia Williams",
                Category = "Data",
                DurationHours = 5,
                IsPublished = true,
                Lessons =
                [
                    new Lesson { Title = "Ask a clear question", Summary = "Define the decision and the evidence you need.", DurationMinutes = 17, Order = 1 },
                    new Lesson { Title = "Read the numbers", Summary = "Understand common measures, charts, and their limitations.", DurationMinutes = 23, Order = 2 },
                    new Lesson { Title = "Tell the story", Summary = "Share a concise, responsible data narrative.", DurationMinutes = 21, Order = 3 }
                ]
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
