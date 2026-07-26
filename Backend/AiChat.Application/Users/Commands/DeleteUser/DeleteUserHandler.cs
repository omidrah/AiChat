using AiChat.Application.Abstractions;
using AiChat.Application.Common;
using Shared;

namespace AiChat.Application.Users.Commands.DeleteUser;

public sealed class DeleteUserHandler
{
    private readonly IUserRepository _userRepository;

    public DeleteUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> HandleAsync(DeleteUserCommand command, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(command.Id, ct);

        if (user is null)
        {
            return Result.Failure(
                new Error("NotFound", "کاربر یافت نشد."));
        }

        user.SetIsActive(false);

        await _userRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
