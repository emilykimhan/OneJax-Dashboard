using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddStrategyIdToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StrategyId",
                table: "Events",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_StrategyId",
                table: "Events",
                column: "StrategyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Strategies_StrategyId",
                table: "Events",
                column: "StrategyId",
                principalTable: "Strategies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Strategies_StrategyId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_StrategyId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StrategyId",
                table: "Events");
        }
    }
}
