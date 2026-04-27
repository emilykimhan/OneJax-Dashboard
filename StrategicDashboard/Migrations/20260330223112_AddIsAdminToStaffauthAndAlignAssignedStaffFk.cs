using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAdminToStaffauthAndAlignAssignedStaffFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql("""
                    IF OBJECT_ID(N'[dbo].[FK_Events_Staffauth_OwnerUsername]', N'F') IS NOT NULL
                    BEGIN
                        ALTER TABLE [Events] DROP CONSTRAINT [FK_Events_Staffauth_OwnerUsername];
                    END
                    """);
            }

            if (ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql("""
                    IF COL_LENGTH('Staffauth', 'IsAdmin') IS NULL
                    BEGIN
                        ALTER TABLE [Staffauth]
                        ADD [IsAdmin] bit NOT NULL CONSTRAINT [DF_Staffauth_IsAdmin] DEFAULT(0);
                    END
                    """);
            }
            else
            {
                migrationBuilder.AddColumn<bool>(
                    name: "IsAdmin",
                    table: "Staffauth",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: false);
            }

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Staffauth_OwnerUsername",
                table: "Events",
                column: "OwnerUsername",
                principalTable: "Staffauth",
                principalColumn: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql("""
                    IF OBJECT_ID(N'[dbo].[FK_Events_Staffauth_OwnerUsername]', N'F') IS NOT NULL
                    BEGIN
                        ALTER TABLE [Events] DROP CONSTRAINT [FK_Events_Staffauth_OwnerUsername];
                    END
                    """);
            }
            else
            {
                migrationBuilder.DropForeignKey(
                    name: "FK_Events_Staffauth_OwnerUsername",
                    table: "Events");
            }

            if (ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql("""
                    IF COL_LENGTH('Staffauth', 'IsAdmin') IS NOT NULL
                    BEGIN
                        DECLARE @constraintName sysname;

                        SELECT @constraintName = dc.name
                        FROM sys.default_constraints dc
                        INNER JOIN sys.columns c
                            ON c.default_object_id = dc.object_id
                        WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Staffauth]')
                            AND c.name = N'IsAdmin';

                        IF @constraintName IS NOT NULL
                        BEGIN
                            EXEC('ALTER TABLE [Staffauth] DROP CONSTRAINT [' + @constraintName + ']');
                        END

                        ALTER TABLE [Staffauth] DROP COLUMN [IsAdmin];
                    END
                    """);
            }
            else
            {
                migrationBuilder.DropColumn(
                    name: "IsAdmin",
                    table: "Staffauth");
            }

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Staffauth_OwnerUsername",
                table: "Events",
                column: "OwnerUsername",
                principalTable: "Staffauth",
                principalColumn: "Username",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
