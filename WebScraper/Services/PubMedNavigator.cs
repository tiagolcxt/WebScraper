using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using WebScraper.Interfaces;
using WebScraper.Models;

namespace WebScraper.Services
{
    public class PubMedNavigator : ISearchNavigator
    {
        private readonly IIdentityService _identity;
        private static readonly HttpClient _httpClient = new HttpClient(new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        });

        // Injeção da identidade para manter o padrão de camuflagem
        public PubMedNavigator(IIdentityService identity) => _identity = identity;

        public async Task<List<string>> GetLinksAsync(List<string> mushrooms, DateTime? startDate, DateTime? endDate, SourceType sourceType)
        {
            var links = new HashSet<string>(); // HashSet evita links duplicados na mesma busca
            int currentPage = 1;
            bool hasMorePages = true;

            // Filtra entradas vazias e coloca termos entre aspas para busca exata
            var cleanTerms = mushrooms.Where(m => !string.IsNullOrWhiteSpace(m))
                                      .Select(m => $"\"{m.Trim()}\"").ToList();

            if (!cleanTerms.Any()) return new List<string>();

            string searchTerm = string.Join(" OR ", cleanTerms);
            string dateQuery = "";

            // Monta o filtro de data no formato aceito pelo motor do PubMed
            if (startDate.HasValue)
            {
                string start = startDate.Value.ToString("yyyy/MM/dd");
                string end = endDate.HasValue ? endDate.Value.ToString("yyyy/MM/dd") : DateTime.Now.ToString("yyyy/MM/dd");
                dateQuery = $" AND (\"{start}\"[Date - Publication] : \"{end}\"[Date - Publication])";
            }

            string fullSearchQuery = Uri.EscapeDataString(searchTerm + dateQuery);

            while (hasMorePages)
            {
                // Parâmetro size=200 reduz o número de requisições de paginação
                string url = $"https://pubmed.ncbi.nlm.nih.gov/?term={fullSearchQuery}&size=200&page={currentPage}";

                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    _identity.ApplyIdentity(request); // Aplica User-Agent e Referer rotativos

                    var response = await _httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode) break;

                    var html = await response.Content.ReadAsStringAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    // Seleciona os títulos que contém os links para os artigos
                    var nodes = doc.DocumentNode.SelectNodes("//a[@class='docsum-title']");

                    if (nodes == null || !nodes.Any())
                    {
                        hasMorePages = false;
                        continue;
                    }

                    foreach (var node in nodes)
                    {
                        var href = node.GetAttributeValue("href", "");
                        if (!string.IsNullOrEmpty(href))
                        {
                            // Normaliza URL removendo parâmetros de rastreamento da busca
                            string fullUrl = href.StartsWith("http") ? href : "https://pubmed.ncbi.nlm.nih.gov" + href;
                            links.Add(fullUrl.Split('?')[0]);
                        }
                    }

                    // Se a página veio incompleta, é o fim dos resultados
                    if (nodes.Count < 200) hasMorePages = false;
                    else currentPage++;

                    // Intervalo de segurança para evitar detecção de crawl agressivo
                    await Task.Delay(800);
                }
                catch
                {
                    break; // Interrompe em caso de erro de conexão ou timeout
                }
            }

            return links.ToList();
        }
    }
}