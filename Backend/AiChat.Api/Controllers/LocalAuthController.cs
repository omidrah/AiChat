using AiChat.Application.Authentications.Commands.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class LocalAuthController : ControllerBase
    {
        public LocalAuthController()
        {
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromServices]LoginCommandHandler handler, [FromBody]LoginCommand command,
            CancellationToken ct)
        {
            var result = await handler.HandleAsync(command, ct);

            if (result is null)
                return Unauthorized();

            return Ok(result);
        }
    }

}
