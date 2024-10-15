using Microsoft.EntityFrameworkCore;
using muZilla.Models;

namespace muZilla.Data
{
    public class MuzillaDbContext : DbContext
    {
        public MuzillaDbContext(DbContextOptions<MuzillaDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<FriendsCouple> FriendsCouples { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<BlockedUser> BlockedUsers { get; set; }
        public DbSet<AccessLevel> AccessLevels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.AccessLevel)
                .WithMany()
                .HasForeignKey("AccessLevelId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.ProfilePicture)
                .WithMany()
                .HasForeignKey("ProfilePictureId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendsCouple>()
                .HasOne(fc => fc.User)
                .WithMany(u => u.Friends)
                .HasForeignKey(fc => fc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendsCouple>()
                .HasOne(fc => fc.Friend)
                .WithMany()
                .HasForeignKey(fc => fc.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BlockedUser>()
                .HasOne(bu => bu.User)
                .WithMany(u => u.Blocked)
                .HasForeignKey(bu => bu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BlockedUser>()
                .HasOne(bu => bu.Blocked)
                .WithMany()
                .HasForeignKey(bu => bu.BlockedId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Song>()
                .HasMany(s => s.Authors)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "SongAuthor",
                    sa => sa.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    sa => sa.HasOne<Song>().WithMany().HasForeignKey("SongId"));

            modelBuilder.Entity<Song>()
                .HasOne(s => s.Original)
                .WithMany(s => s.Remixes)
                .HasForeignKey(s => s.OriginalId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Song>()
                .HasOne<Song>()
                .WithMany()
                .HasForeignKey(s => s.OriginalId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Collection>()
                .HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey("AuthorId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Collection>()
                .HasOne(c => c.Cover)
                .WithMany()
                .HasForeignKey("CoverId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Collection>()
                .HasMany(c => c.Songs)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "CollectionSong",
                    cs => cs.HasOne<Song>().WithMany().HasForeignKey("SongId"),
                    cs => cs.HasOne<Collection>().WithMany().HasForeignKey("CollectionId"));
        }
    }
}