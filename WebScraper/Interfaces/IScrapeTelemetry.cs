namespace WebScraper.Interfaces
{
    public interface IScrapeTelemetry
    {
        double CurrentAccuracy { get; }
        void RecordSuccess();
        // Agora aceita a URL para o log de auditoria
        void RecordFailure(string reason, string url);
        void PrintDashboard();
        void PrintFailureReport(); // O novo método de relatório
    }
}