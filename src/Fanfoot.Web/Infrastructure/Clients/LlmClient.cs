using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Fanfoot.Infrastructure.Clients;

public class LlmClient
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly bool _isGroq;

    public LlmClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _http = httpClientFactory.CreateClient("Ollama");
        _isGroq = !string.IsNullOrEmpty(configuration["GroqApiKey"]);
        _model = configuration["LlmModel"]
            ?? (_isGroq ? "llama-3.3-70b-versatile" : "qwen2.5:7b");
    }

    public string Model => _model;

    public async Task<LlmMessage?> ChatAsync(List<LlmMessage> messages, List<LlmTool>? tools, CancellationToken ct = default)
    {
        var request = new LlmChatRequest
        {
            Model = _model,
            Messages = messages,
            Tools = tools,
            Stream = false,
            Options = _isGroq ? null : new Dictionary<string, object> { ["num_ctx"] = 8192 }
        };

        var response = await _http.PostAsJsonAsync("chat/completions", request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"{(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}");
        }
        var result = await response.Content.ReadFromJsonAsync<LlmChatResponse>(ct);
        return result?.Choices?.FirstOrDefault()?.Message;
    }
}
