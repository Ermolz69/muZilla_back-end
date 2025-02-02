namespace muZilla.Utils.User
{
    public enum BanResultType
    {
        UserIsNull,
        AccessLevelIsNull,
        ExecutorIsBanned,
        UserHasBanAccess,
        UserIsAlreadyBanned,
        UsersAreSame,
        CannotBanUsers,
        SongIsAlreadyBanned,
        CannotBanSongs,
        CollectionIsAlreadyBanned,
        CannotBanCollections,
        CannotManageSupports,
        CannotDownloadSongs,
        Success
    }
}
