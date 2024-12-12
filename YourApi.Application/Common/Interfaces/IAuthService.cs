public interface IAuthService
{
    Task<AuthResponse> RegisterUserAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
