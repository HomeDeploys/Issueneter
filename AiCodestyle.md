# Project code style

## General code style

* Enable nullable reference types for all projects
* Enable implicit usings for all projects
* Treat all compiler warnings as errors
* Use record types for immutable data models
* Follow Clean Architecture principles with clear separation between layers:
  * Domain - Core business logic and entities
  * Application - Use cases and business rules
  * Infrastructure - External systems integration
* Use dependency injection for all services
* Prefer immutable objects where possible
* Use async/await for all I/O operations
* Follow SOLID principles in all code
* Use meaningful and descriptive names for all identifiers
* Keep methods small and focused on a single responsibility
* Use XML documentation for public APIs

## Naming conventions

* Use PascalCase for class, record, interface, enum, and method names
* Use camelCase for local variables and parameters
* Prefix interfaces with "I" (e.g., IRepository)
* Use descriptive, intention-revealing names
* Avoid abbreviations except for common ones (e.g., Id, Url)
* Name classes after their responsibility
* Name methods with verb phrases describing the action

## Project structure

* Organize code by feature within each architectural layer
* Keep related files close to each other
* Follow the standard .NET project structure
* Use namespaces that reflect the project structure
* Separate interfaces from implementations

## Database access

* Use Repository pattern for database access
* Define repository interfaces in the Domain layer
* Implement repositories in the Infrastructure layer
* Use snake_case for database table and column names
* Define table names as constants in repository classes
* Use string interpolation with constants for SQL queries
* Use raw string literals (`"""..."""`) for multi-line SQL queries
* Use parameterized queries with named parameters
* Avoid dynamic SQL generation except for simple cases
* Use async/await for all database operations
* Properly dispose database connections using `await using`
* Use DTOs for mapping between database and domain models
* Keep SQL queries in the repository classes
* Use explicit column names in SELECT queries
* Use explicit parameter names in SQL queries with `@{nameof(param)}`
* Follow consistent naming for database methods (Get, Create, Update, etc.)

## Tests code style

* Every test must be named with pattern `Action_Should_Outcome`
* Every test must contain Arrange Act Assert comments
* Every test must check outcome using FluentAssertions
* Every test that requires random fake data must use Bogus Faker
* Use NUnit for all tests
* Group tests in classes by the class they are testing
* Use TestCase attributes for parameterized tests
* Include descriptive TestName for TestCase attributes
* Write tests for both success and failure scenarios
* Keep test code as simple and readable as possible
* Test one concept per test method
* Use meaningful assertion messages with "because" clause

## Error handling

* Use Result pattern for operations that can fail
* Include descriptive error messages
* Avoid throwing exceptions for expected failure scenarios
* Log exceptions appropriately using Serilog
* Validate inputs at the boundaries of the system

## Configuration

* Use YAML for configuration files
* Follow the pattern of having example configuration files
* Use strongly-typed configuration objects
* Keep sensitive information out of source control