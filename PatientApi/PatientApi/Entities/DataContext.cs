using Microsoft.EntityFrameworkCore;

namespace PatientApi.Entities
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }
        public DbSet<Patient> Patients { get; set; }

        public DbSet<GivenName> GivenNames { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GivenName>()
                .HasOne(g => g.Patient)
                .WithMany(p => p.Given)
                .HasForeignKey(g => g.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
