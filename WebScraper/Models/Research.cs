using WebScraper.Models;

namespace WebScraper.Models
{
    public class Research
    {
        public string Title { get; set; }
        public List<string> Mushrooms { get; set; } = new List<string>(); // Categoria da busca
        public string Author { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Abstract { get; set; }
        public string Link { get; set; }
        public SourceType Type { get; set; }

        // Novos campos para acurácia
        public List<string> KeywordsFound { get; set; } = new();
        public List<string> KeywordsNotFound { get; set; } = new();
    }
}