using System;
using System.Collections.Generic;
using System.Threading.Tasks; // OBRIGATÓRIO para Task
using WebScraper.Models;      // OBRIGATÓRIO para SourceType

namespace WebScraper.Interfaces
{
    public interface ISearchNavigator
    {
        /// <summary>
        /// Busca links baseada nos filtros completos da interface.
        /// </summary>
        Task<List<string>> GetLinksAsync(
            List<string> mushrooms,
            DateTime? startDate,
            DateTime? endDate,
            SourceType sourceType
        );
    }
}