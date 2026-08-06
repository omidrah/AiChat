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

        var targets = BuildTargets(_adOptionsMonitor);
        if (targets.Count == 0)
        {
            _logger.LogWarning("AD is enabled but no Server/Servers/Domain is configured.");
            return Task.FromResult<ActiveDirectoryUserInfo?>(null);
        }


        foreach (var server in targets)
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

                var normalizedUserame = NormalizeUserName(userName);

                _logger.LogInformation($"Ad on {server} by {normalizedUserame}");

                var domainPart = !string.IsNullOrWhiteSpace(_adOptionsMonitor.Domain) ? _adOptionsMonitor.Domain : server;

                return Task.FromResult<ActiveDirectoryUserInfo?>(
                    new ActiveDirectoryUserInfo
                    {
                        UserName = normalizedUserame,
                        DisplayName = normalizedUserame,
                        ExternalId = $"{domainPart}\\{normalizedUserame}"
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
    private static List<string> BuildTargets(ActiveDirectoryOptions opt)
    {
        // اولویت: Server (Primary) -> Servers (Fallback) -> Domain (آخرین fallback)
        var list = new List<string>();

        if (!string.IsNullOrWhiteSpace(opt.PrimaryServer))
            list.Add(opt.PrimaryServer.Trim());

        if (opt.FallbackServers is { Count: > 0 })
            list.AddRange(opt.FallbackServers.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));

        if (!string.IsNullOrWhiteSpace(opt.Domain))
            list.Add(opt.Domain.Trim());

        // حذف تکراری‌ها (case-insensitive)
        return list
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
    private static string NormalizeUserName(string userName)
    {
        // user@domain => user
        // domain\user => user
        if (string.IsNullOrWhiteSpace(userName)) return string.Empty;

        if (userName.Contains('\\'))
            return userName.Split('\\', 2)[1];

        if (userName.Contains('@'))
            return userName.Split('@', 2)[0];

        return userName;
    }
}
