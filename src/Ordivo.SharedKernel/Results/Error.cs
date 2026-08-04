namespace Ordivo.SharedKernel.Results;
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static Error Validation(string description) => new("validation", description);
    public static Error NotFound(string description) => new("not_found", description);
    public static Error Conflict(string description) => new("conflict", description);
}
