using Build.Tasks.Standard;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using SkiaSharp;
using Svg.Skia;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static Build.BuildContext;

namespace Build.Tasks;

[TaskName("Process Images")]
[IsDependentOn(typeof(RestoreTask))]
[TaskDescription("Processes source logo image to be used in the read-me and as release icons.")]
public sealed class ProcessImagesTask : AsyncFrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.Config == BuildConfigurations.Release;
    }

    public override async Task RunAsync(BuildContext context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        string contentDir = Path.Combine(context.AbsolutePathToRepo, "content");

        // Convert source logo SVG to PNG and save to content folder. Used in the read-me markdown document.
        context.Log.Information($"Creating project logo image (PNG) from source SVG file...");
        string sourceSVGPath = Path.Combine(contentDir, "logo", BuildContext.LOGO_SVG_FILENAME);
        string pngPath = Path.Combine(contentDir, "logo.png");
        await ConvertSvgToPngAsync(sourceSVGPath, pngPath);

        // Create deployment icons using the logo PNG as their basis.
        context.Log.Information($"Creating icons suitable for various deployments...");
        await Task.WhenAll(
            ConvertPngToIcoAsync(pngPath, Path.Combine(contentDir, "favicon.ico")),
            ConvertPngToIcoAsync(pngPath, Path.Combine(contentDir, "extension-icon.ico"), 64),
            ResizePngAsync(pngPath, Path.Combine(contentDir, "icon-175.png"), 175, 175),
            ResizePngAsync(pngPath, Path.Combine(contentDir, "extension-icon.png"), 90, 90),
            ResizePngAsync(pngPath, Path.Combine(contentDir, "package-icon.png"), 128, 128)
        );

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

        byte[] pngData = EncodePng(bitmap);
        await File.WriteAllBytesAsync(targetPngPath, pngData);
    }

    private static async Task ConvertPngToIcoAsync(string sourcePngPath, string targetIcoPath, int iconSize = 32)
    {
        // ref: https://www.meziantou.net/creating-ico-files-from-multiple-images-in-dotnet.htm

        const short NUM_IMAGES = 1;

        using SKBitmap sourceBitmap = SKBitmap.Decode(sourcePngPath)
            ?? throw new InvalidOperationException($"Failed to load PNG image '{sourcePngPath}'.");
        using SKBitmap resizedBitmap = ResizeBitmap(sourceBitmap, iconSize, iconSize);
        byte[] pngData = EncodePng(resizedBitmap);

        // Create the ICO file.
        await using FileStream output = File.OpenWrite(targetIcoPath);
        await using BinaryWriter iconWriter = new(output);

        // Write ICO header.
        iconWriter.Write((byte)0); // reserved
        iconWriter.Write((byte)0);
        iconWriter.Write((short)1); // image type: icon
        iconWriter.Write(NUM_IMAGES); // number of images

        long offset = 6 + (16 * NUM_IMAGES); // ico header (6 bytes) + image directory (16 bytes per image)

        // Write image directory.
        iconWriter.Write((byte)(iconSize >= 256 ? 0 : iconSize));
        iconWriter.Write((byte)(iconSize >= 256 ? 0 : iconSize));
        iconWriter.Write((byte)0); // number of colors
        iconWriter.Write((byte)0); // reserved
        iconWriter.Write((short)0); // color planes
        iconWriter.Write((short)32); // bits per pixel
        iconWriter.Write((uint)pngData.Length); // size of image data
        iconWriter.Write((uint)offset); // offset of image data

        // Write image data.
        iconWriter.Write(pngData);
    }

    private static byte[] EncodePng(SKBitmap bitmap)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static SKBitmap ResizeBitmap(SKBitmap sourceBitmap, int width, int height)
    {
        SKBitmap resizedBitmap = new(width, height, sourceBitmap.ColorType, sourceBitmap.AlphaType);
        using SKCanvas canvas = new(resizedBitmap);
        canvas.Clear(SKColors.Transparent);
        using SKPaint paint = new() { IsAntialias = true, FilterQuality = SKFilterQuality.High };
        canvas.DrawBitmap(sourceBitmap, new SKRect(0, 0, width, height), paint);
        return resizedBitmap;
    }

    private static async Task ResizePngAsync(string sourcePngPath, string targetPngPath, int width, int height)
    {
        using SKBitmap sourceBitmap = SKBitmap.Decode(sourcePngPath)
            ?? throw new InvalidOperationException($"Failed to load PNG image '{sourcePngPath}'.");
        using SKBitmap resizedBitmap = ResizeBitmap(sourceBitmap, width, height);

        byte[] pngData = EncodePng(resizedBitmap);
        await File.WriteAllBytesAsync(targetPngPath, pngData);
    }
}
