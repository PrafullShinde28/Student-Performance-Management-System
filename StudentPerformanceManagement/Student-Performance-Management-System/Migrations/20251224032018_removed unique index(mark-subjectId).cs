using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Student_Performance_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class removeduniqueindexmarksubjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Marks_SubjectId",
                table: "Marks");

            migrationBuilder.CreateIndex(
                name: "IX_Marks_SubjectId",
                table: "Marks",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Marks_SubjectId",
                table: "Marks");

            migrationBuilder.CreateIndex(
                name: "IX_Marks_SubjectId",
                table: "Marks",
                column: "SubjectId",
                unique: true);
        }
    }
}
