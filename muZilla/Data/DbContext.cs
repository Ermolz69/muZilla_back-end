using Microsoft.EntityFrameworkCore;
using muZilla.Models;
using System;

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
        public DbSet<Message> Messages { get; set; }
        public DbSet<SupportMessage> SupportMessages { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<Report> Reports { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация связи один-ко-многим между User и AccessLevel 
            modelBuilder.Entity<User>()
                .HasOne(u => u.AccessLevel)
                .WithMany()
                .HasForeignKey("AccessLevelId")
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация связи один-ко-многим между User и ProfilePicture (Image)
            modelBuilder.Entity<User>()
                .HasOne(u => u.ProfilePicture)
                .WithMany()
                .HasForeignKey("ProfilePictureId")
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация связи один-ко-многим для FriendsCouple
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

            // Конфигурация связи один-ко-многим для BlockedUser
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

            // Конфигурация связи многие-ко-многим между Song и Author (User)
            modelBuilder.Entity<Song>()
                .HasMany(s => s.Authors)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "SongAuthor",
                    sa => sa.HasOne<User>()
                            .WithMany()
                            .HasForeignKey("UserId")
                            .HasConstraintName("FK_SongAuthor_Users_UserId")
                            .OnDelete(DeleteBehavior.Cascade),
                    sa => sa.HasOne<Song>()
                            .WithMany()
                            .HasForeignKey("SongId")
                            .HasConstraintName("FK_SongAuthor_Songs_SongId")
                            .OnDelete(DeleteBehavior.Cascade));

            // Конфигурация связи один-ко-многим для Remixes
            modelBuilder.Entity<Song>()
                .HasOne(s => s.Original)
                .WithMany(s => s.Remixes)
                .HasForeignKey(s => s.OriginalId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Конфигурация связи один-ко-многим между Collection и Author (User)
            modelBuilder.Entity<Collection>()
                .HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey("AuthorId")
                .OnDelete(DeleteBehavior.SetNull);

            // Конфигурация связи один-ко-многим между Collection и Cover (Image)
            modelBuilder.Entity<Collection>()
                .HasOne(c => c.Cover)
                .WithMany()
                .HasForeignKey("CoverId")
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация связи многие-ко-многим между Collection и Song
            modelBuilder.Entity<Collection>()
                .HasMany(c => c.Songs)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "CollectionSong",
                    cs => cs.HasOne<Song>()
                            .WithMany()
                            .HasForeignKey("SongId")
                            .HasConstraintName("FK_CollectionSong_Songs_SongId")
                            .OnDelete(DeleteBehavior.Cascade),
                    cs => cs.HasOne<Collection>()
                            .WithMany()
                            .HasForeignKey("CollectionId")
                            .HasConstraintName("FK_CollectionSong_Collections_CollectionId")
                            .OnDelete(DeleteBehavior.Cascade));

            // Конфигурация связи один-ко-одному между User и FavoritesCollection (Collection)
            modelBuilder.Entity<User>()
                .HasOne(u => u.FavoritesCollection)
                .WithOne()
                .HasForeignKey<User>(u => u.FavoritesCollectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация связи многие-ко-многим между User и LikedCollections (Collection)
            modelBuilder.Entity<User>()
                .HasMany(u => u.LikedCollections)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserLikedCollections",
                    ulc => ulc.HasOne<Collection>()
                            .WithMany()
                            .HasForeignKey("CollectionId")
                            .HasConstraintName("FK_UserLikedCollections_Collections_CollectionId")
                            .OnDelete(DeleteBehavior.Cascade),
                    ulc => ulc.HasOne<User>()
                            .WithMany()
                            .HasForeignKey("UserId")
                            .HasConstraintName("FK_UserLikedCollections_Users_UserId")
                            .OnDelete(DeleteBehavior.Cascade));

            modelBuilder.Entity<Ban>()
                .HasOne(b => b.BannedByUser)
                .WithMany()
                .HasForeignKey(b => b.BannedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ban>()
                .HasOne(b => b.BannedUser)
                .WithMany()
                .HasForeignKey(b => b.BannedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ban>()
                .HasOne(b => b.BannedSong)
                .WithMany()
                .HasForeignKey(b => b.BannedSongId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ban>()
                .HasOne(b => b.BannedCollection)
                .WithMany()
                .HasForeignKey(b => b.BannedCollectionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
