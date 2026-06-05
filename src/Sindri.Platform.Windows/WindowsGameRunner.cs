using System.Diagnostics;
using System.Runtime.Versioning;
using Sindri.Core;
using Sindri.Graphics;
using Sindri.Input;
using Sindri.Platform.Windows.Graphics;
using Sindri.Platform.Windows.Input;
using Sindri.Platform.Windows.Native;

namespace Sindri.Platform.Windows;

[SupportedOSPlatform("windows")]
public static class WindowsGameRunner
{
    public static int Run(SindriGame game)
    {
        ArgumentNullException.ThrowIfNull(game);

        var engine = new EngineHost(game);

        var input = new WindowsKeyboardInput();
        engine.Services.Register<IInputDevice>(input);

        engine.Initialize();

        var config = engine.Config;

        using var window = NativeWindow.Create(
            title: config.WindowTitle,
            width: config.TargetWidth,
            height: config.TargetHeight);

        var graphics = new WindowsGdiGraphicsDevice(window.Handle);

        window.Show();

        var stopwatch = Stopwatch.StartNew();
        var previous = stopwatch.Elapsed;

        while (!engine.ExitRequested && window.ProcessMessages())
        {
            var current = stopwatch.Elapsed;
            var delta = current - previous;
            previous = current;

            input.Update();

            engine.Tick(delta);

            if (engine.ActiveScene is IRenderableScene renderableScene)
            {
                renderableScene.Render(graphics);
            }

            Thread.Sleep(1);
        }

        engine.Shutdown();
        return 0;
    }
}
