namespace AiChat.Application.Users.Dtos;

public sealed class CreateUserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; }
    public string? DisplayName { get; set; }
}