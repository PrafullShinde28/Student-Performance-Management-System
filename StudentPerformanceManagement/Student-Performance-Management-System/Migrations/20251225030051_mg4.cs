using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Student_Performance_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class mg4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Staffs_StaffId1",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_StaffId1",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "StaffId1",
                table: "Tasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StaffId1",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StaffId1",
                table: "Tasks",
                column: "StaffId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Staffs_StaffId1",
                table: "Tasks",
                column: "StaffId1",
                principalTable: "Staffs",
                principalColumn: "StaffId");
        }
    }
}
