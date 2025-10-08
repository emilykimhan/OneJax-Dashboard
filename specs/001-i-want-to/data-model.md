# Data Model: Database Integration with SQLite

**Feature**: Database Integration with SQLite  
**Created**: 2025-10-07  
**Purpose**: Entity definitions and relationships for persistent storage

## Entity Definitions

### StrategicGoal

**Purpose**: Represents top-level organizational objectives

**Fields**:
- `Id` (int, PK, Identity): Unique identifier
- `Name` (string, 100 chars, Required): Goal name for display
- `Description` (string, 500 chars, Optional): Detailed description
- `CreatedAt` (DateTime, Required): Creation timestamp
- `UpdatedAt` (DateTime, Required): Last modification timestamp
- `RowVersion` (byte[], Concurrency): Optimistic concurrency control

**Relationships**:
- One-to-Many with `Strategy` entities
- Navigation property: `List<Strategy> Strategies`

**Validation Rules**:
- Name: Required, 1-100 characters
- Description: Optional, max 500 characters
- Timestamps: Auto-managed by system

**State Transitions**: N/A (no complex lifecycle)

---

### Strategy

**Purpose**: Specific approaches to achieve strategic goals

**Fields**:
- `Id` (int, PK, Identity): Unique identifier
- `Name` (string, 100 chars, Required): Strategy name
- `Description` (string, 500 chars, Optional): Detailed description
- `StrategicGoalId` (int, FK, Required): Parent goal reference
- `Status` (string, 20 chars, Required): Current status
- `CreatedAt` (DateTime, Required): Creation timestamp
- `UpdatedAt` (DateTime, Required): Last modification timestamp
- `RowVersion` (byte[], Concurrency): Optimistic concurrency control

**Relationships**:
- Many-to-One with `StrategicGoal` (parent)
- One-to-Many with `Metric` entities
- Navigation properties: `StrategicGoal Goal`, `List<Metric> Metrics`

**Validation Rules**:
- Name: Required, 1-100 characters
- Description: Optional, max 500 characters
- Status: Required, valid values ("Active", "Completed", "Upcoming", "On Hold")
- StrategicGoalId: Required, must reference existing goal

**State Transitions**:
- Upcoming → Active → Completed
- Any status → On Hold → Previous status

---

### Metric

**Purpose**: Key performance indicators tied to strategies

**Fields**:
- `Id` (int, PK, Identity): Unique identifier
- `Name` (string, 100 chars, Required): Metric name
- `Description` (string, 500 chars, Optional): Detailed description
- `StrategyId` (int, FK, Required): Parent strategy reference
- `TargetValue` (decimal, Required): Target to achieve
- `CurrentValue` (decimal, Required): Current progress value
- `Unit` (string, 50 chars, Optional): Measurement unit
- `Status` (string, 20 chars, Required): Current status
- `MeasurementPeriod` (string, 20 chars, Required): Reporting frequency
- `CreatedAt` (DateTime, Required): Creation timestamp
- `UpdatedAt` (DateTime, Required): Last modification timestamp
- `RowVersion` (byte[], Concurrency): Optimistic concurrency control

**Relationships**:
- Many-to-One with `Strategy` (parent)
- Navigation property: `Strategy Strategy`

**Validation Rules**:
- Name: Required, 1-100 characters
- Description: Optional, max 500 characters
- TargetValue: Required, must be positive
- CurrentValue: Required, must be non-negative
- Unit: Optional, max 50 characters
- Status: Required, valid values ("Active", "Completed", "At Risk", "Behind")
- MeasurementPeriod: Required, valid values ("Daily", "Weekly", "Monthly", "Quarterly", "Yearly")
- StrategyId: Required, must reference existing strategy

**Calculated Properties**:
- `ProgressPercentage` (decimal): (CurrentValue / TargetValue) * 100, capped at 100%

---

### EventEntry

**Purpose**: Assessment data from events and activities

**Fields**:
- `Id` (int, PK, Identity): Unique identifier
- `EventName` (string, 100 chars, Required): Event name
- `Category` (string, 50 chars, Required): Event category
- `AssessmentData` (string, 2000 chars, Required): JSON-formatted assessment responses
- `OutcomeScore` (decimal, Optional): Numeric outcome if applicable
- `Notes` (string, 1000 chars, Optional): Additional notes
- `EventDate` (DateTime, Required): When the event occurred
- `CreatedAt` (DateTime, Required): Entry creation timestamp
- `UpdatedAt` (DateTime, Required): Last modification timestamp
- `RowVersion` (byte[], Concurrency): Optimistic concurrency control

**Relationships**: None (standalone entity)

**Validation Rules**:
- EventName: Required, 1-100 characters
- Category: Required, 1-50 characters
- AssessmentData: Required, valid JSON format, max 2000 characters
- OutcomeScore: Optional, must be between 0-100 if provided
- Notes: Optional, max 1000 characters
- EventDate: Required, cannot be future date

## Database Schema Considerations

### Indexes
- `StrategicGoal.Name` (non-clustered)
- `Strategy.StrategicGoalId` (foreign key, automatic)
- `Strategy.Status` (non-clustered, for filtering)
- `Metric.StrategyId` (foreign key, automatic)
- `Metric.Status` (non-clustered, for filtering)
- `EventEntry.EventDate` (non-clustered, for date range queries)
- `EventEntry.Category` (non-clustered, for filtering)

### Constraints
- Cascade delete: Goal → Strategies → Metrics (preserve data integrity)
- Check constraints: Positive target values, valid percentage ranges
- Unique constraints: None (allow duplicate names for flexibility)

### Data Volume Estimates
- Goals: ~50-100 per organization
- Strategies: ~200-500 per organization (2-10 per goal)
- Metrics: ~500-2000 per organization (2-10 per strategy)
- Events: ~1000-5000 per year (depends on activity level)

### Performance Considerations
- Connection pooling enabled
- Bulk operations for data export/import
- Lazy loading disabled (explicit Include() for related data)
- Query optimization with appropriate indexes
- Database file size monitoring and maintenance