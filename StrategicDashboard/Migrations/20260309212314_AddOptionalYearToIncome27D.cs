using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionalYearToIncome27D : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "income_27D",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Year",
                table: "income_27D");
        }
    }
}
