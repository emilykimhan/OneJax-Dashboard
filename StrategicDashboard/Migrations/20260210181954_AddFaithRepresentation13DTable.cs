using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddFaithRepresentation13DTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaithRepresentations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StrategyId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventName = table.Column<string>(type: "TEXT", nullable: false),
                    NumberOfFaiths = table.Column<int>(type: "INTEGER", nullable: false),
                    FaithNames = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaithRepresentations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaithRepresentations_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaithRepresentations_StrategyId",
                table: "FaithRepresentations",
                column: "StrategyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaithRepresentations");
        }
    }
}
