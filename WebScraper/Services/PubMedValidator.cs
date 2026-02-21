using WebScraper.Interfaces;
using WebScraper.Models;

namespace WebScraper.Services
{
    public class PubMedValidator : IResearchValidator
    {
        public ValidationResult Validate(Research item)
        {
            // 1. Proteção contra objetos nulos na fila de processamento
            if (item == null) return new ValidationResult(false, "Objeto de pesquisa nulo.");

            // 2. Filtra erros técnicos reportados pelo Parser (ex: HTTP 403, Timeout)
            // Mantém suporte para títulos que usam colchetes legítimos
            if (string.IsNullOrEmpty(item.Title) || item.Title.StartsWith("[ERRO"))
            {
                return new ValidationResult(false, $"Falha técnica na captura: {item.Title}");
            }

            // 3. Validação de integridade: Títulos excessivamente curtos indicam extração parcial
            if (item.Title.Length < 10)
            {
                return new ValidationResult(false, "Qualidade insuficiente: Título muito curto ou inválido.");
            }

            // 4. Se passou pelos filtros, o item é considerado válido para o relatório final
            return new ValidationResult(true, "Sucesso");
        }
    }
}