namespace Hebi_Api.Features.Users.Dtos;

/// <summary>
///     Basic user's info
/// </summary>
public class BasicUserInfoDto : BasicInfoDto
{
    public string? ClinicName { get; set; }
}
