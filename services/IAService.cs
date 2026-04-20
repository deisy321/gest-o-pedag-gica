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

    // TUA NOVA CHAVE ATUALIZADA
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
            string textoArquivo = LerPdfComSeguranca(arquivoBytes);
            string textoCompleto = (textoAluno ?? "").Trim() + "\n" + textoArquivo.Trim();

            if (string.IsNullOrWhiteSpace(textoCompleto.Replace("\n", "")))
                return "Tu não enviaste conteúdo. Por favor, escreve algo ou anexa um PDF.";

            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}";
            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            string promptSistema = "És um Professor avaliador. Fala por 'Tu'. Compara o TRABALHO com a INSTRUÇÃO e corrige erros factuais.";
            string promptUsuario = $"INSTRUÇÃO: {descricaoVertente}\nTRABALHO: {textoCompleto}\n\nResponde em: 1. Pontos positivos, 2. Pontos a melhorar, 3. Dica prática.";

            // CHAMADA À API
            string resultadoFinal = await EnviarParaGroq(promptSistema, promptUsuario);

            if (useCache && !string.IsNullOrEmpty(resultadoFinal) && !resultadoFinal.StartsWith("Erro Real:"))
                _cache[cacheKey] = resultadoFinal.Trim();

            return resultadoFinal;
        }
        catch (Exception ex)
        {
            return $"Erro técnico: {ex.Message}";
        }
    }

    private async Task<string> EnviarParaGroq(string systemPrompt, string userPrompt)
    {
        try
        {
            var requestBody = new
            {
                model = "llama-3.1-8b-instant", // Modelo mais estável
                messages = new[] {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.2
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GroqApiKey.Trim());
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // AQUI: Retornamos o erro real da Groq para o ecrã
                return $"Erro Real: {responseString}";
            }

            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Resposta vazia.";
        }
        catch (Exception ex)
        {
            return $"Erro de Conexão: {ex.Message}";
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