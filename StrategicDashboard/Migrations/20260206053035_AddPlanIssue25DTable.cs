using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanIssue25DTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "planIssue_25D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    IssueName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CrisisDescription = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCompliant = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planIssue_25D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planIssue_25D_Plan2026_24D_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plan2026_24D",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_planIssue_25D_PlanId",
                table: "planIssue_25D",
                column: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "planIssue_25D");
        }
    }
}
