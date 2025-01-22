using MACS.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml;

namespace MACS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<HistoryCar> HistoryCar { get; set; }
        public DbSet<TokenRequest> TokenRequest { get; set; }
    }


}