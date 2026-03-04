using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueFieldsToPlan2026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CrisisDescription",
                table: "Plan2026_24D",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IssueHandled",
                table: "Plan2026_24D",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IssueName",
                table: "Plan2026_24D",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CrisisDescription",
                table: "Plan2026_24D");

            migrationBuilder.DropColumn(
                name: "IssueHandled",
                table: "Plan2026_24D");

            migrationBuilder.DropColumn(
                name: "IssueName",
                table: "Plan2026_24D");
        }
    }
}
