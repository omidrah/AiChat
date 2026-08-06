using System.Diagnostics;
using System.Text;

namespace AiChat.Api.Contracts.Admin
{
    public sealed class ActiveDirectoryDiagnosticService : IActiveDirectoryDiagnosticService
    {
        private readonly IActiveDirectorySettingsService _settingsService;
        private readonly ILogger<ActiveDirectoryDiagnosticService> _logger;

        public ActiveDirectoryDiagnosticService(IActiveDirectorySettingsService settingsService,
            ILogger<ActiveDirectoryDiagnosticService> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        public async Task<ActiveDirectoryDiagnosticResultDto> RunAsync(
         ActiveDirectoryDiagnosticRequest request,
         CancellationToken cancellationToken = default)
        {
            var settings = _settingsService.Get();

            var server = request.Server?.Trim();
            var domain = request.Domain?.Trim();

            server = string.IsNullOrWhiteSpace(server)
                ? settings.PrimaryServer
                : server;

            domain = string.IsNullOrWhiteSpace(domain)
                ? settings.Domain
                : domain;

            if (!AdInputValidator.IsValidHost(server))
                throw new ArgumentException("Server is invalid.");

            if (!AdInputValidator.IsValidDomain(domain))
                throw new ArgumentException("Domain is invalid.");

            return request.Type switch
            {
                ActiveDirectoryDiagnosticType.Ldap389 =>
                    await RunTcpTestAsync(server, 389, "LDAP (389)", cancellationToken),

                ActiveDirectoryDiagnosticType.Kerberos88 =>
                    await RunTcpTestAsync(server, 88, "Kerberos (88)", cancellationToken),

                ActiveDirectoryDiagnosticType.Dns53 =>
                    await RunTcpTestAsync(server, 53, "DNS (53)", cancellationToken),

                ActiveDirectoryDiagnosticType.DomainControllerDiscovery =>
                    await RunNlTestAsync(domain, cancellationToken),

                ActiveDirectoryDiagnosticType.LdapSrvLookup =>
                    await RunLdapSrvLookupAsync(domain, cancellationToken),

                _ => throw new ArgumentOutOfRangeException(nameof(request.Type))
            };
        }

        private static async Task<ActiveDirectoryDiagnosticResultDto> RunTcpTestAsync(
        string server,
        int port,
        string testName,
        CancellationToken cancellationToken)
        {
            var safeServer = EscapePowerShellSingleQuoted(server);

            var command =
                $"Test-NetConnection -ComputerName '{safeServer}' -Port {port} -InformationLevel Detailed";

            return await RunProcessAsync(
                fileName: "powershell.exe",
                arguments: $"-NoProfile -NonInteractive -Command \"{command}\"",
                testName: testName,
                commandForDisplay: command,
                cancellationToken);
        }

        private static async Task<ActiveDirectoryDiagnosticResultDto> RunNlTestAsync(
            string domain,
            CancellationToken cancellationToken)
        {
            var commandForDisplay = $"nltest /dsgetdc:{domain}";

            return await RunProcessAsync(
                fileName: "nltest.exe",
                arguments: $"/dsgetdc:{domain}",
                testName: "Domain Controller Discovery",
                commandForDisplay,
                cancellationToken);
        }

        private static async Task<ActiveDirectoryDiagnosticResultDto> RunLdapSrvLookupAsync(
            string domain,
            CancellationToken cancellationToken)
        {
            var query = $"_ldap._tcp.dc._msdcs.{domain}";
            var safeQuery = EscapePowerShellSingleQuoted(query);

            var command =
                $"Resolve-DnsName -Type SRV -Name '{safeQuery}' | Format-Table -AutoSize";

            return await RunProcessAsync(
                fileName: "powershell.exe",
                arguments: $"-NoProfile -NonInteractive -Command \"{command}\"",
                testName: "LDAP SRV DNS Lookup",
                commandForDisplay: command,
                cancellationToken);
        }

        private static async Task<ActiveDirectoryDiagnosticResultDto> RunProcessAsync(
        string fileName,
        string arguments,
        string testName,
        string commandForDisplay,
        CancellationToken cancellationToken)
        {
            var startedAt = Stopwatch.StartNew();

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            try
            {
                process.Start();

                var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

                await process.WaitForExitAsync(cancellationToken);

                var output = await standardOutputTask;
                var error = await standardErrorTask;

                startedAt.Stop();

                return new ActiveDirectoryDiagnosticResultDto
                {
                    TestName = testName,
                    Command = commandForDisplay,
                    Success = process.ExitCode == 0,
                    Output = output,
                    Error = string.IsNullOrWhiteSpace(error) ? null : error,
                    DurationMs = startedAt.ElapsedMilliseconds
                };
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);

                throw;
            }
            catch (Exception ex)
            {
                startedAt.Stop();

                return new ActiveDirectoryDiagnosticResultDto
                {
                    TestName = testName,
                    Command = commandForDisplay,
                    Success = false,
                    Output = string.Empty,
                    Error = ex.Message,
                    DurationMs = startedAt.ElapsedMilliseconds
                };
            }
        }

        private static string EscapePowerShellSingleQuoted(string value)
        {
            return value.Replace("'", "''");
        }
    }
}



