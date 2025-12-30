using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Student_Performance_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class oneSubjectToManyMarksAndSubjectandStudentUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Marks_SubjectId_StudentId",
                table: "Marks",
                columns: new[] { "SubjectId", "StudentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Marks_SubjectId_StudentId",
                table: "Marks");
        }
    }
}
