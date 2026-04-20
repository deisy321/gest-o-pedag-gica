using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class IAService
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, string> _cache = new();

    // A tua chave que geraste
    private const string GroqApiKey = "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";

    public IAService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
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
            if (string.IsNullOrWhiteSpace(textoAluno) && (arquivoBytes == null || arquivoBytes.Length == 0))
                return "Tu não enviaste conteúdo. Por favor, escreve algo ou anexa um PDF.";

            string textoArquivo = LerPdfComSeguranca(arquivoBytes);
            string textoCompleto = (textoAluno ?? "") + "\n" + textoArquivo;

            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}";
            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            // PROMPT PARA O LLAMA 3 AGIR COMO PROFESSOR DIRETO
            string promptSistema = @"És um Professor mentor.
REGRAS:
1. Fala DIRETAMENTE com o aluno (usa 'Tu', 'Teu', 'Fizeste').
2. Se o aluno disser algo falso ou errado, corrige-o imediatamente com a verdade.
3. Não inventes código nem fales de APIs.";

            string promptUsuario = $@"
INSTRUÇÃO DO PROFESSOR: ""{descricaoVertente}""
TRABALHO DO ALUNO: ""{textoCompleto}""

Responde neste formato:
**1. Pontos positivos:**
**2. O que precisas de corrigir:**
**3. Dica prática:**";

            // Chamada para o LLAMA 3 via Groq
            string resultadoFinal = await EnviarParaGroq(promptSistema, promptUsuario);

            if (useCache) _cache[cacheKey] = resultadoFinal.Trim();

            return resultadoFinal.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro: {ex.Message}");
            return "Erro ao processar sugestões com o Llama.";
        }
    }

    private async Task<string> EnviarParaGroq(string system, string user)
    {
        try
        {
            var requestBody = new
            {
                model = "llama3-8b-8192", // Estamos a usar o motor LLAMA 3 aqui
                messages = new[] {
                    new { role = "system", content = system },
                    new { role = "user", content = user }
                },
                temperature = 0.5
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GroqApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }
        catch { return "Erro de comunicação com o Llama 3."; }
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