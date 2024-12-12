using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

public class SqlServerHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqlServerHealthCheck> _logger;

    public SqlServerHealthCheck(IConfiguration configuration, ILogger<SqlServerHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync(cancellationToken);

            // Check if we can execute a simple query
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("SQL Server is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL Server health check failed");
            return HealthCheckResult.Unhealthy("SQL Server is unhealthy", ex);
        }
    }
}
