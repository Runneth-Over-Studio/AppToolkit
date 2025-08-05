using Cake.Common.IO;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using Svg.Skia;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static Build.BuildContext;

namespace Build.Tasks;

[TaskName("Process Images")]
[IsDependentOn(typeof(RestoreTask))]
[TaskDescription("Processes source logo image to be used in the readme, NuGet package, and documentation.")]
public sealed class ProcessImagesTask : AsyncFrostingTask<BuildContext>
{
    private const string LOGO_SVG_FILENAME = "apptoolkit-logo.svg";

    public override bool ShouldRun(BuildContext context)
    {
        return context.Config == BuildConfigurations.Release;
    }

    public override async Task RunAsync(BuildContext context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Convert source icon SVG to PNG and save to root. Used in the readme markdown document and also converted to a NuGet package icon.
        context.Log.Information($"Creating project logo image (PNG) from source SVG file...");
        DirectoryPath sourceContentDirectory = context.RootDirectory + context.Directory("content");
        DirectoryPath sourceLogoDirectory = sourceContentDirectory + context.Directory("logo");
        string sourceSVGPath = System.IO.Path.Combine(sourceLogoDirectory.FullPath, LOGO_SVG_FILENAME);
        string pngPath = System.IO.Path.Combine(sourceContentDirectory.FullPath, "logo.png");
        await ConvertSvgToPngAsync(sourceSVGPath, pngPath);

        // Create NuGet package icon. Microsoft recommends an image resolution of 128x128 and must be either JPEG or PNG.
        context.Log.Information($"Creating NuGet package icon...");
        string packageIconPath = System.IO.Path.Combine(sourceContentDirectory.FullPath, "package-icon.png");
        using (Image image = await Image.LoadAsync(pngPath))
        using (Image resized = image.Clone(ctx => ctx.Resize(128, 128)))
        {
            await resized.SaveAsync(packageIconPath, new PngEncoder());
        }

        stopwatch.Stop();
        double completionTime = Math.Round(stopwatch.Elapsed.TotalSeconds, 1);
        context.Log.Information($"Processing of project images complete ({completionTime}s)");
    }

    private static async Task ConvertSvgToPngAsync(string sourceSvgPath, string targetPngPath)
    {
        // Load the SVG file.
        SKSvg svg = new();
        svg.Load(sourceSvgPath);

        // Determine the canvas size from the SVG's picture bounds.
        if (svg.Picture == null)
        {
            throw new InvalidOperationException("Failed to load SVG picture.");
        }

        var bounds = svg.Picture.CullRect;
        int width = (int)Math.Ceiling(bounds.Width);
        int height = (int)Math.Ceiling(bounds.Height);

        // Convert SVG to bitmap.
        using SKBitmap bitmap = new(width, height, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        using (SKCanvas canvas = new(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svg.Picture);
        }

        // Convert bitmap to an ImageSharp image.
        using Image<Rgba32> image = Image.LoadPixelData<Rgba32>(bitmap.Bytes, bitmap.Width, bitmap.Height);

        // Save as PNG.
        await image.SaveAsync(targetPngPath, new PngEncoder());
    }
}
