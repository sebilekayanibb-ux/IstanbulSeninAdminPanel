namespace IstanbulSenin.HELPER.Constants
{
    /// Uygulama genelinde kullanılan sabit değerler
    public static class AppConstants
    {
        /// İşletme e-posta alanı
        public const string EmailDomain = "@ibb.gov.tr";

        /// Roller
        public static class Roles
        {
            public const string SuperAdmin = "SuperAdmin";
            public const string Admin = "Admin";
        }


        /// Kullanıcı rolleri koleksiyonu

        public static readonly string[] DefaultRoles = { Roles.SuperAdmin, Roles.Admin };

        /// Bölüm (Section) varsayılan değerleri

        public static class SectionDefaults
        {
            public const string DefaultRole = "all";
            public const string DefaultSize = "slider";
        }


        /// Bildirim (Notification) hedef kitlesi

        public static class NotificationAudience
        {
            public const string All = "all";
            public const string Guest = "guest";
            public const string Regular = "regular";
        }


        /// Yol (Path) sabitleri

        public static class Paths
        {
            public const string LoginPath = "/Account/Login";
            public const string AccessDeniedPath = "/Account/Login";
            public const string IconsFolder = "uploads/icons";
            public const string IconsUrlPrefix = "/uploads/icons/";
        }


        /// Varsayılan rota

        public static class DefaultRoute
        {
            public const string Controller = "Section";
            public const string Action = "Index";
        }
    }
}
