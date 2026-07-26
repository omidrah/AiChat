namespace AiChat.Application.Users.Dtos;
using System;

public sealed class UserDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string AuthProvider { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
