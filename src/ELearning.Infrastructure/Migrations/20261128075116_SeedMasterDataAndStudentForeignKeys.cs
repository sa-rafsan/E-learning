using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedMasterDataAndStudentForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MasterBatchId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "Name" },
                values: new object[,]
                {
                    { 1, "CSE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Computer Science & Engineering" },
                    { 2, "BBA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Business Administration" },
                    { 3, "DES", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Design & Media" }
                });

            migrationBuilder.InsertData(
                table: "MasterBatches",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "Name", "Sections" },
                values: new object[,]
                {
                    { 1, "B70", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Batch 70", "A,B,C,D" },
                    { 2, "B71", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Batch 71", "A,B,C,D" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_DepartmentId",
                table: "Students",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_MasterBatchId",
                table: "Students",
                column: "MasterBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_MasterBatches_MasterBatchId",
                table: "Students",
                column: "MasterBatchId",
                principalTable: "MasterBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_MasterBatches_MasterBatchId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_DepartmentId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_MasterBatchId",
                table: "Students");

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MasterBatches",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MasterBatches",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MasterBatchId",
                table: "Students");
        }
    }
}
