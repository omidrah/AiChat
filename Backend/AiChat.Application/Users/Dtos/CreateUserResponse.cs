using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AiChat.Application.Users.Dtos;

public sealed record CreateUserResponse(Guid userId, IReadOnlyList<Error> Errors);