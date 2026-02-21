namespace WebScraper.Interfaces
{
    /// <summary>
    /// Define o contrato para serviços de camuflagem de identidade do scraper.
    /// </summary>
    public interface IIdentityService
    {
        void ApplyIdentity(HttpRequestMessage request);
    }
}