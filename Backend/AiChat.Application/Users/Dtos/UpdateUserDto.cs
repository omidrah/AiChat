namespace AiChat.Application.Users.Dtos;

public sealed class UpdateUserDto
{
    public string? DisplayName { get; set; }
    public string? Password { get; set; }
    public bool IsActive { get; set; }
}