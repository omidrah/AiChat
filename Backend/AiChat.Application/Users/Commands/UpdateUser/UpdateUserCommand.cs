namespace AiChat.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string? Password,
    string DisplayName,
    bool IsActive);