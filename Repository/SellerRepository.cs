using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using Repository.Data;
using Repository.Interfaces;
using Models;
using Repository.Exceptions;

namespace Repository
{
    public class SellerRepository : ISellerRepository
    {
        private readonly Context _context;
        private readonly IConfiguration _configuration;

        public SellerRepository(Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Seller> FindByIdAsync(int id) => await _context.Seller.Include(sl => sl.Department).Include(sel => sel.SalesRecords).FirstOrDefaultAsync(sel => sel.Id == id);

        public async Task<List<Seller>> FindAllAsync() => await _context.Seller.OrderByDescending(sl => sl.Id).ToListAsync();

        public async Task<List<Seller>> FindAllWithFilterAsync(Expression<Func<Seller, bool>> filter) =>
            await _context.Seller.Include(sl => sl.Department).Include(s => s.SalesRecords).Where(filter).ToListAsync();

        public async Task InsertAsync(Seller tObj)
        {
            _context.Add(tObj);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                Seller seller = _context.Seller.FirstOrDefault(sel => sel.Id == id);
                _context.Remove(seller);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new IntegrityException("This seller has sales");
            }
        }

        public async Task UpdateAsync(Seller tObj)
        {
            if (!await _context.Seller.AnyAsync(sel => sel.Id == tObj.Id))
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

        public async Task<List<Seller>> GetAllPaginatedAsync(int page = 1)
        {
            int pageSize = _configuration.GetValue<int>("Pagination:ItemsPerPage");
            return await _context.Seller.Include(sl => sl.Department).OrderBy(sl => sl.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<List<Seller>> GetAllPaginatedWithFilterAsync(Expression<Func<Seller, bool>> filter, int page = 1)
        {
            int pageSize = _configuration.GetValue<int>("Pagination:ItemsPerPage");
            return await _context.Seller.Include(sl => sl.Department).Where(filter).OrderBy(sl => sl.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<int> TotalItemsAsync()
        {
            return await _context.Seller.CountAsync();
        }

        public async Task<int> TotalItemsWithFilterAsync(Expression<Func<Seller, bool>> filter)
        {
            return await _context.Seller.Where(filter).CountAsync();
        }
    }
}
