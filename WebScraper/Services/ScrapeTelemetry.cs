using System.Collections.Concurrent;
using System.Diagnostics;
using WebScraper.Interfaces;

namespace WebScraper.Services
{
    public class ScrapeTelemetry : IScrapeTelemetry
    {
        private readonly Stopwatch _timer = new();
        private int _totalItems = 0;
        private int _successes = 0;
        private int _failures = 0;

        // Log thread-safe para armazenar detalhes das falhas sem causar race conditions
        private readonly ConcurrentBag<(string Reason, string Url)> _failureLog = new();

        public ScrapeTelemetry() => _timer.Start();

        // Cálculo de acurácia em tempo real para o Dashboard
        public double CurrentAccuracy => (_successes + _failures == 0) ? 100 : (_successes / (double)(_successes + _failures)) * 100;

        // Incremento atômico para garantir contagem exata entre múltiplas threads
        public void RecordSuccess() => Interlocked.Increment(ref _successes);

        public void RecordFailure(string reason, string url)
        {
            Interlocked.Increment(ref _failures);
            _failureLog.Add((reason, url));
        }

        // Acumula o total de links descobertos para cálculo de ETA
        public void AddToTotal(int count) => Interlocked.Add(ref _totalItems, count);

        public void PrintDashboard()
        {
            int processed = _successes + _failures;
            if (processed == 0) return;

            // Velocidade real
            double msPerItem = _timer.ElapsedMilliseconds / (double)processed;

            // Estimativa pessimista: calculamos o tempo real e adicionamos 15% de "gordura"
            double pessimisticMs = (_totalItems - processed) * msPerItem * 1.15;
            TimeSpan eta = TimeSpan.FromMilliseconds(pessimisticMs);

            Console.Write($"\r[PROCESSO] {processed}/{_totalItems} | Acurácia: {CurrentAccuracy:F1}% | ETA Pessimista: {eta.Minutes}m {eta.Seconds}s    ");
        }

        public void PrintFailureReport()
        {
            if (!_failureLog.Any()) return;

            Console.WriteLine("\n\n" + new string('!', 50));
            Console.WriteLine("          RELATÓRIO DE AUDITORIA DE FALHAS");
            Console.WriteLine(new string('!', 50));

            // Agrupa erros por motivo para facilitar a identificação de padrões (ex: bloqueios de IP)
            var errosAgrupados = _failureLog.GroupBy(x => x.Reason);

            foreach (var grupo in errosAgrupados)
            {
                Console.WriteLine($"\nMotivo: {grupo.Key} ({grupo.Count()} ocorrências)");

                // Exibe apenas uma amostra para não sobrecarregar o console
                foreach (var erro in grupo.Take(3))
                {
                    Console.WriteLine($"  - Link: {erro.Url}");
                }
            }
            Console.WriteLine(new string('=', 50) + "\n");
        }
    }
}