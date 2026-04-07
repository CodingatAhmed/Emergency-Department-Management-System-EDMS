using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EDMS.Console.ApiClient;

public class ApiHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private string? _token;

    public ApiHttpClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public void SetToken(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public string? CurrentToken => _token;

    public async Task<T?> GetAsync<T>(string path)
    {
        var response = await _httpClient.GetAsync(path);
        return await Handle<T>(response);
    }

    public async Task<T?> PostAsync<TReq, T>(string path, TReq body)
    {
        var response = await _httpClient.PostAsJsonAsync(path, body);
        return await Handle<T>(response);
    }

    public async Task<T?> PutAsync<TReq, T>(string path, TReq body)
    {
        var response = await _httpClient.PutAsJsonAsync(path, body);
        return await Handle<T>(response);
    }

    private async Task<T?> Handle<T>(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            System.Console.WriteLine($"[API ERROR] {(int)response.StatusCode}: {payload}");
            return default;
        }

        if (string.IsNullOrWhiteSpace(payload))
            return default;

        return JsonSerializer.Deserialize<T>(payload, _jsonOptions);
    }
}
