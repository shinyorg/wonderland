using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ShinyWonderland.UITests;

public class MauiDevFlowDriver : IAsyncDisposable
{
    readonly string screenshotDir;

    public MauiDevFlowDriver(string? screenshotDir = null)
    {
        this.screenshotDir = screenshotDir ?? Path.Combine(
            AppContext.BaseDirectory,
            "screenshots"
        );
        Directory.CreateDirectory(this.screenshotDir);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async Task<string> RunCommand(string args, int timeoutMs = 30_000)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "maui-devflow",
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        process.Start();

        using var cts = new CancellationTokenSource(timeoutMs);
        var output = await process.StandardOutput.ReadToEndAsync(cts.Token);
        var error = await process.StandardError.ReadToEndAsync(cts.Token);
        await process.WaitForExitAsync(cts.Token);

        if (process.ExitCode != 0)
            throw new MauiDevFlowException(process.ExitCode, error, output);

        return output.Trim();
    }

    public async Task<JsonNode?> RunJsonCommand(string args, int timeoutMs = 30_000)
    {
        var output = await RunCommand(args + " --json", timeoutMs);
        if (string.IsNullOrWhiteSpace(output))
            return null;

        return JsonNode.Parse(output);
    }

    public Task WaitForAgent(int timeoutSeconds = 120)
        => RunCommand($"wait --timeout {timeoutSeconds}", timeoutSeconds * 1000 + 5000);

    public Task Navigate(string route)
        => RunCommand($"MAUI navigate \"{route}\"");

    public Task<JsonNode?> Tree(int depth = 15, string? fields = null)
    {
        var fieldsArg = fields != null ? $" --fields \"{fields}\"" : "";
        return RunJsonCommand($"MAUI tree --depth {depth}{fieldsArg}");
    }

    public Task<JsonNode?> Query(
        string? automationId = null,
        string? type = null,
        string? text = null,
        string? fields = null)
    {
        var args = "MAUI query";
        if (automationId != null) args += $" --automationId \"{automationId}\"";
        if (type != null) args += $" --type \"{type}\"";
        if (text != null) args += $" --text \"{text}\"";
        if (fields != null) args += $" --fields \"{fields}\"";
        return RunJsonCommand(args);
    }

    public Task<JsonNode?> WaitUntilExists(string automationId, int timeoutSeconds = 30)
        => RunJsonCommand(
            $"MAUI query --automationId \"{automationId}\" --wait-until exists --timeout {timeoutSeconds}",
            (timeoutSeconds + 5) * 1000);

    public Task<JsonNode?> WaitUntilGone(string automationId, int timeoutSeconds = 30)
        => RunJsonCommand(
            $"MAUI query --automationId \"{automationId}\" --wait-until gone --timeout {timeoutSeconds}",
            (timeoutSeconds + 5) * 1000);

    public Task Tap(string? automationId = null, string? elementId = null, string? type = null, int? index = null)
    {
        var args = "MAUI tap";
        if (elementId != null) args += $" {elementId}";
        if (automationId != null) args += $" --automationId \"{automationId}\"";
        if (type != null) args += $" --type \"{type}\"";
        if (index != null) args += $" --index {index}";
        return RunCommand(args);
    }

    public Task Fill(string text, string? automationId = null, string? elementId = null)
    {
        var args = "MAUI fill";
        if (elementId != null) args += $" {elementId}";
        args += $" \"{text}\"";
        if (automationId != null) args += $" --automationId \"{automationId}\"";
        return RunCommand(args);
    }

    public Task Clear(string? automationId = null, string? elementId = null)
    {
        var args = "MAUI clear";
        if (elementId != null) args += $" {elementId}";
        if (automationId != null) args += $" --automationId \"{automationId}\"";
        return RunCommand(args);
    }

    public async Task AssertProperty(string property, string expected, string? automationId = null, string? elementId = null)
    {
        var args = "MAUI assert";
        if (elementId != null) args += $" --id {elementId}";
        if (automationId != null) args += $" --automationId \"{automationId}\"";
        args += $" {property} \"{expected}\"";
        await RunCommand(args);
    }

    public async Task<string> GetProperty(string elementId, string property)
        => await RunCommand($"MAUI property {elementId} {property}");

    public Task Screenshot(string filename, string? elementAutomationId = null)
    {
        var path = Path.Combine(screenshotDir, filename);
        var args = $"MAUI screenshot --output \"{path}\" --overwrite";
        if (elementAutomationId != null)
            args = $"MAUI screenshot --output \"{path}\" --overwrite --selector \"{elementAutomationId}\"";
        return RunCommand(args);
    }

    public Task<JsonNode?> Element(string elementId)
        => RunJsonCommand($"MAUI element {elementId}");

    public Task Scroll(string? elementId = null, int? dy = null, int? itemIndex = null)
    {
        var args = "MAUI scroll";
        if (elementId != null) args += $" --element {elementId}";
        if (dy != null) args += $" --dy {dy}";
        if (itemIndex != null) args += $" --item-index {itemIndex}";
        return RunCommand(args);
    }

    public async Task<JsonNode?> Status()
        => await RunJsonCommand("MAUI status");

    public async Task<bool> IsElementVisible(string automationId)
    {
        try
        {
            var result = await Query(automationId: automationId);
            return result != null;
        }
        catch (MauiDevFlowException)
        {
            return false;
        }
    }
}

public class MauiDevFlowException : Exception
{
    public int ExitCode { get; }
    public string StdErr { get; }
    public string StdOut { get; }

    public MauiDevFlowException(int exitCode, string stdErr, string stdOut)
        : base($"maui-devflow exited with code {exitCode}: {stdErr}")
    {
        ExitCode = exitCode;
        StdErr = stdErr;
        StdOut = stdOut;
    }
}
