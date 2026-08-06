using AiChat.Application.Authentications.Commands.Login;
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
            [FromServices] LoginCommandHandler handler,
            [FromBody] LoginCommand command,
            CancellationToken ct)
        {
            // ۱. بررسی فعال بودن Active Directory از روی تنظیمات سیستمی
            var isAdEnabled = _configuration.GetValue<bool>("Authentication:ActiveDirectory:Enabled");

            var result = await handler.HandleAsync(command, isAdEnabled, ct);

            if (result is null)
                return Unauthorized();

            return Ok(result);
        }
    }

}
