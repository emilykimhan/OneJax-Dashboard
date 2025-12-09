using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaPlacementsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaPlacements_3D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    January = table.Column<int>(type: "INTEGER", nullable: true),
                    February = table.Column<int>(type: "INTEGER", nullable: true),
                    March = table.Column<int>(type: "INTEGER", nullable: true),
                    April = table.Column<int>(type: "INTEGER", nullable: true),
                    May = table.Column<int>(type: "INTEGER", nullable: true),
                    June = table.Column<int>(type: "INTEGER", nullable: true),
                    July = table.Column<int>(type: "INTEGER", nullable: true),
                    August = table.Column<int>(type: "INTEGER", nullable: true),
                    September = table.Column<int>(type: "INTEGER", nullable: true),
                    October = table.Column<int>(type: "INTEGER", nullable: true),
                    November = table.Column<int>(type: "INTEGER", nullable: true),
                    December = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaPlacements_3D", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaPlacements_3D");
        }
    }
}
