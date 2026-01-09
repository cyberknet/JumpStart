# C# Development Guidelines
* Always use the latest stable C# version features (currently C# 13).
* Write clear, concise comments for complex logic.
* Prioritize maintainability and handle edge cases with clear exception handling.
* Follow the project's existing architectural patterns and naming conventions.
* When creating new code always consider if the problem can be solved using existing libraries or frameworks before implementing a custom solution. Simple problems can be solved with a custom solution, but a large, complex problem should be solved using existing libraries or frameworks, for example object mapping and rest clients.
* When adding an external dependency, consider if we should abstract it so we can replace it with a different implementation later. (For example, if we consume Newtonsoft JSON, we may later want to consume Json.Net - an abstraction for serialize/deserialize may be helpful)
* When creating classes, members, or variable names, never abbreviate.
* Use meaningful names that clearly convey purpose and intent.
* When names contain acronyms, any acronym longer than two letters should be in PascalCase (e.g., XmlParser, not XMLParser).
* Only place one class per file.
* Code should have test cases created. All test cases should pass before submitting.
* Always follow SOLID and DRY principles
* Ensure all public members have XML documentation including code samples sufficient to generate documentation websites using tools like DocFX or Sandcastle.
* If code is modified that does not meet the above requirements, part of the modification must address the deficiency.
* For example, if you modify a class that lacks XML documentation, you must add the necessary XML documentation as part of your changes.
* Similarly, if you modify code that does not follow SOLID or DRY principles, you must refactor the code to adhere to these principles as part of your changes.
* Likewise, if you modify code that lacks sufficient test coverage, you must add the necessary test cases to ensure comprehensive coverage as part of your changes.
* If multiple classes exist in a single file, make sure to separate them into different files named by the class the file will contain
* Ensure that all code adheres to the project's coding standards and guidelines.

# License Guidelines
* Ensure that all code contributions comply with the project's licensing terms and conditions.
* When incorporating third-party libraries or code snippets, verify that their licenses are compatible with the project's license.
* Document any third-party code usage and its associated license in the project's documentation.
* Avoid using code or libraries with restrictive licenses that may impose limitations on the project's distribution or usage.
* Each code file should have a copyright statement on the very first line in the format of:
  Copyright ©2026 Scott Blomfield
* Immediately following the copyright statement, include the license information in the format of:
    This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
* If copyright or licensing information is missing from a file, add it as the first lines of the file.
* Do not ever remove or alter existing license information in any code files.