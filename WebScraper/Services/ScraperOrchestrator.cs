using System.Collections.Concurrent;
using System.Diagnostics;
using WebScraper.Interfaces;
using WebScraper.Models;

namespace WebScraper.Services
{
    public class ScraperOrchestrator
    {
        private readonly ISearchNavigator _navigator;
        private readonly IResearchParser _parser;
        private readonly IResearchValidator _validator;

        public ScraperOrchestrator(ISearchNavigator navigator, IResearchParser parser, IResearchValidator validator)
        {
            _navigator = navigator;
            _parser = parser;
            _validator = validator;
        }

        public async Task<(List<Research> Data, TimeSpan Elapsed)> RunScrapeAsync(List<string> mushrooms, DateTime? start, DateTime? end, SourceType type)
        {
            var sw = Stopwatch.StartNew();
            var resultados = new ConcurrentBag<Research>(); // Coleção segura para escrita multi-thread
            var options = new ParallelOptions { MaxDegreeOfParallelism = 6 }; // Controle de carga para evitar bloqueio de IP
            var telemetry = new ScrapeTelemetry();

            foreach (var cogumelo in mushrooms)
            {
                // Fase 1: Descoberta de links (Síncrona por termo para evitar sobrecarga no motor de busca)
                var links = await _navigator.GetLinksAsync(new List<string> { cogumelo }, start, end, type);
                if (!links.Any()) continue;

                telemetry.AddToTotal(links.Count);

                // Fase 2: Processamento paralelo dos links encontrados
                await Parallel.ForEachAsync(links, options, async (link, token) =>
                {
                    // Cada thread executa seu próprio ciclo de Parse e Validação
                    var item = await _parser.ParseAsync(link, cogumelo);
                    var v = _validator.Validate(item);

                    if (v.IsValid)
                    {
                        resultados.Add(item);
                        telemetry.RecordSuccess();
                    }
                    else
                    {
                        // Registra o motivo específico da falha para auditoria posterior
                        telemetry.RecordFailure(v.Message, link);
                    }

                    // Atualiza o progresso visual no console em tempo real
                    telemetry.PrintDashboard();
                });
            }

            sw.Stop();

            // Exibe o balanço final de erros e sucessos após a conclusão de todos os termos
            telemetry.PrintFailureReport();

            return (resultados.ToList(), sw.Elapsed);
        }
    }
}