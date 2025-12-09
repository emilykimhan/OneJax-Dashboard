using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class FixEventModelConflict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "Events",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StrategyTemplateId",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

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
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Strategies_StrategyTemplateId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_StrategyTemplateId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StrategyTemplateId",
                table: "Events");
        }
    }
}
