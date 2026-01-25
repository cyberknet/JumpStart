# Forms Module Documentation

## Overview

The Forms module provides a flexible, database-driven form builder with support for multiple question types, validation, and response collection. Forms are entirely configurable through the UI - no code changes required to create new forms.

## Key Features

- ✅ **8 Question Types** - Text, number, date, boolean, single/multiple choice, dropdown
- ✅ **Dynamic Forms** - Create and modify forms without code changes
- ✅ **Validation** - Required fields, min/max constraints
- ✅ **Options** - Pre-defined options for choice-based questions
- ✅ **Responses** - Collect and store form submissions
- ✅ **Audit Tracking** - Track who created/modified forms and responses
- ✅ **Statistics** - View response counts and completion rates

## Getting Started

### 1. Inherit from JumpStartDbContext

Your DbContext must inherit from `JumpStartDbContext` to automatically seed QuestionTypes:

```csharp
public class ApplicationDbContext : JumpStartDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // ⚠️ Required
        // Your configurations
    }
}
```

### 2. Register Forms Services

In your **API project** `Program.cs`:

```csharp
services.AddJumpStart(options =>
{
    options.RegisterFormsController = true; // ✅ Expose Forms API
});
```

In your **Blazor project** `Program.cs`:

```csharp
services.AddJumpStart(options =>
{
    options.RegisterFormsApiClient = true; // ✅ Consume Forms API
    options.ApiBaseUrl = "https://localhost:7001"; // Your API URL
});
```

### 3. Create Migrations

QuestionTypes are seeded automatically in migrations:

```bash
dotnet ef migrations add AddForms
dotnet ef database update
```

### 4. Use Forms

Navigate to `/forms` in your Blazor app to create and manage forms.

## Question Types

### Text-Based Questions

#### ShortText
- Single-line text input
- **MinimumValue:** Minimum character count (e.g., "8")
- **MaximumValue:** Maximum character count (e.g., "50")
- Use cases: Names, email, usernames

#### LongText
- Multi-line text area
- **MinimumValue:** Minimum character count (e.g., "100")
- **MaximumValue:** Maximum character count (e.g., "5000")
- Use cases: Comments, descriptions, feedback

### Numeric Questions

#### Number
- Numeric input with decimal support
- **MinimumValue:** Minimum numeric value (e.g., "18")
- **MaximumValue:** Maximum numeric value (e.g., "120")
- Use cases: Age, quantity, rating

### Date Questions

#### Date
- Date picker
- **MinimumValue:** Earliest date (ISO format: "1900-01-01")
- **MaximumValue:** Latest date (ISO format: "2100-12-31")
- Use cases: Birth dates, appointment dates, event dates

### Boolean Questions

#### Boolean
- Yes/No radio buttons
- No min/max constraints
- Use cases: Consent, agreement, yes/no questions

### Choice-Based Questions

#### SingleChoice
- Radio button list
- Requires at least one option
- User must select exactly one option
- Use cases: Gender, preference selection

#### MultipleChoice
- Checkbox list
- Requires at least one option
- User can select multiple options
- Use cases: Interests, skills, multi-select preferences

#### Dropdown
- Dropdown/select list
- Requires at least one option
- User must select exactly one option
- Use cases: Country, state, category selection


#### Ranking
- Drag-and-drop or ordered selection of options
- Requires at least two options
- User must rank all options or a subset (configurable)
- Use cases: Prioritizing features, ranking preferences, ordering items

**Example Configuration (API):**
```csharp
new CreateQuestionDto
{
    QuestionText = "Rank your top 3 favorite fruits",
    QuestionTypeId = rankingTypeId, // Guid of "Ranking" type
    IsRequired = true,
    MinimumValue = "3", // Minimum number of items to rank
    MaximumValue = "5", // Maximum number of items to rank (optional)
    DisplayOrder = 3,
    Options = new[]
    {
        new CreateQuestionOptionDto { OptionText = "Apple" },
        new CreateQuestionOptionDto { OptionText = "Banana" },
        new CreateQuestionOptionDto { OptionText = "Cherry" },
        new CreateQuestionOptionDto { OptionText = "Date" },
        new CreateQuestionOptionDto { OptionText = "Elderberry" }
    }
}
```

