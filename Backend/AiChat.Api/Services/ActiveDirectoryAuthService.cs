namespace AiChat.Api.Services;

using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Dtos;
using Microsoft.Extensions.Options;
using System.DirectoryServices.AccountManagement;

public sealed class ActiveDirectoryAuthService : IActiveDirectoryAuthService
{
    private readonly ActiveDirectoryOptions _options;
    private readonly ILogger<ActiveDirectoryAuthService> _logger;

    public ActiveDirectoryAuthService(
        IOptions<ActiveDirectoryOptions> options,
        ILogger<ActiveDirectoryAuthService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<ActiveDirectoryUserInfo?> ValidateAsync(
        string userName,
        string password,
        CancellationToken ct = default)
    {
        var servers = _options.Servers is { Length: > 0 }
            ? _options.Servers
            : [_options.Domain];

        foreach (var server in servers)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                _logger.LogInformation(
                    "Validating AD user {UserName} against {Server}",
                    userName,
                    server);

                using var context = string.IsNullOrWhiteSpace(_options.Container)
                    ? new PrincipalContext(
                        ContextType.Domain,
                        server)
                    : new PrincipalContext(
                        ContextType.Domain,
                        server,
                        _options.Container);

                var isValid = context.ValidateCredentials(
                    userName,
                    password,
                    ContextOptions.Negotiate);

                if (!isValid)
                {
                    _logger.LogWarning(
                        "AD credentials rejected for {UserName} by {Server}",
                        userName,
                        server);

                    continue;
                }

                using var adUser = UserPrincipal.FindByIdentity(
                    context,
                    IdentityType.SamAccountName,
                    userName);

                if (adUser is null)
                {
                    _logger.LogWarning(
                        "AD authenticated {UserName}, but its user object was not found on {Server}",
                        userName,
                        server);

                    continue;
                }

                var samAccountName = adUser.SamAccountName ?? userName;

                return Task.FromResult<ActiveDirectoryUserInfo?>(
                    new ActiveDirectoryUserInfo
                    {
                        UserName = samAccountName,
                        DisplayName = adUser.DisplayName ?? samAccountName,
                        ExternalId = $"{_options.Domain}\\{samAccountName}"
                    });
            }
            catch (PrincipalServerDownException ex)
            {
                _logger.LogError(
                    ex,
                    "Cannot connect to AD server {Server}",
                    server);
            }
            catch (PrincipalOperationException ex)
            {
                _logger.LogError(
                    ex,
                    "AD operation failed on {Server} for {UserName}",
                    server,
                    userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected AD error on {Server} for {UserName}",
                    server,
                    userName);
            }
        }

        return Task.FromResult<ActiveDirectoryUserInfo?>(null);
    }
}
