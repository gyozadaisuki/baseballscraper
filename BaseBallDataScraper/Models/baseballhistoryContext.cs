using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace BaseBallDataScraper.Models
{
    public partial class baseballhistoryContext : DbContext
    {
        public baseballhistoryContext()
        {
        }

        public baseballhistoryContext(DbContextOptions<baseballhistoryContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Baseballlog> Baseballlogs { get; set; }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            if (!optionsBuilder.IsConfigured)
//            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//                optionsBuilder.UseSqlServer("Server=LAPTOP-FOK6FO48\\SQLEXPRESS;Database=baseballhistorytest;Trusted_Connection=True");
//            }
//        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Japanese_CI_AS");

            modelBuilder.Entity<Baseballlog>(entity =>
            {
                entity.ToTable("baseballlog");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Attackteam)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("attackteam");

                entity.Property(e => e.Ballnumpitcher).HasColumnName("ballnumpitcher");

                entity.Property(e => e.Ballnumtotal).HasColumnName("ballnumtotal");

                entity.Property(e => e.Balltype)
                    .HasMaxLength(50)
                    .HasColumnName("balltype");

                entity.Property(e => e.Batter)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("batter");

                entity.Property(e => e.Batterhand)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("batterhand");

                entity.Property(e => e.Catcher)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("catcher");

                entity.Property(e => e.Gamedate).HasColumnName("gamedate");

                entity.Property(e => e.Gamenumber).HasColumnName("gamenumber");

                entity.Property(e => e.Pitcher)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("pitcher");

                entity.Property(e => e.Pitcherhand)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("pitcherhand");

                entity.Property(e => e.Pitchteam)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("pitchteam");

                entity.Property(e => e.Velocity).HasColumnName("velocity");

                entity.Property(e => e.Xaxis).HasColumnName("xaxis");

                entity.Property(e => e.Yaxis).HasColumnName("yaxis");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
