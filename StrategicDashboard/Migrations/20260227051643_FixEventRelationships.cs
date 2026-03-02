using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class FixEventRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_StrategicGoals_StrategicGoalId1",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "StrategicGoalId1",
                table: "Events",
                newName: "AssignedStaffId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_StrategicGoalId1",
                table: "Events",
                newName: "IX_Events_AssignedStaffId");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Staffauth",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Staffauth_AssignedStaffId",
                table: "Events",
                column: "AssignedStaffId",
                principalTable: "Staffauth",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Staffauth_AssignedStaffId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "AssignedStaffId",
                table: "Events",
                newName: "StrategicGoalId1");

            migrationBuilder.RenameIndex(
                name: "IX_Events_AssignedStaffId",
                table: "Events",
                newName: "IX_Events_StrategicGoalId1");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Staffauth",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_StrategicGoals_StrategicGoalId1",
                table: "Events",
                column: "StrategicGoalId1",
                principalTable: "StrategicGoals",
                principalColumn: "Id");
        }
    }
}
