using Microsoft.AspNetCore.Identity.Data;
using System.Text.Json;
using System.Text;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _keycloakBaseUrl;
    private readonly string _realm;
    private readonly string _clientId;

    public AuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _realm = configuration["Keycloak:Realm"];
        _clientId = configuration["Keycloak:ClientId"];
        _keycloakBaseUrl = configuration["Keycloak:Authority"].Replace($"/realms/{_realm}", "");
    }

    public async Task<AuthResponse> RegisterUserAsync(RegisterRequest request)
    {
    // Redirect user to Keycloak's registration page
        var registrationUrl = $"{_keycloakBaseUrl}/realms/{_realm}/protocol/openid-connect/auth";

        // Or use the registration endpoint if available in your realm
        var parameters = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "username", request.Username },
            { "email", request.Email },
            { "password", request.Password },
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _httpClient.PostAsync(registrationUrl, content);

        if (response.IsSuccessStatusCode)
        {
            return await LoginAsync(new LoginRequest
            {
                Username = request.Username,
                Password = request.Password
            });
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        throw new Exception($"Registration failed: {errorContent}");
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "grant_type", "password" },
            { "username", request.Username },
            { "password", request.Password }
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _httpClient.PostAsync(
            $"{_keycloakBaseUrl}/realms/{_realm}/protocol/openid-connect/token",
            content
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await JsonSerializer.DeserializeAsync<AuthResponse>(
                await response.Content.ReadAsStreamAsync()
            );
            return result;
        }

        throw new Exception("Login failed");
    }
}
