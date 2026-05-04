using UglyToad.PdfPig;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography; // Necessário para gerar o Hash do conteúdo

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
            string textoArquivo = (arquivoBytes != null && arquivoBytes.Length > 0)
                ? LerPdfComSeguranca(arquivoBytes)
                : "";

            // --- LÓGICA DE ADAPTAÇÃO PARA RELATÓRIOS ---
            bool esRelatorio = trabalhoId.Contains("RELATORIO") || trabalhoId.Contains("REPORT");

            string inputCompleto;
            if (esRelatorio)
            {
                inputCompleto = $"[DADOS ESTATÍSTICOS DAS TURMAS]:\n{conteudoParaAnalisar}";
            }
            else
            {
                inputCompleto = $@"
[RESPOSTA DIGITADA PELO ALUNO]:
{conteudoParaAnalisar}

[CONTEÚDO DO FICHEIRO PDF ANEXADO]:
{(string.IsNullOrEmpty(textoArquivo) ? "Nenhum ficheiro PDF foi enviado." : textoArquivo)}".Trim();
            }

            // Validação de entrada
            if (string.IsNullOrWhiteSpace(conteudoParaAnalisar) && string.IsNullOrEmpty(textoArquivo))
                return "Ainda não encontrei conteúdo (texto ou PDF) para analisar.";

            // --- NOVA LÓGICA DE CACHE INTELIGENTE ---
            // Geramos um código único (Hash) baseado no conteúdo enviado + instrução do prof
            // Se o aluno mudar uma letra ou o professor mudar a instrução, o Hash muda.
            string hashConteudo = GerarHashConteudo(inputCompleto + instrucaoPrincipal);

            // A chave agora inclui o Hash. Se o conteúdo mudar, a chave muda e o cache falha.
            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}_{hashConteudo}";

            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            // 3. REFINAMENTO DO PROMPT (Toda a tua lógica original mantida)
            string promptSistemaFinal;
            if (esRelatorio)
            {
                promptSistemaFinal = $@"{promptSistemaOriginal}
Tu és um analista pedagógico. Analisa os números fornecidos, identifica tendências de notas, turmas que precisam de atenção e pontos positivos. Sê objetivo e prático.";
            }
            else
            {
                promptSistemaFinal = $@"
{promptSistemaOriginal}

Tu és um Mentor atento e sagaz. A tua parceria baseia-se na verdade e na qualidade.
Para além de ajudares no conteúdo, a tua intuição deve:
- **Validar a Consistência**: Verifica se o [TEXTO DO ALUNO] e o [CONTEÚDO DO PDF] estão alinhados entre si e com o [OBJETIVO DO PROFESSOR].
- **Detetar Desconexões**: Se o aluno enviou um PDF que não tem nada a ver com o tema, aborda isso de forma empática mas direta.
- **Unificar Fontes**: Se ambos forem válidos, cruza as informações para dar um feedback muito mais rico.

Não dês ordens; sê o espelho que ajuda o aluno a perceber se a sua entrega faz sentido como um todo.";
            }

            string promptUsuario = $@"
[OBJETIVO / CONTEXTO]:
{instrucaoPrincipal}

[FONTES DE DADOS]:
{inputCompleto}";

            // 4. Chamada à API
            string resultadoFinal = await EnviarParaGroq(promptSistemaFinal, promptUsuario);

            // Salvar no cache apenas se houver sucesso
            if (useCache && !string.IsNullOrEmpty(resultadoFinal) && !resultadoFinal.StartsWith("Erro"))
                _cache[cacheKey] = resultadoFinal.Trim();

            return resultadoFinal;
        }
        catch (Exception ex)
        {
            return $"Tive um pequeno precalço técnico ao analisar os dados. (Erro: {ex.Message})";
        }
    }

    // Método para gerar o Hash único do conteúdo
    private string GerarHashConteudo(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).Substring(0, 12); // Usamos os primeiros 12 caracteres do Hash
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
            temperature = 0.5,
            max_tokens = 1024
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GroqApiKey);
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"Erro de ligação com a IA: {response.StatusCode}";

            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()
                   ?? "Não consegui gerar uma resposta.";
        }
        catch (Exception ex)
        {
            return $"Erro na comunicação com o servidor Groq: {ex.Message}";
        }
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
            return "";
        }
    }
}