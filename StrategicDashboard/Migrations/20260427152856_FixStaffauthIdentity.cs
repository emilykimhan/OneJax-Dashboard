using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class FixStaffauthIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Strategies_StrategyId",
                table: "Events");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Strategies_StrategyId",
                table: "Events",
                column: "StrategyId",
                principalTable: "Strategies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
