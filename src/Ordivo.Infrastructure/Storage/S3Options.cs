namespace Ordivo.Infrastructure.Storage;

public sealed class S3Options
{
    public const string SectionName = "S3";
    public string ServiceUrl { get; init; } = string.Empty;
    public string Region { get; init; } = "us-east-1";
    public string Bucket { get; init; } = "ordivo-attachments";
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public bool ForcePathStyle { get; init; } = true;
}
