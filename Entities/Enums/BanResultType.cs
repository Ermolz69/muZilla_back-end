namespace muZilla.Entities.Enums
{
    public enum BanResultType
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        UserIsNull,
        SongIsNull,
        CollectionIsNull,
        AccessLevelIsNull,

        ItBanned,
        ItNotBanned,

        UserHasBanAccess,
        UsersAreSame,

        CannotBanAdmins,
        CannotBanUsers,
        CannotBanSongs,
        CannotBanCollections,
        CannotManageSupports,
        CannotDownloadSongs,

        Success
        #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
