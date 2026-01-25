All instructions below are CRITICAL and MANDATORY.
You may not generate code that deviates from the instructions below under any circumstances without first obtaining permission.

# General C# Development : MANDATORY
* Always use the latest stable C# version features (currently C# 13).
* Prioritize maintainability and handle edge cases with clear exception handling.
* Follow the project's existing architectural patterns and naming conventions.
    * Every class MUST be in its own file. No exceptions.
    * Class, enum, record, struct, property, and function names are PascalCase
    * Private variables start with _ and are camelCase
    * Local variables and method parameters are camelCase
    * Interface names start with I and are PascalCase
    * Enum names are singular
    * Always use Primary Constructors for Dependency Injection in classes and structs when possible. 
    * Large classes must be split into partial classes in multiple files by functionality.
* Code ,ist  have test cases created. All test cases must pass before submitting.
* When modifying code, any modified code must meet all of these guidelines, even if the deficiencies existed prior to your changes.
* Use file scoped types for utility classes meant only for a single file. 
* Use params collections for flexible method signatures.
* Always use Nullable Reference Types (NRT). Use ? for optional data and ensure appropriate null-checks or defaults are provided to avoid NullReferenceException.

# Style: MANDATORY
* Never use emojis in code or comments.

# Architecture : MANDATORY
* DRY (Don't Repeat Yourself) - Never duplicate code. Instead of creating a copy of code, refactored it a shared method or class.
* SOLID:
    - Single Responsibility: A class must have one, and only one, reason to change.
    - Open/Closed: Software entities must be open for extension, but closed for modification.
    - Liskov Substitution: Objects of a superclass must be replaceable with objects of a subclass without affecting the correctness of the program.
    - Interface Segregation: It’s better to have many small, specific interfaces than one giant "do-it-all" interface.
    - Dependency Inversion: High-level logic must not depend on low-level details. Use "Interfaces" or "Abstract Classes" to decouple parts of your app.
* SoC (Separation of Concerns) - different parts of the application must handle distinct responsibilities without overlapping.
* KISS (Keep It Simple, Stupid) - Avoid "clever" code. If a junior developer can't understand your logic at a glance, it's likely too complex.
* YAGNI (You Aren't Gonna Need It) - don't implement features until they are necessary.
* POLA (Principle of Least Astonishment) in mind - code must behave in a way that least surprises other developers.
* Boy Scout Campground - always leave the code cleaner than you found it.
* DI (Dependency Injection) - prefer constructor injection for dependencies to promote testability and maintainability.
* Asynchrony - use async/await for I/O-bound operations to improve scalability and responsiveness.
* Logging - Every method must log with appropriate log levels.
* Testing - Write unit tests for all new features and bug fixes. Aim for high code coverage and use mocking frameworks as needed.

# Entity Framework : MANDATORY
* You MUST use DataAnnotations for ALL entity configuration. Fluent API is **FORBIDDEN** except for query filters and seed data.
* If it is not possible to use DataAnnotations, create a new reusable attribute that accomplish the task and handle the attribute in OnModelCreating.
* Fluent API is to be used only when DataAnnotations are impossible. No Exceptions.
* Always use a Table attribute to specify the table name
* Never use plural table names.
* Always add navigation properties for foreign keys.

# Exception Handling : MANDATORY
* Implement error handling and exception management to ensure application stability. 
* Never swallow exceptions silently.
* Catch Exception as the last resort only, prefer catching specific exceptions.

# Database Access : MANDATORY
* Database access must be abstracted using repository and unit of work patterns. The ONLY exception to this is for Asp.Net Identity where the built-in classes and context must be used.
* Database access must only be done from the WebApi tier. The ONLY exception to this is for Asp.Net Identity where the built-in classes and context must be used.
* No database access may be done directly from the front-end or client applications. The ONLY exception to this is for Asp.Net Identity where the built-in classes and context must be used.

# Library Dependencies : MANDATORY
* Always look for an existing library when complex problems need to be solved outside of the current domain.
* When adding an external dependency, provide a choice to me for an abstracted version or direct usage and let me decide which is needed.

# Naming Conventions : MANDATORY
* Never include type information in a variable name.
* Use meaningful names that clearly convey purpose and intent.
* Never use abbreviations. No Exceptions.
* When names contain acronyms, any acronym longer than two letters must be in PascalCase (e.g., XmlParser, not XMLParser).

# Code Documentation
* Write clear, concise comments for complex logic.
* Ensure all public members have XML documentation including code samples sufficient to generate documentation websites using tools like DocFX or Sandcastle.
* Use fully qualified names when referencing other classes in XML comments to ensure proper linking in generated documentation.
* In XML documentation code examples, always escape < and > as &lt; and &gt; respectively.

# License Requirements: MANDATORY
* Ensure that all code contributions comply with the project's licensing terms and conditions.
* When incorporating third-party libraries or code snippets, verify that their licenses are compatible with the project's license.
* Document any third-party code usage and its associated license in the project's documentation.
* Avoid using code or libraries with restrictive licenses that may impose limitations on the project's distribution or usage.
* Each code file (*.cs, *.razor) must have a copyright statement on the very first line in the format of:
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