**Example (UI):**
1. Add a question and select "Ranking" as the type
2. Enter the question text (e.g., "Rank your top 3 favorite fruits")
3. Add options to be ranked
4. Set minimum/maximum number of items to rank
5. Save and preview the drag-and-drop ranking UI

## Validation

### Question Validation

Use `QuestionValidator` to validate responses:

```csharp
using JumpStart.Forms;

// Validate a response
var question = new Question
{
    QuestionType = new QuestionType { Code = "Number" },
    IsRequired = true,
    MinimumValue = "18",
    MaximumValue = "120"
};

bool isValid = QuestionValidator.ValidateResponseValue(question, "25"); // true
bool isInvalid = QuestionValidator.ValidateResponseValue(question, "10"); // false (below minimum)
```

### Min/Max Validation by Type

| Question Type      | MinimumValue | MaximumValue | Validation Logic                           |
|--------------------|--------------|--------------|--------------------------------------------|
| **Number**         | "18"         | "120"        | Parsed as decimal, numeric comparison      |
| **ShortText**      | "8"          | "50"         | Character count                            |
| **LongText**       | "100"        | "5000"       | Character count                            |
| **Date**           | "1900-01-01" | "2100-12-31" | ISO date, date comparison                  |
| **Boolean**        | N/A          | N/A          | No constraints                             |
| **SingleChoice**   | N/A          | N/A          | At least one option selected               |
| **MultipleChoice** | N/A          | N/A          | At least one option selected (if required) |
| **Dropdown**       | N/A          | N/A          | One option selected                        |
| **Ranking**        | N/A          | N/A          | No constraints                             |

### UI Helpers

`QuestionValidator` provides UI helper methods:

```csharp
// Get placeholder text for UI
string placeholder = QuestionValidator.GetMinimumValuePlaceholder("Number");
// Returns: "18"

string placeholder = QuestionValidator.GetMinimumValuePlaceholder("ShortText");
// Returns: "8 (minimum characters)"

// Get help text for UI
string help = QuestionValidator.GetMinimumValueHelpText("Number");
// Returns: "Minimum numeric value allowed"
```

## Creating Forms

### Via API

```csharp
var createForm = new CreateFormDto
{
    Name = "Customer Feedback",
    Description = "Tell us about your experience",
    IsActive = true,
    Questions = new[]
    {
        new CreateQuestionDto
        {
            QuestionText = "What is your name?",
            QuestionTypeId = shortTextTypeId, // Guid of "ShortText" type
            IsRequired = true,
            MinimumValue = "2",
            MaximumValue = "50",
            DisplayOrder = 1
        },
        new CreateQuestionDto
        {
            QuestionText = "Rate our service (1-10)",
            QuestionTypeId = numberTypeId, // Guid of "Number" type
            IsRequired = true,
            MinimumValue = "1",
            MaximumValue = "10",
            DisplayOrder = 2
        }
    }
};

var form = await formsClient.CreateFormAsync(createForm);
```

### Via UI

1. Navigate to `/forms`
2. Click "Create New Form"
3. Fill in form details (name, description, settings)
4. Add questions:
   - Enter question text
   - Select question type
   - Set required/optional
   - Configure min/max values (if applicable)
   - Add options (for choice questions)
   - Reorder questions with up/down arrows
5. Save form

## Responding to Forms

### Via API

```csharp
var response = new CreateFormResponseDto
{
    FormId = formId,
    Responses = new[]
    {
        new CreateQuestionResponseDto
        {
            QuestionId = nameQuestionId,
            ResponseText = "John Doe"
        },
        new CreateQuestionResponseDto
        {
            QuestionId = ratingQuestionId,
            ResponseText = "9"
        }
    }
};

await formsClient.SubmitFormResponseAsync(response);
```

### Via UI

1. Navigate to `/forms/{formId}/view`
2. Fill in all required fields
3. Validation occurs client-side (HTML5 constraints)
4. Click "Submit"
5. Server-side validation using `QuestionValidator`

## Form Statistics

Get statistics for any form:

```csharp
var stats = await formsClient.GetFormStatisticsAsync(formId);

Console.WriteLine($"Total Responses: {stats.TotalResponses}");
Console.WriteLine($"Completion Rate: {stats.CompletionRate}%");
```

## Database Schema

### QuestionTypes Table

Fixed reference data seeded via migrations:

