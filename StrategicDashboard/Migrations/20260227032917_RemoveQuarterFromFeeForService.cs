using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuarterFromFeeForService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quarter",
                table: "FeeForServices_21D");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Quarter",
                table: "FeeForServices_21D",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
