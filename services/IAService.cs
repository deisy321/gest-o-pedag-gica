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
        string promptSistemaOriginal, // Recebido do TrabalhoService
        string instrucaoPrincipal,
        string conteudoParaAnalisar,
        string alunoId,
        string trabalhoId,
        string vertenteId,
        byte[]? arquivoBytes = null,
        bool useCache = true)
    {
        try
        {
            // 1. Extração e Preparação
            string textoArquivo = LerPdfComSeguranca(arquivoBytes);
            string inputCompleto = $"[TEXTO DO ALUNO]:\n{conteudoParaAnalisar}\n\n[CONTEÚDO DO PDF]:\n{textoArquivo}".Trim();

            if (string.IsNullOrWhiteSpace(inputCompleto.Replace("\n", "")))
                return "Ainda não encontrei conteúdo para analisar. Podes escrever algo ou anexar um PDF?";

            // 2. Gestão de Cache
            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}";
            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            // 3. REFINAMENTO DA INTUIÇÃO (Onde a IA "aprende" a ser Mentor)
            // Aqui fundimos o prompt do sistema com a alma do Mentor Pedagógico de forma orgânica
            string promptSistemaIntuitivo = $@"
{promptSistemaOriginal}

Como Mentor, a tua essência é a parceria. Tu não dás ordens, tu geras clareza. 
A tua intuição guia-te para:
- Perceber onde o aluno hesitou tecnicamente e oferecer a peça que falta no puzzle.
- Transformar correções frias em sugestões fluidas que elevam o profissionalismo da resposta.
- Manter o rigor da matéria como base, mas a empatia como voz.
Sê o suporte que o aluno precisa para que ele próprio sinta orgulho na versão final que vai entregar.";

            string promptUsuario = $@"
OBJETIVO DO PROFESSOR:
{instrucaoPrincipal}

O QUE O ALUNO DESENVOLVEU:
{inputCompleto}";

            // 4. Comunicação com o Cérebro da IA (Groq/Llama)
            string resultadoFinal = await EnviarParaGroq(promptSistemaIntuitivo, promptUsuario);

            if (useCache && !string.IsNullOrEmpty(resultadoFinal) && !resultadoFinal.StartsWith("Erro"))
                _cache[cacheKey] = resultadoFinal.Trim();

            return resultadoFinal;
        }
        catch (Exception ex)
        {
            return $"Tive um pequeno precalço técnico ao analisar o teu trabalho. Podes tentar novamente? (Erro: {ex.Message})";
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
            temperature = 0.5 // Aumentado ligeiramente para permitir uma linguagem mais fluida e menos robótica
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GroqApiKey);
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) return $"Erro de ligação com a IA: {response.StatusCode}";

        using var doc = JsonDocument.Parse(responseString);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Não consegui gerar uma resposta de momento.";
    }

    private string LerPdfComSeguranca(byte[]? arquivoBytes)
    {
        if (arquivoBytes == null || arquivoBytes.Length == 0) return "";
        try
        {
            using var ms = new MemoryStream(arquivoBytes);
            using var pdf = PdfDocument.Open(ms);
            var sb = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                sb.AppendLine(page.Text);
            }
            return sb.ToString();
        }
        catch
        {
            return "[Aviso: O conteúdo do ficheiro PDF não pôde ser lido corretamente]";
        }
    }
}