// Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Keycloak authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Keycloak:Authority"];
    options.Audience = builder.Configuration["Keycloak:ClientId"];
    options.RequireHttpsMetadata = false; // For development only

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,  // Either set this to false
        // OR set explicit validation with multiple audiences:
        /*
        ValidateAudience = true,
        ValidAudiences = new[] { "account", "public-client" },
        */
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Keycloak:Authority"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };

    // Keep the debug events
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        }
    };
});

// Add CORS for Nuxt
builder.Services.AddCors(options =>
{
    options.AddPolicy("NuxtPolicy", corsBuilder =>
    {
        corsBuilder.WithOrigins(builder.Configuration.GetValue<string>("AllowedOrigins"))
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});



// Add infrastructure and application services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CreatePostCommand).Assembly);
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    Console.WriteLine("\n=== Request Headers ===");
    var authHeader = context.Request.Headers["Authorization"].ToString();
    Console.WriteLine($"Authorization header: {authHeader}");

    foreach (var header in context.Request.Headers)
    {
        Console.WriteLine($"{header.Key}: {header.Value}");
    }
    Console.WriteLine("=====================\n");

    await next();
});

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.Services.InitializeDatabaseAsync();

app.UseHttpsRedirection();
app.UseCors("NuxtPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
