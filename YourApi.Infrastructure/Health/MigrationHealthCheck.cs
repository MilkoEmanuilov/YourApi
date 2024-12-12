using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

public class MigrationHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MigrationHealthCheck> _logger;

    public MigrationHealthCheck(ApplicationDbContext context, ILogger<MigrationHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "PendingMigrations", pendingMigrations.ToList() },
                { "AppliedMigrations", appliedMigrations.ToList() }
            };

            if (!pendingMigrations.Any())
            {
                return HealthCheckResult.Healthy("All migrations are applied", data);
            }

            return HealthCheckResult.Degraded("There are pending migrations", null, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration health check failed");
            return HealthCheckResult.Unhealthy("Failed to check migrations", ex);
        }
    }
}
