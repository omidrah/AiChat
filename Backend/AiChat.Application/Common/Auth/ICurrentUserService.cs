namespace AiChat.Application.Common.Auth
{
    public interface ICurrentUserService
    {
        CurrentUser? GetCurrentUser();
    }

}
