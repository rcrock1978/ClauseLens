using ClauseLens.Application.Identity.Commands;
using ClauseLens.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClauseLens.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpPost("invite")]
    public async Task<IActionResult> Invite([FromBody] InviteUserRequest body, CancellationToken ct)
    {
        if (body is null) return BadRequest(new { error = "body required" });
        if (!Enum.TryParse<UserRole>(body.Role, true, out var role))
            return BadRequest(new { error = "role must be one of: ContractOwner, Reviewer", code = "INVALID_ROLE" });
        var result = await _mediator.Send(new InviteUserCommand(body.Email, role), ct);
        if (!result.IsSuccess) return StatusCode(StatusCodes.Status403Forbidden, new { error = result.Error, code = result.ErrorCode });
        return StatusCode(StatusCodes.Status201Created, result.Value);
    }
}

public sealed record InviteUserRequest(string Email, string Role);
