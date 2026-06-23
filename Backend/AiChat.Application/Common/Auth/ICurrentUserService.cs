namespace AiChat.Application.Common.Auth
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }
        CurrentUser? GetCurrentUser();
    }

}
