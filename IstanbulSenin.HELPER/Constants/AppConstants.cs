namespace IstanbulSenin.HELPER.Constants
{
    /// <summary>
    /// Uygulama genelinde kullanılan sabit değerler
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// İşletme e-posta alanı
        /// </summary>
        public const string EmailDomain = "@ibb.gov.tr";

        /// <summary>
        /// Roller
        /// </summary>
        public static class Roles
        {
            public const string SuperAdmin = "SuperAdmin";
            public const string Admin = "Admin";
        }

        /// <summary>
        /// Kullanıcı rolleri koleksiyonu
        /// </summary>
        public static readonly string[] DefaultRoles = { Roles.SuperAdmin, Roles.Admin };

        /// <summary>
        /// Bölüm (Section) varsayılan değerleri
        /// </summary>
        public static class SectionDefaults
        {
            public const string DefaultRole = "all";
            public const string DefaultSize = "slider";
        }

        /// <summary>
        /// Bildirim (Notification) hedef kitlesi
        /// </summary>
        public static class NotificationAudience
        {
            public const string All = "all";
            public const string Guest = "guest";
            public const string Regular = "regular";
        }

        /// <summary>
        /// Yol (Path) sabitleri
        /// </summary>
        public static class Paths
        {
            public const string LoginPath = "/Account/Login";
            public const string AccessDeniedPath = "/Account/Login";
            public const string IconsFolder = "uploads/icons";
            public const string IconsUrlPrefix = "/uploads/icons/";
        }

        /// <summary>
        /// Varsayılan rota
        /// </summary>
        public static class DefaultRoute
        {
            public const string Controller = "Section";
            public const string Action = "Index";
        }
    }
}
