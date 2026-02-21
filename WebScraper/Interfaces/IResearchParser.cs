using System;
using System.Threading.Tasks;
using WebScraper.Models;

namespace WebScraper.Interfaces
{
    public interface IResearchParser
    {
        /// <summary>
        /// Acessa uma URL específica e extrai os dados detalhados para criar um objeto Research.
        /// </summary>
        /// <param name="url">O endereço web da página da pesquisa/artigo.</param>
        /// <returns>Um objeto Research preenchido com os dados da página.</returns>
        Task<Research> ParseAsync(string url, string mushroomName);
    }
}