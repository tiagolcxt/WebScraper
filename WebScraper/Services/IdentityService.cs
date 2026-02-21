using WebScraper.Interfaces;
using System.Net.Http.Headers;

namespace WebScraper.Services
{
    public class IdentityService : IIdentityService
    {
        private static readonly Random _random = new Random();

        // Banco de dados de User-Agents modernos
        private static readonly string[] _userAgents = {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:123.0) Gecko/20100101 Firefox/123.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/122.0.0.0"
        };

        // Origens de tráfego simuladas (Referers)
        private static readonly string[] _referers = {
            "https://www.google.com/",
            "https://www.bing.com/",
            "https://pubmed.ncbi.nlm.nih.gov/",
            "https://scholar.google.com/"
        };

        public void ApplyIdentity(HttpRequestMessage request)
        {
            // Sorteia e injeta User-Agent e Referer na requisição atual
            request.Headers.TryAddWithoutValidation("User-Agent", _userAgents[_random.Next(_userAgents.Length)]);
            request.Headers.TryAddWithoutValidation("Referer", _referers[_random.Next(_referers.Length)]);

            // Configura tipos de conteúdo e idioma aceitos pelo navegador
            request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");

            // Sinaliza preferência por conexões seguras
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
        }
    }
}