| Id         | Code           | Name            | HasOptions | AllowsMultipleValues | InputType |
|------------|----------------|-----------------|------------|----------------------|-----------|
| 1000...001 | ShortText      | Short Text      | false      | false                | text      |
| 1000...002 | LongText       | Long Text       | false      | false                | textarea  |
| 1000...003 | Number         | Number          | false      | false                | number    |
| 1000...004 | Date           | Date            | false      | false                | date      |
| 1000...005 | Boolean        | Yes/No          | false      | false                | boolean   |
| 1000...006 | SingleChoice   | Single Choice   | true       | false                | radio     |
| 1000...007 | MultipleChoice | Multiple Choice | true       | true                 | checkbox  |
| 1000...008 | Dropdown       | Dropdown        | true       | false                | select    |
| 1000...009 | Ranking        | Ranking         | true       | true                 | ranking   |

### Entity Relationships

```
Form
├── Questions (1:M)
│   ├── QuestionType (M:1) ← Reference data
│   └── QuestionOptions (1:M) ← For choice questions
└── FormResponses (1:M)
    └── QuestionResponses (1:M)
        └── QuestionResponseOptions (1:M) ← For choice responses
```

## Best Practices

### Question Design

✅ **DO:**
- Use clear, concise question text
- Provide help text for ambiguous questions
- Set realistic min/max constraints
- Test forms before making them active
- Use appropriate question types for data

❌ **DON'T:**
- Create overly long forms (aim for <20 questions)
- Use confusing or ambiguous language
- Set impossible constraints (min > max)
- Make every question required
- Use ShortText for long responses (use LongText)

### Validation

✅ **DO:**
- Validate on both client and server
- Use `QuestionValidator` for server-side validation
- Provide clear error messages
- Test edge cases (min/max boundaries)

❌ **DON'T:**
- Rely solely on client-side validation
- Skip validation for "optional" fields
- Use invalid constraint formats

### Performance

✅ **DO:**
- Paginate form responses for large forms
- Use eager loading when fetching forms with questions
- Cache QuestionTypes (they rarely change)
- Index frequently queried fields

❌ **DON'T:**
- Load all form responses at once
- Forget to include QuestionType in queries
- Create N+1 queries when loading forms

## Extensibility

### Adding Custom Question Types

1. Insert new QuestionType record in database
2. Set appropriate `Code`, `HasOptions`, `InputType`
3. Update `QuestionValidator` if custom validation needed
4. Update Blazor UI to render new type

### Custom Validation Rules

Extend `QuestionValidator` for custom logic:

```csharp
public static class CustomQuestionValidator
{
    public static bool ValidateEmail(Question question, string? value)
    {
        if (!QuestionValidator.ValidateResponseValue(question, value))
            return false;
            
        // Additional email validation
        return Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
```

## Troubleshooting

### QuestionTypes Not Seeded

**Problem:** Forms throw errors about missing QuestionTypes.

**Solution:**
1. Ensure DbContext inherits from `JumpStartDbContext`
2. Create migration: `dotnet ef migrations add AddJumpStart`
3. Apply migration: `dotnet ef database update`

### Validation Not Working

**Problem:** Responses pass client-side but fail server-side validation.

**Solution:**
- Check MinimumValue/MaximumValue format matches question type
- Ensure QuestionType.Code is correct
- Verify QuestionValidator logic for the specific type

### Forms Not Appearing in UI

**Problem:** `/forms` route shows no forms.

**Solution:**
1. Check FormsController is registered: `options.RegisterFormsController = true`
2. Check FormsApiClient is registered: `options.RegisterFormsApiClient = true`
3. Verify ApiBaseUrl is correct
4. Check forms have `IsActive = true`

## API Reference

See [API Documentation](api/index.html) for complete endpoint reference:

- `GET /api/forms` - List all forms
- `GET /api/forms/{id}` - Get form by ID
- `GET /api/forms/{id}/with-questions` - Get form with questions
- `POST /api/forms` - Create new form
- `PUT /api/forms/{id}` - Update form
- `DELETE /api/forms/{id}` - Delete form
- `POST /api/forms/{id}/responses` - Submit response
- `GET /api/forms/{id}/statistics` - Get form statistics
- `GET /api/forms/question-types` - Get all question types

## Examples

See [Sample Applications](samples.md) for complete working examples.
