using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddMiscellaneousFieldsToBudgetTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MiscellaneousExpenses",
                table: "BudgetTracking_28D",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MiscellaneousRevenue",
                table: "BudgetTracking_28D",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MiscellaneousExpenses",
                table: "BudgetTracking_28D");

            migrationBuilder.DropColumn(
                name: "MiscellaneousRevenue",
                table: "BudgetTracking_28D");
        }
    }
}
