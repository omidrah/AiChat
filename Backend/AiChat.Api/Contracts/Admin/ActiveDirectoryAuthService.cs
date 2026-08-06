namespace AiChat.Api.Contracts.Admin;

using AiChat.Application.Abstractions;
using AiChat.Application.Common.Options;
using AiChat.Application.Conversations.Dtos;
using Microsoft.Extensions.Options;
using System.DirectoryServices.AccountManagement;

public sealed class ActiveDirectoryAuthService : IActiveDirectoryAuthService
{
    private readonly ActiveDirectoryOptions _adOptionsMonitor;
    private readonly ILogger<ActiveDirectoryAuthService> _logger;

    public ActiveDirectoryAuthService(
        IOptions<ActiveDirectoryOptions> options,
        ILogger<ActiveDirectoryAuthService> logger)
    {
        _adOptionsMonitor = options.Value;
        _logger = logger;
    }

    public Task<ActiveDirectoryUserInfo?> ValidateAsync(string userName, string password, CancellationToken ct = default)
    {

        if (!_adOptionsMonitor.Enabled)
        {
            _logger.LogWarning("Active Directory authentication is disabled.");
            return Task.FromResult<ActiveDirectoryUserInfo?>(null);
        }

        var servers = _adOptionsMonitor.Servers is { Count: > 0 }
            ? _adOptionsMonitor.Servers
            : [_adOptionsMonitor.Domain];

      
        foreach (var server in servers)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                _logger.LogInformation(
                    "Validating AD user {UserName} against {Server}",
                    userName,
                    server);

                using var context = string.IsNullOrWhiteSpace(_adOptionsMonitor.Container)
                    ? new PrincipalContext(ContextType.Domain, server)
                    : new PrincipalContext(ContextType.Domain, server, _adOptionsMonitor.Container);

                var isValid = context.ValidateCredentials(userName, password, ContextOptions.Negotiate);

                if (!isValid)
                {
                    _logger.LogWarning("AD credentials rejected for {UserName} by {Server}", userName, server);
                    continue;
                }

                var normalizedUserame = userName.Contains('@')
                    ? userName.Split('@')[0]
                    : userName;

                _logger.LogInformation($"Ad on {server} by {normalizedUserame}");

                return Task.FromResult<ActiveDirectoryUserInfo?>(
                    new ActiveDirectoryUserInfo
                    {
                        UserName = normalizedUserame,
                        DisplayName = normalizedUserame,
                        ExternalId = $"{_adOptionsMonitor.Domain}\\{normalizedUserame}"
                    });
            }
            catch (PrincipalServerDownException ex)
            {
                _logger.LogError(ex, "Cannot connect to AD server {Server}", server);
            }
            catch (PrincipalOperationException ex)
            {
                _logger.LogWarning(ex,
                   "AD authentication succeeded but user lookup failed for {UserName} on {Server}",
                   userName,
                   server);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected AD error on {Server} for {UserName}", server, userName);
            }
        }

        return Task.FromResult<ActiveDirectoryUserInfo?>(null);
    }
}
