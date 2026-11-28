using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLmsBatchesWeeksActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseEnrollments_Students_StudentId",
                table: "CourseEnrollments");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfessionalExperience",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortBiography",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Students",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationalBackground",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourseCode",
                table: "Courses",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurationWeeks",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 12);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CourseBatchId",
                table: "CourseEnrollments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    BatchName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    BatchCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Schedule = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseBatches_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseWeeks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LearningObjectives = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AssignedTeacherId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseWeeks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseWeeks_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseWeeks_Teachers_AssignedTeacherId",
                        column: x => x.AssignedTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeacherQualifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    DegreeName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PassingYear = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherQualifications_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BatchTeachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseBatchId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchTeachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchTeachers_CourseBatches_CourseBatchId",
                        column: x => x.CourseBatchId,
                        principalTable: "CourseBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatchTeachers_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WeekActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseWeekId = table.Column<int>(type: "int", nullable: false),
                    CourseBatchId = table.Column<int>(type: "int", nullable: true),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RequiredTasks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AttachmentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxMarks = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    PassingMarks = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    StartAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionDeadlineUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeLimitMinutes = table.Column<int>(type: "int", nullable: true),
                    AllowLateSubmission = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeekActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeekActivities_CourseBatches_CourseBatchId",
                        column: x => x.CourseBatchId,
                        principalTable: "CourseBatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WeekActivities_CourseWeeks_CourseWeekId",
                        column: x => x.CourseWeekId,
                        principalTable: "CourseWeeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeekActivities_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WeekMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseWeekId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeekMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeekMaterials_CourseWeeks_CourseWeekId",
                        column: x => x.CourseWeekId,
                        principalTable: "CourseWeeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeekTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseWeekId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeekTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeekTopics_CourseWeeks_CourseWeekId",
                        column: x => x.CourseWeekId,
                        principalTable: "CourseWeeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekActivityId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    OptionA = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    OptionB = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    OptionC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    OptionD = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CorrectOption = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Marks = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityQuestions_WeekActivities_WeekActivityId",
                        column: x => x.WeekActivityId,
                        principalTable: "WeekActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivitySubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekActivityId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SubmissionText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AnswersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObtainedMarks = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsAutoEvaluated = table.Column<bool>(type: "bit", nullable: false),
                    IsEvaluated = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EvaluatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitySubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivitySubmissions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ActivitySubmissions_WeekActivities_WeekActivityId",
                        column: x => x.WeekActivityId,
                        principalTable: "WeekActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForumPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekActivityId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorRole = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ParentPostId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumPosts_ForumPosts_ParentPostId",
                        column: x => x.ParentPostId,
                        principalTable: "ForumPosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ForumPosts_WeekActivities_WeekActivityId",
                        column: x => x.WeekActivityId,
                        principalTable: "WeekActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CourseBatchId",
                table: "CourseEnrollments",
                column: "CourseBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityQuestions_WeekActivityId",
                table: "ActivityQuestions",
                column: "WeekActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySubmissions_StudentId",
                table: "ActivitySubmissions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySubmissions_WeekActivityId_StudentId",
                table: "ActivitySubmissions",
                columns: new[] { "WeekActivityId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatchTeachers_CourseBatchId_TeacherId",
                table: "BatchTeachers",
                columns: new[] { "CourseBatchId", "TeacherId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatchTeachers_TeacherId",
                table: "BatchTeachers",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseBatches_CourseId_BatchCode",
                table: "CourseBatches",
                columns: new[] { "CourseId", "BatchCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseWeeks_AssignedTeacherId",
                table: "CourseWeeks",
                column: "AssignedTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseWeeks_CourseId_WeekNumber",
                table: "CourseWeeks",
                columns: new[] { "CourseId", "WeekNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumPosts_ParentPostId",
                table: "ForumPosts",
                column: "ParentPostId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumPosts_WeekActivityId",
                table: "ForumPosts",
                column: "WeekActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherQualifications_TeacherId",
                table: "TeacherQualifications",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_WeekActivities_CourseBatchId",
                table: "WeekActivities",
                column: "CourseBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_WeekActivities_CourseWeekId",
                table: "WeekActivities",
                column: "CourseWeekId");

            migrationBuilder.CreateIndex(
                name: "IX_WeekActivities_TeacherId",
                table: "WeekActivities",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_WeekMaterials_CourseWeekId",
                table: "WeekMaterials",
                column: "CourseWeekId");

            migrationBuilder.CreateIndex(
                name: "IX_WeekTopics_CourseWeekId",
                table: "WeekTopics",
                column: "CourseWeekId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEnrollments_CourseBatches_CourseBatchId",
                table: "CourseEnrollments",
                column: "CourseBatchId",
                principalTable: "CourseBatches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEnrollments_Students_StudentId",
                table: "CourseEnrollments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseEnrollments_CourseBatches_CourseBatchId",
                table: "CourseEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseEnrollments_Students_StudentId",
                table: "CourseEnrollments");

            migrationBuilder.DropTable(
                name: "ActivityQuestions");

            migrationBuilder.DropTable(
                name: "ActivitySubmissions");

            migrationBuilder.DropTable(
                name: "BatchTeachers");

            migrationBuilder.DropTable(
                name: "ForumPosts");

            migrationBuilder.DropTable(
                name: "TeacherQualifications");

            migrationBuilder.DropTable(
                name: "WeekMaterials");

            migrationBuilder.DropTable(
                name: "WeekTopics");

            migrationBuilder.DropTable(
                name: "WeekActivities");

            migrationBuilder.DropTable(
                name: "CourseBatches");

            migrationBuilder.DropTable(
                name: "CourseWeeks");

            migrationBuilder.DropIndex(
                name: "IX_CourseEnrollments_CourseBatchId",
                table: "CourseEnrollments");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "ProfessionalExperience",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "ShortBiography",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EducationalBackground",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CourseCode",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DurationWeeks",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CourseBatchId",
                table: "CourseEnrollments");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEnrollments_Students_StudentId",
                table: "CourseEnrollments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
