namespace LMS.Infrastructure.Configurations;

// Pixel coordinates are calibrated against the source template image's own
// resolution (top-left origin). If the uploaded template PNG is re-exported
// at a different size, these values must be recalibrated via appsettings.
public class CertificateTemplateLayoutOptions
{
    public string TemplateBlobName { get; set; } = "Certificate_template.png";

    public string FontFamily { get; set; } = "Arial";

    public string DateFormat { get; set; } = "MMMM dd, yyyy";

    public CertificateFieldLayout StudentName { get; set; } = new()
    {
        X = 760,
        Y = 458,
        FontSize = 34,
        Bold = true,
        ColorHex = "#0D2B66",
        Alignment = "Center"
    };

    public CertificateFieldLayout LearningPathName { get; set; } = new()
    {
        X = 760,
        Y = 590,
        FontSize = 26,
        Bold = true,
        ColorHex = "#1E3A8A",
        Alignment = "Center"
    };

    public CertificateFieldLayout CompletionDate { get; set; } = new()
    {
        X = 366,
        Y = 775,
        FontSize = 16,
        Bold = false,
        ColorHex = "#1A1A2E",
        Alignment = "Center"
    };

    public CertificateFieldLayout CredentialId { get; set; } = new()
    {
        X = 756,
        Y = 775,
        FontSize = 16,
        Bold = false,
        ColorHex = "#1A1A2E",
        Alignment = "Center"
    };

    public CertificateFieldLayout CertificateNumber { get; set; } = new()
    {
        X = 1146,
        Y = 775,
        FontSize = 16,
        Bold = false,
        ColorHex = "#1A1A2E",
        Alignment = "Center"
    };
}

public class CertificateFieldLayout
{
    public float X { get; set; }

    public float Y { get; set; }

    public float FontSize { get; set; } = 20;

    public bool Bold { get; set; }

    public string ColorHex { get; set; } = "#000000";

    // Left, Center, or Right
    public string Alignment { get; set; } = "Center";
}
