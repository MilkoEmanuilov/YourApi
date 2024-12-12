public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddHealthChecks()
            .AddCheck<SqlServerHealthCheck>("sql_server", tags: new[] { "database" })
            .AddCheck<MigrationHealthCheck>("migrations", tags: new[] { "database" });

        services.AddHttpClient<IAuthService, AuthService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        await DatabaseInitializer.InitializeAsync(serviceProvider);
    }
}