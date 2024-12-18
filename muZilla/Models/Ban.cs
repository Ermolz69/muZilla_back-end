using muZilla.Models;

public class Ban
{
    public int Id { get; set; }
    public int BannedByUserId { get; set; }
    public int? BannedUserId { get; set; }
    public int? BannedSongId { get; set; }
    public int? BannedCollectionId { get; set; } 

    public int BanType { get; set; }

    public string Reason { get; set; } = string.Empty;
    public DateTime BanUntilUtc { get; set; }
    public DateTime BannedAtUtc { get; set; }

    public User BannedByUser { get; set; } = null!;
    public User? BannedUser { get; set; }
    public Song? BannedSong { get; set; }
    public Collection? BannedCollection { get; set; }
}
