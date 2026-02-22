using WebScraper.Interfaces;
using WebScraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebScraper.Services
{
    public class PubMedValidator : IResearchValidator
    {
        private readonly QueryService _queryService = new QueryService();

        public ValidationResult Validate(Research item)
        {
            // 1. Proteção básica
            if (item == null) return new ValidationResult(false, "Objeto de pesquisa nulo.");

            if (string.IsNullOrEmpty(item.Title) || item.Title.StartsWith("[ERRO"))
            {
                return new ValidationResult(false, $"Falha técnica na captura: {item.Title}");
            }

            // 2. FILTRO DE OBRIGATORIEDADE (Acurácia)
            // Extraímos os termos da categoria (mushrooms[0] contém a fórmula usada)
            var formulaOriginal = item.Mushrooms.FirstOrDefault();
            if (!string.IsNullOrEmpty(formulaOriginal))
            {
                var keywordsObrigatorias = _queryService.ExtractKeywords(formulaOriginal);

                // Verificamos se pelo menos uma das palavras-chave principais está no texto
                // Isso evita que buscas genéricas tragam lixo.
                bool possuiTermoChave = keywordsObrigatorias.Any(kw =>
                    item.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    item.Abstract.Contains(kw, StringComparison.OrdinalIgnoreCase));

                if (!possuiTermoChave)
                {
                    return new ValidationResult(false, $"Acurácia: Termos obrigatórios não encontrados no texto.");
                }

                // Preenchemos as Keywords encontradas para o relatório
                item.KeywordsFound = keywordsObrigatorias
                    .Where(kw => item.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                                 item.Abstract.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                item.KeywordsNotFound = keywordsObrigatorias.Except(item.KeywordsFound).ToList();
            }

            if (item.Title.Length < 10)
            {
                return new ValidationResult(false, "Qualidade insuficiente: Título muito curto.");
            }

            return new ValidationResult(true, "Sucesso");
        }
    }
}