using System;
using System.Collections.Generic;

namespace WebScraper.Models
{
    public class Research
    {
        public string Title { get; set; }
        public List<string> Mushrooms { get; set; } = new List<string>();
        public string Author { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Abstract { get; set; }
        public string Link { get; set; }
        public SourceType Type { get; set; } // O C# entende que isso vem do outro arquivo porque o namespace é o mesmo
    }
}