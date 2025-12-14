using CQRS_Project.Entities;
using Microsoft.EntityFrameworkCore;

namespace CQRS_Project.Context
{
    public class CqrsContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server=DESKTOP-BBV0NC6;database=CQRSProjectDB;integrated security=true;trust server certificate=true");
        }

        public DbSet<About> Abouts { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Slider> Sliders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🚫 Cascade Delete kapatıyoruz → Hatanın %100 çözümü
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.PickUpLocation)
                .WithMany()
                .HasForeignKey(r => r.PickUpLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.DropOffLocation)
                .WithMany()
                .HasForeignKey(r => r.DropOffLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
