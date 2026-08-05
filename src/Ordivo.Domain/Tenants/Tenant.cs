using System.Globalization;
using System.Text;
using Ordivo.SharedKernel.Domain;

namespace Ordivo.Domain.Tenants;

public sealed class Tenant : AggregateRoot<Guid>
{
    private Tenant(Guid id) : base(id) { }

    public static Tenant Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tenant name is required.", nameof(name));
        var id = Guid.NewGuid();
        return new Tenant(id) { Name = name.Trim(), Slug = GenerateSlug(name, id), IsActive = true };
    }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public void Rename(string name) => Name = string.IsNullOrWhiteSpace(name)
        ? throw new ArgumentException("Tenant name is required.", nameof(name))
        : name.Trim();
    public void Deactivate() => IsActive = false;

    private static string GenerateSlug(string name, Guid id)
    {
        var normalized = name.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        var separatorPending = false;

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(character))
            {
                if (separatorPending && builder.Length > 0) builder.Append('-');
                builder.Append(char.ToLowerInvariant(character));
                separatorPending = false;
            }
            else
            {
                separatorPending = true;
            }
        }

        var prefix = builder.Length == 0 ? "tenant" : builder.ToString();
        if (prefix.Length > 140) prefix = prefix[..140].TrimEnd('-');
        return $"{prefix}-{id:N}"[..(prefix.Length + 9)];
    }
}
