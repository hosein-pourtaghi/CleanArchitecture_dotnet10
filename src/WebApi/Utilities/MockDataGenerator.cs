using Domain.Entities.Checklists;

namespace WebApi.Utilities;

public class MockDataGenerator
{
    private static readonly Lazy<MockDataGenerator> _instance = new(() => new MockDataGenerator());

    public static MockDataGenerator Instance => _instance.Value;

    // Common data sets for generating fake data
    private static readonly string[] Titles = new[]
    {
        "Project Management Checklist", "Risk Assessment Checklist",
        "Quality Assurance Checklist", "Health and Safety Checklist"
    };

    private static readonly string[] GroupTitles = new[]
    {
        "Pre-Planning", "Execution", "Review", "Reporting"
    };

    private static readonly string[] QuestionTitles = new[]
    {
        "Define Project Scope", "Assign Roles and Responsibilities",
        "Plan Task Timeline", "Monitor Progress", "Evaluate Completion"
    };

    private static readonly ChecklistQuestionType[] QuestionTypes = Enum.GetValues<ChecklistQuestionType>();

    private static readonly InputType[] OptionTypes = { InputType.String, InputType.Boolean, InputType.Number };

    public string GenerateTitle()
    {
        return Titles[Random.Shared.Next(Titles.Length)];
    }

    public string GenerateGroupTitle()
    {
        return GroupTitles[Random.Shared.Next(GroupTitles.Length)];
    }

    public string GenerateQuestionTitle()
    {
        return QuestionTitles[Random.Shared.Next(QuestionTitles.Length)];
    }

    public ChecklistQuestionType GenerateQuestionType()
    {
        return QuestionTypes[Random.Shared.Next(QuestionTypes.Length)];
    }

    public InputType GenerateOptionType()
    {
        return OptionTypes[Random.Shared.Next(OptionTypes.Length)];
    }

    public int GeneratePriority(int maxPriority = 10) =>
        Random.Shared.Next(1, maxPriority + 1);

    public bool GenerateIsActive() => true;

    public float? GenerateScore(ChecklistQuestionType type)
    {
        return type == ChecklistQuestionType.RadioButton || type == ChecklistQuestionType.CheckBox ? (float?)Random.Shared.Next(1, 5) : null;
    }

    public int GenerateVersion() =>
        Random.Shared.Next(1, 10);

    public string GenerateTitle(int maxLength = 100)
    {
        return GenerateTitle();
    }
}

