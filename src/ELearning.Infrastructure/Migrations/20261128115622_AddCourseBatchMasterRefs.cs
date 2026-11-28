using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseBatchMasterRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MasterBatchId",
                table: "CourseBatches",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseBatchSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseBatchId = table.Column<int>(type: "int", nullable: false),
                    MasterSectionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseBatchSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseBatchSections_CourseBatches_CourseBatchId",
                        column: x => x.CourseBatchId,
                        principalTable: "CourseBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseBatchSections_MasterSections_MasterSectionId",
                        column: x => x.MasterSectionId,
                        principalTable: "MasterSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseBatches_MasterBatchId",
                table: "CourseBatches",
                column: "MasterBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseBatchSections_CourseBatchId_MasterSectionId",
                table: "CourseBatchSections",
                columns: new[] { "CourseBatchId", "MasterSectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseBatchSections_MasterSectionId",
                table: "CourseBatchSections",
                column: "MasterSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseBatches_MasterBatches_MasterBatchId",
                table: "CourseBatches",
                column: "MasterBatchId",
                principalTable: "MasterBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseBatches_MasterBatches_MasterBatchId",
                table: "CourseBatches");

            migrationBuilder.DropTable(
                name: "CourseBatchSections");

            migrationBuilder.DropIndex(
                name: "IX_CourseBatches_MasterBatchId",
                table: "CourseBatches");

            migrationBuilder.DropColumn(
                name: "MasterBatchId",
                table: "CourseBatches");
        }
    }
}
