
CREATE TABLE IF NOT EXISTS "EFMigrationsLock" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK___EFMigrationsLock" PRIMARY KEY,
    "Timestamp" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "ProfessionalDevelopments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProfessionalDevelopments" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "ProfessionalDevelopmentYear26" INTEGER NOT NULL,
    "ProfessionalDevelopmentYear27" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00'
);
CREATE TABLE sqlite_sequence(name,seq);
CREATE TABLE IF NOT EXISTS "Staffauth" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Staffauth" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Username" TEXT NULL,
    "Password" TEXT NULL,
    "Email" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "StaffSurveys_22D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StaffSurveys_22D" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "SatisfactionRate" INTEGER NOT NULL,
    "ProfessionalDevelopmentCount" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00'
);
CREATE TABLE IF NOT EXISTS "StrategicGoals" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StrategicGoals" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Color" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "GoalMetrics" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_GoalMetrics" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "StrategicGoalId" INTEGER NOT NULL,
    "Target" TEXT NOT NULL,
    "CurrentValue" TEXT NOT NULL,
    "Unit" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "TargetDate" TEXT NOT NULL,
    "Q1Value" TEXT NOT NULL,
    "Q2Value" TEXT NOT NULL,
    "Q3Value" TEXT NOT NULL,
    "Q4Value" TEXT NOT NULL,
    "DataSource" TEXT NOT NULL DEFAULT '',
    "FiscalYear" TEXT NOT NULL DEFAULT '',
    "IsPublic" INTEGER NOT NULL DEFAULT 0,
    "MetricType" TEXT NOT NULL DEFAULT '',
    CONSTRAINT "FK_GoalMetrics_StrategicGoals_StrategicGoalId" FOREIGN KEY ("StrategicGoalId") REFERENCES "StrategicGoals" ("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "Strategies" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Strategies" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "StrategicGoalId" INTEGER NOT NULL,
    "Description" TEXT NOT NULL,
    "Date" TEXT NULL,
    "Time" TEXT NULL,
    "EventType" TEXT NOT NULL DEFAULT '',
    "EventFYear" TEXT NOT NULL DEFAULT '',
    CONSTRAINT "FK_Strategies_StrategicGoals_StrategicGoalId" FOREIGN KEY ("StrategicGoalId") REFERENCES "StrategicGoals" ("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "Metric" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Metric" PRIMARY KEY AUTOINCREMENT,
    "Description" TEXT NOT NULL,
    "Target" TEXT NOT NULL,
    "Progress" TEXT NOT NULL,
    "StrategyId" INTEGER NOT NULL,
    "Status" TEXT NOT NULL,
    "TimePeriod" TEXT NOT NULL,
    CONSTRAINT "FK_Metric_Strategies_StrategyId" FOREIGN KEY ("StrategyId") REFERENCES "Strategies" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_GoalMetrics_StrategicGoalId" ON "GoalMetrics" ("StrategicGoalId");
CREATE INDEX "IX_Metric_StrategyId" ON "Metric" ("StrategyId");
CREATE UNIQUE INDEX "IX_Staffauth_Username" ON "Staffauth" ("Username") WHERE [Username] IS NOT NULL;
CREATE INDEX "IX_Strategies_StrategicGoalId" ON "Strategies" ("StrategicGoalId");
CREATE TABLE IF NOT EXISTS "MediaPlacements_3D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MediaPlacements_3D" PRIMARY KEY AUTOINCREMENT,
    "January" INTEGER NULL,
    "February" INTEGER NULL,
    "March" INTEGER NULL,
    "April" INTEGER NULL,
    "May" INTEGER NULL,
    "June" INTEGER NULL,
    "July" INTEGER NULL,
    "August" INTEGER NULL,
    "September" INTEGER NULL,
    "October" INTEGER NULL,
    "November" INTEGER NULL,
    "December" INTEGER NULL,
    "CreatedDate" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "Events" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Events" PRIMARY KEY AUTOINCREMENT,
    "AdminNotes" TEXT NOT NULL,
    "AssignmentDate" TEXT NULL,
    "Attendees" INTEGER NOT NULL,
    "CompletionDate" TEXT NULL,
    "Description" TEXT NOT NULL,
    "DueDate" TEXT NULL,
    "EndDate" TEXT NULL,
    "IsArchived" INTEGER NOT NULL,
    "IsAssignedByAdmin" INTEGER NOT NULL,
    "Location" TEXT NOT NULL,
    "Notes" TEXT NOT NULL,
    "OwnerUsername" TEXT NOT NULL,
    "PostAssessmentData" TEXT NOT NULL,
    "PreAssessmentData" TEXT NOT NULL,
    "SatisfactionScore" TEXT NULL,
    "StartDate" TEXT NULL,
    "Status" TEXT NOT NULL,
    "StrategicGoalId" INTEGER NULL,
    "StrategicGoalId1" INTEGER NULL,
    "StrategyId" INTEGER NULL,
    "StrategyTemplateId" INTEGER NULL,
    "Title" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    CONSTRAINT "FK_Events_StrategicGoals_StrategicGoalId" FOREIGN KEY ("StrategicGoalId") REFERENCES "StrategicGoals" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Events_StrategicGoals_StrategicGoalId1" FOREIGN KEY ("StrategicGoalId1") REFERENCES "StrategicGoals" ("Id"),
    CONSTRAINT "FK_Events_Strategies_StrategyId" FOREIGN KEY ("StrategyId") REFERENCES "Strategies" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Events_Strategies_StrategyTemplateId" FOREIGN KEY ("StrategyTemplateId") REFERENCES "Strategies" ("Id")
);
CREATE INDEX "IX_Events_StrategicGoalId" ON "Events" ("StrategicGoalId");
CREATE INDEX "IX_Events_StrategicGoalId1" ON "Events" ("StrategicGoalId1");
CREATE INDEX "IX_Events_StrategyId" ON "Events" ("StrategyId");
CREATE INDEX "IX_Events_StrategyTemplateId" ON "Events" ("StrategyTemplateId");
CREATE TABLE IF NOT EXISTS "CommunicationRate" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CommunicationRate" PRIMARY KEY AUTOINCREMENT,
    "Year" INTEGER NOT NULL,
    "AverageCommunicationSatisfaction" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "CrossSectorCollabs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CrossSectorCollabs" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Year" INTEGER NOT NULL,
    "partner_satisfaction_ratings" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "WebsiteTraffic" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_WebsiteTraffic" PRIMARY KEY AUTOINCREMENT,
    "CreatedDate" TEXT NOT NULL,
    "Q1_JulySeptember" INTEGER NULL,
    "Q2_OctoberDecember" INTEGER NULL,
    "Q3_JanuaryMarch" INTEGER NULL,
    "Q4_AprilJune" INTEGER NULL
);
CREATE TABLE IF NOT EXISTS "Annual_average_7D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Annual_average_7D" PRIMARY KEY AUTOINCREMENT,
    "Year" INTEGER NOT NULL,
    "Percentage" TEXT NOT NULL,
    "TotalRespondents" INTEGER NULL,
    "RespondentsIdentifyingAsTrusted" INTEGER NULL,
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "demographics_8D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_demographics_8D" PRIMARY KEY AUTOINCREMENT,
    "StrategyId" INTEGER NOT NULL,
    "Year" INTEGER NOT NULL,
    "ZipCodes" TEXT NOT NULL,
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL,
    CONSTRAINT "FK_demographics_8D_Strategies_StrategyId" FOREIGN KEY ("StrategyId") REFERENCES "Strategies" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_demographics_8D_StrategyId" ON "demographics_8D" ("StrategyId");
