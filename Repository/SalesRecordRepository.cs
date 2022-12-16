using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using Repository.Data;
using Repository.Interfaces;
using Models;
using Repository.Exceptions;

namespace Repository
{
    public class SalesRecordRepository : ISalesRecordRepository
    {
        private readonly Context _context;
        private readonly IConfiguration _configuration;

        public SalesRecordRepository(Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<List<SalesRecord>> FinByDateAsync(DateTime? minDate, DateTime? maxDate)
        {
            IQueryable<SalesRecord> querie = _context.SalesRecord.Include(sale => sale.Seller).ThenInclude(seller => seller.Department).AsQueryable();

            if (minDate.HasValue)
                querie = querie.Where(sale => sale.Date >= minDate);

            if (maxDate.HasValue)
                querie = querie.Where(sale => sale.Date <= maxDate);

            return await querie.OrderBy(sl => sl.Date).ToListAsync();
        }

        public async Task<List<SalesRecord>> GetAllPaginatedAsync(int page = 1)
        {
            int pageSize = _configuration.GetValue<int>("Pagination:ItemsPerPage");
            return await _context.SalesRecord.OrderBy(dep => dep.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<List<SalesRecord>> GetAllPaginatedWithFilterAsync(Expression<Func<SalesRecord, bool>> filter, int page = 1)
        {
            int pageSize = _configuration.GetValue<int>("Pagination:ItemsPerPage");
            return await _context.SalesRecord.Where(filter).OrderBy(dep => dep.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<List<SalesRecord>> GetAllPaginatedByDateAsync(DateTime? minDate, DateTime? maxDate, int page)
        {
            IQueryable<SalesRecord> querie = _context.SalesRecord.Include(sale => sale.Seller).ThenInclude(seller => seller.Department).AsQueryable();

            if (minDate.HasValue)
                querie = querie.Where(sale => sale.Date >= minDate);

            if (maxDate.HasValue)
                querie = querie.Where(sale => sale.Date <= maxDate);

            int pageSize = _configuration.GetValue<int>("Pagination:ItemsPerPage");
            return await querie.OrderBy(sale => sale.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }



        public async Task<int> TotalItemsAsync()
        {
            return await _context.SalesRecord.CountAsync();
        }

        public async Task<int> TotalItemsWithFilterAsync(Expression<Func<SalesRecord, bool>> filter)
        {
            return await _context.SalesRecord.Where(filter).CountAsync();
        }

        public async Task<int> TotalItemsByDateAsync(DateTime? minDate, DateTime? maxDate)
        {
            IQueryable<SalesRecord> querie = _context.SalesRecord.AsQueryable();

            if (minDate.HasValue)
                querie = querie.Where(sale => sale.Date >= minDate);

            if (maxDate.HasValue)
                querie = querie.Where(sale => sale.Date <= maxDate);
            return await querie.CountAsync();
        }

        public async Task DeleteAsync(int id)
        {

        }
        public async Task UpdateAsync(SalesRecord tObj)
        {
            if (!await _context.SalesRecord.AnyAsync(sale => sale.Id == tObj.Id))
                throw new NotFoundException("Id not founded");

            try
            {
                _context.Update(tObj);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                throw new DbConcurrencyException(e.Message);
            }
        }

        public async Task<SalesRecord> FindByIdAsync(int id) => await _context.SalesRecord.FirstOrDefaultAsync(sale => sale.Id == id);

        public async Task<List<SalesRecord>> FindAllAsync() => await _context.SalesRecord.OrderByDescending(sale => sale.Id).ToListAsync();

        public async Task<List<SalesRecord>> FindAllWithFilterAsync(Expression<Func<SalesRecord, bool>> filter) =>
            await _context.SalesRecord.Where(filter).ToListAsync();

        public async Task InsertAsync(SalesRecord tObj)
        {
            _context.Add(tObj);
            await _context.SaveChangesAsync();
        }
    }
}
