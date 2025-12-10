using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class MakeStrategyTemplateIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Strategies_StrategyTemplateId",
                table: "Events");

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "Strategies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "StrategyTemplateId",
                table: "Events",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Strategies_StrategyTemplateId",
                table: "Events",
                column: "StrategyTemplateId",
                principalTable: "Strategies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Strategies_StrategyTemplateId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "Strategies");

            migrationBuilder.AlterColumn<int>(
                name: "StrategyTemplateId",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Strategies_StrategyTemplateId",
                table: "Events",
                column: "StrategyTemplateId",
                principalTable: "Strategies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
