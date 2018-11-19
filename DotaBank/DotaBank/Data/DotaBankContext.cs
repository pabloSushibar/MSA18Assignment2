using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DotaBank.Models
{
    public class DotaBankContext : DbContext
    {
        public DotaBankContext (DbContextOptions<DotaBankContext> options)
            : base(options)
        {
        }

        public DbSet<DotaBank.Models.DotaItem> DotaItem { get; set; }
    }
}
