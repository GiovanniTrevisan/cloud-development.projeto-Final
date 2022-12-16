using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MagicTheGathering_Catalog.Models.ViewModel
{
    public class SalesRecordFormViewModel
    {
        public SalesRecord SalesRecord { get; set; }
        public List<Seller> Sellers { get; set; } = new List<Seller>();
    }
}
