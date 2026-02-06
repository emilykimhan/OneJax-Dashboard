using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddPlan202624DTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plan2026_24D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Quarter = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    FrameworkStatus = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    GoalMet = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plan2026_24D", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plan2026_24D");
        }
    }
}
