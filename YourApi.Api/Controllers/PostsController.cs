using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public ActionResult Get()
    {
        return Ok("Ok");
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreatePostCommand command)
    {
        try
        {
            return await _mediator.Send(command);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }
}
