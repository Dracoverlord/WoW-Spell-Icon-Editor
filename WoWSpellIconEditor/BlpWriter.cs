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

        const uint type = 1;          // 1 = direct texture / uncompressed
        const uint encoding = 1;      // 1 = direct / non-compressed
        const uint alphaDepth = 8;    // 8-bit alpha
        const uint alphaEncoding = 0; // 0 = none special
        const uint hasMipmap = 0;     // sin mipmaps en esta versión

        var width = (uint)image.Width;
        var height = (uint)image.Height;
        var pixelDataSize = (uint)(image.Width * image.Height * 4);

        // BLP1 header: 32 bytes de campos + 64 bytes offsets + 64 bytes sizes = 160 bytes
        var header = new byte[160];
        Array.Copy(Encoding.ASCII.GetBytes("BLP1"), 0, header, 0, 4);

        WriteUInt32(header, 4, type);
        WriteUInt32(header, 8, encoding);
        WriteUInt32(header, 12, alphaDepth);
        WriteUInt32(header, 16, alphaEncoding);
        WriteUInt32(header, 20, hasMipmap);
        WriteUInt32(header, 24, width);
        WriteUInt32(header, 28, height);

        // offsets: solo hay 1 mipmap, empieza justo después del header (160 bytes)
        WriteUInt32(header, 32, 160U);
        for (var i = 1; i < 16; i++)
        {
            WriteUInt32(header, 32 + (i * 4), 0U);
        }

        // sizes: solo 1 mipmap
        WriteUInt32(header, 96, pixelDataSize);
        for (var i = 1; i < 16; i++)
        {
            WriteUInt32(header, 96 + (i * 4), 0U);
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

                    // pixel format BGRA para BLP1 directo con alpha
                    rgbaData[index++] = rawPixels[pixelOffset + 0]; // B
                    rgbaData[index++] = rawPixels[pixelOffset + 1]; // G
                    rgbaData[index++] = rawPixels[pixelOffset + 2]; // R
                    rgbaData[index++] = rawPixels[pixelOffset + 3]; // A
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
