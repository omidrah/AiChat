using AiChat.Application.Abstractions;
using AiChat.Application.Common.Enums;
using AiChat.Application.Users.Dtos;
using AiChat.Domain.Entities;
using Shared;

namespace AiChat.Application.Users.Commands.CreateUser;

public class CreateUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _hasher;
    public CreateUserHandler(IUserRepository repository, IPasswordHasher hasher)
    {
        _userRepository = repository;
        _hasher = hasher;
    }

    public async Task<Result<UserDto>> HandleAsync(CreateUserCommand request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            return Result.Failure<UserDto>(
                new Error("BadRequest", "نام کاربری الزامی است."));

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure<UserDto>(
                new Error("BadRequest", "کلمه عبور الزامی است."));

        var normalizedUserName = request.UserName.Trim();

        var exists = await _userRepository.GetByUserNameAsync(request.UserName, ct);
        if (exists is not null)
        {
            return Result.Failure<UserDto>(
                new Error("BadRequest", "این نام کاربری قبلاً ثبت شده است."));
        }

        var user = User.CreateUser(
             normalizedUserName,
            _hasher.Hash(request.Password),
            request.DisplayName, 
            null,
            AuthenticationProviderEnum.Local.ToString());

        _userRepository.Add(user);
        await _userRepository.SaveChangesAsync(ct);

        return Result.Success(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            AuthProvider = user.AuthProvider!,
            IsActive = user.IsActive
        });
    }
}