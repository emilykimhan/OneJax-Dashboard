using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTotalNumberOfWorkshopsFromFeeForService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalNumberOfWorkshops",
                table: "FeeForServices_21D");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalNumberOfWorkshops",
                table: "FeeForServices_21D",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
