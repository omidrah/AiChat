namespace AiChat.Application.Authentications.Commands.Login
{
    public sealed record LoginCommand(
      string UserName,
      string Password
  );
}
