using AiChat.Application.Abstractions;
using AiChat.Application.Users.Dtos;
using Shared;

namespace AiChat.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _hasher;

    public UpdateUserHandler(IUserRepository userRepository, IPasswordHasher hasher)
    {
        _userRepository = userRepository;
        _hasher = hasher;
    }

    public async Task<Result<UserDto>> HandleAsync(
        UpdateUserCommand command,
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(command.Id, ct);

        if (user is null)
        {
            return Result.Failure<UserDto>(
                new Error("NotFound", "کاربر یافت نشد."));
        }

        user.SetIsActive(command.IsActive);
        user.SetDisplayName(command.DisplayName);   

        if (!string.IsNullOrWhiteSpace(command.Password))
        {
            user.ChangePassword(_hasher.Hash(command.Password));
        }

        await _userRepository.SaveChangesAsync(ct);

        return Result.Success(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            AuthProvider = user.AuthProvider,
            IsActive = user.IsActive
        });
    }
}
