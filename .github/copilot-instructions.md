# General C# Development
* Always use the latest stable C# version features (currently C# 13).
* Prioritize maintainability and handle edge cases with clear exception handling.
* Follow the project's existing architectural patterns and naming conventions.
* Only place one class per file.
* Code should have test cases created. All test cases should pass before submitting.
* When modifying code, any modified code must meet all of these guidelines, even if the deficiencies existed prior to your changes.
* Ensure that all code adheres to the project's coding standards and guidelines.
* Prefer Primary Constructors for Dependency Injection in classes and structs. 
* Use file scoped types for utility classes meant only for a single file. 
* Use params collections for flexible method signatures.
* Adhere to Nullable Reference Types (NRT). Use ? for optional data and ensure appropriate null-checks or defaults are provided to avoid NullReferenceException.

# Architecture
* DRY (Don't Repeat Yourself) - When duplicating code, consider if it should be refactored into a shared method or class.
* SOLID:
    - Single Responsibility: A class should have one, and only one, reason to change.
    - Open/Closed: Software entities should be open for extension, but closed for modification.
    - Liskov Substitution: Objects of a superclass should be replaceable with objects of a subclass without affecting the correctness of the program.
    - Interface Segregation: It’s better to have many small, specific interfaces than one giant "do-it-all" interface.
    - Dependency Inversion: High-level logic shouldn't depend on low-level details. Use "Interfaces" or "Abstract Classes" to decouple parts of your app.
* SoC (Separation of Concerns) - different parts of the application should handle distinct responsibilities without overlapping.
* KISS (Keep It Simple, Stupid) - Avoid "clever" code. If a junior developer can't understand your logic at a glance, it's likely too complex.
* YAGNI (You Aren't Gonna Need It) - don't implement features until they are necessary.
* POLA (Principle of Least Astonishment) in mind - code should behave in a way that least surprises other developers.
* Boy Scout Campground - always leave the code cleaner than you found it.
* DI (Dependency Injection) - prefer constructor injection for dependencies to promote testability and maintainability.
* Asynchrony - use async/await for I/O-bound operations to improve scalability and responsiveness.
* Logging - Every method should log with appropriate log levels.
* Testing - Write unit tests for all new features and bug fixes. Aim for high code coverage and use mocking frameworks as needed.

# Entity Framework
* Always use DataAnnotations where possible for entity configuration.
* If it is not possible to use DataAnnotations, consider creating new reusable attributes that accomplish the task and handle them in OnModelCreating.
* Fluent API is to be used only when DataAnnotations are impossible.

# Exception Handling
* Implement error handling and exception management to ensure application stability. 
* Never swallow exceptions silently.
* Catch Exception as the last resort only, prefer catching specific exceptions.

# Database Access
* Database access should be abstracted using repository and unit of work patterns.
* Database access should only be done from the WebApi tier
* No database access should be done directly from the front-end or client applications.

# Library Dependencies
* When creating new code always consider i the problem can be solved using existing libraries or frameworks before implementing a custom solution. Simple problems can be solved with a custom solution, but a large, complex problem should be solved using existing libraries or frameworks, for example object mapping and rest clients.
* When adding an external dependency, consider if we should abstract it so we can replace it with a different implementation later. (For example, if we consume Newtonsoft JSON, we may later want to consume System.Text.Json - an abstraction for serialize/deserialize may be helpful)

# Naming Conventions
* Never include type information in a variable name.
* Use meaningful names that clearly convey purpose and intent.
* Do not use abbreviations.
* When names contain acronyms, any acronym longer than two letters should be in PascalCase (e.g., XmlParser, not XMLParser).

# Code Documentation
* Write clear, concise comments for complex logic.
* Ensure all public members have XML documentation including code samples sufficient to generate documentation websites using tools like DocFX or Sandcastle.
* use fully qualified names when referencing other classes in XML comments to ensure proper linking in generated documentation.

# License Requirements
* Ensure that all code contributions comply with the project's licensing terms and conditions.
* When incorporating third-party libraries or code snippets, verify that their licenses are compatible with the project's license.
* Document any third-party code usage and its associated license in the project's documentation.
* Avoid using code or libraries with restrictive licenses that may impose limitations on the project's distribution or usage.
* Each code file should have a copyright statement on the very first line in the format of:
  ```
  // Copyright ©(year) (Author)
  ```
  For example:
  ```
  // Copyright ©2026 Scott Blomfield
  ```
* Immediately following the copyright statement, include the license information in the format of:
  ```
  /*
   *  This program is free software: you can redistribute it and/or modify it under the terms of the
   *  GNU General Public License as published by the Free Software Foundation, either version 3 of the
   *  License, or (at your option) any later version.
   *
   *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
   *  even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
   *  General Public License for more details.
   *
   *  You should have received a copy of the GNU General Public License along with this program. If not,
   *  see <https://www.gnu.org/licenses/>. 
   */
  ```
* If copyright or licensing information is missing from a file, add it as the first lines of the file.
* Do not ever remove or alter existing license and/or copyright information in any code files.