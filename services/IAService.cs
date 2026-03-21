using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

public class IAService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    // Cache simples em memória: chave = alunoId + trabalhoId
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public IAService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey ?? throw new Exception("OpenAI API key não fornecida");
    }

    /// <summary>
    /// Sobrecarga compatível com chamadas antigas (apenas texto do aluno + descrição do trabalho).
    /// </summary>
    public async Task<string> ObterSugestoes(string textoAluno, string descricaoTrabalho)
    {
        // Chama a versão nova com valores padrão para IDs (sem cache por usuário/trabalho)
        return await ObterSugestoes("defaultAlunoId", "defaultTrabalhoId", textoAluno, descricaoTrabalho, useCache: false);
    }

    /// <summary>
    /// Versão completa com cache por aluno e trabalho.
    /// </summary>
    public async Task<string> ObterSugestoes(string alunoId, string trabalhoId, string textoAluno, string descricaoTrabalho, bool useCache = true)
    {
        if (string.IsNullOrWhiteSpace(textoAluno))
            return "Conteúdo do aluno vazio, impossível gerar feedback.";

        if (string.IsNullOrWhiteSpace(descricaoTrabalho))
            descricaoTrabalho = "Descrição do trabalho indisponível.";

        // Criar chave do cache
        var cacheKey = $"{alunoId}_{trabalhoId}";
        if (useCache && _cache.TryGetValue(cacheKey, out var cachedFeedback))
            return cachedFeedback;

        // Mensagens no formato de chat (requerido para gpt-3.5-turbo)
        var messages = new[]
        {
            new { role = "system", content = "Você é um assistente que fornece feedback construtivo para trabalhos escolares." },
            new { role = "user", content = $@"
Leia a descrição do trabalho:
{descricaoTrabalho}

Agora analise a resposta do aluno:
{textoAluno}

Forneça:
1. Pontos positivos do texto.
2. Sugestões de melhoria (clareza, estrutura, ortografia, argumentos).
3. Dicas para aproximar melhor do objetivo do trabalho.

Retorne o feedback de forma objetiva e organizada." }
        };

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages,
            max_tokens = 400,
            temperature = 0.7
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = JsonContent.Create(requestBody);

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Se for limite ou cota, retornar mensagem amigável
                if ((int)response.StatusCode == 429 || content.Contains("insufficient_quota"))
                    return "Não foi possível gerar feedback neste momento: limite de requisições atingido ou cota esgotada.";

                return $"Não foi possível gerar feedback. Status: {response.StatusCode}. Detalhes: {content}";
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var feedback = doc.RootElement
                              .GetProperty("choices")[0]
                              .GetProperty("message")
                              .GetProperty("content")
                              .GetString();

            if (string.IsNullOrWhiteSpace(feedback))
                feedback = "Nenhum feedback gerado pelo modelo.";

            // Salvar no cache
            if (useCache) _cache[cacheKey] = feedback.Trim();

            return feedback.Trim();
        }
        catch
        {
            return "Erro ao gerar feedback. Tente novamente mais tarde.";
        }
    }
}