using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OAuthBackend.Data;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using OAuthBackend.Models;

[ApiController]
[Route("api/[controller]")]
public class OAuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly OAuthDbContext _dbContext;

    public OAuthController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        OAuthDbContext dbContext) // 添加這個參數
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _dbContext = dbContext; // 添加這行
    }

    [HttpPost("exchange-token")]
    public async Task<IActionResult> ExchangeToken([FromBody] OAuthRequest request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _configuration["Strava:ClientId"]),
                new KeyValuePair<string, string>("client_secret", _configuration["Strava:ClientSecret"]),
                new KeyValuePair<string, string>("code", request.Code),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            var response = await client.PostAsync("https://www.strava.com/oauth/token", formData);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Raw response: {responseBody}");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            try
            {
                var tokenResponse = JsonSerializer.Deserialize<OAuthResponse>(responseBody, options);
                Console.WriteLine($"Deserialized object: {JsonSerializer.Serialize(tokenResponse, options)}");

                if (tokenResponse == null)
                {
                    Console.WriteLine("Deserialization resulted in null object");
                    return BadRequest("Failed to deserialize response");
                }

                // 確保所有需要的屬性都有值
                if (string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    Console.WriteLine("AccessToken is null or empty");
                }
                if (string.IsNullOrEmpty(tokenResponse.RefreshToken))
                {
                    Console.WriteLine("RefreshToken is null or empty");
                }
                if (tokenResponse.ExpiresAt == 0)
                {
                    Console.WriteLine("ExpiresAt is 0");
                }

                // 嘗試直接從 JObject 解析
                using (JsonDocument document = JsonDocument.Parse(responseBody))
                {
                    JsonElement root = document.RootElement;
                    var accessToken = root.GetProperty("access_token").GetString();
                    var refreshToken = root.GetProperty("refresh_token").GetString();
                    var expiresAt = root.GetProperty("expires_at").GetInt32();

                    Console.WriteLine($"Directly parsed values:");
                    Console.WriteLine($"AccessToken: {accessToken}");
                    Console.WriteLine($"RefreshToken: {refreshToken}");
                    Console.WriteLine($"ExpiresAt: {expiresAt}");

                    var userToken = new UserToken
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresAt = expiresAt
                    };

                    _dbContext.UserTokens.Add(userToken);
                    await _dbContext.SaveChangesAsync();

                    return Ok(new { message = "Token saved successfully" });
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON deserialization error: {ex.Message}");
                Console.WriteLine($"Path: {ex.Path}");
                Console.WriteLine($"LineNumber: {ex.LineNumber}");
                return BadRequest($"Failed to parse response: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }


    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var client = _httpClientFactory.CreateClient();
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", "YOUR_CLIENT_ID"),
            new KeyValuePair<string, string>("client_secret", "YOUR_CLIENT_SECRET"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("grant_type", "refresh_token")
        });

        var response = await client.PostAsync("https://www.strava.com/oauth/token", formData);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest(responseBody);
        }

        var newToken = JsonSerializer.Deserialize<OAuthResponse>(responseBody);

        // 更新資料庫中的 Token
        using (var scope = new OAuthDbContext(new DbContextOptions<OAuthDbContext>()))
        {
            var userToken = scope.UserTokens.FirstOrDefault(u => u.RefreshToken == refreshToken);
            if (userToken != null)
            {
                userToken.AccessToken = newToken.AccessToken;
                userToken.ExpiresAt = newToken.ExpiresAt;
                scope.SaveChanges();
            }
        }

        return Ok(newToken);
    }
}

public class OAuthRequest
{
    public string Code { get; set; }
}

public class OAuthResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresAt { get; set; }
}
