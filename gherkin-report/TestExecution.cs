using CommandLine;
using JetBrains.Annotations;

namespace GherkinReport;

public enum ScenarioExecutionStatus
{
    Ok,
    StepDefinitionPending,
    UndefinedStep,
    BindingError,
    TestError,
    Skipped
}

public sealed record StepResult(
    string Keyword,
    string Text,
    ScenarioExecutionStatus Status,
    long DurationInMilliseconds,
    string? ErrorMessage)
{
    public bool IsSuccess => Status == ScenarioExecutionStatus.Ok;

    public bool IsInconclusive =>
        Status is ScenarioExecutionStatus.Skipped
            or ScenarioExecutionStatus.StepDefinitionPending
            or ScenarioExecutionStatus.UndefinedStep;
}

public sealed record ScenarioResult(
    string Name,
    List<string> Tags,
    List<StepResult> Steps)
{
    public bool IsSuccess => Steps.All(s => s.IsSuccess);
    public bool IsInconclusive => Steps.Any(s => s.IsInconclusive);
}

public sealed record FeatureResult(
    string Name,
    string? Description,
    string RelativePath,
    List<ScenarioResult> Scenarios);

public record TestExecution(
    string TestProject,
    DateTime ExecutionStartTimestamp,
    DateTime ExecutionEndTimestamp,
    List<FeatureResult> Features);
    
public class Options
{
    [Option(
        'i',
        "input",
        Required = true,
        HelpText = "path to test execution json result")]
    public string InputFile { get; [UsedImplicitly] set; } = null!;

    [Option(
        'o',
        "output",
        Required = true,
        HelpText = "path to htm output path to generate html report in")]
    public string OutputPath { get; [UsedImplicitly] set; } = null!;
}