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
        BorderStyle = BorderStyle.FixedSingle,
        SizeMode = PictureBoxSizeMode.Zoom,
        Margin = new Padding(12)
    };

    private readonly ComboBox sizeComboBox = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 140,
        Font = new Font(FontFamily.GenericSansSerif, 11f)
    };

    private readonly Button importButton = new()
    {
        Text = "Importar",
        Width = 150,
        Height = 38,
        BackColor = Color.FromArgb(70, 120, 255),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font(FontFamily.GenericSansSerif, 11f, FontStyle.Bold)
    };

    private readonly Button exportButton = new()
    {
        Text = "Exportar a BLP",
        Width = 170,
        Height = 38,
        BackColor = Color.FromArgb(34, 170, 120),
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

    private readonly CheckBox transparencyCheckBox = new()
    {
        Text = "Mostrar fondo transparente",
        Checked = true,
        AutoSize = true,
        ForeColor = Color.White,
        Font = new Font(FontFamily.GenericSansSerif, 10.5f)
    };

    private readonly Label statusLabel = new()
    {
        Text = "Sin imagen cargada",
        AutoSize = true,
        ForeColor = Color.White,
        Font = new Font(FontFamily.GenericSansSerif, 10.5f)
    };

    private Bitmap? sourceBitmap;

    public MainForm()
    {
        Text = "WoW Spell Icon Editor";
        Width = 980;
        Height = 720;
        MinimumSize = new Size(760, 520);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(20, 20, 24);
        ForeColor = Color.White;

        importButton.Click += ImportButton_Click;
        exportButton.Click += ExportButton_Click;
        clearButton.Click += ClearButton_Click;
        sizeComboBox.SelectedIndexChanged += (_, _) => RefreshPreview();
        transparencyCheckBox.CheckedChanged += (_, _) => RefreshPreview();

        sizeComboBox.Items.AddRange(new object[] { 16, 24, 32, 40, 48, 56, 64, 128, 256 });
        sizeComboBox.SelectedIndex = 6;

        BuildLayout();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            BackColor = Color.FromArgb(20, 20, 24),
            ColumnCount = 1,
            RowCount = 2
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var topBar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(26, 26, 31),
            Padding = new Padding(8),
            ColumnCount = 4,
            RowCount = 1
        };

        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.FromArgb(26, 26, 31),
            Padding = new Padding(0)
        };
        buttonPanel.Controls.Add(importButton);
        buttonPanel.Controls.Add(exportButton);
        buttonPanel.Controls.Add(clearButton);

        var sizePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(26, 26, 31),
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0)
        };
        sizePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sizePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));

        var sizeLabel = new Label
        {
            Text = "Tamaño",
            ForeColor = Color.White,
            Font = new Font(FontFamily.GenericSansSerif, 10.5f),
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            TextAlign = ContentAlignment.MiddleLeft
        };

        sizePanel.Controls.Add(sizeLabel, 0, 0);
        sizePanel.Controls.Add(sizeComboBox, 1, 0);

        var checkPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.FromArgb(26, 26, 31),
            Padding = new Padding(0)
        };
        checkPanel.Controls.Add(transparencyCheckBox);

        topBar.Controls.Add(buttonPanel, 0, 0);
        topBar.Controls.Add(sizePanel, 1, 0);
        topBar.Controls.Add(checkPanel, 2, 0);
        topBar.Controls.Add(statusLabel, 3, 0);

        root.Controls.Add(topBar, 0, 0);
        root.Controls.Add(previewBox, 0, 1);
        Controls.Add(root);
    }

    private void ImportButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Imágenes compatibles|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff",
            Title = "Selecciona una imagen para convertir en icono de WoW"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            using var loaded = Image.FromFile(dialog.FileName);
            sourceBitmap = new Bitmap(loaded);

            if (sourceBitmap.Width <= 0 || sourceBitmap.Height <= 0)
            {
                throw new InvalidOperationException("La imagen no tiene tamaño válido.");
            }

            exportButton.Enabled = true;
            RefreshPreview();
            statusLabel.Text = $"Cargada: {sourceBitmap.Width}x{sourceBitmap.Height}px";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"No se pudo abrir la imagen.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            Filter = "Archivo WoW BLP|*.blp",
            DefaultExt = "blp",
            FileName = "spell_icon.blp",
            Title = "Guardar icono exportado como BLP"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var targetSize = GetSelectedSize();
            using var icon = IconProcessor.CreateSquareIcon(sourceBitmap, targetSize, allowPadding: true);
            BlpWriter.WriteBlp(dialog.FileName, icon);

            statusLabel.Text = $"Exportado: {dialog.FileName} ({targetSize}x{targetSize}px)";
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
        statusLabel.Text = "Sin imagen cargada";
    }

    private void RefreshPreview()
    {
        if (sourceBitmap is null)
        {
            previewBox.Image = null;
            return;
        }

        var size = GetSelectedSize();
        using var icon = IconProcessor.CreateSquareIcon(sourceBitmap, size, allowPadding: true);
        previewBox.Image = transparencyCheckBox.Checked ? CreateCheckerboardPreview(icon) : new Bitmap(icon);
    }

    private static Bitmap CreateCheckerboardPreview(Bitmap icon)
    {
        var canvas = new Bitmap(512, 512, PixelFormat.Format32bppArgb);
        var cell = 16;

        using var g = Graphics.FromImage(canvas);
        g.Clear(Color.FromArgb(240, 240, 240));

        for (var y = 0; y < canvas.Height; y += cell)
        {
            for (var x = 0; x < canvas.Width; x += cell)
            {
                var isDark = ((x / cell) + (y / cell)) % 2 == 0;
                using var brush = new SolidBrush(isDark ? Color.FromArgb(230, 230, 230) : Color.FromArgb(180, 180, 180));
                g.FillRectangle(brush, x, y, cell, cell);
            }
        }

        var offsetX = (canvas.Width - icon.Width) / 2;
        var offsetY = (canvas.Height - icon.Height) / 2;
        g.DrawImage(icon, offsetX, offsetY, icon.Width, icon.Height);

        return canvas;
    }

    private int GetSelectedSize()
    {
        return sizeComboBox.SelectedItem is int value ? value : 64;
    }
}

