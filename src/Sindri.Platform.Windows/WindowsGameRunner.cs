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
        var previousFrameTime = stopwatch.Elapsed;

        var fpsTimer = TimeSpan.Zero;
        var framesThisSecond = 0;
        var currentFps = 0;

        var targetFrameTime = GetTargetFrameTime(config);

        while (!engine.ExitRequested && window.ProcessMessages())
        {
            var frameStart = stopwatch.Elapsed;

            var delta = frameStart - previousFrameTime;
            previousFrameTime = frameStart;

            input.Update();

            engine.Tick(delta);

            if (engine.ActiveScene is IRenderableScene renderableScene)
            {
                renderableScene.Render(graphics);
            }

            framesThisSecond++;
            fpsTimer += delta;

            if (fpsTimer >= TimeSpan.FromSeconds(1))
            {
                currentFps = framesThisSecond;
                framesThisSecond = 0;
                fpsTimer -= TimeSpan.FromSeconds(1);

                if (config.ShowFrameRateInTitle)
                {
                    var viewport = graphics.ViewportSize;
                    window.SetTitle($"{config.WindowTitle} - {currentFps} FPS - {viewport.Width}x{viewport.Height}");
                }
            }

            if (config.LimitFrameRate)
            {
                WaitForNextFrame(stopwatch, frameStart, targetFrameTime);
            }
        }

        engine.Shutdown();
        return 0;
    }

    private static TimeSpan GetTargetFrameTime(EngineConfig config)
    {
        var targetFps = Math.Max(1, config.TargetFramesPerSecond);
        return TimeSpan.FromSeconds(1.0 / targetFps);
    }

    private static void WaitForNextFrame(Stopwatch stopwatch, TimeSpan frameStart, TimeSpan targetFrameTime)
    {
        while (true)
        {
            var elapsed = stopwatch.Elapsed - frameStart;
            var remaining = targetFrameTime - elapsed;

            if (remaining <= TimeSpan.Zero)
            {
                break;
            }

            if (remaining.TotalMilliseconds > 2)
            {
                Thread.Sleep(1);
            }
            else
            {
                Thread.Yield();
            }
        }
    }
}
