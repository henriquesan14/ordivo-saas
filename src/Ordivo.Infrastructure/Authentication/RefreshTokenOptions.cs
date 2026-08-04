namespace Ordivo.Infrastructure.Authentication;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";
    public int ExpirationDays { get; init; } = 30;
}
