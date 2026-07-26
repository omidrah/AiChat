
using AiChat.Application.Abstractions;
using AiChat.Application.Users.Dtos;
using Shared;

namespace AiChat.Application.Users.Queries.GetAllUsers;

public sealed class GetUsersHandler
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<List<UserDto>>> HandleAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);

        var result = users
            .Select(x => new UserDto
            {
                Id = x.Id,
                UserName = x.UserName,
                DisplayName = x.DisplayName,
                AuthProvider = x.AuthProvider,
                IsActive = x.IsActive
            })
            .ToList();

        return Result.Success(result);
    }
}