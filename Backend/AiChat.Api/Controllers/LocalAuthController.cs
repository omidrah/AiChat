using AiChat.Application.Authentications.Commands.Login;
using AiChat.Application.Authentications.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class LocalAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LocalAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromServices] ActiveDirectoryLoginHandler adHandler,
            [FromServices]LoginCommandHandler handler,
            [FromBody]LoginCommand command,
            CancellationToken ct)
        {
            var mode = _configuration["Authentication:Mode"] ?? "Local";

            LoginResultDto? result = mode.Equals("ActiveDirectory", StringComparison.OrdinalIgnoreCase)
            ? await adHandler.HandleAsync(command, ct)
            : await handler.HandleAsync(command, ct);

            if (result is null)
                return Unauthorized();

            return Ok(result);
        }
    }

}
