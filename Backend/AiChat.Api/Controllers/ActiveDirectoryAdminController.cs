using AiChat.Api.Contracts.Admin;
using AiChat.Application.Authentications.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Api.Controllers
{
    [ApiController]
    [Route("api/active-directory")]
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class ActiveDirectoryAdminController : Controller
    {
        private readonly IActiveDirectorySettingsService _settingsService;
        private readonly IActiveDirectoryDiagnosticService _diagnosticService;

        public ActiveDirectoryAdminController(
            IActiveDirectorySettingsService settingsService,
            IActiveDirectoryDiagnosticService diagnosticService)
        {
            _settingsService = settingsService;
            _diagnosticService = diagnosticService;
        }

        [HttpGet("settings")]
        [ProducesResponseType(typeof(ActiveDirectorySettingsDto), StatusCodes.Status200OK)]
        public ActionResult<ActiveDirectorySettingsDto> GetSettings()
        {
            var settings = _settingsService.Get();

            return Ok(new ActiveDirectorySettingsDto
            {
                Enabled = settings.Enabled,
                Domain = settings.Domain,
                Container = settings.Container,
                Server = settings.PrimaryServer,
                Servers = settings.FallbackServers,
                UseSsl = settings.UseSsl
            });
        }

        [HttpPut("settings")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateSettings([FromBody] ActiveDirectorySettingsDto request, CancellationToken ct)
        {
            await _settingsService.SaveAsync(
           new ActiveDirectoryOptions
           {
               Enabled = request.Enabled,
               Domain = request.Domain.Trim(),
               Container = request.Container.Trim(),
               PrimaryServer = request.Server.Trim(),
               FallbackServers = request.Servers
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Select(x => x.Trim())
                   .Distinct(StringComparer.OrdinalIgnoreCase)
                   .ToList(),
               UseSsl = request.UseSsl
           },
           ct);

            return NoContent();
        }

        [HttpPost("diagnostics")]
        [ProducesResponseType(typeof(ActiveDirectoryDiagnosticResultDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ActiveDirectoryDiagnosticResultDto>> RunDiagnostic(
        [FromBody] ActiveDirectoryDiagnosticRequest request,
        CancellationToken ct)
        {
            var result = await _diagnosticService.RunAsync(request, ct);

            return Ok(result);
        }
    }
}
