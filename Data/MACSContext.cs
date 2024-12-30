using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MACS.Models;

namespace MACS.Data
{
    public class MACSContext : DbContext
    {
        public MACSContext (DbContextOptions<MACSContext> options)
            : base(options)
        {
        }

        public DbSet<MACS.Models.HistoryCar> HistoryCar { get; set; } = default!;
    }
}
