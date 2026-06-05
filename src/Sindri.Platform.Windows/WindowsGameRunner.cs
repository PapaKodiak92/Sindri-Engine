using System.Diagnostics;
using System.Runtime.Versioning;
using Sindri.Core;
using Sindri.Platform.Windows.Native;

namespace Sindri.Platform.Windows;

[SupportedOSPlatform("windows")]
public static class WindowsGameRunner
{
    public static int Run(SindriGame game)
    {
        ArgumentNullException.ThrowIfNull(game);

        var engine = new EngineHost(game);
        engine.Initialize();

        var config = engine.Config;

        using var window = NativeWindow.Create(
            title: config.WindowTitle,
            width: config.TargetWidth,
            height: config.TargetHeight);

        window.Show();

        var stopwatch = Stopwatch.StartNew();
        var previous = stopwatch.Elapsed;

        while (window.ProcessMessages())
        {
            var current = stopwatch.Elapsed;
            var delta = current - previous;
            previous = current;

            engine.Tick(delta);

            Thread.Sleep(1);
        }

        engine.Shutdown();
        return 0;
    }
}
