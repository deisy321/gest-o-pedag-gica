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

            string textoArquivo = LerPdfComSeguranca(arquivoBytes);
            string textoCompleto = (textoAluno ?? "") + "\n" + textoArquivo;

            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}";
            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            // PROMPT GENÉRICO E BLINDADO
            string promptSistema = @"És um Professor avaliador. 
Regras de escrita:
1. Fala diretamente com o aluno usando 'Tu' (ex: Tu fizeste, Teu trabalho).
2. Compara o 'TRABALHO' com a 'INSTRUÇÃO'.
3. Se houver erros factuais ou falta de conteúdo no TRABALHO, corrige-os.
4. Sê curto, direto e não inventes informações fora do contexto fornecido.";

            string promptUsuario = $@"
INSTRUÇÃO DO PROFESSOR: ""{descricaoVertente}""
TRABALHO DO ALUNO: ""{textoCompleto}""

Responde apenas neste formato:
**1. Pontos positivos:**
**2. Pontos a melhorar:**
**3. Dica prática:**";

            // Chamada à IA
            string resultadoFinal = await EnviarParaOllama($"{promptSistema}\n\n{promptUsuario}");

            // Filtro de segurança para falhas do modelo
            if (string.IsNullOrWhiteSpace(resultadoFinal) || resultadoFinal.Length < 10)
                resultadoFinal = "Não consegui gerar um feedback preciso. Tenta reformular o teu texto.";

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
                    // ALTERAÇÃO CRÍTICA: Temperatura mínima para evitar alucinações
                    temperature = 0.1,
                    top_p = 0.1,
                    repeat_penalty = 1.2,
                    num_predict = 250 // Resposta curta para manter a coesão
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