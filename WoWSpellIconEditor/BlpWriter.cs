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

        const uint compression = 1;
        const uint flags = 8;
        var width = (uint)image.Width;
        var height = (uint)image.Height;
        var pixelDataSize = (uint)(image.Width * image.Height * 4);

        var header = new byte[156];
        Array.Copy(Encoding.ASCII.GetBytes("BLP1"), 0, header, 0, 4);

        WriteUInt32(header, 4, compression);
        WriteUInt32(header, 8, flags);
        WriteUInt32(header, 12, width);
        WriteUInt32(header, 16, height);
        WriteUInt32(header, 20, 0U);
        WriteUInt32(header, 24, 0U);

        WriteUInt32(header, 28, 156U);
        for (var i = 1; i < 16; i++)
        {
            WriteUInt32(header, 28 + (i * 4), 0U);
        }

        WriteUInt32(header, 92, pixelDataSize);
        for (var i = 1; i < 16; i++)
        {
            WriteUInt32(header, 92 + (i * 4), 0U);
        }

        var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try
        {
            var rawPixels = new byte[Math.Abs(bitmapData.Stride) * image.Height];
            Marshal.Copy(bitmapData.Scan0, rawPixels, 0, rawPixels.Length);

            var rgbaData = new byte[pixelDataSize];
            var index = 0;

            for (var y = 0; y < image.Height; y++)
            {
                var rowOffset = y * bitmapData.Stride;
                for (var x = 0; x < image.Width; x++)
                {
                    var pixelOffset = rowOffset + (x * 4);
                    rgbaData[index++] = rawPixels[pixelOffset + 0];
                    rgbaData[index++] = rawPixels[pixelOffset + 1];
                    rgbaData[index++] = rawPixels[pixelOffset + 2];
                    rgbaData[index++] = rawPixels[pixelOffset + 3];
                }
            }

            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            stream.Write(header, 0, header.Length);
            stream.Write(rgbaData, 0, rgbaData.Length);
        }
        finally
        {
            image.UnlockBits(bitmapData);
        }
    }

    private static void WriteUInt32(byte[] buffer, int offset, uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Copy(bytes, 0, buffer, offset, bytes.Length);
    }
}
