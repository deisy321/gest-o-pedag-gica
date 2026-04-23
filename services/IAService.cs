using UglyToad.PdfPig;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class IAService
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, string> _cache = new();
    private const string GroqApiKey = "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";

    public IAService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TriadeLearn/1.0");
        }
    }

    public async Task<string> ObterSugestoes(
        string promptSistema,        // Persona e regras (definido no TrabalhoService)
        string instrucaoPrincipal,   // O que o professor pediu (Roteiro)
        string conteudoParaAnalisar, // Texto digitado pelo aluno
        string alunoId,
        string trabalhoId,
        string vertenteId,
        byte[]? arquivoBytes = null,
        bool useCache = true)
    {
        try
        {
            // 1. Extração de texto do PDF (Visão completa do ficheiro)
            string textoArquivo = LerPdfComSeguranca(arquivoBytes);

            // 2. União de fontes de dados do aluno
            string inputCompleto = $"[TEXTO DO ALUNO]:\n{conteudoParaAnalisar}\n\n[CONTEÚDO DO PDF]:\n{textoArquivo}".Trim();

            if (string.IsNullOrWhiteSpace(inputCompleto.Replace("\n", "")))
                return "Não foi encontrado conteúdo (texto ou PDF) para análise.";

            // Cache para performance e economia de API
            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}";
            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            // 3. Montagem do Prompt de Utilizador (O que a IA deve cruzar)
            string promptUsuario = $@"
INSTRUÇÃO DO PROFESSOR (OBJETIVO):
{instrucaoPrincipal}

ENTREGA REALIZADA PELO ALUNO:
{inputCompleto}";

            // 4. Chamada à Groq
            string resultadoFinal = await EnviarParaGroq(promptSistema, promptUsuario);

            if (useCache && !string.IsNullOrEmpty(resultadoFinal) && !resultadoFinal.StartsWith("Erro"))
                _cache[cacheKey] = resultadoFinal.Trim();

            return resultadoFinal;
        }
        catch (Exception ex)
        {
            return $"Erro técnico na IA: {ex.Message}";
        }
    }

    private async Task<string> EnviarParaGroq(string systemPrompt, string userPrompt)
    {
        var requestBody = new
        {
            model = "llama-3.1-8b-instant",
            messages = new[] {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.4 // Balanço entre criatividade e rigor
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GroqApiKey);
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) return $"Erro Real: {responseString}";

        using var doc = JsonDocument.Parse(responseString);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Resposta vazia.";
    }

    private string LerPdfComSeguranca(byte[]? arquivoBytes)
    {
        if (arquivoBytes == null || arquivoBytes.Length == 0) return "";
        try
        {
            using var ms = new MemoryStream(arquivoBytes);
            using var pdf = PdfDocument.Open(ms);
            var sb = new StringBuilder();
            foreach (var page in pdf.GetPages()) sb.AppendLine(page.Text);
            return sb.ToString();
        }
        catch { return "[Aviso: Não foi possível ler o conteúdo deste PDF]"; }
    }
}