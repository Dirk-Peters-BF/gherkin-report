using System.Text.Json;
using System.Text.Json.Serialization;
using HtmlAgilityPack;

namespace GherkinReport;

public static class HtmlReportGenerator
{
    public static void Generate(string jsonPath, string htmlOutputPath)
    {
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException("Test execution JSON not found", jsonPath);

        var json = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        var execution = JsonSerializer.Deserialize<TestExecution>(json, options)
                        ?? throw new InvalidOperationException("Unable to deserialize TestExecution.json");

        var doc = new HtmlDocument();
        var html = doc.CreateElement("html");
        doc.DocumentNode.AppendChild(html);

        // HEAD
        var head = doc.CreateElement("head");
        html.AppendChild(head);
        head.AppendChild(doc.CreateElement("meta")).SetAttributeValue("charset", "UTF-8");
        head.AppendChild(doc.CreateElement("title")).InnerHtml = "Test Execution Report";
        var style = doc.CreateElement("style");
        style.InnerHtml =
            """

                            body { font-family: sans-serif; margin: 2rem; }
                            h1, h2, h3 { margin-bottom: 0.3em; }
                            .feature, .scenario { margin-bottom: 1.5em; }
                            .steps { padding-left: 1.5em; }
                            .step { margin-bottom: 0.3em; padding: 0.2em 0.5em; border-radius: 4px; }
                            .Passed { color: #155724; background-color: #d4edda; }
                            .Failed { color: #721c24; background-color: #f8d7da; }
                            .Inconclusive { color: #856404; background-color: #fff3cd; }
                            .scenario { border-left: 3px solid #ddd; padding-left: 1rem; }
                            .scenario.Passed { border-color: #28a745; }
                            .scenario.Failed { border-color: #dc3545; }
                            .scenario.Inconclusive { border-color: #ffc107; }
                            .scenario > h3 { cursor: pointer; }
                            .steps.collapsed { display: none; }
                            #summary ul { list-style: none; padding: 0; }
                            #summary li { margin-bottom: 0.5em; font-weight: bold; }
                        
            """;
        head.AppendChild(style);

        var body = doc.CreateElement("body");
        html.AppendChild(body);
        body.AppendChild(doc.CreateElement("h1")).InnerHtml = "Test Report";
        body.AppendChild(CreateMeta(doc, execution));
        body.AppendChild(CreateSummary(execution, doc));

        // features
        var reportDiv = doc.CreateElement("div");
        reportDiv.SetAttributeValue("id", "report");
        body.AppendChild(reportDiv);

        // Build feature sections
        foreach (var feature in execution.Features)
        {
            var fdiv = doc.CreateElement("div");
            fdiv.SetAttributeValue("class", "feature");

            fdiv.AppendChild(doc.CreateElement("h2")).InnerHtml = $"Feature: {feature.Name}";
            if (feature.Description != null) fdiv.AppendChild(doc.CreateElement("pre")).InnerHtml = feature.Description;

            // Scenarios
            foreach (var scenario in feature.Scenarios)
            {
                // Determine scenario overall status
                var scenarioStatus = ScenarioStatus(scenario);

                var sdiv = doc.CreateElement("div");
                sdiv.SetAttributeValue("class", $"scenario {scenarioStatus}");

                var h3 = doc.CreateElement("h3");
                h3.InnerHtml = scenario.Name + " " + string.Join(' ', scenario.Tags.ConvertAll(t => "[" + t + "]"));
                sdiv.AppendChild(h3);

                // Steps container
                var stepsDiv = doc.CreateElement("div");
                stepsDiv.SetAttributeValue("class", "steps");

                // Click handler for collapse
                h3.SetAttributeValue("onclick", "this.nextSibling.classList.toggle('collapsed')");

                foreach (var step in scenario.Steps)
                {
                    var st = doc.CreateElement("div");
                    st.SetAttributeValue("class", "step " + StepStatus(step));
                    st.InnerHtml = $@"
                            <span class='keyword'>{step.Keyword}</span>
                            <span class='text'>{step.Text}</span>
                            — <em>{step.Status}</em>
                            ({step.DurationInMilliseconds} ms)
                            {(string.IsNullOrEmpty(step.ErrorMessage) ? "" : $"<div><strong>Error:</strong> {step.ErrorMessage}</div>")}";
                    stepsDiv.AppendChild(st);
                }

                sdiv.AppendChild(stepsDiv);
                fdiv.AppendChild(sdiv);
            }

            reportDiv.AppendChild(fdiv);
        }

        // Save output
        doc.Save(htmlOutputPath);
    }

    private static HtmlNode CreateMeta(HtmlDocument doc, TestExecution execution)
    {
        var metaDiv = doc.CreateElement("div");
        metaDiv.SetAttributeValue("id", "meta");
        metaDiv.InnerHtml = $@"
                <p><strong>Project:</strong> {execution.TestProject}</p>
                <p><strong>Started:</strong> {execution.ExecutionStartTimestamp.ToLocalTime()}</p>
                <p><strong>Ended:</strong> {execution.ExecutionEndTimestamp.ToLocalTime()}</p>
            ";
        return metaDiv;
    }

    private static HtmlNode CreateSummary(TestExecution execution, HtmlDocument doc)
    {
        var scenarios = execution.Features
            .SelectMany(f => f.Scenarios)
            .ToList();
        var passedCount = scenarios.Count(st => st.IsSuccess);
        var failedCount = scenarios.Count(st => !st.IsSuccess);
        var inconclusiveCount = scenarios.Count(st => st.IsInconclusive);
        var summaryDiv = doc.CreateElement("div");
        summaryDiv.SetAttributeValue("id", "summary");
        summaryDiv.InnerHtml = $@"
                <h2>Summary</h2>
                <ul>
                    <li class='Passed'>Passed: {passedCount}</li>
                    <li class='Failed'>Failed: {failedCount}</li>
                    <li class='Inconclusive'>Inconclusive: {inconclusiveCount}</li>
                    <li>Total: {scenarios.Count}</li>
                </ul>
            ";
        return summaryDiv;
    }

    private static string ScenarioStatus(ScenarioResult scenario) =>
        scenario switch
        {
            _ when scenario.IsSuccess => "Passed",
            _ => string.Empty
        };

    private static string StepStatus(StepResult step) =>
        step switch
        {
            _ when !step.IsSuccess => "Failed",
            _ when step.IsInconclusive => "Inconclusive",
            _ => "Passed"
        };

    // Example usage from command-line:
    // HtmlReportGenerator.Generate("TestExecution.json", "LivingDocLite.html");
}