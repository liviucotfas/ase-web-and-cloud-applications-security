# Copilot Instructions

## Repository Context
- This is an educational repository for teaching ASP.NET Core web and cloud application security
- Examples are organized in numbered folders (01, 02, etc.) with corresponding markdown documentation
- Each example builds on concepts progressively

## Technical Standards
- This repository uses .NET 9
- All C# code should follow .NET 9 conventions and APIs
- Use nullable reference types and handle null cases appropriately
- Prefer async/await patterns for I/O operations
- Follow ASP.NET Core best practices for MVC, Web API, and middleware

## Security Focus
- Demonstrate both vulnerable and secure code patterns where applicable
- Include comments explaining security implications
- Follow OWASP security guidelines
- Clearly mark intentionally vulnerable code with warnings

## Code Quality
- Best software development practices should be followed
- Write unit tests for business logic (use xUnit)
- Use dependency injection appropriately
- Keep controllers thin, move logic to services/repositories
- Follow SOLID principles

## Documentation
- Keep markdown files updated with code changes
- Use clear, educational language suitable for learning
- Include code examples inline in markdown files
- Explain both "what" and "why" for security concepts
