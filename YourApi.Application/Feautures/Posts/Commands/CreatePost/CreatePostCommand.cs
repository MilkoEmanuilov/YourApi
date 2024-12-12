public record CreatePostCommand : IRequest<int>
{
    public string Title { get; init; }
    public string Content { get; init; }
}

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreatePostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var entity = new Post
        {
            Title = request.Title,
            Content = request.Content,
            Created = DateTime.UtcNow
        };

        _context.Posts.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}