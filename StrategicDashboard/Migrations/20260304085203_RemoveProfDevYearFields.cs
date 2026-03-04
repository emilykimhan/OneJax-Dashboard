using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProfDevYearFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfessionalDevelopmentYear26",
                table: "ProfessionalDevelopments");

            migrationBuilder.DropColumn(
                name: "ProfessionalDevelopmentYear27",
                table: "ProfessionalDevelopments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfessionalDevelopmentYear26",
                table: "ProfessionalDevelopments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProfessionalDevelopmentYear27",
                table: "ProfessionalDevelopments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
