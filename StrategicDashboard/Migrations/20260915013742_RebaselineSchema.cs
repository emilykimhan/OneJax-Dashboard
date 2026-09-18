using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class RebaselineSchema : Migration
    {
        // This migration is intentionally a no-op.
        //
        // The migration history had drifted from the real schema over time: several
        // schema changes (including the BudgetTracking_28D column renames and the
        // CrossColab ContactName column) were applied directly to the databases
        // outside of EF migrations, and a couple of tables were left behind after
        // their features were removed from the code. Both Azure SQL (production) and
        // the local SQLite dev database were confirmed (by direct inspection) to
        // already match the current model exactly as of 2026-09-15, so there is
        // nothing left to actually apply here. This migration exists purely to
        // realign the recorded migration history and the model snapshot with that
        // already-true state, so future `dotnet ef migrations add` runs produce
        // accurate, minimal diffs instead of hundreds of phantom operations.
        //
        // The handful of genuinely-dead orphaned tables (CollabTouch_47D,
        // Plan2026_24D, volunteerProgram_40D) are cleaned up separately via the
        // idempotent startup steps in Program.cs, matching this codebase's existing
        // convention for schema changes made outside of migrations.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
