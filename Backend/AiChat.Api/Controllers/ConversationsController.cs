using AiChat.Api.Contracts;
using AiChat.Application.Common.Auth;
using AiChat.Application.Conversations.Commands.CreateConversation;
using AiChat.Application.Conversations.Commands.CreateMessage;
using AiChat.Application.Conversations.Commands.DeleteConversaion;
using AiChat.Application.Conversations.Dtos;
using AiChat.Application.Conversations.Queries.GetConversationList;
using AiChat.Application.Conversations.Queries.GetConverstaions;
using AiChat.Domain.Entities;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Api.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly ICurrentUserService _currentUser;

        public ConversationsController(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }
        /// <summary>
        /// generate New Conversation 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromServices]CreateConversationHandler handler)
        {
            if (_currentUser.UserId is not Guid userId)
                return Unauthorized();
            var createConversationCommand = new CreateConversationCommand(userId, _currentUser.UserName!, "New Chat");

            var id = await handler.HandleAsync(createConversationCommand);
            return Ok(id);
        }

        /// <summary>
        /// List All of Converation
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromServices] GetConversationListHandler getConversationListHandler)
        {
            if (_currentUser.UserId is not Guid userId)
                return Unauthorized();

            var getConversationListCommand = new GetConversationListQuery(userId);

            var conversations = await getConversationListHandler.HandleAsync(getConversationListCommand);
            return Ok(
                conversations.Select(x =>
                    new ConversationListItemDto
                    {
                            CreatedAt= x.CreatedAt,
                            Id= x.Id,
                            Title= x.Title
                    })
                );   
        }
        /// <summary>
        /// Get History of Converation 
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetMessageByConversationId(Guid conversationId, [FromServices] GetConversationHandler getConversationHandler)
        {
            if(_currentUser.UserId is not Guid userId)
                return Unauthorized();

            var getConversation = new GetConversationQuery(conversationId,userId);
            var conversation = await getConversationHandler.HandleAsync(getConversation);

            if (conversation is null)
                return NotFound();

            return Ok(conversation);
        }


        /// <summary>
        /// Add new message from user and Get response from ollama and save in Db
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="request"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        [HttpPost("{conversationId}/messages")]
        public async Task<IActionResult> SendMessage(Guid conversationId, 
            [FromBody] SendMessageRequest request,
            [FromServices] SendMessageHandler handler)
        {
            if (_currentUser.UserId is not Guid userId)
                return Unauthorized();

            var command = new SendMessageCommand(conversationId, userId, request.Message);

            var answer = await handler.HandleAsync(command);

            return Ok(new
            {
                success = true
            });
        }

        [HttpDelete("{conversationId}")]
        public async Task<IActionResult> Delete(Guid conversationId, [FromServices] DeleteConversationHandler handler)
        {
            if (_currentUser.UserId is not Guid userId)
                return Unauthorized();

            var deleteConversation = new DeleteConversation(conversationId,userId);

            var result = await handler.HandleAsync(deleteConversation);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
