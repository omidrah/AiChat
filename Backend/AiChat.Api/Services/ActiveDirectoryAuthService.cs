namespace AiChat.Api.Services;

using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Dtos;
using Microsoft.Extensions.Options;
using System.DirectoryServices.AccountManagement;

public sealed class ActiveDirectoryAuthService : IActiveDirectoryAuthService
{
    private readonly ActiveDirectoryOptions _options;

    public ActiveDirectoryAuthService(IOptions<ActiveDirectoryOptions> options)
    {
        _options = options.Value;
    }

    public Task<ActiveDirectoryUserInfo?> ValidateAsync(
        string userName,
        string password,
        CancellationToken ct = default)
    {
        using PrincipalContext context = new(ContextType.Domain,
                                             string.IsNullOrWhiteSpace(_options.Server) ? _options.Domain : _options.Server,
                                             string.IsNullOrWhiteSpace(_options.Container) ? null : _options.Container);

        bool isValid = context.ValidateCredentials(userName, password); //connecto ad server

        if (!isValid)
            return Task.FromResult<ActiveDirectoryUserInfo?>(null);

        var activedirectoryUser = UserPrincipal.FindByIdentity(context, userName); //get user info from ad server

        if (activedirectoryUser is null)
            return Task.FromResult<ActiveDirectoryUserInfo?>(null);

        var info = new ActiveDirectoryUserInfo
        {
            UserName = activedirectoryUser.SamAccountName ?? userName,
            DisplayName = activedirectoryUser.DisplayName ?? userName,
            ExternalId = $"{_options.Domain}\\{activedirectoryUser.SamAccountName ?? userName}"
        };

        return Task.FromResult<ActiveDirectoryUserInfo?>(info);
    }
}
