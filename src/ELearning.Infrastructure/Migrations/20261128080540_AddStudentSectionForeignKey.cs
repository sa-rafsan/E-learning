using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentSectionForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MasterSectionId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_MasterSectionId",
                table: "Students",
                column: "MasterSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_MasterSections_MasterSectionId",
                table: "Students",
                column: "MasterSectionId",
                principalTable: "MasterSections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_MasterSections_MasterSectionId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_MasterSectionId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MasterSectionId",
                table: "Students");
        }
    }
}
