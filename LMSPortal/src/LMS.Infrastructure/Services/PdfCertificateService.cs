using LMS.Application.Features.Certificate.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LMS.Infrastructure.Services;

public class PdfCertificateService : IPdfCertificateService
{
    public byte[] GenerateCertificatePdf(
        string studentName,
        string courseName,
        DateTime completionDate,
        string credentialId,
        string certificateId)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginTop(30);
                page.MarginBottom(30);
                page.MarginLeft(40);
                page.MarginRight(40);

                page.Content().Column(column =>
                {
                    // ── Outer decorative border ──
                    column.Item()
                        .Border(3)
                        .BorderColor(Colors.Amber.Darken2)
                        .Padding(15)
                        .Border(1)
                        .BorderColor(Colors.Amber.Medium)
                        .Padding(20)
                        .Column(inner =>
                        {
                            // ── Company Logo / Brand Header ──
                            inner.Item()
                                .AlignCenter()
                                .Text("SkillToRole")
                                .FontSize(28)
                                .Bold()
                                .FontColor(Colors.Amber.Darken3);

                            inner.Item()
                                .AlignCenter()
                                .Text("LEARNING MANAGEMENT SYSTEM")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1)
                                .LetterSpacing(3f);

                            inner.Item().Height(20);

                            // ── Decorative line ──
                            inner.Item()
                                .AlignCenter()
                                .Width(400)
                                .Height(2)
                                .Background(Colors.Amber.Darken2);

                            inner.Item().Height(20);

                            // ── Certificate Title ──
                            inner.Item()
                                .AlignCenter()
                                .Text("CERTIFICATE OF COMPLETION")
                                .FontSize(26)
                                .Bold()
                                .FontColor(Colors.Grey.Darken3)
                                .LetterSpacing(2f);

                            inner.Item().Height(15);

                            // ── Subtitle ──
                            inner.Item()
                                .AlignCenter()
                                .Text("This is to certify that")
                                .FontSize(14)
                                .FontColor(Colors.Grey.Darken1);

                            inner.Item().Height(10);

                            // ── Student Name ──
                            inner.Item()
                                .AlignCenter()
                                .Text(studentName)
                                .FontSize(30)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3)
                                .Italic();

                            inner.Item().Height(10);

                            // ── Decorative line under name ──
                            inner.Item()
                                .AlignCenter()
                                .Width(300)
                                .Height(1)
                                .Background(Colors.Grey.Lighten1);

                            inner.Item().Height(10);

                            // ── Course completion text ──
                            inner.Item()
                                .AlignCenter()
                                .Text("has successfully completed the course")
                                .FontSize(14)
                                .FontColor(Colors.Grey.Darken1);

                            inner.Item().Height(10);

                            // ── Course Name ──
                            inner.Item()
                                .AlignCenter()
                                .Text(courseName)
                                .FontSize(22)
                                .Bold()
                                .FontColor(Colors.Amber.Darken3);

                            inner.Item().Height(15);

                            // ── Completion Date ──
                            inner.Item()
                                .AlignCenter()
                                .Text($"Completed on {completionDate:MMMM dd, yyyy}")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken2);

                            inner.Item().Height(25);

                            // ── Credential & Certificate IDs ──
                            inner.Item()
                                .AlignCenter()
                                .Row(row =>
                                {
                                    row.AutoItem().PaddingRight(40).Column(col =>
                                    {
                                        col.Item()
                                            .AlignCenter()
                                            .Text("Credential ID")
                                            .FontSize(9)
                                            .FontColor(Colors.Grey.Darken1);

                                        col.Item()
                                            .AlignCenter()
                                            .Text(credentialId)
                                            .FontSize(11)
                                            .Bold()
                                            .FontColor(Colors.Grey.Darken3);
                                    });

                                    row.AutoItem().PaddingLeft(40).Column(col =>
                                    {
                                        col.Item()
                                            .AlignCenter()
                                            .Text("Certificate Number")
                                            .FontSize(9)
                                            .FontColor(Colors.Grey.Darken1);

                                        col.Item()
                                            .AlignCenter()
                                            .Text(certificateId)
                                            .FontSize(11)
                                            .Bold()
                                            .FontColor(Colors.Grey.Darken3);
                                    });
                                });

                            inner.Item().Height(20);

                            // ── Decorative bottom line ──
                            inner.Item()
                                .AlignCenter()
                                .Width(400)
                                .Height(2)
                                .Background(Colors.Amber.Darken2);

                            inner.Item().Height(10);

                            // ── Footer ──
                            inner.Item()
                                .AlignCenter()
                                .Text("Powered By SkillToRole LMS")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Medium)
                                .LetterSpacing(1f);
                        });
                });
            });
        });

        return document.GeneratePdf();
    }
}
