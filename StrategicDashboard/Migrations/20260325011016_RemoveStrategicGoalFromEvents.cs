using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStrategicGoalFromEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_StrategicGoals_StrategicGoalId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_StrategicGoalId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StrategicGoalId",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StrategicGoalId",
                table: "Events",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_StrategicGoalId",
                table: "Events",
                column: "StrategicGoalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_StrategicGoals_StrategicGoalId",
                table: "Events",
                column: "StrategicGoalId",
                principalTable: "StrategicGoals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
