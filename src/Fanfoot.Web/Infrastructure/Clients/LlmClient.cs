using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Fanfoot.Infrastructure.Clients;

public class LlmClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string? _groqApiKey;
    private readonly string? _deepSeekApiKey;
    private readonly string _ollamaModel;

    public LlmClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _groqApiKey = configuration["GroqApiKey"];
        _deepSeekApiKey = configuration["DeepSeekApiKey"];
        _ollamaModel = configuration["OllamaModel"] ?? "qwen2.5:7b";
    }

    public async Task<IReadOnlyList<(string Provider, string Model)>> GetModelsAsync(CancellationToken ct = default)
    {
        var models = new List<(string Provider, string Model)>();

        try
        {
            var ollama = _httpClientFactory.CreateClient("Ollama");
            var result = await ollama.GetFromJsonAsync<OllamaModelsResponse>("api/tags", ct);
            models.AddRange(result?.Models?.Select(model => ("ollama", model.Name)) ?? []);
        }
        catch (HttpRequestException) { }

        if (!models.Any(model => model.Provider == "ollama"))
            models.Add(("ollama", _ollamaModel));

        if (!string.IsNullOrEmpty(_groqApiKey))
        {
            try
            {
                var groq = _httpClientFactory.CreateClient("Groq");
                var result = await groq.GetFromJsonAsync<HostedModelsResponse>("models", ct);
                models.AddRange(result?.Data?.Select(model => ("groq", model.Id)) ?? []);
            }
            catch (HttpRequestException) { }

            if (!models.Any(model => model.Provider == "groq"))
                models.Add(("groq", "llama-3.3-70b-versatile"));
        }

        if (!string.IsNullOrEmpty(_deepSeekApiKey))
        {
            try
            {
                var deepSeek = _httpClientFactory.CreateClient("DeepSeek");
                var result = await deepSeek.GetFromJsonAsync<HostedModelsResponse>("models", ct);
                models.AddRange(result?.Data?.Select(model => ("deepseek", model.Id)) ?? []);
            }
            catch (HttpRequestException) { }

            if (!models.Any(model => model.Provider == "deepseek"))
                models.Add(("deepseek", "deepseek-chat"));
        }

        return models;
    }

    public async Task<LlmMessage?> ChatAsync(string? provider, string? model, List<LlmMessage> messages, List<LlmTool>? tools, CancellationToken ct = default)
    {
        var isGroq = string.Equals(provider, "groq", StringComparison.OrdinalIgnoreCase);
        var isDeepSeek = string.Equals(provider, "deepseek", StringComparison.OrdinalIgnoreCase);
        var isOllama = string.IsNullOrEmpty(provider) || string.Equals(provider, "ollama", StringComparison.OrdinalIgnoreCase);
        if (!isOllama && !isGroq && !isDeepSeek)
            throw new InvalidOperationException("The selected AI provider is not supported.");
        if (isGroq && string.IsNullOrEmpty(_groqApiKey))
            throw new InvalidOperationException("Groq is not configured.");
        if (isDeepSeek && string.IsNullOrEmpty(_deepSeekApiKey))
            throw new InvalidOperationException("DeepSeek is not configured.");

        var selectedModel = string.IsNullOrWhiteSpace(model)
            ? isGroq ? "llama-3.3-70b-versatile" : isDeepSeek ? "deepseek-chat" : _ollamaModel
            : model;
        var request = new LlmChatRequest
        {
            Model = selectedModel,
            Messages = messages,
            Tools = tools,
            Stream = false,
            Options = isGroq || isDeepSeek ? null : new Dictionary<string, object> { ["num_ctx"] = 8192 }
        };

        var http = _httpClientFactory.CreateClient(isGroq ? "Groq" : isDeepSeek ? "DeepSeek" : "Ollama");
        var endpoint = isOllama ? "v1/chat/completions" : "chat/completions";
        var response = await http.PostAsJsonAsync(endpoint, request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"{(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}");
        }
        var result = await response.Content.ReadFromJsonAsync<LlmChatResponse>(ct);
        return result?.Choices?.FirstOrDefault()?.Message;
    }

    private sealed class OllamaModelsResponse
    {
        public List<OllamaModel>? Models { get; set; }
    }

    private sealed class OllamaModel
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class HostedModelsResponse
    {
        public List<HostedModel>? Data { get; set; }
    }

    private sealed class HostedModel
    {
        public string Id { get; set; } = string.Empty;
    }
}
