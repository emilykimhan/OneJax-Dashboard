using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStrategyTemplateIdFromEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Strategies_StrategyTemplateId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_StrategyTemplateId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StrategyTemplateId",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StrategyTemplateId",
                table: "Events",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_StrategyTemplateId",
                table: "Events",
                column: "StrategyTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Strategies_StrategyTemplateId",
                table: "Events",
                column: "StrategyTemplateId",
                principalTable: "Strategies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
