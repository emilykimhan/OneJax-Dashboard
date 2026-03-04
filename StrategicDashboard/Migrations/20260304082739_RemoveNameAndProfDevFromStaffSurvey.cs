using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNameAndProfDevFromStaffSurvey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "StaffSurveys_22D");

            migrationBuilder.DropColumn(
                name: "ProfessionalDevelopmentCount",
                table: "StaffSurveys_22D");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "StaffSurveys_22D",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProfessionalDevelopmentCount",
                table: "StaffSurveys_22D",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
