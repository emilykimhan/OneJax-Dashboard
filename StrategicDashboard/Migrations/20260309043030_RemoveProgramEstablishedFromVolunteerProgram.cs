using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProgramEstablishedFromVolunteerProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgramEstablished",
                table: "volunteerProgram_40D");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ProgramEstablished",
                table: "volunteerProgram_40D",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
