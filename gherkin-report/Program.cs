// See https://aka.ms/new-console-template for more information

using CommandLine;
using GherkinReport;

Console.WriteLine(Environment.CurrentDirectory);

var parseResult = Parser.Default.ParseArguments<Options>(args);
if (parseResult.Tag == ParserResultType.Parsed)
{
    if (!File.Exists(parseResult.Value.InputFile))
    {
        Console.WriteLine($"{parseResult.Value.InputFile} does not exist");
        return -1;
    }

    HtmlReportGenerator.Generate(parseResult.Value.InputFile, parseResult.Value.OutputPath);
}

return 0;