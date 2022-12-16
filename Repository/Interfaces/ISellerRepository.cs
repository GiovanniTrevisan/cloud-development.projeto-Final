using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace Repository.Interfaces
{
    public interface ISellerRepository : ICrud<Seller>, IPagination<Seller>
    {
    }
}
