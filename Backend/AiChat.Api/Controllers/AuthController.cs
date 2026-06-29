using AiChat.Application.Authentications.Queries.GetCurrentUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me([FromServices] GetCurrentUserQueryHandler handler , CancellationToken ct)
        {
            var user =await handler.HandleAsync(new GetCurrentUserQuery(),ct);

            if (user is null)
                return Unauthorized();

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpGet("mode")]
        public IActionResult Mode()
        {
            return Ok(new
            {
                mode = _configuration["Authentication:Mode"] ?? "Local"
            });
        }
    }

}
