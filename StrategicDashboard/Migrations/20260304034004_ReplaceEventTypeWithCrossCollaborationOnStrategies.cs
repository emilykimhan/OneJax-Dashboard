using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEventTypeWithCrossCollaborationOnStrategies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "Strategies");

            migrationBuilder.AddColumn<string>(
                name: "CrossCollaboration",
                table: "Strategies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CrossCollaboration",
                table: "Strategies");

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "Strategies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
