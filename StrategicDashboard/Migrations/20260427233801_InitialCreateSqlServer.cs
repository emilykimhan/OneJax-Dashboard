using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrategicDashboard.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "achieveMile_6D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    achievedReview = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_achieveMile_6D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    User = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Entity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Annual_average_7D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalRespondents = table.Column<int>(type: "int", nullable: true),
                    RespondentsIdentifyingAsTrusted = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annual_average_7D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchivedPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalProgramId = table.Column<int>(type: "int", nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArchivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedPrograms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoardMeetingAttendance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MembersInAttendance = table.Column<int>(type: "int", nullable: false),
                    TotalBoardMembers = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardMeetingAttendance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoardMember_29D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberNames = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Quarter = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    NumberRecruited = table.Column<int>(type: "int", nullable: true),
                    ProspectNames = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TotalProspectOutreach = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardMember_29D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BudgetTracking_28D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quarter = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    CommunityPrograms = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OneYouthPrograms = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InterfaithPrograms = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HumanitarianEvent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MiscellaneousExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CorporateGiving = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IndividualGiving = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GrantsFoundations = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CommunityEvents = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PeopleCultureWorkshops = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MiscellaneousRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetTracking_28D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationRate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    AverageCommunicationSatisfaction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactsInterfaith_14D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: true),
                    TotalInterfaithContacts = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactsInterfaith_14D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "income_27D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncomeSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_income_27D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaPlacements_3D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    January = table.Column<int>(type: "int", nullable: true),
                    February = table.Column<int>(type: "int", nullable: true),
                    March = table.Column<int>(type: "int", nullable: true),
                    April = table.Column<int>(type: "int", nullable: true),
                    May = table.Column<int>(type: "int", nullable: true),
                    June = table.Column<int>(type: "int", nullable: true),
                    July = table.Column<int>(type: "int", nullable: true),
                    August = table.Column<int>(type: "int", nullable: true),
                    September = table.Column<int>(type: "int", nullable: true),
                    October = table.Column<int>(type: "int", nullable: true),
                    November = table.Column<int>(type: "int", nullable: true),
                    December = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaPlacements_3D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plan2026_24D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Quarter = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    FrameworkStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GoalMet = table.Column<bool>(type: "bit", nullable: false),
                    IssueName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CrisisDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IssueHandled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plan2026_24D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionalDevelopments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activities = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionalDevelopments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "selfAssess_31D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SelfAssessmentScore = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_selfAssess_31D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "socialMedia_5D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    JulySeptEngagementRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OctDecEngagementRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    JanMarEngagementRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AprilJuneEngagementRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GoalMet = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_socialMedia_5D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Staffauth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staffauth", x => x.Id);
                    table.UniqueConstraint("AK_Staffauth_Username", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "StaffSurveys_22D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SatisfactionRate = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffSurveys_22D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StrategicGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategicGoals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "volunteerProgram_40D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quarter = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    NumberOfVolunteers = table.Column<int>(type: "int", nullable: false),
                    CommunicationsActivities = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RecognitionActivities = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    VolunteerLedInitiatives = table.Column<int>(type: "int", nullable: false),
                    InitiativeDescriptions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_volunteerProgram_40D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebsiteTraffic",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Q1_JulySeptember = table.Column<int>(type: "int", nullable: true),
                    Q2_OctoberDecember = table.Column<int>(type: "int", nullable: true),
                    Q3_JanuaryMarch = table.Column<int>(type: "int", nullable: true),
                    Q4_AprilJune = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteTraffic", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoalMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrategicGoalId = table.Column<int>(type: "int", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    FiscalYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Q1Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Q2Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Q3Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Q4Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalMetrics_StrategicGoals_StrategicGoalId",
                        column: x => x.StrategicGoalId,
                        principalTable: "StrategicGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Strategies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgramType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgramId = table.Column<int>(type: "int", nullable: true),
                    StrategicGoalId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Time = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CrossCollaboration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Partners = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventFYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Strategies_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Strategies_StrategicGoals_StrategicGoalId",
                        column: x => x.StrategicGoalId,
                        principalTable: "StrategicGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollabTouch_47D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PartnerOrganization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    Touchpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "demographics_8D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    ZipCodes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demographics_8D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_demographics_8D_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Diversity_37D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    DiversityCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diversity_37D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diversity_37D_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonorEvents_19D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    NumberOfParticipants = table.Column<int>(type: "int", nullable: false),
                    EventSatisfactionRating = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonorEvents_19D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonorEvents_19D_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SatisfactionScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Attendees = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreAssessmentData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostAssessmentData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StrategyId = table.Column<int>(type: "int", nullable: true),
                    OwnerUsername = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsAssignedByAdmin = table.Column<bool>(type: "bit", nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Staffauth_OwnerUsername",
                        column: x => x.OwnerUsername,
                        principalTable: "Staffauth",
                        principalColumn: "Username");
                    table.ForeignKey(
                        name: "FK_Events_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventSatisfaction_12D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    EventAttendeeSatisfactionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSatisfaction_12D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSatisfaction_12D_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FaithCommunity_13D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    NumberOfFaithsRepresented = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaithCommunity_13D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaithCommunity_13D_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeForServices_21D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WorkshopFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkshopLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WorkshopDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventPartners = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumberOfAttendees = table.Column<int>(type: "int", nullable: false),
                    ParticipantSatisfactionRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PartnerSatisfactionRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RevenueReceived = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpenseReceived = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "FirstTime_38D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    TotalAttendees = table.Column<int>(type: "int", nullable: false),
                    NumberOfFirstTimeParticipants = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstTime_38D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirstTime_38D_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interfaith_11D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    NumberOfFaithsRepresented = table.Column<int>(type: "int", nullable: false),
                    PostEventSatisfactionSurvey = table.Column<int>(type: "int", nullable: false),
                    TotalAttendance = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interfaith_11D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interfaith_11D_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YouthAttend_15D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    NumberOfYouthAttendees = table.Column<int>(type: "int", nullable: false),
                    PostEventSurveySatisfaction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AveragePreAssessment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AveragePostAssessment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouthAttend_15D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YouthAttend_15D_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollabTouch_47D_StrategyId",
                table: "CollabTouch_47D",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_demographics_8D_StrategyId",
                table: "demographics_8D",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_Diversity_37D_StrategyId",
                table: "Diversity_37D",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorEvents_19D_StrategyId",
                table: "DonorEvents_19D",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_OwnerUsername",
                table: "Events",
                column: "OwnerUsername");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StrategyId",
                table: "Events",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSatisfaction_12D_StrategyId",
                table: "EventSatisfaction_12D",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_FaithCommunity_13D_StrategyId",
                table: "FaithCommunity_13D",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeForServices_21D_StrategyId",
                table: "FeeForServices_21D",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_FirstTime_38D_StrategyId",
                table: "FirstTime_38D",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalMetrics_StrategicGoalId",
                table: "GoalMetrics",
                column: "StrategicGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_Interfaith_11D_StrategyId",
                table: "Interfaith_11D",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_Staffauth_Username",
                table: "Staffauth",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_ProgramId",
                table: "Strategies",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_StrategicGoalId",
                table: "Strategies",
                column: "StrategicGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_YouthAttend_15D_StrategyId",
                table: "YouthAttend_15D",
                column: "StrategyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "achieveMile_6D");

            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "Annual_average_7D");

            migrationBuilder.DropTable(
                name: "ArchivedPrograms");

            migrationBuilder.DropTable(
                name: "BoardMeetingAttendance");

            migrationBuilder.DropTable(
                name: "BoardMember_29D");

            migrationBuilder.DropTable(
                name: "BudgetTracking_28D");

            migrationBuilder.DropTable(
                name: "CollabTouch_47D");

            migrationBuilder.DropTable(
                name: "CommunicationRate");

            migrationBuilder.DropTable(
                name: "ContactsInterfaith_14D");

            migrationBuilder.DropTable(
                name: "demographics_8D");

            migrationBuilder.DropTable(
                name: "Diversity_37D");

            migrationBuilder.DropTable(
                name: "DonorEvents_19D");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "EventSatisfaction_12D");

            migrationBuilder.DropTable(
                name: "FaithCommunity_13D");

            migrationBuilder.DropTable(
                name: "FeeForServices_21D");

            migrationBuilder.DropTable(
                name: "FirstTime_38D");

            migrationBuilder.DropTable(
                name: "GoalMetrics");

            migrationBuilder.DropTable(
                name: "income_27D");

            migrationBuilder.DropTable(
                name: "Interfaith_11D");

            migrationBuilder.DropTable(
                name: "MediaPlacements_3D");

            migrationBuilder.DropTable(
                name: "Plan2026_24D");

            migrationBuilder.DropTable(
                name: "ProfessionalDevelopments");

            migrationBuilder.DropTable(
                name: "selfAssess_31D");

            migrationBuilder.DropTable(
                name: "socialMedia_5D");

            migrationBuilder.DropTable(
                name: "StaffSurveys_22D");

            migrationBuilder.DropTable(
                name: "volunteerProgram_40D");

            migrationBuilder.DropTable(
                name: "WebsiteTraffic");

            migrationBuilder.DropTable(
                name: "YouthAttend_15D");

            migrationBuilder.DropTable(
                name: "Staffauth");

            migrationBuilder.DropTable(
                name: "Strategies");

            migrationBuilder.DropTable(
                name: "Programs");

            migrationBuilder.DropTable(
                name: "StrategicGoals");
        }
    }
}
