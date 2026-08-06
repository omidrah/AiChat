namespace AiChat.Api.Controllers;

using AiChat.Application.Users.Commands.CreateUser;
using AiChat.Application.Users.Commands.DeleteUser;
using AiChat.Application.Users.Commands.UpdateUser;
using AiChat.Application.Users.Dtos;
using AiChat.Application.Users.Queries.GetAllUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    public UsersController()
    {
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll(
        [FromServices] GetUsersHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(ct);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        [FromServices] CreateUserHandler createUserHandler,
        [FromBody] CreateUserDto dto,
        CancellationToken ct)
    {
        var createUserCommand = new CreateUserCommand(dto.UserName, dto.Password, dto.DisplayName);
        var result = await createUserHandler.HandleAsync(createUserCommand, ct);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    // ۳. ویرایش کاربر
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(
       Guid id,
       [FromServices] UpdateUserHandler handler,
       [FromBody] UpdateUserDto dto,
       CancellationToken ct)
    {
        var command = new UpdateUserCommand(
            id,
            dto.Password!,
            dto.DisplayName,
            dto.IsActive);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return NotFound(result.Error);

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteUserHandler handler,
        CancellationToken ct)
    {
        var command = new DeleteUserCommand(id);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return NotFound(result.Error);

            return BadRequest(result.Error);
        }

        return NoContent();
    }
}
