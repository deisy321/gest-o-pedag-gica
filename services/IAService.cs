using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class IAService
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, string> _cache = new();

    // Chave API da Groq
    private const string GroqApiKey = "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";

    public IAService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        // Configuração de User-Agent para evitar bloqueios de API
        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TriadeLearn/1.0");
        }
    }

    public async Task<string> ObterSugestoes(
        string textoAluno,
        string descricaoVertente,
        string vertenteId,
        string alunoId,
        string trabalhoId,
        byte[]? arquivoBytes = null,
        bool useCache = true)
    {
        try
        {
            // Verificação de conteúdo vazio
            if (string.IsNullOrWhiteSpace(textoAluno) && (arquivoBytes == null || arquivoBytes.Length == 0))
                return "Tu não enviaste conteúdo. Por favor, escreve algo ou anexa um PDF.";

            string textoArquivo = LerPdfComSeguranca(arquivoBytes);
            string textoCompleto = (textoAluno ?? "") + "\n" + textoArquivo;

            // Lógica de Cache
            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}";
            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            // Definição dos Prompts
            string promptSistema = @"És um Professor avaliador rigoroso. 
Regras:
1. Fala diretamente com o aluno usando 'Tu' (ex: Tu fizeste, Teu erro).
2. Compara o TRABALHO com a INSTRUÇÃO.
3. Se o aluno cometeu erros factuais, corrige-os com a verdade científica.
4. Sê pedagógico, direto e não inventes informações.";

            string promptUsuario = $@"
INSTRUÇÃO DO PROFESSOR: ""{descricaoVertente}""
TRABALHO DO ALUNO: ""{textoCompleto}""

Responde apenas neste formato:
**1. Pontos positivos:**
**2. Pontos a melhorar:**
**3. Dica prática:**";

            // Chamada à API da Groq
            string resultadoFinal = await EnviarParaGroq(promptSistema, promptUsuario);

            if (useCache && !string.IsNullOrEmpty(resultadoFinal))
                _cache[cacheKey] = resultadoFinal.Trim();

            return string.IsNullOrEmpty(resultadoFinal) ? "Erro ao gerar feedback." : resultadoFinal.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro Geral: {ex.Message}");
            return "Erro técnico ao processar sugestões.";
        }
    }

    private async Task<string> EnviarParaGroq(string systemPrompt, string userPrompt)
    {
        try
        {
            var requestBody = new
            {
                // Modelo atualizado para Llama 3.1 70B (Versátil e Inteligente)
                model = "llama-3.1-70b-versatile",
                messages = new[] {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.3
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");

            // Autenticação Bearer Token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GroqApiKey.Trim());
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Log de erro visível na consola do Render
                Console.WriteLine($"💥 Erro API Groq: {responseString}");
                return "Ocorreu um erro na ligação à IA. O modelo pode estar em manutenção.";
            }

            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro de Conexão: {ex.Message}");
            return "Erro de comunicação com o Llama 3.";
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
            foreach (var page in pdf.GetPages()) sb.AppendLine(page.Text);
            return sb.ToString();
        }
        catch { return ""; }
    }
}