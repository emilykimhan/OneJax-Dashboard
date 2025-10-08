# API Contracts: Database Integration

**Feature**: Database Integration with SQLite  
**Created**: 2025-10-07  
**Purpose**: HTTP endpoints and data transfer objects for database operations

## Strategic Goals API

### GET /Home/Index
**Purpose**: Retrieve dashboard with strategic goals (existing endpoint enhanced)  
**Method**: GET  
**Parameters**: 
- `goal` (query, optional): Filter by goal name
- `status` (query, optional): Filter by status
- `time` (query, optional): Time period filter

**Response**: HTML view with `DashboardViewModel`
```csharp
public class DashboardViewModel
{
    public IEnumerable<StrategyGoalDto> StrategicGoals { get; set; }
    public string SelectedGoal { get; set; }
    public string SelectedStatus { get; set; }
    public string SelectedTime { get; set; }
}
```

### POST /StrategicGoals/Create
**Purpose**: Create new strategic goal  
**Method**: POST  
**Content-Type**: application/x-www-form-urlencoded

**Request Body**:
```csharp
public class CreateStrategicGoalRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
}
```

**Response**: 
- Success: HTTP 302 Redirect to dashboard
- Validation Error: HTTP 400 with error details
- Conflict: HTTP 409 for concurrency conflicts

### PUT /StrategicGoals/{id}
**Purpose**: Update existing strategic goal  
**Method**: PUT  
**Content-Type**: application/x-www-form-urlencoded

**Request Body**:
```csharp
public class UpdateStrategicGoalRequest
{
    [Required]
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    [Required]
    public byte[] RowVersion { get; set; } // For optimistic concurrency
}
```

**Response**:
- Success: HTTP 302 Redirect to dashboard
- Not Found: HTTP 404
- Validation Error: HTTP 400 with error details
- Concurrency Conflict: HTTP 409 with current data

### DELETE /StrategicGoals/{id}
**Purpose**: Delete strategic goal and cascade to strategies/metrics  
**Method**: DELETE

**Request**: Goal ID in URL path
**Response**:
- Success: HTTP 302 Redirect to dashboard
- Not Found: HTTP 404
- Conflict: HTTP 409 if concurrency conflict

---

## Strategies API

### GET /Strategy/Index
**Purpose**: View strategies for a goal (existing endpoint enhanced)  
**Method**: GET  
**Parameters**: 
- `goalId` (query, optional): Filter by parent goal

**Response**: HTML view with strategy list

### POST /Strategy/Create
**Purpose**: Create new strategy  
**Method**: POST  
**Content-Type**: application/x-www-form-urlencoded

**Request Body**:
```csharp
public class CreateStrategyRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    [Required]
    public int StrategicGoalId { get; set; }
    
    [Required, StringLength(20)]
    public string Status { get; set; } // Active, Completed, Upcoming, On Hold
}
```

### PUT /Strategy/{id}
**Purpose**: Update existing strategy  
**Method**: PUT

**Request Body**: Similar to create with Id and RowVersion for concurrency

### DELETE /Strategy/{id}
**Purpose**: Delete strategy and cascade to metrics  
**Method**: DELETE

---

## Metrics API

### POST /Metrics/Create
**Purpose**: Create new metric  
**Method**: POST

**Request Body**:
```csharp
public class CreateMetricRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    [Required]
    public int StrategyId { get; set; }
    
    [Required, Range(0.01, double.MaxValue)]
    public decimal TargetValue { get; set; }
    
    [Required, Range(0, double.MaxValue)]
    public decimal CurrentValue { get; set; }
    
    [StringLength(50)]
    public string Unit { get; set; }
    
    [Required, StringLength(20)]
    public string Status { get; set; } // Active, Completed, At Risk, Behind
    
    [Required, StringLength(20)]
    public string MeasurementPeriod { get; set; } // Daily, Weekly, Monthly, Quarterly, Yearly
}
```

### PUT /Metrics/{id}
**Purpose**: Update metric progress and status  
**Method**: PUT

---

## Data Management API

### GET /DataManagement/Export
**Purpose**: Export all strategic data  
**Method**: GET  
**Response**: 
- Content-Type: application/json
- File download with all strategic data in JSON format

**Response Schema**:
```json
{
  "exportDate": "2025-10-07T10:30:00Z",
  "version": "1.0",
  "data": {
    "strategicGoals": [
      {
        "id": 1,
        "name": "Community Engagement",
        "description": "...",
        "createdAt": "2025-10-01T09:00:00Z",
        "strategies": [
          {
            "id": 1,
            "name": "Social Media Outreach",
            "status": "Active",
            "metrics": [...]
          }
        ]
      }
    ],
    "eventEntries": [...]
  }
}
```

### POST /DataManagement/Import
**Purpose**: Import strategic data from backup  
**Method**: POST  
**Content-Type**: multipart/form-data

**Request**: JSON file upload matching export schema
**Response**: 
- Success: HTTP 302 Redirect to dashboard with success message
- Validation Error: HTTP 400 with detailed error information

### GET /DataManagement/Settings
**Purpose**: Database configuration settings  
**Method**: GET  
**Response**: HTML form for database path configuration

### POST /DataManagement/Settings
**Purpose**: Update database configuration  
**Method**: POST

**Request Body**:
```csharp
public class DatabaseSettingsRequest
{
    [Required]
    public string DatabasePath { get; set; }
    
    public bool CreateBackup { get; set; }
}
```

---

## Error Handling

### Standard Error Response
```csharp
public class ErrorResponse
{
    public string Message { get; set; }
    public string[] ValidationErrors { get; set; }
    public string ErrorCode { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Common HTTP Status Codes
- `200 OK`: Successful GET requests
- `302 Found`: Successful POST/PUT/DELETE with redirect
- `400 Bad Request`: Validation errors
- `404 Not Found`: Resource not found
- `409 Conflict`: Optimistic concurrency conflicts
- `500 Internal Server Error`: Database connection failures

### Concurrency Conflict Response
```csharp
public class ConcurrencyConflictResponse : ErrorResponse
{
    public object CurrentData { get; set; } // Current database values
    public string ConflictType { get; set; } // "OptimisticConcurrency"
}
```