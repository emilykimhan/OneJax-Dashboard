using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStrategyIdFromEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Events
                SET StrategyTemplateId = StrategyId
                WHERE StrategyTemplateId IS NULL AND StrategyId IS NOT NULL;
                """);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StrategyId",
                table: "Events",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Events
                SET StrategyId = StrategyTemplateId
                WHERE StrategyId IS NULL AND StrategyTemplateId IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Events_StrategyId",
                table: "Events",
                column: "StrategyId");

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
