using System;

namespace WebScraper.Models
{
    /// <summary>
    /// Representa o resultado de uma validação de dados.
    /// A estrutura imutável (record) garante segurança em operações paralelas.
    /// </summary>
    /// <param name="IsValid">Indica se o item cumpre os requisitos de qualidade.</param>
    /// <param name="Message">Descrição do sucesso ou detalhamento técnico da falha.</param>
    public record ValidationResult(bool IsValid, string Message);
}