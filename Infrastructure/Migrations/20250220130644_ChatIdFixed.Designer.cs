﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using muZilla.Infrastructure.Data;

#nullable disable

namespace muZilla.Infrastructure.Migrations
{
    [DbContext(typeof(MuzillaDbContext))]
    [Migration("20250220130644_ChatIdFixed")]
    partial class ChatIdFixed
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ChatUser", b =>
                {
                    b.Property<int>("ChatId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("ChatId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("ChatUser");
                });

            modelBuilder.Entity("CollectionSong", b =>
                {
                    b.Property<int>("CollectionId")
                        .HasColumnType("int");

                    b.Property<int>("SongId")
                        .HasColumnType("int");

                    b.HasKey("CollectionId", "SongId");

                    b.HasIndex("SongId");

                    b.ToTable("CollectionSong");
                });

            modelBuilder.Entity("SongAuthor", b =>
                {
                    b.Property<int>("SongId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("SongId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("SongAuthor");
                });

            modelBuilder.Entity("UserLikedCollections", b =>
                {
                    b.Property<int>("CollectionId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("CollectionId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("UserLikedCollections");
                });

            modelBuilder.Entity("muZilla.Entities.Models.AccessLevel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("CanBanCollection")
                        .HasColumnType("bit");

                    b.Property<bool>("CanBanSong")
                        .HasColumnType("bit");

                    b.Property<bool>("CanBanUser")
                        .HasColumnType("bit");

                    b.Property<bool>("CanDownload")
                        .HasColumnType("bit");

                    b.Property<bool>("CanManageAL")
                        .HasColumnType("bit");

                    b.Property<bool>("CanManageReports")
                        .HasColumnType("bit");

                    b.Property<bool>("CanManageSupports")
                        .HasColumnType("bit");

                    b.Property<bool>("CanReport")
                        .HasColumnType("bit");

                    b.Property<bool>("CanUpload")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("AccessLevels");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Ban", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BanType")
                        .HasColumnType("int");

                    b.Property<DateTime>("BanUntilUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("BannedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("BannedByUserId")
                        .HasColumnType("int");

                    b.Property<int?>("BannedCollectionId")
                        .HasColumnType("int");

                    b.Property<int?>("BannedSongId")
                        .HasColumnType("int");

                    b.Property<int?>("BannedUserId")
                        .HasColumnType("int");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BannedByUserId");

                    b.HasIndex("BannedCollectionId");

                    b.HasIndex("BannedSongId");

                    b.HasIndex("BannedUserId");

                    b.ToTable("Bans");
                });

            modelBuilder.Entity("muZilla.Entities.Models.BlockedUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BlockedId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BlockedId");

                    b.HasIndex("UserId");

                    b.ToTable("BlockedUsers");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Chat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("Chats");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Collection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AuthorId")
                        .HasColumnType("int");

                    b.Property<int>("CoverId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsBanned")
                        .HasColumnType("bit");

                    b.Property<bool>("IsFavorite")
                        .HasColumnType("bit");

                    b.Property<int>("Likes")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ViewingAccess")
                        .HasColumnType("int");

                    b.Property<int>("Views")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("CoverId");

                    b.ToTable("Collections");
                });

            modelBuilder.Entity("muZilla.Entities.Models.FriendsCouple", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("FriendId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FriendId");

                    b.HasIndex("UserId");

                    b.ToTable("FriendsCouples");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Image", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DomainColor")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageFilePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ChatId")
                        .HasColumnType("int");

                    b.Property<int>("ChatId1")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("FileData")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("SenderId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ChatId");

                    b.HasIndex("ChatId1");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Report", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatorLogin")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsClosed")
                        .HasColumnType("bit");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Song", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CoverId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Genres")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("HasExplicitLyrics")
                        .HasColumnType("bit");

                    b.Property<bool>("IsBanned")
                        .HasColumnType("bit");

                    b.Property<int>("Length")
                        .HasColumnType("int");

                    b.Property<int>("Likes")
                        .HasColumnType("int");

                    b.Property<int?>("OriginalId")
                        .HasColumnType("int");

                    b.Property<DateTime>("PublishDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("RemixesAllowed")
                        .HasColumnType("bit");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Views")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CoverId");

                    b.HasIndex("OriginalId");

                    b.ToTable("Songs");
                });

            modelBuilder.Entity("muZilla.Entities.Models.SupportMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReceiverLogin")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SenderLogin")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SupporterId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SupportMessages");
                });

            modelBuilder.Entity("muZilla.Entities.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccessLevelId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("FavoritesCollectionId")
                        .HasColumnType("int");

                    b.Property<bool>("IsBanned")
                        .HasColumnType("bit");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProfilePictureId")
                        .HasColumnType("int");

                    b.Property<int>("PublicId")
                        .HasColumnType("int");

                    b.Property<bool>("ReceiveNotifications")
                        .HasColumnType("bit");

                    b.Property<bool>("TwoFactoredAuthentification")
                        .HasColumnType("bit");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AccessLevelId");

                    b.HasIndex("FavoritesCollectionId")
                        .IsUnique()
                        .HasFilter("[FavoritesCollectionId] IS NOT NULL");

                    b.HasIndex("ProfilePictureId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ChatUser", b =>
                {
                    b.HasOne("muZilla.Entities.Models.Chat", null)
                        .WithMany()
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("muZilla.Entities.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("CollectionSong", b =>
                {
                    b.HasOne("muZilla.Entities.Models.Collection", null)
                        .WithMany()
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_CollectionSong_Collections_CollectionId");

                    b.HasOne("muZilla.Entities.Models.Song", null)
                        .WithMany()
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_CollectionSong_Songs_SongId");
                });

            modelBuilder.Entity("SongAuthor", b =>
                {
                    b.HasOne("muZilla.Entities.Models.Song", null)
                        .WithMany()
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_SongAuthor_Songs_SongId");

                    b.HasOne("muZilla.Entities.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_SongAuthor_Users_UserId");
                });

            modelBuilder.Entity("UserLikedCollections", b =>
                {
                    b.HasOne("muZilla.Entities.Models.Collection", null)
                        .WithMany()
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_UserLikedCollections_Collections_CollectionId");

                    b.HasOne("muZilla.Entities.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_UserLikedCollections_Users_UserId");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Ban", b =>
                {
                    b.HasOne("muZilla.Entities.Models.User", "BannedByUser")
                        .WithMany()
                        .HasForeignKey("BannedByUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("muZilla.Entities.Models.Collection", "BannedCollection")
                        .WithMany()
                        .HasForeignKey("BannedCollectionId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("muZilla.Entities.Models.Song", "BannedSong")
                        .WithMany()
                        .HasForeignKey("BannedSongId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("muZilla.Entities.Models.User", "BannedUser")
                        .WithMany()
                        .HasForeignKey("BannedUserId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("BannedByUser");

                    b.Navigation("BannedCollection");

                    b.Navigation("BannedSong");

                    b.Navigation("BannedUser");
                });

            modelBuilder.Entity("muZilla.Entities.Models.BlockedUser", b =>
                {
                    b.HasOne("muZilla.Entities.Models.User", "Blocked")
                        .WithMany()
                        .HasForeignKey("BlockedId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("muZilla.Entities.Models.User", "User")
                        .WithMany("Blocked")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Blocked");

                    b.Navigation("User");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Collection", b =>
                {
                    b.HasOne("muZilla.Entities.Models.User", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.HasOne("muZilla.Entities.Models.Image", "Cover")
                        .WithMany()
                        .HasForeignKey("CoverId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("Cover");
                });

            modelBuilder.Entity("muZilla.Entities.Models.FriendsCouple", b =>
                {
                    b.HasOne("muZilla.Entities.Models.User", "Friend")
                        .WithMany()
                        .HasForeignKey("FriendId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("muZilla.Entities.Models.User", "User")
                        .WithMany("Friends")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Friend");

                    b.Navigation("User");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Message", b =>
                {
                    b.HasOne("muZilla.Entities.Models.Chat", null)
                        .WithMany("Messages")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("muZilla.Entities.Models.Chat", "Chat")
                        .WithMany()
                        .HasForeignKey("ChatId1")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chat");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Song", b =>
                {
                    b.HasOne("muZilla.Entities.Models.Image", "Cover")
                        .WithMany()
                        .HasForeignKey("CoverId");

                    b.HasOne("muZilla.Entities.Models.Song", "Original")
                        .WithMany("Remixes")
                        .HasForeignKey("OriginalId");

                    b.Navigation("Cover");

                    b.Navigation("Original");
                });

            modelBuilder.Entity("muZilla.Entities.Models.User", b =>
                {
                    b.HasOne("muZilla.Entities.Models.AccessLevel", "AccessLevel")
                        .WithMany()
                        .HasForeignKey("AccessLevelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("muZilla.Entities.Models.Collection", "FavoritesCollection")
                        .WithOne()
                        .HasForeignKey("muZilla.Entities.Models.User", "FavoritesCollectionId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("muZilla.Entities.Models.Image", "ProfilePicture")
                        .WithMany()
                        .HasForeignKey("ProfilePictureId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("AccessLevel");

                    b.Navigation("FavoritesCollection");

                    b.Navigation("ProfilePicture");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Chat", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("muZilla.Entities.Models.Song", b =>
                {
                    b.Navigation("Remixes");
                });

            modelBuilder.Entity("muZilla.Entities.Models.User", b =>
                {
                    b.Navigation("Blocked");

                    b.Navigation("Friends");
                });
#pragma warning restore 612, 618
        }
    }
}
