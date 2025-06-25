using System;
using System.IO;
using System.Windows.Media.Imaging;
using QRCoder;

namespace TerrariaTrader.Helpers
{
    public static class TerrariaQRCodeHelper
    {
        public static BitmapImage GenerateQRCode(string text, int pixelsPerModule = 20)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("QR code text cannot be null or empty.", nameof(text));
            }

            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    var qrCodeAsBitmapBytes = qrCode.GetGraphic(pixelsPerModule);
                    using (var memory = new MemoryStream(qrCodeAsBitmapBytes))
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memory;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze(); // Make it thread-safe

                        return bitmapImage;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate QR code: {ex.Message}", ex);
            }
        }
    }
}