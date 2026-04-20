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

    // A tua chave Groq atualizada
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

            // PROMPT PROFISSIONAL PARA LLAMA 3
            string promptSistema = @"És um Professor avaliador rigoroso. 
Regras:
1. Fala diretamente com o aluno usando 'Tu' (ex: Tu fizeste, Teu erro).
2. Compara o TRABALHO com a INSTRUÇÃO.
3. Se o aluno cometeu erros factuais (como trocar conceitos científicos), corrige-os com a verdade.
4. Sê pedagógico, direto e não inventes informações.";

            string promptUsuario = $@"
INSTRUÇÃO DO PROFESSOR: ""{descricaoVertente}""
TRABALHO DO ALUNO: ""{textoCompleto}""

Responde apenas neste formato:
**1. Pontos positivos:**
**2. Pontos a melhorar:**
**3. Dica prática:**";

            // Chamada à API da Groq (Llama 3 8B)
            string resultadoFinal = await EnviarParaGroq(promptSistema, promptUsuario);

            if (string.IsNullOrWhiteSpace(resultadoFinal))
                resultadoFinal = "Tive um problema ao contactar o meu cérebro (IA). Tenta novamente.";

            if (useCache) _cache[cacheKey] = resultadoFinal.Trim();

            return resultadoFinal.Trim();
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
                model = "llama3-8b-8192", // Modelo potente que não falha em Química
                messages = new[] {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.5 // Equilíbrio perfeito entre foco e fluidez
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");

            // Configuração dos Headers de Segurança
            request.Headers.Add("Authorization", $"Bearer {GroqApiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"💥 Erro API Groq: {erro}");
                return "Ocorreu um erro na ligação à IA.";
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            // Navega no JSON da Groq para extrair a resposta
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro Conexão: {ex.Message}");
            return "";
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