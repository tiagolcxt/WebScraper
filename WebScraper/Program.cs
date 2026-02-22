using WebScraper.Models;
using WebScraper.Services;
using WebScraper.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

/* * ============================================================================
 * TUTORIAL DE CONFIGURAÇÃO DE PESQUISA (LOGICA BOOLEANA 2026)
 * ============================================================================
 * * 1. OPERADORES DISPONÍVEIS:
 * && (AND) -> OBRIGA que ambos os termos estejam presentes.
 * || (OR)  -> Aceita QUALQUER um dos termos.
 * !  (NOT) -> EXCLUI resultados que contenham este termo.
 * * 2. ASPAS DUPLAS (""):
 * Sempre use aspas para termos compostos (ex: "Hericium erinaceus").
 * * 3. REGRAS DE OURO:
 * - O sistema valida no resumo se as Keywords da fórmula realmente existem.
 * - Artigos que não citam o cogumelo e o tema são descartados pelo Validator.
 * ============================================================================
 */

// --- CONFIGURAÇÃO ---
var minhasPesquisas = new List<string>
{
    "\"Hericium erinaceus\" && \"alcoholic extraction\"",
    "\"Ganoderma lucidum\" && (\"anti-tumor\" || \"cancer\")",
    "\"Lentinula edodes\" && !\"water extraction\""
};

var dataInicio = new DateTime(2023, 01, 01);
var tipoFonte = SourceType.ScientificArticle;

// --- INSTANCIAÇÃO DOS SERVIÇOS ---
IIdentityService identity = new IdentityService();
QueryService queryService = new QueryService();
IResearchValidator validador = new PubMedValidator(); // Já integra a lógica de keywords
ISearchNavigator navegador = new PubMedNavigator(identity);
IResearchParser extrator = new PubMedParser(identity);

var maestro = new ScraperOrchestrator(navegador, extrator, validador);
var exportador = new DataExporter("Minhas_Pesquisas_2026");

Console.Clear();
Console.WriteLine("============================================================");
Console.WriteLine("      SISTEMA DE EXTRAÇÃO SEGMENTADA - PUBMED 2026");
Console.WriteLine("      CONTROLE DE ACURÁCIA E EXPORTAÇÃO EXCEL");
Console.WriteLine("============================================================\n");

// --- EXECUÇÃO DO SCRAPE ---
var (dadosExtraidos, tempoGeral) = await maestro.RunScrapeAsync(minhasPesquisas, dataInicio, null, tipoFonte);

if (!dadosExtraidos.Any())
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\n[AVISO] Nenhum artigo válido foi encontrado para os critérios informados.");
    Console.ResetColor();
    return;
}

// --- RELATÓRIO VISUAL E ORGANIZAÇÃO ---
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("             PROCESSANDO RESULTADOS POR CATEGORIA");
Console.WriteLine(new string('=', 60));

var grupos = dadosExtraidos
    .SelectMany(r => r.Mushrooms, (research, formula) => new { research, formula })
    .GroupBy(x => x.formula);

foreach (var grupo in grupos)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n[FÓRMULA: {grupo.Key.ToUpper()}]");
    Console.ResetColor();
    Console.WriteLine($"Encontrados: {grupo.Count()} artigos válidos.");

    // Exportação Individual por Fórmula
    var listaItensGrupo = grupo.Select(g => g.research).ToList();
    exportador.ExportToExcel(listaItensGrupo, $"Busca_{grupo.Key}");
}

// --- EXPORTAÇÃO FINAL CONSOLIDADA ---
// Gera um arquivo único com todos os resultados de todas as pesquisas
exportador.ExportToExcel(dadosExtraidos, "Busca_Integrada_Cogumelos_Full");

// --- DASHBOARD FINAL ---
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("               RESUMO DA OPERAÇÃO");
Console.WriteLine(new string('=', 60));
Console.WriteLine($"{"Total de Artigos Válidos",-28} | {dadosExtraidos.Count}");
Console.WriteLine($"{"Tempo Total Decorrido",-28} | {tempoGeral.Minutes}m {tempoGeral.Seconds}s");
Console.WriteLine($"{"Pasta de Destino",-28} | /Minhas_Pesquisas_2026/");
Console.WriteLine(new string('=', 60));

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\n[SUCESSO] Todos os arquivos Excel foram gerados e validados.");
Console.ResetColor();
Console.WriteLine("Pressione qualquer tecla para fechar o sistema...");
Console.ReadKey();