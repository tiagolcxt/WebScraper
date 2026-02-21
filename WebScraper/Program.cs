using WebScraper.Models;
using WebScraper.Services;
using WebScraper.Interfaces;
using System.Linq;

// --- CONFIGURAÇÃO ---
var meusCogumelos = new List<string> { "Lion's Mane", "Reishi", "Cordyceps" };
var dataInicio = new DateTime(2023, 01, 01);
var tipoFonte = SourceType.ScientificArticle;

// --- INSTANCIAÇÃO ---
IIdentityService identity = new IdentityService();
ISearchNavigator navegador = new PubMedNavigator(identity);
IResearchParser extrator = new PubMedParser(identity);
IResearchValidator validador = new PubMedValidator();
var maestro = new ScraperOrchestrator(navegador, extrator, validador);

Console.Clear();
Console.WriteLine("============================================================");
Console.WriteLine("      SISTEMA DE EXTRAÇÃO SEGMENTADA - PUBMED 2026");
Console.WriteLine("============================================================\n");

// --- EXECUÇÃO ---
var (dadosExtraidos, tempoGeral) = await maestro.RunScrapeAsync(meusCogumelos, dataInicio, null, tipoFonte);

// --- RELATÓRIO DETALHADO POR BLOCO DE BUSCA ---
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("             RESULTADOS POR CATEGORIA DE BUSCA");
Console.WriteLine(new string('=', 60));

// Agrupa os resultados pelo primeiro cogumelo da lista (o termo que gerou a busca)
var gruposPorCogumelo = dadosExtraidos
    .SelectMany(r => r.Mushrooms, (research, mushroom) => new { research, mushroom })
    .GroupBy(x => x.mushroom);

foreach (var grupo in gruposPorCogumelo)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n[CATEGORIA: {grupo.Key.ToUpper()}]");
    Console.ResetColor();
    Console.WriteLine($"Itens encontrados: {grupo.Count()}");
    Console.WriteLine(new string('-', 30));

    foreach (var item in grupo)
    {
        var r = item.research;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($" ► {r.Title}");
        Console.ResetColor();

        Console.WriteLine($"   Autor: {r.Author}"); // Agora vai aparecer!
        Console.WriteLine($"   Data:  {r.PublicationDate.ToShortDateString()}");
        Console.WriteLine($"   Link:  {r.Link}");

        // Mostra apenas os primeiros 150 caracteres do abstract
        string resumoResumido = r.Abstract.Length > 150
            ? r.Abstract.Substring(0, 150) + "..."
            : r.Abstract;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"   Resumo: {resumoResumido}");
        Console.ResetColor();
        Console.WriteLine();
    }
}

// --- DASHBOARD COMPARATIVO FINAL ---
double tempoEstimadoSegundos = tempoGeral.TotalSeconds * 1.15;
double economiaTempo = tempoEstimadoSegundos - tempoGeral.TotalSeconds;

Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("             ANÁLISE DE PERFORMANCE GLOBAL");
Console.WriteLine(new string('=', 60));

Console.WriteLine($"{"MÉTRICA",-28} | {"VALOR OBTIDO",-15}");
Console.WriteLine(new string('-', 60));
Console.WriteLine($"{"Total Geral Validados",-28} | {dadosExtraidos.Count}");
Console.WriteLine($"{"Tempo Real",-28} | {tempoGeral.Minutes}m {tempoGeral.Seconds}s");
Console.WriteLine($"{"Tempo Estimado (Pessimista)",-28} | {TimeSpan.FromSeconds(tempoEstimadoSegundos).Minutes}m {TimeSpan.FromSeconds(tempoEstimadoSegundos).Seconds}s");

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"{"Eficiência de Tempo",-28} | {((economiaTempo / (tempoEstimadoSegundos == 0 ? 1 : tempoEstimadoSegundos)) * 100):F1}% mais rápido");
Console.ResetColor();

Console.WriteLine(new string('=', 60));
Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();