CREATE TABLE IF NOT EXISTS "Plan2026_24D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Plan2026_24D" PRIMARY KEY AUTOINCREMENT,
    "Year" INTEGER NOT NULL,
    "Quarter" TEXT NOT NULL,
    "FrameworkStatus" TEXT NOT NULL,
    "Notes" TEXT NULL,
    "GoalMet" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "Name" TEXT NOT NULL DEFAULT ''
);
CREATE TABLE IF NOT EXISTS "planIssue_25D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_planIssue_25D" PRIMARY KEY AUTOINCREMENT,
    "PlanId" INTEGER NOT NULL,
    "IssueName" TEXT NOT NULL,
    "CrisisDescription" TEXT NOT NULL,
    "Year" INTEGER NOT NULL,
    "IsCompliant" INTEGER NOT NULL,
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL,
    CONSTRAINT "FK_planIssue_25D_Plan2026_24D_PlanId" FOREIGN KEY ("PlanId") REFERENCES "Plan2026_24D" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_planIssue_25D_PlanId" ON "planIssue_25D" ("PlanId");
CREATE TABLE IF NOT EXISTS "DonorEvents_19D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DonorEvents_19D" PRIMARY KEY AUTOINCREMENT,
    "StrategyId" INTEGER NOT NULL,
    "NumberOfParticipants" INTEGER NOT NULL,
    "EventSatisfactionRating" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "CreatedBy" TEXT NULL,
    CONSTRAINT "FK_DonorEvents_19D_Strategies_StrategyId" FOREIGN KEY ("StrategyId") REFERENCES "Strategies" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_DonorEvents_19D_StrategyId" ON "DonorEvents_19D" ("StrategyId");
