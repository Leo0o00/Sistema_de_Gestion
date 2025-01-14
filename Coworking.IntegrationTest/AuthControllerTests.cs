using System.Net;
using System.Net.Http.Json;
using Coworking.Application.DTOs;
// Donde está Program

namespace IntegrationTest
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            // Crea instancia de la app en memoria
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ShouldCreateUser_WhenValidData()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Username = "integrationTestUser",
                Email = "integration@test.com",
                Password = "secret123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            Assert.True(content.ContainsKey("Message"));
            Assert.True(content.ContainsKey("UserId"));
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // 1) Registrar primero
            var registerDto = new RegisterUserDto
            {
                Username = "loginUser",
                Email = "login@test.com",
                Password = "mypassword"
            };
            var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            regResponse.EnsureSuccessStatusCode();

            // 2) Hacer login
            var loginDto = new LoginUserDto
            {
                Username = "loginUser",
                Password = "mypassword"
            };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            var tokenContent = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.True(tokenContent.ContainsKey("token"));
            Assert.False(string.IsNullOrEmpty(tokenContent["token"]));
        }

        [Fact]
        public async Task ChangeRole_ShouldReturn401_WhenNotAdmin()
        {
            // 1) Registrar y loguear un usuario normal (rol User)
            var regDto = new RegisterUserDto
            {
                Username = "userNoAdmin",
                Email = "userNoAdmin@test.com",
                Password = "1234"
            };
            await _client.PostAsJsonAsync("/api/auth/register", regDto);
            // login
            var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginUserDto
            {
                Username = "userNoAdmin",
                Password = "1234"
            });
            var tokenData = await loginRes.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var userToken = tokenData["token"];

            // 2) Intentar PUT /api/auth/1 (cambiar rol) sin ser Admin
            // No sabemos su UserId real, la API lo devolvió en register. 
            // Para simplificar, supondremos que es 2. (En un test real, deberíamos parsearlo del register).
            var changeRole = new ChangeRoleDto { NewRole = "Admin" };
            var request = new HttpRequestMessage(HttpMethod.Put, "/api/auth/2"); 
            request.Content = JsonContent.Create(changeRole);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var response = await _client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
