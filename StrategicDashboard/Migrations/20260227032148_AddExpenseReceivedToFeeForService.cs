using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseReceivedToFeeForService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExpenseReceived",
                table: "FeeForServices_21D",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpenseReceived",
                table: "FeeForServices_21D");
        }
    }
}
