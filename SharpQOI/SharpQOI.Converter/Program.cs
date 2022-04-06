using QoiImage = SharpQOI.Image;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

var inputPath = string.Join(' ', args);
var outputPath = Path.Combine(inputPath, "converted");
Directory.CreateDirectory(outputPath);

await Parallel.ForEachAsync(Directory.EnumerateFiles(inputPath, "*.qoi"), async (file, token) =>
{
    var image = await QoiImage.LoadAsync(file);
    using var outputImage = new Image<Rgba32>(image.Width, image.Height);
    for (int y = 0; y < image.Height; y++)
    {
        for (int x = 0; x < image.Width; x++)
        {
            var pixel = image[x, y];
            outputImage[x, y] = new Rgba32(pixel.Red.Value, pixel.Green.Value, pixel.Blue.Value, pixel.Alpha.Value);
        }
    }

    await outputImage.SaveAsPngAsync(Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file)+".png"), token);
});

