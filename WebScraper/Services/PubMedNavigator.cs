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
        private readonly QueryService _queryService = new QueryService(); // Instanciado para tradução
        private static readonly HttpClient _httpClient = new HttpClient(new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        });

        public PubMedNavigator(IIdentityService identity) => _identity = identity;

        public async Task<List<string>> GetLinksAsync(List<string> formulas, DateTime? startDate, DateTime? endDate, SourceType sourceType)
        {
            var allLinks = new HashSet<string>();

            foreach (var formula in formulas)
            {
                // Traduz && para AND, || para OR, etc.
                string translated = _queryService.TranslateToPubMed(formula);
                var linksDaFormula = await GetLinksByFormulaAsync(translated, startDate, endDate);

                foreach (var link in linksDaFormula) allLinks.Add(link);
            }

            return allLinks.ToList();
        }

        private async Task<List<string>> GetLinksByFormulaAsync(string translatedFormula, DateTime? startDate, DateTime? endDate)
        {
            var links = new HashSet<string>();
            int currentPage = 1;
            bool hasMorePages = true;

            string dateQuery = "";
            if (startDate.HasValue)
            {
                string start = startDate.Value.ToString("yyyy/MM/dd");
                string end = endDate.HasValue ? endDate.Value.ToString("yyyy/MM/dd") : DateTime.Now.ToString("yyyy/MM/dd");
                dateQuery = $" AND (\"{start}\"[Date - Publication] : \"{end}\"[Date - Publication])";
            }

            string fullSearchQuery = Uri.EscapeDataString(translatedFormula + dateQuery);

            while (hasMorePages)
            {
                string url = $"https://pubmed.ncbi.nlm.nih.gov/?term={fullSearchQuery}&size=200&page={currentPage}";

                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    _identity.ApplyIdentity(request);

                    var response = await _httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode) break;

                    var html = await response.Content.ReadAsStringAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var nodes = doc.DocumentNode.SelectNodes("//a[@class='docsum-title']");
                    if (nodes == null || !nodes.Any()) break;

                    foreach (var node in nodes)
                    {
                        var href = node.GetAttributeValue("href", "");
                        if (!string.IsNullOrEmpty(href))
                        {
                            string fullUrl = href.StartsWith("http") ? href : "https://pubmed.ncbi.nlm.nih.gov" + href;
                            links.Add(fullUrl.Split('?')[0]);
                        }
                    }

                    if (nodes.Count < 200) hasMorePages = false;
                    else currentPage++;

                    await Task.Delay(800);
                }
                catch { break; }
            }
            return links.ToList();
        }
    }
}