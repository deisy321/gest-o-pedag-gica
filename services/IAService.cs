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
            if (string.IsNullOrWhiteSpace(textoAluno) && (arquivoBytes == null || arquivoBytes.Length == 0))
                return "Tu não enviaste conteúdo. Por favor, escreve algo ou anexa um PDF.";

            // Extração de texto
            string textoArquivo = LerPdfComSeguranca(arquivoBytes);
            string textoCompleto = (textoAluno ?? "") + "\n" + textoArquivo;

            // Cache
            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}";
            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            // PROMPT REFORÇADO PARA EVITAR CÓDIGO INVENTADO
            string promptSistema = @"Age como um Professor avaliador. 
O teu único trabalho é escrever um texto de feedback para o teu aluno.
Regras:
1. Fala na 2ª pessoa (Tu).
2. Não escrevas código C#, APIs ou URLs.
3. Compara o 'Trabalho do Aluno' com a 'Instrução do Professor'.
4. Se o aluno não seguiu a instrução, diz o que falta.";

            string promptUsuario = $@"
INSTRUÇÃO DO PROFESSOR: ""{descricaoVertente}""
TRABALHO DO ALUNO: ""{textoCompleto}""

Responde apenas com este formato:
**1. Pontos positivos:** (o que o aluno fez bem)
**2. Pontos a melhorar:** (o que falta segundo a instrução)
**3. Dica prática:** (como corrigir agora)";

            // Chamada à IA
            string resultadoFinal = await EnviarParaOllama($"{promptSistema}\n\n{promptUsuario}");

            if (string.IsNullOrWhiteSpace(resultadoFinal) || resultadoFinal.Contains("using "))
                resultadoFinal = "Ocorreu um erro ao gerar o feedback. Por favor, tenta simplificar o teu texto.";

            if (useCache) _cache[cacheKey] = resultadoFinal.Trim();

            return resultadoFinal.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro: {ex.Message}");
            return "Erro técnico ao processar sugestões.";
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

    private async Task<string> EnviarParaOllama(string prompt)
    {
        try
        {
            var requestBody = new
            {
                model = "qwen2:0.5b",
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = 0.3, // Menor temperatura = menos invenções/alucinações
                    top_p = 0.9,
                    num_predict = 400  // Limita o tamanho da resposta
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/generate", content);

            if (!response.IsSuccessStatusCode) return "";

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("response").GetString() ?? "";
        }
        catch { return ""; }
    }
}