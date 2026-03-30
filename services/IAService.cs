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

public class IAService
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public IAService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> ObterSugestoes(string textoAluno, string descricaoTrabalho)
    {
        return await ObterSugestoes("defaultAlunoId", "defaultTrabalhoId", textoAluno, descricaoTrabalho, null, false);
    }

    public async Task<string> ObterSugestoes(
        string alunoId,
        string trabalhoId,
        string textoAluno,
        string descricaoTrabalho,
        byte[]? arquivoBytes,
        bool useCache = true)
    {
        if (string.IsNullOrWhiteSpace(textoAluno) && arquivoBytes == null)
            return "Conteúdo do aluno vazio, impossível gerar feedback.";

        if (string.IsNullOrWhiteSpace(descricaoTrabalho))
            descricaoTrabalho = "Descrição do trabalho indisponível.";

        // ----------- PDF -----------
        string textoArquivo = "";
        if (arquivoBytes != null)
        {
            try
            {
                using var ms = new MemoryStream(arquivoBytes);
                using var pdf = PdfDocument.Open(ms);

                var sb = new StringBuilder();
                foreach (Page page in pdf.GetPages())
                    sb.AppendLine(page.Text);

                textoArquivo = sb.ToString();
            }
            catch
            {
                textoArquivo = "";
            }
        }

        // ----------- TEXTO FINAL -----------
        string textoCompleto = (textoAluno ?? "") + "\n" + textoArquivo;

        var cacheKey = $"{alunoId}_{trabalhoId}";
        if (useCache && _cache.TryGetValue(cacheKey, out var cached))
            return cached;

        string resultadoFinal;

        // 🔥 DIVISÃO DE TEXTO GRANDE
        if (textoCompleto.Length > 4000)
        {
            var partes = DividirTexto(textoCompleto);
            var resultados = new List<string>();

            foreach (var parte in partes)
            {
                var promptParte = $@"
És um professor do ensino secundário.

Analisa esta parte de um trabalho:

{parte}

Dá feedback curto:
- Pontos fortes
- Problemas
";

                var resposta = await EnviarParaOllama(promptParte);
                resultados.Add(resposta);
            }

            var feedbackParcial = string.Join("\n\n", resultados);

            var promptFinal = $@"
Com base nestes feedbacks:

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

Descrição do trabalho:
{descricaoTrabalho}

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

    // =============================
    // DIVIDIR TEXTO
    // =============================
    private List<string> DividirTexto(string texto, int tamanhoMax = 3000)
    {
        var partes = new List<string>();

        for (int i = 0; i < texto.Length; i += tamanhoMax)
        {
            partes.Add(texto.Substring(i, Math.Min(tamanhoMax, texto.Length - i)));
        }

        return partes;
    }

    // =============================
    // CHAMADA OLLAMA (CORRIGIDA)
    // =============================
    private async Task<string> EnviarParaOllama(string prompt)
    {
        try
        {
            Console.WriteLine("➡️ A chamar Ollama...");

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

            Console.WriteLine("⬅️ Resposta recebida");

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                Console.WriteLine("ERRO HTTP: " + erro);
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
            Console.WriteLine("⏱️ Timeout");
            return "A IA demorou demasiado tempo.";
        }
        catch (Exception ex)
        {
            Console.WriteLine("💥 ERRO: " + ex.ToString());
            return "Erro interno ao chamar IA.";
        }
    }
}