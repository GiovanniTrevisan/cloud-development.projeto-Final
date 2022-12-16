using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Models;

namespace Repository.Interfaces
{
    public interface ISalesRecordRepository : ICrud<SalesRecord>, IPagination<SalesRecord>
    {
        Task<List<SalesRecord>> FinByDateAsync(DateTime? minDate, DateTime? maxDate);
        Task<List<SalesRecord>> GetAllPaginatedByDateAsync(DateTime? minDate, DateTime? maxDate, int page);
        Task<int> TotalItemsByDateAsync(DateTime? minDate, DateTime? maxDate);
    }
}
