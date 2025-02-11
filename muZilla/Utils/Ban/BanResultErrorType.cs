using System;
using System.ComponentModel;
using System.Reflection;

namespace muZilla.Utils.Ban
{
    public enum BanResultType
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        [Description("Пользователь не найден")]
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
