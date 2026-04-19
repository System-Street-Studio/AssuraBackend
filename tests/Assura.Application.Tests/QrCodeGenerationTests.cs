using QRCoder;
using ZXing;
using System.Runtime.Versioning;

namespace Assura.Application.Tests;

public class QrCodeGenerationTests
{
    [Fact]
    [SupportedOSPlatform("windows")]
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

        // Assert
        var reader = new BarcodeReaderGeneric();
        using var bitmap = new System.Drawing.Bitmap(new MemoryStream(pngBytes));
        var luminance = new ZXing.Windows.Compatibility.BitmapLuminanceSource(bitmap);
        var result = reader.Decode(luminance);

        Assert.NotNull(result);
        Assert.Equal(assetCode, result!.Text);
    }
}
