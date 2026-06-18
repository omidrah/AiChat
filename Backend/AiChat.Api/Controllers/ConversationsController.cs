using AiChat.Api.Contracts;
using AiChat.Application.Conversations.Commands.CreateConversation;
using AiChat.Application.Conversations.Commands.CreateMessage;
using AiChat.Application.Conversations.Commands.DeleteConversaion;
using AiChat.Application.Conversations.Queries.GetConversationList;
using AiChat.Application.Conversations.Queries.GetConverstaions;
using AiChat.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Api.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationsController : ControllerBase
    {
        public ConversationsController()
        {
        }
        /// <summary>
        /// generate New Conversation 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromServices]CreateConversationHandler handler)
        {
            var id = await handler.HandleAsync( new CreateConversation("New Chat"));
            return Ok(id);
        }

        /// <summary>
        /// List All of Converation
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromServices] GetConversationListHandler listhandler)
        {
            var conversations = await listhandler.HandleAsync();
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
            var conversation = await getConversationHandler.HandleAsync(new GetConversationQuery(conversationId));

            if (conversation is null)
                return NotFound();

            return Ok(conversation);
        }

       
        /// <summary>
        /// Add new message from user and Get response from ollama and save in Db
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        [HttpPost("{id}/messages")]
        public async Task<IActionResult> SendMessage(Guid id,  [FromBody] SendMessageRequest request,
                                 [FromServices] SendMessageHandler handler)
        {
            var answer = await handler.HandleAsync(new SendMessageCommand(id, request.Message));

            return Ok(new
            {
                success = true
            });
        }

        [HttpDelete("{conversationId}")]
        public async Task<IActionResult> Delete(Guid conversationId, [FromServices] DeleteConversationHandler handler)
        {
            var result = await handler.HandleAsync(new DeleteConversation(conversationId));

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
