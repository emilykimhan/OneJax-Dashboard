using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddCollabTouch47D : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollabTouch_47D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FiscalYear = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PartnerOrganization = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Contact = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    StrategyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Touchpoint = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollabTouch_47D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollabTouch_47D_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollabTouch_47D_StrategyId",
                table: "CollabTouch_47D",
                column: "StrategyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollabTouch_47D");
        }
    }
}
