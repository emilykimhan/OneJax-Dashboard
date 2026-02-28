using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class MakeUsernameRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_StrategicGoals_StrategicGoalId1",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Staffauth_Username",
                table: "Staffauth");

            migrationBuilder.DropIndex(
                name: "IX_Events_StrategicGoalId1",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StrategicGoalId1",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_Staffauth_Username",
                table: "Staffauth",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Staffauth_Username",
                table: "Staffauth");

            migrationBuilder.AddColumn<int>(
                name: "StrategicGoalId1",
                table: "Events",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffauth_Username",
                table: "Staffauth",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StrategicGoalId1",
                table: "Events",
                column: "StrategicGoalId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_StrategicGoals_StrategicGoalId1",
                table: "Events",
                column: "StrategicGoalId1",
                principalTable: "StrategicGoals",
                principalColumn: "Id");
        }
    }
}
