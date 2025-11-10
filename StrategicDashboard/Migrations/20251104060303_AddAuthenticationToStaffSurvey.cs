using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationToStaffSurvey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StaffSurveys_22D",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "StaffSurveys_22D",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "StaffSurveys_22D",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffSurveys_22D_Username",
                table: "StaffSurveys_22D",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StaffSurveys_22D_Username",
                table: "StaffSurveys_22D");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "StaffSurveys_22D");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "StaffSurveys_22D");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "StaffSurveys_22D");
        }
    }
}
