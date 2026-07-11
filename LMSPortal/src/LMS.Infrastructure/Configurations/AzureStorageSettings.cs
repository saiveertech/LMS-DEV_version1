namespace LMS.Infrastructure.Configurations;

public class AzureStorageSettings
{
    public string ConnectionString { get; set; } = string.Empty;

    public string ContainerName { get; set; } = string.Empty;

    public string CertificatesContainerName { get; set; } = "certificates";

    // Container that holds the source certificate template image (blank template, no student data).
    public string CertificateTemplatePath { get; set; } = string.Empty;
}
