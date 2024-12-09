using Microsoft.EntityFrameworkCore;

namespace SalesService
{
    public class SalesDbContext : DbContext
    {
        public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

        public DbSet<Sale> Sales { get; set; }
        public DbSet<DemandForecast> DemandForecasts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Если есть дополнительные настройки, добавьте их здесь
            modelBuilder.Entity<Sale>().ToTable("Sales");
            modelBuilder.Entity<DemandForecast>().ToTable("DemandForecasts");
        }
    }



}
