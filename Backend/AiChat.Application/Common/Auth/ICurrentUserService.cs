namespace AiChat.Application.Common.Auth
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        CurrentUser? GetCurrentUser();
    }

}
