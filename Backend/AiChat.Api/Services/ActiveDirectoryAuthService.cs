namespace AiChat.Api.Services;

using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Dtos;
using AiChat.Domain.Entities;
using Microsoft.Extensions.Options;
using System.DirectoryServices.AccountManagement;

public sealed class ActiveDirectoryAuthService : IActiveDirectoryAuthService
{
    private readonly ActiveDirectoryOptions _options;
    private readonly ILogger<ActiveDirectoryAuthService> _logger;

    public ActiveDirectoryAuthService(IOptions<ActiveDirectoryOptions> options, ILogger<ActiveDirectoryAuthService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<ActiveDirectoryUserInfo?> ValidateAsync(
        string userName,
        string password,
        CancellationToken ct = default)
    {

        var targets = _options.Servers?.Length > 0
                    ? _options.Servers
                    : new[] { _options.Domain };
        
        foreach (var server in targets)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                using PrincipalContext context = string.IsNullOrWhiteSpace(_options.Container)
                    ? new PrincipalContext(ContextType.Domain, server)
                    : new PrincipalContext(ContextType.Domain, server, _options.Container);


                bool isValid = context.ValidateCredentials(userName, password); //connecto ad server

                if (!isValid)
                    return Task.FromResult<ActiveDirectoryUserInfo?>(null);

                var activedirectoryUser = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName); //get user info from ad server

                if (activedirectoryUser is null)
                {
                    _logger.LogWarning($"Credentials validated on AD target {server}, but user {userName} was not found.");

                    continue;
                }

                var samAccountName = activedirectoryUser.SamAccountName ?? userName;

                var info = new ActiveDirectoryUserInfo
                {
                    UserName = samAccountName,
                    DisplayName = activedirectoryUser.DisplayName ?? userName,
                    ExternalId = $"{_options.Domain}\\{samAccountName}"
                };

                return Task.FromResult<ActiveDirectoryUserInfo?>(info);               
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Active Directory server failed: {server}");
            }
        }

        return Task.FromResult<ActiveDirectoryUserInfo?>(default);
    }
}
