using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    public partial class FixStaffauthIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("EXEC sp_rename 'Staffauth', 'Staffauth_backup'");

            migrationBuilder.Sql(@"
                CREATE TABLE Staffauth (
                    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    Name NVARCHAR(MAX) NOT NULL DEFAULT(''),
                    Username NVARCHAR(256) NOT NULL DEFAULT(''),
                    Password NVARCHAR(MAX) NULL,
                    Email NVARCHAR(MAX) NOT NULL DEFAULT(''),
                    IsAdmin BIT NOT NULL DEFAULT(0)
                )");

            migrationBuilder.Sql(@"
                INSERT INTO Staffauth (Name, Username, Password, Email, IsAdmin)
                SELECT Name, Username, Password, Email, IsAdmin
                FROM Staffauth_backup");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("EXEC sp_rename 'Staffauth_backup', 'Staffauth'");
            migrationBuilder.Sql("DROP TABLE Staffauth");
        }
    }
}