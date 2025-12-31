using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Student_Performance_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class uniqueTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_SubjectId",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SubjectId_CourseGroupId",
                table: "Tasks",
                columns: new[] { "SubjectId", "CourseGroupId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_SubjectId_CourseGroupId",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SubjectId",
                table: "Tasks",
                column: "SubjectId");
        }
    }
}
