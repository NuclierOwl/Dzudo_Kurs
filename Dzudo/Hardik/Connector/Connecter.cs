using Dzudo.Hardik.Connector.Date;
using Kurs_Dzudo.Hardik.Connector.Date;
using Microsoft.EntityFrameworkCore;

namespace Kurs_Dzudo.Hardik.Connector
{
    public class Connector : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=45.67.56.214,5421;Database=user16;User Id=user16;Password=dZ28IVE5;",
                options => options.EnableRetryOnFailure()
            );
        }

        public DbSet<UkhasnikiDao> Ukhasniki { get; set; }
        public DbSet<OrganizatorDao> Organizatori { get; set; }
        public DbSet<GroupDao_2> Groups { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Tatami> Tatamis { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<UkhasnikiDao>(entity =>
            {
                entity.ToTable("ukhasniki", "Sec");
                entity.HasKey(e => e.Name);
            });

            mb.Entity<OrganizatorDao>(entity =>
            {
                entity.ToTable("organizator", "Sec");
                entity.HasKey(e => e.login);
            });

            mb.Entity<GroupDao_2>(entity =>
            {
                entity.ToTable("groups", "Sec");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Id).ValueGeneratedOnAdd();

                entity.HasOne(g => g.Tatami)
                      .WithMany()
                      .HasForeignKey(g => g.TatamiId);
            });

            mb.Entity<Match>(entity =>
            {
                entity.ToTable("matches", "Sec");
                entity.HasKey(m => new { m.GroupId, m.participant1_name, m.participant2_name });

                entity.HasOne(m => m.Participant1)
                      .WithMany()
                      .HasForeignKey(m => m.participant1_name)
                      .HasPrincipalKey(u => u.Name)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Participant2)
                      .WithMany()
                      .HasForeignKey(m => m.participant2_name)
                      .HasPrincipalKey(u => u.Name)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Winner)
                      .WithMany()
                      .HasForeignKey(m => m.winner_name)
                      .HasPrincipalKey(u => u.Name)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Loser)
                      .WithMany()
                      .HasForeignKey(m => m.loser_name)
                      .HasPrincipalKey(u => u.Name)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Group)
                      .WithMany()
                      .HasForeignKey(m => m.GroupId);
            });

            mb.Entity<Tatami>(entity =>
            {
                entity.ToTable("tatami", "Sec");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).ValueGeneratedOnAdd();
                entity.Property(t => t.numdber).HasColumnName("number");
                entity.Property(t => t.Activ).HasColumnName("is_active");
            });
        }
    }
}