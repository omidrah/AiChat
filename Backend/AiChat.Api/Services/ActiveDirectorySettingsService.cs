using AiChat.Api.Contracts.Admin;
using AiChat.Application.Common.Options;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AiChat.Api.Services;

public sealed class ActiveDirectorySettingsService : IActiveDirectorySettingsService
{
    private static readonly SemaphoreSlim FileLock = new(1, 1);

    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ActiveDirectorySettingsService> _logger;

    public ActiveDirectorySettingsService(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<ActiveDirectorySettingsService> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    public ActiveDirectoryOptions Get()
    {
        return _configuration
            .GetSection("Authentication:ActiveDirectory")
            .Get<ActiveDirectoryOptions>()
            ?? new ActiveDirectoryOptions();
    }

    public async Task SaveAsync(
        ActiveDirectoryOptions options,
        CancellationToken cancellationToken = default)
    {
        Validate(options);

        var appSettingsPath = Path.Combine(
            _environment.ContentRootPath,
            "appsettings.Runtime.json");

        await FileLock.WaitAsync(cancellationToken);

        try
        {
            var json = await File.ReadAllTextAsync(appSettingsPath, cancellationToken);

            var root = JsonNode.Parse(json)?.AsObject()
                       ?? throw new InvalidOperationException("Invalid appsettings.Runtime.json.");

            root["Authentication:ActiveDirectory"] = JsonSerializer.SerializeToNode(
                options,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            var updatedJson = root.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // ابتدا فایل موقت و سپس جایگزینی؛ برای کاهش احتمال خراب‌شدن فایل.
            var temporaryPath = $"{appSettingsPath}.{Guid.NewGuid():N}.tmp";

            await File.WriteAllTextAsync(
                temporaryPath,
                updatedJson,
                cancellationToken);

            File.Move(
                temporaryPath,
                appSettingsPath,
                overwrite: true);

            _logger.LogWarning(
                "Active Directory settings were updated by an administrator. Domain: {Domain}, Server: {Server}",
                options.Domain,
                options.Servers);
        }
        finally
        {
            FileLock.Release();
        }
    }

    private static void Validate(ActiveDirectoryOptions options)
    {
        if (!options.Enabled)
            return;

        if (string.IsNullOrWhiteSpace(options.Domain))
            throw new ArgumentException("Domain is required.");

        if (string.IsNullOrWhiteSpace(options.Server))
            throw new ArgumentException("Primary Server is required.");

        if (!AdInputValidator.IsValidHost(options.Server))
            throw new ArgumentException("Server format is invalid.");

        if (!AdInputValidator.IsValidDomain(options.Domain))
            throw new ArgumentException("Domain format is invalid.");

        if (!string.IsNullOrWhiteSpace(options.Container)
            && !AdInputValidator.IsValidDistinguishedName(options.Container))
        {
            throw new ArgumentException(
                "Container must be a valid LDAP Distinguished Name. Example: DC=omid,DC=ir");
        }

        foreach (var server in options.Servers.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            if (!AdInputValidator.IsValidHost(server))
                throw new ArgumentException($"Invalid server: {server}");
        }
    }
}