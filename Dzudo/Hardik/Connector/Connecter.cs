/*
 * using Kurs_Dzudo.Hardik.Connector.Date;
using Microsoft.EntityFrameworkCore;

namespace Kurs_Dzudo.Hardik.Connector;

    public class Connector : DbContext 
    {
        protected override void OnConfiguring(DbContextOptionsBuilder oB)
        {
        oB.UseSqlServer("Server = 45.67.56.214,5421;Database=user16;User Id=user16;Password=dZ28IVE5;"
             ,  options => options.EnableRetryOnFailure());
        }

    public DbSet<UkhasnikiDao> ukhasniki { get; set; }
    public DbSet<OrganizatorDao> organizatori { get; set; }

    protected override void OnModelCreating(ModelBuilder md)
    {
        md.Entity<UkhasnikiDao>().ToTable("ukhasniki", "Sec").HasKey(ukhasniki => ukhasniki.Name);
        md.Entity<OrganizatorDao>().ToTable("organizator", "Sec").HasKey(organizatori => organizatori.login);
    }
} */