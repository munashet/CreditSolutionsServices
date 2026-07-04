using CreditSolutions.Domain.Common;

namespace CreditSolutions.Domain.Users;

public sealed class User : Entity
{
    private User() { }

    public User(string displayName, string email)
    {
        if (string.IsNullOrWhiteSpace(displayName)) throw new DomainException("Display name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email is required.");

        DisplayName = displayName.Trim();
        Email = email.Trim().ToLowerInvariant();
    }

    public string DisplayName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
}
