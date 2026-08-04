using Ordivo.SharedKernel.Domain;

namespace Ordivo.Domain.Tenants;

public sealed class Tenant : AggregateRoot<Guid>
{
    private Tenant(Guid id) : base(id) { }

    public static Tenant Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tenant name is required.", nameof(name));
        return new Tenant(Guid.NewGuid()) { Name = name.Trim(), IsActive = true };
    }

    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public void Rename(string name) => Name = string.IsNullOrWhiteSpace(name)
        ? throw new ArgumentException("Tenant name is required.", nameof(name))
        : name.Trim();
    public void Deactivate() => IsActive = false;
}
