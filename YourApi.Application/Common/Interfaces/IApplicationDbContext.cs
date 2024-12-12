public interface IApplicationDbContext
{
    DbSet<Post> Posts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
