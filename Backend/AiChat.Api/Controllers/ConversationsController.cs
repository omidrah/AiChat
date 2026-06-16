using AiChat.Api.Contracts;
using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Commands;
using AiChat.Application.Dtos;
using AiChat.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AiChat.Api.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationRepository _repository;

        public ConversationsController(IConversationRepository repository)
        {
            _repository = repository;
        }
        /// <summary>
        /// generate New Conversation 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var conversation = Conversation.CreateConversation($"New Chat on {DateTime.Now}");

            await _repository.AddAsync(conversation);

            await _repository.SaveChangesAsync();

            return Ok(conversation.Id);
        }

        /// <summary>
        /// List All of Converation
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var conversations = await _repository.GetAllAsync();

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
        public async Task<IActionResult> GetMessageByConversationId(Guid conversationId)
        {
            var conversation =
                await _repository.GetAsync(conversationId);

            if (conversation is null)
                return NotFound();

            return Ok(
                new ConversationDetailsDto
                {
                    Id = conversation.Id,
                    Title = conversation.Title,
                    Messages =
                        conversation.Messages.OrderBy(x => x.CreatedAt)
                            .Select(x => new MessageDto
                            {
                                Role = x.Role.ToString().ToLower(),
                                Content = x.Content                              
                            })
                            .ToList()
                });
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

            return Ok(answer);
        }

        [HttpDelete("{conversationId}")]
        public async Task<IActionResult> Delete(Guid conversationId)
        {
            var conversation =
                await _repository.GetAsync(conversationId);

            if (conversation is null)
                return NotFound();

            await _repository.DeleteAsync(
                conversation);

            await _repository.SaveChangesAsync();

            return NoContent();
        }
    }
}
