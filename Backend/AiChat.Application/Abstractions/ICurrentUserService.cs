using AiChat.Application.Common;

namespace AiChat.Application.Abstractions
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        CurrentUser? GetCurrentUser();
    }

}
