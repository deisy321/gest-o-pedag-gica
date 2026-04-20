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
                descricaoVertente = "Instrução geral do trabalho.";

            // Extração de texto do PDF (se houver)
            string textoArquivo = LerPdfComSeguranca(arquivoBytes);
            string textoCompleto = (textoAluno ?? "") + "\n" + textoArquivo;

            // Sistema de Cache
            var cacheKey = $"{alunoId}_{trabalhoId}_{vertenteId}";
            if (useCache && _cache.TryGetValue(cacheKey, out var cached))
                return cached;

            // --- PROMPT DE SISTEMA: FOCO NO ALUNO E ISOLAMENTO ---
            string promptSistema = @"
És um Professor que está a falar DIRETAMENTE com o seu aluno para o ajudar a melhorar.
Usa SEMPRE a 2ª pessoa do singular (Tu, Teu, Fizeste). O tom deve ser encorajador mas rigoroso.

REGRAS DE OURO:
1. FOCO EXCLUSIVO: Avalia APENAS se o aluno cumpriu o que está na 'INSTRUÇÃO ESPECÍFICA'.
2. ISOLAMENTO TOTAL: Se a instrução pede 'Orçamento', ignora se falta 'Transporte' ou 'Monumentos'. Não critiques a falta de algo que não foi pedido NESTA instrução.
3. TRATAMENTO DIRETO: Não fales na terceira pessoa (ex: Não digas 'O aluno esqueceu'). Diz: 'Tu esqueceste-te' ou 'Fizeste um bom trabalho'.
4. NÃO INVENTES: Se o aluno cumpriu o pedido, não peças detalhes extras que o professor não solicitou.";

            string templateTarefa = $@"
### INSTRUÇÃO ESPECÍFICA DO PROFESSOR (Avalia APENAS isto):
""{descricaoVertente}""

### RESPOSTA/TRABALHO DO ALUNO:
""{textoCompleto}""

### TAREFA:
Analisa a resposta do aluno face à instrução. Se ele cumpriu tudo o que foi pedido NESTA INSTRUÇÃO, elogia-o. 
Se falta algo que o professor pediu especificamente aqui, explica como ele pode corrigir.

FORMATO OBRIGATÓRIO DE RESPOSTA:
**1. Pontos positivos:** (O que fizeste bem nesta vertente)
**2. Pontos a melhorar:** (O que falta ou pode ser melhorado especificamente aqui)
**3. Sugestões concretas:** (Dicas para corrigires o trabalho agora)";

            string resultadoFinal;

            // Lógica para textos muito longos
            if (textoCompleto.Length > 4000)
            {
                var partes = DividirTexto(textoCompleto);
                var tarefas = partes.Select(parte =>
                    EnviarParaOllama($"{promptSistema}\nAnalisa esta parte: {parte}\nInstrução: {descricaoVertente}\nFeedback curto na 2ª pessoa.")
                ).ToList();

                var resultados = await Task.WhenAll(tarefas);
                var resumoParcial = string.Join("\n", resultados);

                resultadoFinal = await EnviarParaOllama($"{promptSistema}\nResumo dos pontos encontrados: {resumoParcial}\n{templateTarefa}");
            }
            else
            {
                resultadoFinal = await EnviarParaOllama($"{promptSistema}\n{templateTarefa}");
            }

            if (string.IsNullOrWhiteSpace(resultadoFinal))
                resultadoFinal = "Não foi possível gerar feedback automático. Verifica se o texto é suficiente.";

            // Salvar no Cache
            if (useCache)
                _cache[cacheKey] = resultadoFinal.Trim();

            return resultadoFinal.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 ERRO IAService: {ex.Message}");
            return "Ocorreu um erro ao processar o feedback com a IA.";
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
        if (string.IsNullOrEmpty(texto)) return partes;

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
            // Alterado para tinydolphin para caber nos 512MB de RAM do Render
            var requestBody = new
            {
                model = "tinydolphin",
                prompt = prompt,
                stream = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("/api/generate", content);

            if (!response.IsSuccessStatusCode)
            {
                return "";
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("response", out var resp))
            {
                return resp.GetString() ?? "";
            }

            return "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro comunicação Ollama: {ex.Message}");
            return "";
        }
    }
}