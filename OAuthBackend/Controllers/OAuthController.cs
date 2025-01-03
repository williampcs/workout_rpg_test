using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OAuthBackend.Data;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class OAuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OAuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("exchange-token")]
    public async Task<IActionResult> ExchangeToken([FromBody] OAuthRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", "YOUR_CLIENT_ID"),
            new KeyValuePair<string, string>("client_secret", "YOUR_CLIENT_SECRET"),
            new KeyValuePair<string, string>("code", request.Code),
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        });

        var response = await client.PostAsync("https://www.strava.com/oauth/token", formData);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest(responseBody);
        }

        return Ok(JsonSerializer.Deserialize<OAuthResponse>(responseBody));
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
