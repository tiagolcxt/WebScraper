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

        public async Task<(List<Research> Data, TimeSpan Elapsed)> RunScrapeAsync(List<string> formulas, DateTime? start, DateTime? end, SourceType type)
        {
            var sw = Stopwatch.StartNew();
            var resultados = new ConcurrentBag<Research>();
            var options = new ParallelOptions { MaxDegreeOfParallelism = 6 };
            var telemetry = new ScrapeTelemetry();

            foreach (var formula in formulas)
            {
                // Fase 1: Descoberta de links baseada na fórmula booleana
                var links = await _navigator.GetLinksAsync(new List<string> { formula }, start, end, type);
                if (!links.Any()) continue;

                telemetry.AddToTotal(links.Count);

                // Fase 2: Processamento paralelo
                await Parallel.ForEachAsync(links, options, async (link, token) =>
                {
                    // Passamos a formula para o parser para que o objeto Research saiba sua origem
                    var item = await _parser.ParseAsync(link, formula);

                    // O Validator agora checa a obrigatoriedade baseada na fórmula
                    var v = _validator.Validate(item);

                    if (v.IsValid)
                    {
                        resultados.Add(item);
                        telemetry.RecordSuccess();
                    }
                    else
                    {
                        telemetry.RecordFailure(v.Message, link);
                    }

                    telemetry.PrintDashboard();
                });
            }

            sw.Stop();
            telemetry.PrintFailureReport();

            return (resultados.ToList(), sw.Elapsed);
        }
    }
}