CREATE TABLE IF NOT EXISTS "FeeForServices_21D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_FeeForServices_21D" PRIMARY KEY AUTOINCREMENT,
    "ClientName" TEXT NOT NULL,
    "StrategyId" INTEGER NOT NULL,
    "Date" TEXT NOT NULL,
    "TotalNumberOfWorkshops" INTEGER NOT NULL,
    "WorkshopFormat" TEXT NOT NULL,
    "WorkshopLocation" TEXT NULL,
    "WorkshopDate" TEXT NOT NULL,
    "EventPartners" TEXT NULL,
    "NumberOfAttendees" INTEGER NOT NULL,
    "ParticipantSatisfactionRating" TEXT NOT NULL,
    "PartnerSatisfactionRating" TEXT NOT NULL,
    "RevenueReceived" TEXT NOT NULL,
    "Quarter" TEXT NOT NULL,
    "Year" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "ProgramTitle" TEXT NULL,
    CONSTRAINT "FK_FeeForServices_21D_Strategies_StrategyId" FOREIGN KEY ("StrategyId") REFERENCES "Strategies" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_FeeForServices_21D_StrategyId" ON "FeeForServices_21D" ("StrategyId");
CREATE TABLE IF NOT EXISTS "income_27D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_income_27D" PRIMARY KEY AUTOINCREMENT,
    "IncomeSource" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "Month" TEXT NOT NULL,
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "BudgetTracking_28D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BudgetTracking_28D" PRIMARY KEY AUTOINCREMENT,
    "Quarter" TEXT NOT NULL,
    "Year" INTEGER NOT NULL,
    "CommunityPrograms" TEXT NULL,
    "OneYouthPrograms" TEXT NULL,
    "InterfaithPrograms" TEXT NULL,
    "HumanitarianEvent" TEXT NULL,
    "CorporateGiving" TEXT NULL,
    "IndividualGiving" TEXT NULL,
    "GrantsFoundations" TEXT NULL,
    "CommunityEvents" TEXT NULL,
    "PeopleCultureWorkshops" TEXT NULL,
    "Notes" TEXT NULL,
    "CreatedDate" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "BoardMember_29D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BoardMember_29D" PRIMARY KEY AUTOINCREMENT,
    "MemberNames" TEXT NOT NULL,
    "Quarter" INTEGER NOT NULL,
    "Year" INTEGER NOT NULL,
    "NumberRecruited" INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS "BoardMeetingAttendance" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BoardMeetingAttendance" PRIMARY KEY AUTOINCREMENT,
    "MeetingDate" TEXT NOT NULL,
    "MembersInAttendance" INTEGER NOT NULL,
    "TotalBoardMembers" INTEGER NULL,
    "CreatedDate" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "Interfaith_11D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Interfaith_11D" PRIMARY KEY AUTOINCREMENT,
    "StrategyId" INTEGER NOT NULL,
    "NumberOfFaithsRepresented" INTEGER NOT NULL,
    "PostEventSatisfactionSurvey" INTEGER NOT NULL,
    "TotalAttendance" INTEGER NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    CONSTRAINT "FK_Interfaith_11D_Strategies_StrategyId" FOREIGN KEY ("StrategyId") REFERENCES "Strategies" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_Interfaith_11D_StrategyId" ON "Interfaith_11D" ("StrategyId");
CREATE TABLE IF NOT EXISTS "EventSatisfaction_12D" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EventSatisfaction_12D" PRIMARY KEY AUTOINCREMENT,
    "StrategyId" INTEGER NOT NULL,
    "EventAttendeeSatisfactionPercentage" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    CONSTRAINT "FK_EventSatisfaction_12D_Strategies_StrategyId" FOREIGN KEY ("StrategyId") REFERENCES "Strategies" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_EventSatisfaction_12D_StrategyId" ON "EventSatisfaction_12D" ("StrategyId");

