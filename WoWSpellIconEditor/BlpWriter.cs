using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace WoWSpellIconEditor;

public static class BlpWriter
{
    public static void WriteBlp(string filePath, Bitmap image)
    {
        if (image.Width != image.Height)
        {
            throw new InvalidOperationException("Los iconos BLP deben ser cuadrados.");
        }

        if (image.Width <= 0 || image.Height <= 0)
        {
            throw new InvalidOperationException("La imagen no tiene dimensiones válidas.");
        }

        if (!IsPowerOfTwo(image.Width) || image.Width < 4 || image.Width > 2048)
        {
            throw new InvalidOperationException("El tamaño del icono debe ser una potencia de dos válida para WoW 3.3.5 (mínimo 4, máximo 2048). ");
        }

        var mipImages = BuildMipChain(image);
        var header = new byte[160];

        Array.Copy(Encoding.ASCII.GetBytes("BLP1"), 0, header, 0, 4);

        const uint type = 1;
        const uint encoding = 1;
        const uint alphaDepth = 8;
        const uint alphaEncoding = 0;
        const uint hasMipmap = 1;

        WriteUInt32(header, 4, type);
        WriteUInt32(header, 8, encoding);
        WriteUInt32(header, 12, alphaDepth);
        WriteUInt32(header, 16, alphaEncoding);
        WriteUInt32(header, 20, hasMipmap);
        WriteUInt32(header, 24, (uint)image.Width);
        WriteUInt32(header, 28, (uint)image.Height);

        var dataOffset = 160U;
        for (var i = 0; i < 16; i++)
        {
            if (i < mipImages.Count)
            {
                WriteUInt32(header, 32 + (i * 4), dataOffset);
                dataOffset += (uint)mipImages[i].Length;
            }
            else
            {
                WriteUInt32(header, 32 + (i * 4), 0U);
            }
        }

        for (var i = 0; i < 16; i++)
        {
            if (i < mipImages.Count)
            {
                WriteUInt32(header, 96 + (i * 4), (uint)mipImages[i].Length);
            }
            else
            {
                WriteUInt32(header, 96 + (i * 4), 0U);
            }
        }

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        stream.Write(header, 0, header.Length);

        foreach (var mip in mipImages)
        {
            stream.Write(mip, 0, mip.Length);
        }
    }

    private static List<byte[]> BuildMipChain(Bitmap source)
    {
        var mipImages = new List<byte[]>();
        var current = new Bitmap(source);

        var limit = 16;
        for (var level = 0; level < limit; level++)
        {
            mipImages.Add(ToBgra8(current));

            if (current.Width == 1 && current.Height == 1)
            {
                break;
            }

            var nextWidth = Math.Max(1, current.Width / 2);
            var nextHeight = Math.Max(1, current.Height / 2);

            if (nextWidth == current.Width && nextHeight == current.Height)
            {
                break;
            }

            current = Downsample(current, nextWidth, nextHeight);
        }

        return mipImages;
    }

    private static Bitmap Downsample(Bitmap source, int targetWidth, int targetHeight)
    {
        var output = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);

        using var graphics = Graphics.FromImage(output);
        graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

        graphics.DrawImage(source, new Rectangle(0, 0, targetWidth, targetHeight));
        return output;
    }

    private static byte[] ToBgra8(Bitmap bitmap)
    {
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try
        {
            var raw = new byte[Math.Abs(bitmapData.Stride) * bitmap.Height];
            Marshal.Copy(bitmapData.Scan0, raw, 0, raw.Length);

            var bgra = new byte[bitmap.Width * bitmap.Height * 4];
            var index = 0;

            for (var y = 0; y < bitmap.Height; y++)
            {
                var rowOffset = y * bitmapData.Stride;
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var pixelOffset = rowOffset + (x * 4);
                    bgra[index++] = raw[pixelOffset + 0];
                    bgra[index++] = raw[pixelOffset + 1];
                    bgra[index++] = raw[pixelOffset + 2];
                    bgra[index++] = raw[pixelOffset + 3];
                }
            }

            return bgra;
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }
    }

    private static bool IsPowerOfTwo(int value)
    {
        if (value <= 0)
        {
            return false;
        }

        return (value & (value - 1)) == 0;
    }

    private static void WriteUInt32(byte[] buffer, int offset, uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Copy(bytes, 0, buffer, offset, bytes.Length);
    }
}
