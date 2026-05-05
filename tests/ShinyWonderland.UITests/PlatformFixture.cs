using System.Diagnostics;
using System.Runtime.InteropServices;
using TUnit.Core.Interfaces;

namespace ShinyWonderland.UITests;

public enum Platform
{
    iOS,
    Android
}

public abstract class PlatformFixture : IAsyncInitializer, IAsyncDisposable
{
    public abstract Platform Platform { get; }
    public MauiDevFlowDriver Driver { get; private set; } = null!;

    public bool CanRunOnCurrentOS() => Platform switch
    {
        Platform.iOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
        Platform.Android => true,
        _ => false
    };

    public async Task InitializeAsync()
    {
        if (!CanRunOnCurrentOS())
            return;

        var platformStr = Platform == Platform.iOS ? "ios" : "android";
        var screenshotSubDir = Path.Combine(
            AppContext.BaseDirectory,
            "screenshots",
            platformStr
        );

        Driver = new MauiDevFlowDriver(screenshotSubDir)
        {
            TargetPlatform = Platform
        };

        // Try quick connect first — if app is already running, skip build/deploy
        try
        {
            await Driver.WaitForAgent(timeoutSeconds: 5);
        }
        catch
        {
            // App not running — build and deploy
            await BuildAndDeploy(platformStr);
            await Driver.WaitForAgent(timeoutSeconds: 180);
        }

        await Driver.WaitUntilExists("RideTimesPage", timeoutSeconds: 30);
    }

    async Task BuildAndDeploy(string platformStr)
    {
        var tfm = $"net10.0-{platformStr}";
        var projectPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ShinyWonderland", "ShinyWonderland.csproj")
        );

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectPath}\" -f {tfm} -t:Run -p:Configuration=Debug",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        process.Start();

        // Don't wait for the process to complete — the app will keep running.
        // Just give it time to start building and deploying.
        await Task.Delay(5000);
    }

    public async ValueTask DisposeAsync()
    {
        if (Driver != null)
            await Driver.DisposeAsync();
    }
}
