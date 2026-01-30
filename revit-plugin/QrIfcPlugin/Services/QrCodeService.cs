using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using QRCoder;

namespace QrIfcPlugin.Services
{
    /// <summary>
    /// Service for generating QR codes
    /// </summary>
    public class QrCodeService
    {
        /// <summary>
        /// Generate a QR code PNG image from a URL
        /// </summary>
        /// <param name="url">The URL to encode</param>
        /// <param name="pixelSize">Size in pixels (default 512)</param>
        /// <returns>PNG image as byte array</returns>
        public byte[] GenerateQrCode(string url, int pixelSize = 512)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be empty", nameof(url));
            }

            if (pixelSize < 128 || pixelSize > 2048)
            {
                throw new ArgumentException("Pixel size must be between 128 and 2048", nameof(pixelSize));
            }

            using (var qrGenerator = new QRCodeGenerator())
            {
                // Generate QR code data with Q error correction level (25% recovery)
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

                using (var qrCode = new QRCode(qrCodeData))
                {
                    // Generate bitmap with white background and black foreground
                    using (var qrBitmap = qrCode.GetGraphic(
                        pixelsPerModule: pixelSize / 25, // Approximately 25 modules per QR code
                        darkColor: Color.Black,
                        lightColor: Color.White,
                        drawQuietZones: true
                    ))
                    {
                        // Convert to PNG byte array
                        using (var ms = new MemoryStream())
                        {
                            qrBitmap.Save(ms, ImageFormat.Png);
                            return ms.ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save QR code to a temporary file
        /// </summary>
        /// <param name="url">The URL to encode</param>
        /// <param name="pixelSize">Size in pixels</param>
        /// <returns>Path to the temporary PNG file</returns>
        public string GenerateQrCodeToFile(string url, int pixelSize = 512)
        {
            var qrBytes = GenerateQrCode(url, pixelSize);
            var tempPath = Path.Combine(Path.GetTempPath(), $"qr_{Guid.NewGuid()}.png");
            File.WriteAllBytes(tempPath, qrBytes);
            return tempPath;
        }
    }
}