public static class IconProcessor
{
    public static Bitmap CreateSquareIcon(Bitmap source, int size, bool allowPadding)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "El tamaño del icono debe ser mayor que 0.");
        }

        var output = new Bitmap(size, size, PixelFormat.Format32bppArgb);

        using var graphics = Graphics.FromImage(output);
        graphics.Clear(Color.Transparent);
        graphics.CompositingMode = CompositingMode.SourceOver;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        if (allowPadding)
        {
            var scale = Math.Min((float)size / source.Width, (float)size / source.Height);
            var drawWidth = (int)Math.Round(source.Width * scale);
            var drawHeight = (int)Math.Round(source.Height * scale);
            var x = (size - drawWidth) / 2;
            var y = (size - drawHeight) / 2;

            graphics.DrawImage(source, new Rectangle(x, y, drawWidth, drawHeight));
            return output;
        }

        var cropRect = GetCenteredCropRectangle(source.Width, source.Height, size, size);
        graphics.DrawImage(source, new Rectangle(0, 0, size, size), cropRect, GraphicsUnit.Pixel);
        return output;
    }

    private static Rectangle GetCenteredCropRectangle(int srcWidth, int srcHeight, int targetWidth, int targetHeight)
    {
        var scale = Math.Max((float)targetWidth / srcWidth, (float)targetHeight / srcHeight);
        var cropWidth = (int)Math.Round(targetWidth / scale);
        var cropHeight = (int)Math.Round(targetHeight / scale);
        var x = (srcWidth - cropWidth) / 2;
        var y = (srcHeight - cropHeight) / 2;
        return new Rectangle(x, y, cropWidth, cropHeight);
    }
}
