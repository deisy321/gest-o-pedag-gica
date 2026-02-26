using gestaopedagogica.Models;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gestaopedagogica.Services
{
    public class PDFExportService
    {
        public async Task<byte[]> ExportarTrabalhoAvancadoAsync(Trabalho trabalho)
        {
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            var corPrimaria = new DeviceRgb(30, 58, 138);
            var corSucesso = new DeviceRgb(5, 150, 105);

            // Título (negrito removido para compatibilidade)
            document.Add(new Paragraph($"📋 {trabalho.Titulo}")
                .SetFontSize(24)
                .SetFontColor(corPrimaria)
                .SetTextAlignment(TextAlignment.CENTER));

            // Info aluno
            document.Add(new Paragraph($"Aluno: {trabalho.Aluno?.UserName ?? "N/A"}")
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n"));

            // Tabela info geral
            var table = new Table(2).SetWidth(UnitValue.CreatePercentValue(100));
            table.AddCell(CriarCelula("Disciplina:", trabalho.Disciplina?.Nome ?? "N/A", corPrimaria));
            table.AddCell(CriarCelula("Módulo:", trabalho.Modulo?.Nome ?? "N/A", corPrimaria));
            table.AddCell(CriarCelula("Data de Criação:", trabalho.DataCriacao.ToString("dd/MM/yyyy"), corPrimaria));
            table.AddCell(CriarCelula("Prazo:", trabalho.PrazoEntrega.ToString("dd/MM/yyyy HH:mm"), corPrimaria));
            document.Add(table);

            document.Add(new Paragraph("\n"));

            // Vertentes e notas
            document.Add(new Paragraph("Avaliação por Vertentes")
                .SetFontSize(14)
                .SetFontColor(corPrimaria));

            foreach (var vertente in trabalho.TrabalhoVertentes)
            {
                var vertTable = new Table(2).SetWidth(UnitValue.CreatePercentValue(100));

                vertTable.AddCell(CriarCelula($"✓ {vertente.Tipo}", "", corSucesso));
                vertTable.AddCell(CriarCelula($"Nota: {(vertente.Nota?.ToString("F1") ?? "Não avaliado")}/20", "", corSucesso));

                if (!string.IsNullOrEmpty(vertente.ConteudoTextoAluno))
                    vertTable.AddCell(CriarCelula("Resposta do Aluno:", vertente.ConteudoTextoAluno, null));

                if (!string.IsNullOrEmpty(vertente.Feedback))
                    vertTable.AddCell(CriarCelula("Feedback:", vertente.Feedback, null));

                document.Add(vertTable);
                document.Add(new Paragraph("\n"));
            }

            // Footer
            document.Add(new Paragraph("\n\n"));
            document.Add(new Paragraph("TriadeLearn - Sistema de Gestão Pedagógica")
                .SetFontSize(10)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER));

            return ms.ToArray();
        }

        private Cell CriarCelula(string chave, string valor, Color corFundo)
        {
            var cell = new Cell();
            cell.Add(new Paragraph(chave));
            if (!string.IsNullOrEmpty(valor))
                cell.Add(new Paragraph(valor));
            if (corFundo != null)
                cell.SetBackgroundColor(corFundo);
            return cell;
        }

        public async Task<byte[]> ExportarCertificadoAsync(Trabalho trabalho, decimal mediaGeral)
        {
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            var corGold = new DeviceRgb(218, 165, 32);

            document.Add(new Paragraph("CERTIFICADO DE CONCLUSÃO")
                .SetFontSize(20)
                .SetFontColor(corGold)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n\n"));

            document.Add(new Paragraph($"Certificamos que {trabalho.Aluno?.UserName ?? "o aluno"} completou com sucesso o trabalho:")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"\"{trabalho.Titulo}\"")
                .SetFontSize(14)
                .SetFontColor(corGold)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Com a nota final de {mediaGeral:F1}/20")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n"));

            var infoTable = new Table(2).SetWidth(UnitValue.CreatePercentValue(100));
            infoTable.AddCell(CriarCelula("Disciplina:", trabalho.Disciplina?.Nome ?? "N/A", null));
            infoTable.AddCell(CriarCelula("Módulo:", trabalho.Modulo?.Nome ?? "N/A", null));
            infoTable.AddCell(CriarCelula("Data de Conclusão:", DateTime.Now.ToString("dd/MM/yyyy"), null));
            document.Add(infoTable);

            document.Add(new Paragraph("\n\n"));
            document.Add(new Paragraph("_________________________________").SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph("Assinatura do Professor").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));

            return ms.ToArray();
        }
    }
}