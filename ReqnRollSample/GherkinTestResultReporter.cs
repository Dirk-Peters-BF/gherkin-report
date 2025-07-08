using Reqnroll;

namespace ReqnRollSample;

[Binding]
public class GherkinTestResultReporter
{
    private static readonly TestResultContext Context = new();

    [BeforeTestRun]
    public static void OnTestRunStart() =>
        Context.StartTestRun();

    [BeforeFeature]
    public static void OnBeforeFeature(FeatureContext featureContext) =>
        Context.StartFeature(featureContext);

    [BeforeScenario]
    public static void OnBeforeScenario(ScenarioContext scenarioContext) =>
        Context.StartScenario(scenarioContext);

    [BeforeStep]
    public static void OnBeforeStep(ScenarioContext scenarioContext) =>
        Context.StartStep(scenarioContext);

    [AfterStep]
    public static void OnAfterStep(ScenarioContext scenarioContext) =>
        Context.EndStep(scenarioContext);

    [AfterTestRun]
    public static void OnTestRunEnd()
    {
        Context.EndTestRun();
        Context.Save("TestExecution.json");
    }
}