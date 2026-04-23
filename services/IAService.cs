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
        string promptSistemaOriginal,
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
            // 1. Extração de texto do PDF
            string textoArquivo = LerPdfComSeguranca(arquivoBytes);

            // Criamos uma estrutura clara para a IA saber o que veio de onde
            string inputCompleto = $@"
[RESPOSTA DIGITADA PELO ALUNO]:
{conteudoParaAnalisar}

[CONTEÚDO DO FICHEIRO PDF ANEXADO]:
{(string.IsNullOrEmpty(textoArquivo) ? "Nenhum ficheiro PDF foi enviado." : textoArquivo)}".Trim();

            if (string.IsNullOrWhiteSpace(conteudoParaAnalisar) && string.IsNullOrEmpty(textoArquivo))
                return "Ainda não encontrei conteúdo (texto ou PDF) para analisar. Podes partilhar o teu trabalho comigo?";

            // 2. Gestão de Cache
            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}";
            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            // 3. REFINAMENTO DA INTUIÇÃO (Agora com Verificação de Consistência)
            string promptSistemaIntuitivo = $@"
{promptSistemaOriginal}

Tu és um Mentor atento e sagaz. A tua parceria baseia-se na verdade e na qualidade.
Para além de ajudares no conteúdo, a tua intuição deve:
- **Validar a Consistência**: Verifica se o [TEXTO DO ALUNO] e o [CONTEÚDO DO PDF] estão alinhados entre si e com o [OBJETIVO DO PROFESSOR].
- **Detetar Desconexões**: Se o aluno enviou um PDF que não tem nada a ver com o tema (ou se o texto dele foge completamente ao assunto), aborda isso de forma empática mas direta. Exemplo: 'Reparei que o ficheiro que anexaste fala sobre X, mas o nosso trabalho é sobre Y. Queres verificar se enviaste o documento correto?'.
- **Unificar Fontes**: Se ambos forem válidos, cruza as informações para dar um feedback muito mais rico.

Não dês ordens; sê o espelho que ajuda o aluno a perceber se a sua entrega faz sentido como um todo.";

            string promptUsuario = $@"
[OBJETIVO DO PROFESSOR]:
{instrucaoPrincipal}

[FONTES DE DADOS DO ALUNO]:
{inputCompleto}";

            // 4. Chamada à API
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
            temperature = 0.5
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GroqApiKey);
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) return $"Erro de ligação com a IA: {response.StatusCode}";

        using var doc = JsonDocument.Parse(responseString);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Não consegui gerar uma resposta.";
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
            return ""; // Se falhar, tratamos como sem conteúdo no ObterSugestoes
        }
    }
}