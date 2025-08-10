// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Villagers.Api.Configuration;

public class IdentitySettings
{
    public PasswordSettings Password { get; set; } = new();
    public UserSettings User { get; set; } = new();
}

public class PasswordSettings
{
    public bool RequireDigit { get; set; } = true;
    public int RequiredLength { get; set; } = 6;
    public bool RequireNonAlphanumeric { get; set; } = false;
    public bool RequireUppercase { get; set; } = false;
    public bool RequireLowercase { get; set; } = false;
}

public class UserSettings
{
    public bool RequireUniqueEmail { get; set; } = false;
}