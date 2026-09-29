using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;

namespace WoWSpellIconEditor;

internal static class Program
{
    private static readonly string[] SupportedExtensions =
    {
        ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff"
    };

    private static readonly string WoWInterfaceIconsPath = Path.Combine("Interface", "ICONS");

    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
            return;
        }

        RunCli(args);
    }

    private static void RunCli(string[] args)
    {
        try
        {
            if (args[0].Equals("--help", StringComparison.OrdinalIgnoreCase) || 
                args[0].Equals("-h", StringComparison.OrdinalIgnoreCase))
            {
                PrintHelp();
                return;
            }

            if (args[0].Equals("--batch", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length < 2)
                {
                    throw new ArgumentException("Uso: --batch <input_folder> [size]. Ejemplo: --batch ./spells 64");
                }

                var inputFolder = args[1];
                var size = args.Length >= 3 ? ParseSize(args[2]) : 64;

                BatchExportToWoWIcons(inputFolder, size);
                return;
            }

            if (args.Length < 2)
            {
                throw new ArgumentException("Uso: <input> <output> [size] | --batch <folder> [size] | --help");
            }

            var inputPath = args[0];
            var outputPath = args[1];
            var outputSize = args.Length >= 3 ? ParseSize(args[2]) : 64;

            if (File.Exists(inputPath))
            {
                ExportSingleImage(inputPath, outputPath, outputSize);
                Console.WriteLine($"✓ Exportado: {outputPath} ({outputSize}x{outputSize})");
                return;
            }

            if (Directory.Exists(inputPath))
            {
                BatchExport(inputPath, outputPath, outputSize);
                return;
            }

            throw new FileNotFoundException($"No existe la ruta: {inputPath}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Error: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine(@"
WoW Spell Icon Editor - Exportador BLP1 para World of Warcraft 3.3.5

USO:
  Sin argumentos:
    dotnet run --project WoWSpellIconEditor/WoWSpellIconEditor.csproj
    Abre la interfaz gráfica.

  Archivo único:
    dotnet run --project WoWSpellIconEditor/WoWSpellIconEditor.csproj -- input.png output.blp [size]

  Carpeta entera:
    dotnet run --project WoWSpellIconEditor/WoWSpellIconEditor.csproj -- ./input ./output [size]

  Exportar a Interface\ICONS (batch):
    dotnet run --project WoWSpellIconEditor/WoWSpellIconEditor.csproj -- --batch ./spells [size]

PARÁMETROS:
  input           Archivo o carpeta de entrada (PNG, JPG, BMP, GIF, etc.)
  output          Archivo o carpeta de salida (.blp)
  size            Tamaño del icono (por defecto 64). Soportados: 4-2048 (potencias de 2)
  --batch         Modo lote: exporta directamente a Interface\ICONS
  --help, -h      Muestra esta ayuda

EJEMPLOS:
  # Exportar un icono individual
  dotnet run --project WoWSpellIconEditor/WoWSpellIconEditor.csproj -- spell.png spell.blp 64

  # Procesar una carpeta completa de hechizos
  dotnet run --project WoWSpellIconEditor/WoWSpellIconEditor.csproj -- ./my_spells ./output 64

  # Exportar directamente a Interface\ICONS con nombres automáticos
  dotnet run --project WoWSpellIconEditor/WoWSpellIconEditor.csproj -- --batch ./my_spells 64
");
    }

    private static void BatchExportToWoWIcons(string inputFolder, int size)
    {
        if (!Directory.Exists(inputFolder))
        {
            throw new DirectoryNotFoundException($"No existe la carpeta: {inputFolder}");
        }

        var inputDir = new DirectoryInfo(inputFolder);
        var outputDir = new DirectoryInfo(WoWInterfaceIconsPath);

        if (!outputDir.Exists)
        {
            outputDir.Create();
            Console.WriteLine($"✓ Carpeta creada: {WoWInterfaceIconsPath}");
        }

        var files = inputDir
            .EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Where(file => SupportedExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
            .OrderBy(file => file.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
        {
            throw new InvalidOperationException($"No se encontraron imágenes en: {inputFolder}");
        }

        Console.WriteLine($"\n=== Procesando {files.Count} iconos de hechizos ===\n");

        var successCount = 0;
        var failureCount = 0;

        foreach (var file in files)
        {
            try
            {
                var spellName = SanitizeSpellName(Path.GetFileNameWithoutExtension(file.Name));
                var targetFile = Path.Combine(outputDir.FullName, $"{spellName}.blp");

                ExportSingleImage(file.FullName, targetFile, size);
                Console.WriteLine($"✓ {spellName,-40} → spell_{spellName}.blp ({size}x{size})");
                successCount++;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"✗ {file.Name,-40} → Error: {ex.Message}");
                failureCount++;
            }
        }

        Console.WriteLine($"\n=== Resumen ===");
        Console.WriteLine($"Procesados exitosamente: {successCount}");
        if (failureCount > 0)
        {
            Console.WriteLine($"Errores: {failureCount}");
        }
        Console.WriteLine($"Ubicación: {Path.GetFullPath(outputDir.FullName)}");
    }

    private static void BatchExport(string inputPath, string outputPath, int size)
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

        Console.WriteLine($"\n=== Procesando {files.Count} imágenes ===\n");

        var successCount = 0;
        var failureCount = 0;

        foreach (var file in files)
        {
            try
            {
                var relativePath = Path.GetRelativePath(inputDir.FullName, file.FullName);
                var targetRelative = Path.ChangeExtension(relativePath, ".blp");
                var targetFile = Path.Combine(outputDir.FullName, targetRelative);

                var targetDirectory = Path.GetDirectoryName(targetFile);
                if (!string.IsNullOrEmpty(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                ExportSingleImage(file.FullName, targetFile, size);
                Console.WriteLine($"✓ {Path.GetFileName(file.Name),-40} → {targetRelative} ({size}x{size})");
                successCount++;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"✗ {file.Name,-40} → Error: {ex.Message}");
                failureCount++;
            }
        }

        Console.WriteLine($"\n=== Resumen ===");
        Console.WriteLine($"Procesados exitosamente: {successCount}");
        if (failureCount > 0)
        {
            Console.WriteLine($"Errores: {failureCount}");
        }
    }

    private static string SanitizeSpellName(string name)
    {
        var sb = new StringBuilder();
        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
            {
                sb.Append(char.ToLowerInvariant(c));
            }
            else if (char.IsWhiteSpace(c))
            {
                sb.Append('_');
            }
        }

        var result = sb.ToString();
        while (result.Contains("__"))
        {
            result = result.Replace("__", "_");
        }

        return result.Trim('_');
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

        if (!IsPowerOfTwo(size) || size < 4 || size > 2048)
        {
            throw new ArgumentException("El tamaño debe ser una potencia de dos entre 4 y 2048.");
        }

        return size;
    }

    private static bool IsPowerOfTwo(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
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
