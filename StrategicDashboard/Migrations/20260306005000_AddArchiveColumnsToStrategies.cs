using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OneJaxDashboard.Data;

#nullable disable

namespace StrategicDashboard.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260306005000_AddArchiveColumnsToStrategies")]
    /// <inheritdoc />
    public partial class AddArchiveColumnsToStrategies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAtUtc",
                table: "Strategies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Strategies",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAtUtc",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Strategies");
        }
    }
}
