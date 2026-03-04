using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddYouthAttend15D : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YouthAttend_15D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StrategyId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumberOfYouthAttendees = table.Column<int>(type: "INTEGER", nullable: false),
                    PostEventSurveySatisfaction = table.Column<decimal>(type: "TEXT", nullable: false),
                    AveragePreAssessment = table.Column<decimal>(type: "TEXT", nullable: false),
                    AveragePostAssessment = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouthAttend_15D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YouthAttend_15D_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YouthAttend_15D_StrategyId",
                table: "YouthAttend_15D",
                column: "StrategyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YouthAttend_15D");
        }
    }
}
