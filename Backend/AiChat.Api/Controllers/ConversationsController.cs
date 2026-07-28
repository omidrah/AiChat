using AiChat.Api.Contracts;
using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Commands.CreateConversation;
using AiChat.Application.Conversations.Commands.CreateMessage;
using AiChat.Application.Conversations.Commands.DeleteConversaion;
using AiChat.Application.Conversations.Commands.RenameConversation;
using AiChat.Application.Conversations.Dtos;
using AiChat.Application.Conversations.Queries.GetConversationList;
using AiChat.Application.Conversations.Queries.GetConverstaions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Api.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    [Authorize(Policy = "UserPolicy")]
    public class ConversationsController : ControllerBase
    {
        private readonly IUserResolver userResolver;
        private readonly IChatCancellationTracker _cancellationTracker;

        public ConversationsController(IUserResolver userResolver, IChatCancellationTracker cancellationTracker)
        {
            this.userResolver = userResolver;
            _cancellationTracker = cancellationTracker;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromServices] CreateConversationHandler handler, CancellationToken ct)
        {
            var user = await userResolver.GetCurrentUserAsync(ct);
            var command = new CreateConversationCommand(user.Id, user.UserName, "New Chat");
            var id = await handler.HandleAsync(command, ct);
            return Ok(id);
        }

        /// <summary>
        /// List All of Converation
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromServices] GetConversationListHandler getConversationListHandler, CancellationToken ct)
        {
            var user = await userResolver.GetCurrentUserAsync(ct);

            var getConversationListCommand = new GetConversationListQuery(user.Id);

            var conversations = await getConversationListHandler.HandleAsync(getConversationListCommand, ct);
            return Ok(
                conversations.Select(x =>
                    new ConversationListItemDto
                    {
                        CreatedAt = x.CreatedAt,
                        Id = x.Id,
                        Title = x.Title
                    })
                );
        }
        /// <summary>
        /// Get History of Converation 
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetMessageByConversationId(Guid conversationId, [FromServices] GetConversationHandler getConversationHandler, CancellationToken ct)
        {
            var user = await userResolver.GetCurrentUserAsync(ct);

            var getConversation = new GetConversationQuery(conversationId, user.Id);
            var conversation = await getConversationHandler.HandleAsync(getConversation, ct);

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
            [FromServices] SendMessageHandler handler,
            CancellationToken ct)
        {
            // ۱. ساخت و دریافت CancellationToken اختصاصی برای این مکالمه
            var token = _cancellationTracker.Register(conversationId.ToString());
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, token);

            try
            {
                var user = await userResolver.GetCurrentUserAsync(ct);

                var command = new SendMessageCommand(conversationId, user.Id, request.Message, request.Model);

                var answer = await handler.HandleAsync(command, linkedCts.Token);

                return Ok(new
                {
                    success = true
                });

            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "عملیات توسط کاربر متوقف شد.");
            }
            finally
            {
                // پس از اتمام موفق یا ناموفق، رکوردهای اضافی پاک شوند
                _cancellationTracker.Remove(conversationId.ToString());
            }
        }

        [HttpPost("{conversationId}/cancel")]
        public IActionResult CancelMessage(string conversationId)
        {
            // ۳. فراخوانی دستور لغو برای مکالمه جاری
            _cancellationTracker.Cancel(conversationId);
            return Ok(new { message = "درخواست لغو شد." });
        }



        [HttpDelete("{conversationId}")]
        public async Task<IActionResult> Delete(Guid conversationId, [FromServices] DeleteConversationHandler handler, CancellationToken ct)
        {
            var user = await userResolver.GetCurrentUserAsync(ct);

            var deleteConversation = new DeleteConversation(conversationId, user.Id);

            var result = await handler.HandleAsync(deleteConversation, ct);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Rename(
             Guid id,
            [FromBody] RenameConversationRequest request,
            [FromServices] RenameConversationHandler handler,
            CancellationToken ct)
        {

            var user = await userResolver.GetCurrentUserAsync(ct);

            await handler.HandleAsync(
                new RenameConversationCommand(
                        id,
                        user.Id,
                        request.Title),
                ct);

            return NoContent();
        }
    }
}
