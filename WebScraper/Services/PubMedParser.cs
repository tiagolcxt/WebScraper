using System.Net;
using HtmlAgilityPack;
using WebScraper.Interfaces;
using WebScraper.Models;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WebScraper.Services
{
    public class PubMedParser : IResearchParser
    {
        private readonly IIdentityService _identity;
        private static readonly HttpClient _httpClient = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            AutomaticDecompression = DecompressionMethods.All,
            MaxConnectionsPerServer = 20
        });

        public PubMedParser(IIdentityService identity) => _identity = identity;

        public async Task<Research> ParseAsync(string url, string mushroomName)
        {
            try
            {
                await Task.Delay(new Random().Next(1000, 2500));
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                _identity.ApplyIdentity(request); // Certifique-se que seu IdentityService tem esse método recebendo HttpRequestMessage

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return new Research { Link = url, Title = $"[ERRO HTTP {response.StatusCode}]" };

                string html = await response.Content.ReadAsStringAsync();
                if (html.Length < 2000) return new Research { Link = url, Title = "[ERRO: Bloqueio de IP]" };

                return ExtractData(html, url, mushroomName);
            }
            catch (Exception ex)
            {
                return new Research { Link = url, Title = $"[ERRO CRÍTICO: {ex.Message}]" };
            }
        }

        private Research ExtractData(string html, string url, string mushroomName)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var res = new Research { Link = url, Type = SourceType.ScientificArticle };
            res.Mushrooms.Add(mushroomName);

            // 1. Título (Busca em meta tags primeiro por ser mais estável)
            var titleNode = doc.DocumentNode.SelectSingleNode("//meta[@name='citation_title']")
                         ?? doc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'heading-title')]");
            res.Title = FormatTitle(WebUtility.HtmlDecode(titleNode?.GetAttributeValue("content", titleNode.InnerText) ?? "").Trim());

            // 2. Autor (NOVO: Extraindo da meta tag ou lista de autores)
            var authorNode = doc.DocumentNode.SelectSingleNode("//meta[@name='citation_author']")
                          ?? doc.DocumentNode.SelectSingleNode("//div[@class='authors-list']//a[@class='full-name'][1]");
            res.Author = WebUtility.HtmlDecode(authorNode?.GetAttributeValue("content", authorNode.InnerText) ?? "Autor não informado").Trim();

            // 3. Data (Correção do Bug: Tenta pegar a data completa)
            var dateNode = doc.DocumentNode.SelectSingleNode("//meta[@name='citation_date']")
                        ?? doc.DocumentNode.SelectSingleNode("//span[@class='cit']")
                        ?? doc.DocumentNode.SelectSingleNode("//span[@class='secondary-date']");

            string rawDate = dateNode?.GetAttributeValue("content", dateNode.InnerText) ?? "";
            res.PublicationDate = ParsePubMedDate(rawDate);

            // 4. Abstract (Unificando parágrafos estruturados)
            var abstractNodes = doc.DocumentNode.SelectNodes("//div[@id='abstract']//p")
                             ?? doc.DocumentNode.SelectNodes("//div[contains(@class, 'abstract-content')]//p")
                             ?? doc.DocumentNode.SelectNodes("//div[@class='abstract']");

            res.Abstract = abstractNodes != null
                ? Regex.Replace(string.Join(" ", abstractNodes.Select(n => WebUtility.HtmlDecode(n.InnerText).Trim())), @"\s+", " ")
                : "Resumo (Abstract) não disponível.";

            return res;
        }

        private DateTime ParsePubMedDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return DateTime.MinValue;

            // Tenta capturar o ano
            var yearMatch = Regex.Match(raw, @"(\d{4})");
            if (!yearMatch.Success) return DateTime.MinValue;

            int year = int.Parse(yearMatch.Value);

            // Se for apenas o ano ou formato curto, retornamos 1º de Janeiro daquele ano
            // Mas tentamos o parse completo primeiro
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fullDate))
                return fullDate;

            return new DateTime(year, 1, 1);
        }

        private string FormatTitle(string t) =>
            (t == t.ToUpper() && t.Length > 10) ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.ToLower()) : t;
    }
}