using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace WoWSpellIconEditor;

internal static class Program
{
    private static readonly string[] SupportedExtensions =
    {
        ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff"
    };

    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length >= 2)
        {
            RunCli(args);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    private static void RunCli(string[] args)
    {
        try
        {
            var inputPath = args[0];
            var outputPath = args[1];
            var size = args.Length >= 3 ? ParseSize(args[2]) : 64;

            if (File.Exists(inputPath))
            {
                ExportSingleImage(inputPath, outputPath, size);
                Console.WriteLine($"Exportado: {outputPath} ({size}x{size})");
                return;
            }

            if (Directory.Exists(inputPath))
            {
                var inputDir = new DirectoryInfo(inputPath);
                var outputDir = new DirectoryInfo(outputPath);

                if (!outputDir.Exists)
                {
                    outputDir.Create();
                }

                var files = inputDir
                    .EnumerateFiles("*.*", SearchOption.AllDirectories)
                    .Where(file => SupportedExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(file => file.FullName, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (files.Count == 0)
                {
                    throw new InvalidOperationException($"No se encontraron imágenes válidas en: {inputPath}");
                }

                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(inputDir.FullName, file.FullName);
                    var targetFile = Path.Combine(outputDir.FullName, Path.ChangeExtension(relativePath, ".blp"));
                    var targetDirectory = Path.GetDirectoryName(targetFile);
                    if (!string.IsNullOrEmpty(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    ExportSingleImage(file.FullName, targetFile, size);
                    Console.WriteLine($"Exportado: {targetFile} ({size}x{size})");
                }

                Console.WriteLine($"Procesadas {files.Count} imágenes desde {inputDir.FullName}.");
                return;
            }

            throw new FileNotFoundException($"No existe la ruta: {inputPath}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }

    private static void ExportSingleImage(string inputPath, string outputPath, int size)
    {
        using var source = Image.FromFile(inputPath);
        using var bitmap = new Bitmap(source);
        using var square = CreateSquareIcon(bitmap, size);

        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        BlpWriter.WriteBlp(outputPath, square);
    }

    private static int ParseSize(string value)
    {
        if (!int.TryParse(value, out var size) || size <= 0)
        {
            throw new ArgumentException("El tamaño debe ser un entero positivo.");
        }

        return size;
    }

    private static Bitmap CreateSquareIcon(Bitmap source, int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "El tamaño debe ser mayor que 0.");
        }

        var output = new Bitmap(size, size, PixelFormat.Format32bppArgb);

        using var g = Graphics.FromImage(output);
        g.Clear(Color.Transparent);
        g.CompositingMode = CompositingMode.SourceOver;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var scale = Math.Min((float)size / source.Width, (float)size / source.Height);
        var drawWidth = (int)Math.Round(source.Width * scale);
        var drawHeight = (int)Math.Round(source.Height * scale);
        var x = (size - drawWidth) / 2;
        var y = (size - drawHeight) / 2;

        var rect = new Rectangle(x, y, drawWidth, drawHeight);
        g.DrawImage(source, rect);

        return output;
    }
}
