using ClosedXML.Excel;
using WebScraper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebScraper.Services
{
    public class DataExporter
    {
        private readonly string _diretorioBase;

        public DataExporter(string nomePasta = "Resultados_Extraidos")
        {
            _diretorioBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nomePasta);

            if (!Directory.Exists(_diretorioBase))
            {
                Directory.CreateDirectory(_diretorioBase);
            }
        }

        public void ExportToExcel(List<Research> data, string nomePesquisa)
        {
            // 1. Saneamento do nome do arquivo
            string nomeArquivoSaneado = string.Join("_", nomePesquisa.Split(Path.GetInvalidFileNameChars()));
            // Limitamos o tamanho do nome para evitar erros de caminho longo no Windows
            if (nomeArquivoSaneado.Length > 30) nomeArquivoSaneado = nomeArquivoSaneado.Substring(0, 30);

            string caminhoCompleto = Path.Combine(_diretorioBase, $"{nomeArquivoSaneado}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Dados PubMed");

                // 2. Cabeçalhos Estruturados
                string[] headers = {
                    "Título",
                    "Autor",
                    "Data",
                    "Keywords Encontradas",
                    "Keywords Ausentes",
                    "Link Original",
                    "Abstract Completo"
                };

                for (int col = 0; col < headers.Length; col++)
                {
                    worksheet.Cell(1, col + 1).Value = headers[col];
                }

                // 3. Preenchimento Dinâmico
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    int row = i + 2;

                    worksheet.Cell(row, 1).Value = item.Title;
                    worksheet.Cell(row, 2).Value = item.Author;
                    worksheet.Cell(row, 3).Value = item.PublicationDate;

                    // Colunas de Acurácia (Transformando List em String separada por vírgula)
                    worksheet.Cell(row, 4).Value = string.Join(", ", item.KeywordsFound);
                    worksheet.Cell(row, 5).Value = string.Join(", ", item.KeywordsNotFound);

                    // Link com Hyperlink real do Excel
                    worksheet.Cell(row, 6).Value = "Acessar Artigo";
                    worksheet.Cell(row, 6).GetHyperlink().ExternalAddress = new Uri(item.Link);

                    worksheet.Cell(row, 7).Value = item.Abstract;

                    // Formatação Condicional Visual: Se não encontrou keywords importantes, pinta o texto de vermelho
                    if (item.KeywordsNotFound.Any())
                    {
                        worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.Red;
                    }
                    worksheet.Cell(row, 4).Style.Font.FontColor = XLColor.DarkGreen;
                }

                // 4. Transformação em Tabela e Design "Bonito"
                var totalRows = data.Count + 1;
                var totalCols = headers.Length;
                var range = worksheet.Range(1, 1, totalRows, totalCols);

                var table = range.CreateTable();
                table.Theme = XLTableTheme.TableStyleMedium10; // Verde/Cinza profissional
                table.ShowAutoFilter = true;

                // 5. Ajustes de Layout e Estética
                worksheet.Columns().AdjustToContents(); // Auto-ajuste inicial

                // Configurações específicas de largura
                worksheet.Column(1).Width = 40; // Título
                worksheet.Column(4).Width = 25; // Keywords Found
                worksheet.Column(5).Width = 25; // Keywords Not Found
                worksheet.Column(7).Width = 60; // Abstract

                // Quebra de texto para o Abstract e Título
                worksheet.Column(1).Style.Alignment.WrapText = true;
                worksheet.Column(7).Style.Alignment.WrapText = true;

                // Alinhamento vertical no topo (padrão científico)
                worksheet.Rows().Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);

                // Congelar a primeira linha (cabeçalho) para facilitar o scroll
                worksheet.SheetView.FreezeRows(1);

                workbook.SaveAs(caminhoCompleto);
                Console.WriteLine($"[EXCEL PROFISSIONAL] Gerado: {Path.GetFileName(caminhoCompleto)}");
            }
        }
    }
}