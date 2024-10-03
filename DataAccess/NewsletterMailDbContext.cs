using EmailPOC.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace NAEPortal.Core.DataAccess
{
    public class NewsletterMailDbContext : DbContext
    {
        public NewsletterMailDbContext(DbContextOptions<NewsletterMailDbContext> options)
            : base (options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NewsletterMailEntity>(entity =>
            {
                entity.ToTable("NewsletterMails");
                entity.HasKey(e => e.Id);

            });
        }

        public DbSet<NewsletterMailEntity> NewsletterMails { get; set; }
        public DbSet<FailedMailEntity> FaildMails { get; set; }
    }
}
