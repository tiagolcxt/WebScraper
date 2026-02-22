using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace WebScraper.Services
{
    public class QueryService
    {
        public string TranslateToPubMed(string userQuery)
        {
            if (string.IsNullOrWhiteSpace(userQuery)) return "";

            // Tradução de operadores para o padrão PubMed
            string translated = userQuery
                .Replace("&&", " AND ")
                .Replace("||", " OR ")
                .Replace("!", " NOT ");

            // Garante que espaços duplos não quebrem a Query
            return Regex.Replace(translated, @"\s+", " ").Trim();
        }

        public List<string> ExtractKeywords(string userQuery)
        {
            // Regex melhorado: Captura o que está entre aspas OU palavras individuais
            var matches = Regex.Matches(userQuery, @"\""([^\""]+)\""|(\w+)");

            var operadores = new[] { "AND", "OR", "NOT", "&&", "||" };

            return matches.Cast<Match>()
                .Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)
                .Where(v => !operadores.Contains(v.ToUpper()) && v.Length > 2)
                .Distinct()
                .ToList();
        }
    }
}