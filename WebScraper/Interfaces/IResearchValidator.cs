using WebScraper.Models;

namespace WebScraper.Interfaces
{
    public interface IResearchValidator
    {
        // Agora o C# sabe que este é o SEU ValidationResult
        ValidationResult Validate(Research item);
    }
}