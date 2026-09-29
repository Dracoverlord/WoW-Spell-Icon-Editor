using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace WoWSpellIconEditor;

public class MainForm : Form
{
    private readonly PictureBox previewBox = new()
    {
        Dock = DockStyle.Fill,
        BackColor = Color.FromArgb(20, 20, 24),
        Margin = new Padding(12),
        BorderStyle = BorderStyle.FixedSingle,
        SizeMode = PictureBoxSizeMode.Zoom
    };

    private readonly ComboBox sizeComboBox = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 140,
        Font = new Font(FontFamily.GenericSansSerif, 11f)
    };

    private readonly Button importButton = new()
    {
        Text = "Importar imagen",
        Width = 170,
        Height = 38,
        BackColor = Color.FromArgb(70, 122, 255),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font(FontFamily.GenericSansSerif, 11f, FontStyle.Bold)
    };

    private readonly Button exportButton = new()
    {
        Text = "Exportar a BLP",
        Width = 170,
        Height = 38,
        BackColor = Color.FromArgb(28, 152, 112),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font(FontFamily.GenericSansSerif, 11f, FontStyle.Bold),
        Enabled = false
    };

    private readonly Button clearButton = new()
    {
        Text = "Limpiar",
        Width = 120,
        Height = 38,
        BackColor = Color.FromArgb(120, 120, 130),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font(FontFamily.GenericSansSerif, 11f, FontStyle.Bold)
    };

    private readonly Label infoLabel = new()
    {
        Text = "Sin imagen cargada",
        AutoSize = true,
        ForeColor = Color.White,
        Font = new Font(FontFamily.GenericSansSerif, 11f)
    };

    private Bitmap? sourceBitmap;

    public MainForm()
    {
        Text = "WoW Spell Icon Editor";
        Width = 980;
        Height = 720;
        MinimumSize = new Size(700, 500);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(17, 17, 22);

        importButton.Click += ImportButton_Click;
        exportButton.Click += ExportButton_Click;
        clearButton.Click += ClearButton_Click;
        sizeComboBox.SelectedIndexChanged += (_, _) => UpdatePreview();

        sizeComboBox.Items.AddRange(new object[] { 16, 32, 40, 56, 64, 128, 256 });
        sizeComboBox.SelectedIndex = 4;

        BuildLayout();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            BackColor = Color.FromArgb(17, 17, 22),
            ColumnCount = 2,
            RowCount = 2,
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = false,
            Padding = new Padding(0),
            BackColor = Color.FromArgb(17, 17, 22),
            BorderStyle = BorderStyle.None
        };

        toolbar.Controls.Add(importButton);
        toolbar.Controls.Add(exportButton);
        toolbar.Controls.Add(clearButton);

        var sizePanel = new Panel
        {
            Width = 170,
            Height = 38,
            BackColor = Color.FromArgb(17, 17, 22)
        };

        var sizeLabel = new Label
        {
            Text = "Tamaño del icono",
            ForeColor = Color.White,
            Font = new Font(FontFamily.GenericSansSerif, 10f),
            AutoSize = true,
            Location = new Point(0, 9)
        };

        sizeComboBox.Location = new Point(0, 0);
        sizePanel.Controls.Add(sizeLabel);
        sizePanel.Controls.Add(sizeComboBox);

        var labelRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(17, 17, 22),
            Padding = new Padding(0),
        };

        labelRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        labelRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));

        labelRow.Controls.Add(infoLabel, 0, 0);
        labelRow.Controls.Add(sizePanel, 1, 0);

        root.Controls.Add(toolbar, 0, 0);
        root.Controls.Add(labelRow, 0, 0);

        root.SetColumnSpan(toolbar, 2);
        root.SetRowSpan(toolbar, 1);

        root.Controls.Add(previewBox, 0, 1);
        root.SetColumnSpan(previewBox, 2);

        Controls.Add(root);
    }

    private void ImportButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff",
            Title = "Selecciona una imagen para convertir a icono"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            using var image = Image.FromFile(dialog.FileName);
            sourceBitmap = new Bitmap(image);

            if (sourceBitmap.Width <= 0 || sourceBitmap.Height <= 0)
            {
                throw new InvalidOperationException("La imagen no tiene dimensiones válidas.");
            }

            exportButton.Enabled = true;
            UpdatePreview();
            infoLabel.Text = $"Imagen cargada: {sourceBitmap.Width}x{sourceBitmap.Height}px";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"No se pudo cargar la imagen.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportButton_Click(object? sender, EventArgs e)
    {
        if (sourceBitmap is null)
        {
            MessageBox.Show(this, "Primero importa una imagen.", "Sin imagen", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "WoW BLP|*.blp",
            DefaultExt = "blp",
            FileName = "spell_icon.blp",
            Title = "Guardar icono en formato BLP"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var exportSize = GetSelectedSize();
            using var square = CreateSquareIcon(sourceBitmap, exportSize);
            BlpWriter.WriteBlp(dialog.FileName, square);

            infoLabel.Text = $"Archivo exportado: {dialog.FileName} ({exportSize}x{exportSize}px)";
            MessageBox.Show(this, "El icono se exportó correctamente como BLP.", "Exportación completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"No se pudo exportar el archivo.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        sourceBitmap = null;
        previewBox.Image = null;
        exportButton.Enabled = false;
        infoLabel.Text = "Sin imagen cargada";
    }

    private void UpdatePreview()
    {
        if (sourceBitmap is null)
        {
            previewBox.Image = null;
            return;
        }

        var size = GetSelectedSize();
        var square = CreateSquareIcon(sourceBitmap, size);
        previewBox.Image = square;
    }

    private int GetSelectedSize()
    {
        return sizeComboBox.SelectedItem is int value ? value : 64;
    }

    private static Bitmap CreateSquareIcon(Bitmap source, int size)
    {
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

public static class BlpWriter
{
    public static void WriteBlp(string filePath, Bitmap image)
    {
        const uint compression = 1;
        const uint flags = 8;

        var width = (uint)image.Width;
        var height = (uint)image.Height;
        var dataSize = width * height * 4U;

        using var stream = File.Create(filePath);
        using var writer = new BinaryWriter(stream);

        writer.Write(Encoding.ASCII.GetBytes("BLP1"));
        writer.Write(compression);
        writer.Write(flags);
        writer.Write(width);
        writer.Write(height);
        writer.Write(0U);
        writer.Write(0U);

        for (var i = 0; i < 16; i++)
        {
            writer.Write(i == 0 ? 156U : 0U);
        }

        for (var i = 0; i < 16; i++)
        {
            writer.Write(i == 0 ? dataSize : 0U);
        }

        var rect = new Rectangle(0, 0, image.Width, image.Height);
        var data = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try
        {
            var bytes = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var offset = (y * data.Stride) + (x * 4);
                    var b = bytes[offset];
                    var g = bytes[offset + 1];
                    var r = bytes[offset + 2];
                    var a = bytes[offset + 3];

                    writer.Write(b);
                    writer.Write(g);
                    writer.Write(r);
                    writer.Write(a);
                }
            }
        }
        finally
        {
            image.UnlockBits(data);
        }
    }
}
