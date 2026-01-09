# C# Development Guidelines
* Always use the latest stable C# version features (currently C# 13).
* Write clear, concise comments for complex logic.
* Prioritize maintainability and handle edge cases with clear exception handling.
* Follow the project's existing architectural patterns and naming conventions.
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