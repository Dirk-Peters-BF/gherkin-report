using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GherkinReport;
using JetBrains.Annotations;
using Reqnroll;
using ScenarioExecutionStatus = GherkinReport.ScenarioExecutionStatus;

namespace ReqnRollSample;

[UsedImplicitly]
public sealed class TestResultContext
{
    private TestExecution? execution;
    private FeatureResult? currentFeature;
    private ScenarioResult? currentScenario;

    public void StartTestRun() =>
        execution = new TestExecution(
            Assembly.GetExecutingAssembly().GetName().Name + ".dll",
            DateTime.UtcNow,
            DateTime.UtcNow,
            []);

    public void StartFeature(FeatureContext featureContext) =>
        execution!.Features.Add(
            currentFeature = new FeatureResult(
                featureContext.FeatureInfo.Title,
                featureContext.FeatureInfo.Description,
                featureContext.FeatureInfo.FolderPath,
                []));

    public void StartScenario(ScenarioContext scenarioContext) =>
        currentFeature!.Scenarios.Add(
            currentScenario = new ScenarioResult(
                scenarioContext.ScenarioInfo.Title,
                [..scenarioContext.ScenarioInfo.Tags],
                []));

    public void StartStep(ScenarioContext scenarioContext) => scenarioContext["StepStart"] = DateTime.UtcNow;

    public void EndStep(ScenarioContext scenarioContext)
    {
        var stepInfo = scenarioContext.StepContext.StepInfo;
        var start = (DateTime)scenarioContext["StepStart"];
        var duration = DateTime.UtcNow - start;

        var result = new StepResult(
            stepInfo.StepDefinitionType.ToString(),
            stepInfo.Text,
            (ScenarioExecutionStatus)scenarioContext.StepContext.Status,
            (long)duration.TotalMilliseconds,
            scenarioContext.TestError?.Message);

        currentScenario!.Steps.Add(result);
    }

    public void EndTestRun()
    {
        execution = execution! with { ExecutionEndTimestamp = DateTime.UtcNow };

        // HtmlReportGenerator.Generate("TestExecution.json", "LivingDocLite.html");
    }

    public void Save(string targetPath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var json = JsonSerializer.Serialize(execution, options);
        File.WriteAllText(targetPath, json);
    }
}