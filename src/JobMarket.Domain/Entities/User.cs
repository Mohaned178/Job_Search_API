using JobMarket.Domain.Common;
using JobMarket.Domain.Enums;

namespace JobMarket.Domain.Entities;

public class User : AuditableEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    public UserPreferences? Preferences { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash)
        => new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Role = UserRole.JobSeeker
        };

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
    }

    public bool IsRefreshTokenValid(string token)
        => RefreshToken == token && RefreshTokenExpiry > DateTime.UtcNow;
}
