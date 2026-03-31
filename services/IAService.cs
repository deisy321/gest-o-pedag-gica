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
                descricaoVertente = "O professor não forneceu instruções específicas para esta vertente.";

            string textoArquivo = LerPdfComSeguranca(arquivoBytes);
            string textoCompleto = (textoAluno ?? "") + "\n" + textoArquivo;

            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}";
            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            string resultadoFinal;

            // Prompt base que força a IA a seguir as instruções do professor
            string promptSistema = @"
És um Professor Auxiliar rigoroso. A tua missão é ajudar o aluno a melhorar o trabalho ANTES da entrega final.
REGRAS DE OURO:
1. Compara a 'RESPOSTA DO ALUNO' estritamente com a 'INSTRUÇÃO DO PROFESSOR'.
2. NÃO inventes requisitos extras (ex: se o prof não pediu hotel ou orçamento, não critiques a falta deles).
3. Se o aluno cumpriu o que foi pedido, valida e incentiva.
4. Se o aluno fugiu ao tema ou esqueceu algo da instrução, aponta de forma construtiva.
5. Responde em Português de Portugal.";

            if (textoCompleto.Length > 4000)
            {
                var partes = DividirTexto(textoCompleto);
                var tarefas = partes.Select(parte =>
                {
                    var promptParte = $@"{promptSistema}

### INSTRUÇÃO DO PROFESSOR PARA ESTA VERTENTE:
{descricaoVertente}

### RESPOSTA PARCIAL DO ALUNO:
{parte}

Dá feedback curto (máximo 3 frases) focado apenas no que foi pedido.";
                    return EnviarParaOllama(promptParte);
                }).ToList();

                var resultados = await Task.WhenAll(tarefas);
                var feedbackParcial = string.Join("\n\n", resultados);

                var promptFinal = $@"{promptSistema}
Com base nestes feedbacks parciais, cria um resumo final organizado:

### FEEDBACKS PARCIAIS:
{feedbackParcial}

### FORMATO DE RESPOSTA:
**1. Pontos positivos:**
**2. Pontos a melhorar:** (Apenas se não cumprir a instrução do professor)
**3. Sugestões concretas:**";

                resultadoFinal = await EnviarParaOllama(promptFinal);
            }
            else
            {
                var prompt = $@"{promptSistema}

### INSTRUÇÃO ESPECÍFICA DO PROFESSOR:
""{descricaoVertente}""

### RESPOSTA DO ALUNO:
""{textoCompleto}""

### TAREFA:
Analisa se a resposta do aluno satisfaz a instrução acima. 
Responde obrigatoriamente neste formato:
**1. Pontos positivos:** **2. Pontos a melhorar:** **3. Sugestões concretas:**";

                resultadoFinal = await EnviarParaOllama(prompt);
            }

            if (string.IsNullOrWhiteSpace(resultadoFinal))
                resultadoFinal = "A IA não conseguiu gerar um feedback válido para esta vertente.";

            if (useCache)
                _cache[cacheKey] = resultadoFinal.Trim();

            return resultadoFinal.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 ERRO IAService: {ex.Message}");
            return "Erro técnico ao processar feedback com a IA.";
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
            foreach (Page page in pdf.GetPages())
            {
                var text = page.Text?.Trim();
                if (!string.IsNullOrEmpty(text)) sb.AppendLine(text);
            }
            return sb.ToString();
        }
        catch { return ""; }
    }

    private List<string> DividirTexto(string texto, int tamanhoMax = 3000)
    {
        var partes = new List<string>();
        if (string.IsNullOrEmpty(texto)) return partes;
        for (int i = 0; i < texto.Length; i += tamanhoMax)
            partes.Add(texto.Substring(i, Math.Min(tamanhoMax, texto.Length - i)));
        return partes;
    }

    private async Task<string> EnviarParaOllama(string prompt)
    {
        try
        {
            var requestBody = new { model = "llama3", prompt = prompt, stream = false };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/generate", content);

            if (!response.IsSuccessStatusCode) return "";

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("response", out var resp) ? resp.GetString() ?? "" : "";
        }
        catch { return "Erro na comunicação com o servidor de IA."; }
    }
}