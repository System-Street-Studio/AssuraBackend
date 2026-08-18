using QRCoder;
using ZXing;
using System.Runtime.Versioning;

namespace Assura.Application.Tests;

public class QrCodeGenerationTests
{
    [Fact]
    public void Generated_QrCode_IsDecodable_And_Contains_AssetCode()
    {
        // Arrange
        const string assetCode = "ASSET-QR-TEST-001";

        // Act
        byte[] pngBytes;
        using (var qrGenerator = new QRCodeGenerator())
        using (var qrCodeData = qrGenerator.CreateQrCode(assetCode, QRCodeGenerator.ECCLevel.Q))
        using (var qrCode = new PngByteQRCode(qrCodeData))
        {
            pngBytes = qrCode.GetGraphic(20);
        }

        // Assert basic PNG generation
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 8);

        // Verify PNG magic header (0x89, 'P', 'N', 'G', 0x0D, 0x0A, 0x1A, 0x0A)
        byte[] pngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        for (int i = 0; i < pngHeader.Length; i++)
        {
            Assert.Equal(pngHeader[i], pngBytes[i]);
        }

        // System.Drawing.Common is only supported on Windows
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var reader = new BarcodeReaderGeneric();
        using var bitmap = new System.Drawing.Bitmap(new MemoryStream(pngBytes));
        var luminance = new ZXing.Windows.Compatibility.BitmapLuminanceSource(bitmap);
        var result = reader.Decode(luminance);

        Assert.NotNull(result);
        Assert.Equal(assetCode, result!.Text);
    }
}
