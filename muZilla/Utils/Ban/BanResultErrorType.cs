namespace muZilla.Utils.Ban
{
    public enum BanResultType
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        UserIsNull,
        AccessLevelIsNull,
        UserIsBanned,
        UserHasBanAccess,
        UsersAreSame,
        SongIsAlreadyBanned,
        CollectionIsAlreadyBanned,
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
