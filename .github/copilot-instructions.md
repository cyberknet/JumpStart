# C# Development Guidelines
* Always use the latest stable C# version features (currently C# 13).
* Write clear, concise comments for complex logic.
* Prioritize maintainability and handle edge cases with clear exception handling.
* Follow the project's existing architectural patterns and naming conventions.
* Code should have test cases created. All test cases should pass before submitting.
* Always follow SOLID and DRY principles
* All code must have thorough code comments
* Ensure all public members have XML documentation including code samples sufficient to generate documentation websites using tools like DocFX or Sandcastle.
* If code is modified that does not meet the above requirements, part of the modification must address the deficiency.
* For example, if you modify a class that lacks XML documentation, you must add the necessary XML documentation as part of your changes.
* Similarly, if you modify code that does not follow SOLID or DRY principles, you must refactor the code to adhere to these principles as part of your changes.
* Likewise, if you modify code that lacks sufficient test coverage, you must add the necessary test cases to ensure comprehensive coverage as part of your changes.