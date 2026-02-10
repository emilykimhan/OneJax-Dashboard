using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetTracking28DTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetTracking_28D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Quarter = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    CommunityPrograms = table.Column<decimal>(type: "TEXT", nullable: true),
                    OneYouthPrograms = table.Column<decimal>(type: "TEXT", nullable: true),
                    InterfaithPrograms = table.Column<decimal>(type: "TEXT", nullable: true),
                    HumanitarianEvent = table.Column<decimal>(type: "TEXT", nullable: true),
                    CorporateGiving = table.Column<decimal>(type: "TEXT", nullable: true),
                    IndividualGiving = table.Column<decimal>(type: "TEXT", nullable: true),
                    GrantsFoundations = table.Column<decimal>(type: "TEXT", nullable: true),
                    CommunityEvents = table.Column<decimal>(type: "TEXT", nullable: true),
                    PeopleCultureWorkshops = table.Column<decimal>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetTracking_28D", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetTracking_28D");
        }
    }
}
