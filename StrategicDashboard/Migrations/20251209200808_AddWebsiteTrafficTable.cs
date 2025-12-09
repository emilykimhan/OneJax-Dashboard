using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteTrafficTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebsiteTraffic",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Q1_JulySeptember = table.Column<int>(type: "INTEGER", nullable: true),
                    Q2_OctoberDecember = table.Column<int>(type: "INTEGER", nullable: true),
                    Q3_JanuaryMarch = table.Column<int>(type: "INTEGER", nullable: true),
                    Q4_AprilJune = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteTraffic", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebsiteTraffic");
        }
    }
}
