using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeForService21DTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DonorEvents_19D",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FeeForServices_21D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    StrategyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalNumberOfWorkshops = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkshopFormat = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    WorkshopLocation = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    WorkshopDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EventPartners = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NumberOfAttendees = table.Column<int>(type: "INTEGER", nullable: false),
                    ParticipantSatisfactionRating = table.Column<decimal>(type: "TEXT", nullable: false),
                    PartnerSatisfactionRating = table.Column<decimal>(type: "TEXT", nullable: false),
                    RevenueReceived = table.Column<decimal>(type: "TEXT", nullable: false),
                    Quarter = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeForServices_21D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeForServices_21D_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeeForServices_21D_StrategyId",
                table: "FeeForServices_21D",
                column: "StrategyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeeForServices_21D");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DonorEvents_19D");
        }
    }
}
