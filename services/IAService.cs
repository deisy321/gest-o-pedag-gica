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
            if (string.IsNullOrWhiteSpace(textoAluno) && arquivoBytes == null)
                return "Conteúdo do aluno vazio, impossível gerar feedback.";

            if (string.IsNullOrWhiteSpace(descricaoVertente))
                descricaoVertente = "Descrição da vertente indisponível.";

            string textoArquivo = LerPdfComSeguranca(arquivoBytes);
            string textoCompleto = (textoAluno ?? "") + "\n" + textoArquivo;

            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}";
            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            string resultadoFinal;

            if (textoCompleto.Length > 4000)
            {
                var partes = DividirTexto(textoCompleto);
                var tarefas = partes.Select(parte =>
                {
                    var promptParte = $@"
És um professor do ensino secundário.

Analisa esta parte da vertente:

{descricaoVertente}

Conteúdo do aluno:
{parte}

Dá feedback curto:
- Pontos fortes
- Pontos a melhorar
";
                    return EnviarParaOllama(promptParte);
                }).ToList();

                var resultados = await Task.WhenAll(tarefas);
                var feedbackParcial = string.Join("\n\n", resultados);

                var promptFinal = $@"
Com base nestes feedbacks parciais da vertente:

{feedbackParcial}

Cria um feedback final organizado:

1. Pontos positivos
2. Pontos a melhorar
3. Sugestões concretas
";

                resultadoFinal = await EnviarParaOllama(promptFinal);
            }
            else
            {
                var prompt = $@"
És um professor do ensino secundário em Portugal.

Descrição da vertente:
{descricaoVertente}

Resposta do aluno:
{textoCompleto}

Dá feedback estruturado:
1. Pontos positivos
2. Pontos a melhorar
3. Sugestões concretas
";

                resultadoFinal = await EnviarParaOllama(prompt);
            }

            if (string.IsNullOrWhiteSpace(resultadoFinal))
                resultadoFinal = "Nenhum feedback gerado.";

            if (useCache)
                _cache[cacheKey] = resultadoFinal.Trim();

            return resultadoFinal.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 ERRO IAService: {ex.Message}");
            return "Erro interno ao gerar feedback.";
        }
    }

    private string LerPdfComSeguranca(byte[]? arquivoBytes)
    {
        if (arquivoBytes == null || arquivoBytes.Length == 0)
            return "";

        try
        {
            using var ms = new MemoryStream(arquivoBytes);
            using var pdf = PdfDocument.Open(ms);

            var sb = new StringBuilder();
            foreach (Page page in pdf.GetPages())
            {
                var text = page.Text?.Trim();
                if (!string.IsNullOrEmpty(text))
                    sb.AppendLine(text);
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao ler PDF: {ex.Message}");
            return "";
        }
    }

    private List<string> DividirTexto(string texto, int tamanhoMax = 3000)
    {
        var partes = new List<string>();
        for (int i = 0; i < texto.Length; i += tamanhoMax)
        {
            partes.Add(texto.Substring(i, Math.Min(tamanhoMax, texto.Length - i)));
        }
        return partes;
    }

    private async Task<string> EnviarParaOllama(string prompt)
    {
        try
        {
            var requestBody = new
            {
                model = "llama3",
                prompt = prompt,
                stream = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("api/generate", content);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                Console.WriteLine("ERRO HTTP Ollama: " + erro);
                return "Erro ao comunicar com IA.";
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("response", out var resp))
                return "Resposta inválida da IA.";

            return resp.GetString() ?? "";
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("⏱️ Timeout na IA");
            return "A IA demorou demasiado tempo.";
        }
        catch (Exception ex)
        {
            Console.WriteLine("💥 ERRO ao chamar IA: " + ex.Message);
            return "Erro interno ao chamar IA.";
        }
    }
}