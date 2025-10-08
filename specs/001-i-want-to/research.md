# Research: Database Integration with SQLite

**Feature**: Database Integration with SQLite  
**Created**: 2025-10-07  
**Purpose**: Research findings for technical decisions and best practices

## Entity Framework Core with SQLite

**Decision**: Use Entity Framework Core with SQLite provider  
**Rationale**: 
- Native integration with ASP.NET Core
- Code-first migrations for schema evolution  
- Strong typing and LINQ support
- Excellent tooling and documentation
- Built-in connection pooling and optimization

**Alternatives considered**:
- Dapper (lightweight ORM) - rejected due to need for migrations and code-first approach
- ADO.NET direct - rejected due to complexity and maintenance overhead
- SQLite-net - rejected due to lack of ASP.NET Core integration

## Optimistic Concurrency Implementation

**Decision**: Use EF Core row versioning with `RowVersion` property  
**Rationale**:
- Built-in optimistic concurrency support in EF Core
- Automatic conflict detection without custom logic
- Clear exception handling for conflicts
- Standard pattern across .NET applications

**Alternatives considered**:
- Timestamp-based comparison - rejected due to precision issues
- Custom version fields - rejected due to implementation complexity
- Pessimistic locking - rejected due to user experience concerns

## Database Configuration and Location

**Decision**: Implement `IConfiguration`-based database path with default fallback  
**Rationale**:
- Follows .NET configuration patterns
- Supports environment-specific settings
- Easy to configure via appsettings.json
- Supports command-line overrides

**Implementation approach**:
- Default location: `Data/OneJaxDashboard.db` relative to application
- Configuration key: `DatabaseSettings:FilePath`
- Create directory if it doesn't exist
- Validate path permissions on startup

## Export/Import Strategy

**Decision**: JSON-based export/import with full data fidelity  
**Rationale**:
- Human-readable format for troubleshooting
- Preserves all relationships and metadata
- Easy to version and extend schema
- Cross-platform compatibility

**Alternatives considered**:
- SQLite backup API - rejected due to binary format limitations
- CSV export - rejected due to relational data complexity
- XML format - rejected due to verbosity and parsing complexity

## Data Validation Strategy

**Decision**: Use Data Annotations with custom validation attributes  
**Rationale**:
- Integrates with ASP.NET Core model binding
- Client-side validation support
- Declarative approach reduces code complexity
- Follows framework conventions

**Validation rules**:
- Required fields: Name properties, target values
- String lengths: Names (100 chars), descriptions (500 chars)
- Data types: Numeric constraints for targets and progress
- Custom validation: Progress percentage (0-100), positive target values

## Database Migration and Startup

**Decision**: Automatic migration on application startup with error handling  
**Rationale**:
- Ensures database schema is always current
- Reduces deployment complexity
- Handles development and production scenarios
- Provides clear error messages for failures

**Implementation**:
- Check database exists, create if missing
- Apply pending migrations automatically
- Log migration activities
- Graceful error handling with user-friendly messages

## Connection Management

**Decision**: Use EF Core dependency injection with scoped lifetime  
**Rationale**:
- Follows ASP.NET Core patterns
- Automatic connection lifecycle management
- Built-in connection pooling
- Thread-safe by default

**Configuration**:
- Connection string with pragmas for performance
- Command timeout configuration
- Retry policies for transient failures
- Connection state monitoring for health checks