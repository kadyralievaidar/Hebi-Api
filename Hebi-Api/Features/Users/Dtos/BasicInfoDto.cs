namespace Hebi_Api.Features.Users.Dtos;

/// <summary>
///     Basic user's info
/// </summary>
public class BasicInfoDto
{
    /// <summary>
    ///     User name
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    ///     User's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    ///     User's last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    ///     User's last name
    /// </summary>
    public string? PhoneNumber { get; set; }
}
