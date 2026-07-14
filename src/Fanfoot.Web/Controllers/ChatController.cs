using System.Security.Claims;
using Fanfoot.Domain.Models;
using Fanfoot.Domain.Services;
using Fanfoot.Web.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fanfoot.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatController(ChatService chat, ResourceAccessService access) : ControllerBase
{
    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyList<ChatSessionDto>>> Sessions()
    {
        var sessions = await chat.GetSessionsAsync(UserId);
        return Ok(sessions.Select(ToSummary).ToList());
    }

    [HttpGet("sessions/{sessionId}")]
    public async Task<ActionResult<ChatSessionDetailDto>> Session(string sessionId)
    {
        if (!await access.OwnsSessionAsync(UserId, sessionId)) return NotFound();
        var session = await chat.GetSessionAsync(UserId, sessionId);
        return session == null ? NotFound() : Ok(ToDetail(session));
    }

    [HttpPost("messages")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<SendChatMessageResponse>> Send(SendChatMessageRequest request)
    {
        var message = request.Message?.Trim();
        if (string.IsNullOrWhiteSpace(message)) return BadRequest(new { error = "A message is required." });
        if (!string.IsNullOrEmpty(request.LeagueId) && !await access.IsLeagueMemberAsync(UserId, request.LeagueId)) return NotFound();

        var sessionId = string.IsNullOrEmpty(request.SessionId) ? Guid.NewGuid().ToString() : request.SessionId;
        if (request.SessionId != null && !await access.OwnsSessionAsync(UserId, sessionId)) return NotFound();
        var existing = await chat.GetSessionAsync(UserId, sessionId);
        if (request.SessionId != null && existing == null) return NotFound();
        var history = existing == null ? new List<(string Role, string Content)>() : ChatService.DeserializeHistory(existing.MessagesJson);
        history.Add(("user", message));
        var prompt = string.IsNullOrEmpty(request.LeagueId)
            ? await chat.GetUserContextAsync(UserId)
            : await chat.GetLeagueContextAsync(request.LeagueId, UserId);
        var answer = await chat.AskAsync(prompt, history);
        history.Add(("assistant", answer));
        await chat.SaveSessionAsync(sessionId, UserId, request.LeagueId, history.First(item => item.Role == "user").Content, history);
        var saved = await chat.GetSessionAsync(UserId, sessionId);
        return Ok(new SendChatMessageResponse(ToDetail(saved!), new ChatMessageDto("assistant", answer)));
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private static ChatSessionDto ToSummary(ChatSession session) => new(session.Id, session.LeagueId, session.Title, session.UpdatedAt);
    private static ChatSessionDetailDto ToDetail(ChatSession session) => new(ToSummary(session), ChatService.DeserializeHistory(session.MessagesJson).Select(message => new ChatMessageDto(message.Role, message.Content)).ToList());
}
