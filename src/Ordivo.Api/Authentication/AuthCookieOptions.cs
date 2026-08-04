namespace Ordivo.Api.Authentication;

public sealed class AuthCookieOptions
{
    public const string SectionName = "AuthCookie";
    public const string DefaultName = "ordivo.access_token";

    public string Name { get; init; } = DefaultName;
    public bool Secure { get; init; } = true;
    public SameSiteMode SameSite { get; init; } = SameSiteMode.Strict;
}
