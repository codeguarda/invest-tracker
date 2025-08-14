namespace InvestTracker.Domain.Users;

public sealed class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!; // BCrypt
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private User(){}

    public static User Create(string email, string passwordHash) => new()
    {
        Email = email.Trim().ToLowerInvariant(),
        PasswordHash = passwordHash
    };
}
