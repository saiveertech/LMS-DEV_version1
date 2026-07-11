using LMS.Application.Common;
using LMS.Application.Features.Certificate.Services;
using LMS.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LMS.Infrastructure.Services;

public class CertificateTemplateRenderer : ICertificateTemplateRenderer
{
    private readonly IBlobStorageService _blobService;
    private readonly AzureStorageSettings _storageSettings;
    private readonly CertificateTemplateLayoutOptions _layout;

    public CertificateTemplateRenderer(
        IBlobStorageService blobService,
        IOptions<AzureStorageSettings> storageOptions,
        IOptions<CertificateTemplateLayoutOptions> layoutOptions)
    {
        _blobService = blobService;
        _storageSettings = storageOptions.Value;
        _layout = layoutOptions.Value;
    }

    public async Task<byte[]> RenderPngAsync(
        string studentName,
        string learningPathName,
        DateTime completionDate,
        string credentialId,
        string certificateNumber)
    {
        var templateBytes = await _blobService.DownloadAsync(
            _storageSettings.CertificateTemplatePath,
            _layout.TemplateBlobName);

        using var image = Image.Load<Rgba32>(templateBytes);

        var fontFamily = ResolveFontFamily(_layout.FontFamily);

        image.Mutate(ctx =>
        {
            DrawField(ctx, fontFamily, _layout.StudentName, studentName);
            DrawField(ctx, fontFamily, _layout.LearningPathName, learningPathName);
            DrawField(ctx, fontFamily, _layout.CompletionDate, completionDate.ToString(_layout.DateFormat));
            DrawField(ctx, fontFamily, _layout.CredentialId, credentialId);
            DrawField(ctx, fontFamily, _layout.CertificateNumber, certificateNumber);
        });

        using var output = new MemoryStream();

        image.SaveAsPng(output);

        return output.ToArray();
    }

    public byte[] WrapPngInPdf(byte[] pngBytes)
    {
        using var image = Image.Load<Rgba32>(pngBytes);

        // Convert pixel dimensions (96 DPI) to PDF points (72 DPI) so the page
        // matches the certificate image exactly, edge to edge.
        var widthPoints = image.Width * 72f / 96f;
        var heightPoints = image.Height * 72f / 96f;

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(widthPoints, heightPoints);
                page.Margin(0);
                page.Content().Image(pngBytes).FitArea();
            });
        });

        return document.GeneratePdf();
    }

    private static FontFamily ResolveFontFamily(string requestedFamily)
    {
        if (SystemFonts.TryGet(requestedFamily, out var family))
        {
            return family;
        }

        return SystemFonts.Families.First();
    }

    private static void DrawField(
        IImageProcessingContext ctx,
        FontFamily fontFamily,
        CertificateFieldLayout field,
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var font = fontFamily.CreateFont(
            field.FontSize,
            field.Bold ? FontStyle.Bold : FontStyle.Regular);

        var color = Color.ParseHex(field.ColorHex);

        var horizontalAlignment = field.Alignment switch
        {
            "Left" => HorizontalAlignment.Left,
            "Right" => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Center
        };

        var textOptions = new RichTextOptions(font)
        {
            Origin = new PointF(field.X, field.Y),
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = VerticalAlignment.Center
        };

        ctx.DrawText(textOptions, text, color);
    }
}
